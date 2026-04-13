using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using Microsoft.Data.SqlClient;
using RepoDb;
using RepoDb.Extensions;
using static StudioHub.Hub;
using Word = Microsoft.Office.Interop.Word;

namespace StudioHub.Services;

/// <summary>
/// Servizio per la gestione dei template Word, inclusa modifica, salvataggio, duplicazione e lock. Gestisce
/// l'integrazione con Microsoft Word tramite COM e l'accesso ai dati tramite RepoDb.
/// </summary>
public class ManageWordTemplatesService {

    private const string WORD_FILE_EXT = ".docx";
    private const string HEADERS_FILENAME = "headers.tsv";
    private const string DATASOURCE_FILENAME = "datasource.tsv";
    private const string SCHEMA_CONTENT = $"[{HEADERS_FILENAME}]\nFormat=TabDelimited\nColNameHeader=True\nMaxScanRows=0\nCharacterSet=UTF8";

    private string _tempGuid = string.Empty;
    private string _tempFolder = string.Empty;

    private TaskCompletionSource? _tcs;
    private SynchronizationContext? _syncContext;
    private volatile bool _isShuttingDown;

    private Word.Application? _wordApp;
    private Word.Document? _wordDoc;

    private Thread? _wordSTAThread;
    private Dispatcher? _wordDispatcher;
    private TaskCompletionSource? _wordSTAReadyTCS;

    /// <summary>
    /// Costruttore predefinito.
    /// </summary>
    public ManageWordTemplatesService() { }

    /// <summary>
    /// Verifica se un template è attualmente bloccato.
    /// </summary>
    /// <param name="id">
    /// Id del template.
    /// </param>
    /// <returns>
    /// <see langword="true"/> se il template è bloccato, altrimenti <see langword="false"/>.
    /// </returns>
    public static async Task<bool> IsTemplateLockedAsync(int id) {
        using SqlConnection connection = new(DataSource.StudioHubConnectionString);
        bool isLocked = await connection.ExecuteScalarAsync<bool>(@"
SELECT Locked
FROM   Hub.WordTemplates
WHERE  Id = @Id",
            new { Id = id }
        );
        return isLocked;
    }

    /// <summary>
    /// Imposta lo stato di lock di un template.
    /// </summary>
    /// <param name="id">
    /// Id del template.
    /// </param>
    /// <param name="locked">
    /// Valore di lock da impostare.
    /// </param>
    public static async Task SetTemplateLockAsync(int id, bool locked) {
        using SqlConnection connection = new(DataSource.StudioHubConnectionString);
        await connection.ExecuteNonQueryAsync(@"
UPDATE Hub.WordTemplates
SET    Locked = @Locked
WHERE  Id = @Id",
            new { Locked = locked, Id = id }
        );
    }

    /// <summary>
    /// Crea una cartella temporanea e i file necessari per la modifica del template.
    /// </summary>
    /// <param name="headers">
    /// Intestazioni per il datasource del mail merge.
    /// </param>
    /// <returns>
    /// Task di completamento.
    /// </returns>
    private Task createTempFolderAsync(string[] headers) {

        Directory.CreateDirectory(_tempFolder);

        string tsvContent = $"{string.Join("\t", headers)}\r\n{new string('\t', headers.Length - 1)}";
        Task tsvTask = File.WriteAllTextAsync(Path.Combine(_tempFolder, HEADERS_FILENAME), tsvContent);
        Task iniTask = File.WriteAllTextAsync(Path.Combine(_tempFolder, "schema.ini"), SCHEMA_CONTENT);

        return Task.WhenAll(tsvTask, iniTask);
    }

    /// <summary>
    /// Permette la modifica del contenuto di un template Word tramite Word interattivo.
    /// </summary>
    /// <param name="id">
    /// Id del template, -1 per nuovo.
    /// </param>
    /// <param name="headers">
    /// Intestazioni per il mail merge.
    /// </param>
    /// <returns>
    /// Contenuto del file Word modificato come array di byte.
    /// </returns>
    public async Task<byte[]> EditTemplateContentAsync(int id, string[] headers) {

        _syncContext = SynchronizationContext.Current;
        _tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _isShuttingDown = false;

        _tempGuid = Guid.NewGuid().ToString();
        _tempFolder = Path.Combine(Path.GetTempPath(), SOLUTION_NAME, _tempGuid);

        await createTempFolderAsync(headers);

        string filename;
        if (id == -1) {
            filename = Path.Combine(_tempFolder, "Nuovo modello" + WORD_FILE_EXT);
        }
        else {
            using SqlConnection connection = new(DataSource.StudioHubConnectionString);
            WordTemplate? template = (await connection.QueryAsync<WordTemplate>(t => t.Id == id)).FirstOrDefault();

            filename = Path.Combine(_tempFolder, (template is null ? "Modello" : template.Name) + WORD_FILE_EXT);
            if (template != null && template.Content.Length > 0) {
                await File.WriteAllBytesAsync(filename, template.Content);
                await File.WriteAllTextAsync(Path.Combine(_tempFolder, "template_id"), template.Id.ToString());
            }
        }

        try {
            await startWordSTAThreadAndSetupAsync(filename).ConfigureAwait(false);
            await _tcs.Task;

            cleanSaveCloseDocument();

            bool isUnlocked = await waitFileUnlockingAsync(filename);
            if (!isUnlocked) {
                throw new IOException("Impossibile accedere al file salvato. Il file risulta ancora in uso dal sistema dopo il timeout.");
            }

            return await File.ReadAllBytesAsync(filename);
        }
        finally {
            releaseCOMObjects();
            deleteTempFolder();
            shutdownWordSTAThread();
        }
    }

    /// <summary>
    /// Attende che il file non sia più bloccato da altri processi.
    /// </summary>
    /// <param name="filePath">
    /// Percorso del file.
    /// </param>
    /// <param name="timeoutMs">
    /// Timeout in millisecondi.
    /// </param>
    /// <returns>
    /// <see langword="true"/> se il file è sbloccato, altrimenti <see langword="false"/>.
    /// </returns>
    private static async Task<bool> waitFileUnlockingAsync(string filePath, int timeoutMs = 5000) {

        long deadline = Environment.TickCount64 + timeoutMs;
        while (Environment.TickCount64 < deadline) {
            try {
                using FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                return true;
            }
            catch (IOException) {
                await Task.Delay(500);
            }
        }
        return false;
    }

    /// <summary>
    /// Esegue un'azione sul thread STA di Word, se disponibile.
    /// </summary>
    /// <param name="action">
    /// Azione da eseguire.
    /// </param>
    private void invokeOnWordSTAThread(Action action) {
        if (_wordDispatcher != null) {
            try {
                _wordDispatcher.Invoke(action);
                return;
            }
            catch { }
        }
        action();
    }

    /// <summary>
    /// Rimuove gli event handler associati all'applicazione Word.
    /// </summary>
    private void removeWordEventHandlers() {
        if (_wordApp != null) {
            try {
                _wordApp.DocumentBeforeSave -= wordApp_DocumentBeforeSave;
            }
            catch { }
            try {
                _wordApp.DocumentBeforeClose -= wordApp_DocumentBeforeClose;
            }
            catch { }
        }
    }

    /// <summary>
    /// Salva e chiude il documento Word in modo pulito.
    /// </summary>
    private void cleanSaveCloseDocument() {

        invokeOnWordSTAThread(() => {
            if (_wordDoc == null) {
                return;
            }

            _isShuttingDown = true;

            try {
                removeWordEventHandlers();
                _wordDoc.MailMerge.MainDocumentType = Word.WdMailMergeMainDocType.wdNotAMergeDocument;
                _wordDoc.Variables[SOLUTION_NAME].Delete();
                _wordDoc.Save();
                _wordDoc.Close();
            }
            catch { }
            finally {
                try {
                    _wordApp?.Quit();
                }
                catch { }
            }
        });
    }

    /// <summary>
    /// Forza la chiusura di Word e del documento senza salvare.
    /// </summary>
    private void forceCloseWord() {

        _isShuttingDown = true;
        if (_wordDoc != null) {
            try {
                removeWordEventHandlers();
                _wordDoc.Close(Word.WdSaveOptions.wdDoNotSaveChanges);
            }
            catch { }
        }
        if (_wordApp != null) {
            try {
                _wordApp.Quit();
            }
            catch { }
        }
    }

    /// <summary>
    /// Inizializza l'applicazione Word e apre o crea il documento specificato.
    /// </summary>
    /// <param name="filename">
    /// Percorso del file Word.
    /// </param>
    private void initializeWordApp(string filename) {

        _wordApp = new Word.Application { Visible = false };
        if (File.Exists(filename)) {
            _wordDoc = _wordApp.Documents.Open(filename);
        }
        else {
            _wordDoc = _wordApp.Documents.Add();
            _wordDoc.SaveAs2(filename);
        }

        _wordDoc.Variables.Add(SOLUTION_NAME, _tempGuid);
        _wordDoc.MailMerge.OpenDataSource(Path.Combine(_tempFolder, HEADERS_FILENAME));
        _wordDoc.Save();

        _wordApp.DocumentBeforeSave += wordApp_DocumentBeforeSave;
        _wordApp.DocumentBeforeClose += wordApp_DocumentBeforeClose;

        _wordApp.Visible = true;
        _wordApp.Activate();
    }

    /// <summary>
    /// Verifica la presenza e il valore della variabile di controllo nel documento Word.
    /// </summary>
    /// <param name="wdoc">
    /// Documento Word da controllare.
    /// </param>
    /// <returns>
    /// <see langword="true"/> se la variabile è presente e valida, altrimenti <see langword="false"/>.
    /// </returns>
    private bool checkVariable(Word.Document wdoc) {

        try {
            Word.Variable variable = wdoc.Variables[SOLUTION_NAME];
            if (variable != null && variable.Value == _tempGuid) {
                return true;
            }
        }
        catch { }
        return false;
    }

    /// <summary>
    /// Gestisce l'evento di salvataggio del documento Word.
    /// </summary>
    /// <param name="wdoc">
    /// Documento Word.
    /// </param>
    /// <param name="SaveAsUI">
    /// Indica se è stato richiesto un "Salva con nome".
    /// </param>
    /// <param name="Cancel">
    /// Imposta <see langword="true"/> per annullare il salvataggio.
    /// </param>
    private void wordApp_DocumentBeforeSave(Word.Document wdoc, ref bool SaveAsUI, ref bool Cancel) {

        if (_isShuttingDown) {
            return;
        }
        try {
            if (checkVariable(wdoc) && SaveAsUI) {
                Cancel = true;
            }
        }
        catch { }
    }

    /// <summary>
    /// Gestisce l'evento di chiusura del documento Word.
    /// </summary>
    /// <param name="wdoc">
    /// Documento Word.
    /// </param>
    /// <param name="Cancel">
    /// Imposta <see langword="true"/> per annullare la chiusura.
    /// </param>
    private void wordApp_DocumentBeforeClose(Word.Document wdoc, ref bool Cancel) {

        if (_isShuttingDown) {
            return;
        }
        try {
            if (checkVariable(wdoc)) {
                Cancel = true;
                _syncContext?.Post(_ => _tcs?.TrySetResult(), null);
            }
        }
        catch { }
    }

    /// <summary>
    /// Rilascia le risorse COM associate a Word e al documento.
    /// </summary>
    private void releaseCOMObjects() {

        invokeOnWordSTAThread(() => {
            removeWordEventHandlers();
            if (_wordDoc != null) {
                try {
                    Marshal.ReleaseComObject(_wordDoc);
                }
                catch { }
            }
            if (_wordApp != null) {
                try {
                    Marshal.ReleaseComObject(_wordApp);
                }
                catch { }
            }
        });

        _wordDoc = null;
        _wordApp = null;
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }

    /// <summary>
    /// Elimina la cartella temporanea creata per la modifica del template.
    /// </summary>
    private void deleteTempFolder() {
        try {
            if (Directory.Exists(_tempFolder)) {
                Directory.Delete(_tempFolder, true);
            }
        }
        catch { }
    }

    /// <summary>
    /// Avvia un thread STA per l'interazione con Word e prepara l'ambiente.
    /// </summary>
    /// <param name="filename">
    /// Percorso del file Word.
    /// </param>
    /// <returns>
    /// Task di completamento.
    /// </returns>
    private Task startWordSTAThreadAndSetupAsync(string filename) {

        _wordSTAReadyTCS = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _wordSTAThread = new Thread(() => {
            try {
                _wordDispatcher = Dispatcher.CurrentDispatcher;
                initializeWordApp(filename);
                _wordSTAReadyTCS.TrySetResult();
                Dispatcher.Run();
            }
            catch (Exception ex) {
                _wordSTAReadyTCS.TrySetException(ex);
            }
        });

        _wordSTAThread.SetApartmentState(ApartmentState.STA);
        _wordSTAThread.IsBackground = true;
        _wordSTAThread.Start();

        return _wordSTAReadyTCS.Task;
    }

    /// <summary>
    /// Arresta il thread STA di Word e libera le risorse.
    /// </summary>
    private void shutdownWordSTAThread() {

        if (_wordDispatcher != null) {
            try { _wordDispatcher.BeginInvokeShutdown(DispatcherPriority.Normal); }
            catch { }
        }
        if (_wordSTAThread != null) {
            try { _wordSTAThread.Join(2000); }
            catch { }
        }

        _wordDispatcher = null;
        _wordSTAReadyTCS = null;
        _wordSTAThread = null;
    }

    private const string SUMMARY_SQL = @"
SELECT Id,
       Name,
       Description,
       TargetApp,
       Created,
       Modified,
       Locked
FROM   Hub.WordTemplates";

    /// <summary>
    /// Restituisce la lista di tutti i template Word, escludendo il contenuto binario.
    /// </summary>
    /// <returns>
    /// Lista di <see cref="WordTemplate"/> senza il campo <see cref="WordTemplate.Content"/>.
    /// </returns>
    public static async Task<List<WordTemplate>> GetTemplatesAsync() {
        using SqlConnection connection = new(DataSource.StudioHubConnectionString);
        IEnumerable<WordTemplate> templates = await connection.ExecuteQueryAsync<WordTemplate>(SUMMARY_SQL);
        return templates.AsList();
    }

    /// <summary>
    /// Restituisce la lista dei template Word filtrati per applicazione target, escludendo il contenuto binario.
    /// </summary>
    /// <param name="targetApp">
    /// Nome dell'applicazione target.
    /// </param>
    /// <returns>
    /// Lista di <see cref="WordTemplate"/> senza il campo <see cref="WordTemplate.Content"/>.
    /// </returns>
    public static async Task<List<WordTemplate>> GetTemplatesAsync(string targetApp) {
        using SqlConnection connection = new(DataSource.StudioHubConnectionString);
        IEnumerable<WordTemplate> templates = await connection.ExecuteQueryAsync<WordTemplate>(
            SUMMARY_SQL + "\nWHERE  TargetApp = @TargetApp",
            new { TargetApp = targetApp }
        );
        return templates.AsList();
    }

    /// <summary>
    /// Salva un template Word nel database. Se l'Id è uguale a -1, viene creato un nuovo record.
    /// </summary>
    /// <param name="template">
    /// Template da salvare.
    /// </param>
    /// <returns>
    /// Template salvato, con Id aggiornato se nuovo.
    /// </returns>
    public static async Task<WordTemplate> SaveTemplateAsync(WordTemplate template) {

        using SqlConnection connection = new(DataSource.StudioHubConnectionString);
        if (template.Id == -1) {
            int id = await connection.InsertAsync<WordTemplate, int>(template);
            return template with { Id = id };
        }
        else {
            await connection.UpdateAsync<WordTemplate>(template);
            return template;
        }
    }

    /// <summary>
    /// Elimina un template Word dal database.
    /// </summary>
    /// <param name="id">
    /// Id del template da eliminare.
    /// </param>
    public static async Task DeleteTemplateAsync(int id) {
        using SqlConnection connection = new(DataSource.StudioHubConnectionString);
        await connection.DeleteAsync<WordTemplate>(id);
    }

    /// <summary>
    /// Duplica un template Word esistente con un nuovo nome.
    /// </summary>
    /// <param name="id">
    /// Id del template da duplicare.
    /// </param>
    /// <param name="name">
    /// Nome del nuovo template.
    /// </param>
    public static async Task DuplicateTemplateAsync(int id, string name) {

        using SqlConnection connection = new(DataSource.StudioHubConnectionString);
        WordTemplate? template = (await connection.QueryAsync<WordTemplate>(t => t.Id == id)).FirstOrDefault();
        if (template == null) {
            return;
        }

        DateTime now = DateTime.UtcNow;
        WordTemplate clone = template with {
            Id = -1,
            Name = name,
            Created = now,
            Modified = now,
            Locked = false
        };

        await SaveTemplateAsync(clone);
    }

    /// <summary>
    /// Aggiorna i dettagli (nome e descrizione) di un template Word.
    /// </summary>
    /// <param name="id">
    /// Id del template.
    /// </param>
    /// <param name="name">
    /// Nuovo nome.
    /// </param>
    /// <param name="description">
    /// Nuova descrizione.
    /// </param>
    public static async Task UpdateTemplateDetailsAsync(int id, string name, string description) {
        using SqlConnection connection = new(DataSource.StudioHubConnectionString);
        await connection.ExecuteNonQueryAsync(@"
UPDATE Hub.WordTemplates
SET    Name = @Name,
       Description = @Desc,
       Modified = @Mod
WHERE  Id = @Id",
            new { Name = name, Desc = description, Mod = DateTime.UtcNow, Id = id }
        );
    }
}
