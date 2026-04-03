using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using Word = Microsoft.Office.Interop.Word;
using System.Data;
using Microsoft.Data.SqlClient;
using RepoDb;

namespace StudioHub.Services;

public class ManageWordTemplatesService {

    private string _tempGuid = string.Empty;
    private string _tempFolder = string.Empty;

    private const string HEADERS_TSV = "headers.tsv";
    private const string SCHEMA_CONTENT =
@$"[{HEADERS_TSV}]
Format=TabDelimited
ColNameHeader=True
MaxScanRows=0
CharacterSet=UTF8";

    private TaskCompletionSource? _tcs;
    private SynchronizationContext? _syncContext;
    private volatile bool _isCanceledByUser;
    private volatile bool _isShuttingDown;

    private Word.Application? _wordApp;
    private Word.Document? _wordDoc;

    // Thread/Dispatcher dedicati per Word COM (evita COMException dovute a apartment/thread errati)
    private Thread? _wordStaThread;
    private Dispatcher? _wordDispatcher;
    private TaskCompletionSource? _wordStaReadyTcs;

    /// <summary>
    /// Costruttore senza parametri (niente DI).
    /// </summary>
    public ManageWordTemplatesService() {
    }

    // --- SEZIONE 1: Gestione Database e Concorrenza ---

    /// <summary>
    /// Verifica se un template è bloccato. Metodo stub per futura integrazione con RepoDb.
    /// </summary>
    public Task<bool> IsTemplateLockedAsync(int templateId) {
        // TODO con RepoDb: SELECT Locked FROM WordTemplates WHERE Id = @Id
        return Task.FromResult(false);
    }

    /// <summary>
    /// Imposta il lock sul template. Metodo stub per futura integrazione con RepoDb.
    /// </summary>
    public Task SetTemplateLockAsync(int templateId, bool isLocked) {
        // TODO con RepoDb: UPDATE WordTemplates SET Locked = @isLocked WHERE Id = @Id
        return Task.CompletedTask;
    }

    /// <summary>
    /// Crea la cartella temporanea e i file necessari per il datasource di Word.
    /// </summary>
    private Task createTempFolderAsync(string[] headers) {

        Directory.CreateDirectory(_tempFolder);

        // Crea il contenuto del file delle intestazioni
        string tsvContent = $"{string.Join("\t", headers)}\r\n{new string('\t', headers.Length - 1)}";

        // Scrittura parallela dei due file indipendenti
        Task tsvTask = File.WriteAllTextAsync(Path.Combine(_tempFolder, HEADERS_TSV), tsvContent);
        Task iniTask = File.WriteAllTextAsync(Path.Combine(_tempFolder, "schema.ini"), SCHEMA_CONTENT);

        return Task.WhenAll(tsvTask, iniTask);
    }

    /// <summary>
    /// Avvia Word su un thread STA dedicato, attende la chiusura da parte dell'utente e ritorna il file modificato in
    /// byte.
    /// </summary>
    public async Task<byte[]> EditTemplateContentAsync(int templateId, string[] headers, CancellationToken ct) {

        _syncContext = SynchronizationContext.Current;
        _tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _isCanceledByUser = false;
        _isShuttingDown = false;

        _tempGuid = Guid.NewGuid().ToString();
        _tempFolder = Path.Combine(Path.GetTempPath(), "StudioHub", _tempGuid);

        string wdocFilename;
        if (templateId == -1) {
            wdocFilename = Path.Combine(_tempFolder, "Nuovo Modello.docx");
        }
        else {
            // TODO: recupera dal DB i byte del modello esistente
            // e salvali su disco: await File.WriteAllBytesAsync(wdocFilename, byteDalDb);
            wdocFilename = Path.Combine(_tempFolder, "Modello.docx");
        }

        await createTempFolderAsync(headers);

        using CancellationTokenRegistration ctr = ct.Register(() => {
            _isCanceledByUser = true;
            forceCloseWord();
            _tcs.TrySetCanceled();
        });

        try {
            // Avvia COM Word su un thread separato (STA) per non bloccare la UI
            await startWordStaThreadAndSetupAsync(wdocFilename, ct).ConfigureAwait(false);

            // Attesa (asincrona) finché il documento non viene chiuso
            await _tcs.Task;

            // Pulizia di Word e Polling (se non annullato)
            if (!_isCanceledByUser) {
                cleanAndCloseDocument();

                bool isUnlocked = await waitForFileUnlockAsync(wdocFilename);
                if (!isUnlocked) {
                    throw new IOException("Impossibile accedere al file salvato. Il file risulta ancora in uso dal sistema dopo il timeout.");
                }

                byte[] resultBytes = await File.ReadAllBytesAsync(wdocFilename, ct);
                return resultBytes;
            }

            return [];
        }
        finally {
            releaseComObjects();
            cleanupTempEnvironment();
            shutdownWordStaThread();
        }
    }

    /// <summary>
    /// Attende che il file venga rilasciato dal sistema operativo o da processi esterni (es. antivirus).
    /// </summary>
    private static async Task<bool> waitForFileUnlockAsync(string filePath, int timeoutMs = 5000) {

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
    /// Esegue un'azione sul thread STA di Word se il dispatcher è disponibile, altrimenti sul thread corrente.
    /// </summary>
    private void invokeOnWordThread(Action action) {
        if (_wordDispatcher != null) {
            try {
                _wordDispatcher.Invoke(action);
                return;
            }
            catch {
                // Dispatcher non disponibile, fallback al thread corrente
            }
        }
        action();
    }

    /// <summary>
    /// Rimuove gli event handler da Word per evitare callback durante lo shutdown.
    /// </summary>
    private void detachWordEvents() {
        if (_wordApp != null) {
            try { _wordApp.DocumentBeforeSave -= wordApp_DocumentBeforeSave; }
            catch { }
            try { _wordApp.DocumentBeforeClose -= wordApp_DocumentBeforeClose; }
            catch { }
        }
    }

    /// <summary>
    /// Pulisce e chiude il documento; deve essere invocato sul thread STA che ospita Word.
    /// </summary>
    private void cleanAndCloseDocument() {

        invokeOnWordThread(() => {
            if (_wordDoc == null) {
                return;
            }

            _isShuttingDown = true;

            try {
                detachWordEvents();
                _wordDoc.MailMerge.MainDocumentType = Word.WdMailMergeMainDocType.wdNotAMergeDocument;
                _wordDoc.Variables["StudioHub"].Delete();
                _wordDoc.Save();
                _wordDoc.Close();
            }
            catch {
                // Ignora errori di cleanup su Word
            }
            finally {
                try { _wordApp?.Quit(); }
                catch { }
            }
        });
    }

    /// <summary>
    /// Chiude forzatamente Word senza salvare (usato in caso di cancellazione dall'utente).
    /// </summary>
    private void forceCloseWord() {
        _isShuttingDown = true;

        if (_wordDoc != null) {
            try {
                detachWordEvents();
                _wordDoc.Close(Word.WdSaveOptions.wdDoNotSaveChanges);
            }
            catch { }
        }

        if (_wordApp != null) {
            try { _wordApp.Quit(); } catch { }
        }
    }

    /// <summary>
    /// Inizializza l'ambiente Word sul thread corrente (deve essere chiamato su un thread STA).
    /// </summary>
    private void setupWordEnvironment(string filename) {

        _wordApp = new Word.Application { Visible = false };
        if (File.Exists(filename)) {
            _wordDoc = _wordApp.Documents.Open(filename);
        }
        else {
            _wordDoc = _wordApp.Documents.Add();
            _wordDoc.SaveAs2(filename);
        }

        _wordDoc.Variables.Add("StudioHub", _tempGuid);
        _wordDoc.MailMerge.OpenDataSource(Path.Combine(_tempFolder, HEADERS_TSV));
        _wordDoc.Save();

        _wordApp.DocumentBeforeSave += wordApp_DocumentBeforeSave;
        _wordApp.DocumentBeforeClose += wordApp_DocumentBeforeClose;

        _wordApp.Visible = true;
        _wordApp.Activate();
    }

    /// <summary>
    /// Evento intercettato prima del salvataggio di un documento Word.
    /// </summary>
    private void wordApp_DocumentBeforeSave(Word.Document wdoc, ref bool SaveAsUI, ref bool Cancel) {

        if (_isShuttingDown) {
            return;
        }
        try {
            if (isOurDoc(wdoc) && SaveAsUI) {
                Cancel = true;
            }
        }
        catch {
            // Ignora errori COM transienti
        }
    }

    /// <summary>
    /// Evento intercettato prima della chiusura di un documento Word. Se è il nostro documento completa la
    /// TaskCompletionSource sul thread UI.
    /// </summary>
    private void wordApp_DocumentBeforeClose(Word.Document wdoc, ref bool Cancel) {

        if (_isCanceledByUser || _isShuttingDown) {
            return;
        }
        try {
            if (isOurDoc(wdoc)) {
                Cancel = true;
                _syncContext?.Post(_ => { _tcs?.TrySetResult(); }, null);
            }
        }
        catch {
            // Ignora
        }
    }

    /// <summary>
    /// Determina se il documento ricevuto è stato creato dal servizio StudioHub.
    /// </summary>
    private bool isOurDoc(Word.Document wdoc) {

        try {
            Word.Variable variable = wdoc.Variables["StudioHub"];
            if (variable != null && variable.Value == _tempGuid) {
                return true;
            }
        }
        catch {
            // Ignora eccezioni COM che indicano che l'oggetto non è più valido
        }

        return false;
    }

    /// <summary>
    /// Rilascia gli oggetti COM; esegue il rilascio sul thread STA di Word quando possibile.
    /// </summary>
    private void releaseComObjects() {

        invokeOnWordThread(() => {
            detachWordEvents();

            if (_wordDoc != null) {
                try { Marshal.ReleaseComObject(_wordDoc); } catch { }
            }
            if (_wordApp != null) {
                try { Marshal.ReleaseComObject(_wordApp); } catch { }
            }
        });

        _wordDoc = null;
        _wordApp = null;

        GC.Collect();
        GC.WaitForPendingFinalizers();
    }

    /// <summary>
    /// Rimuove la cartella temporanea e il suo contenuto in modo sicuro.
    /// </summary>
    private void cleanupTempEnvironment() {
        try {
            if (Directory.Exists(_tempFolder)) {
                Directory.Delete(_tempFolder, true);
            }
        }
        catch {
            // Ignoriamo le eccezioni di file in uso residui
        }
    }

    /// <summary>
    /// Avvia un thread STA dedicato, esegue setupWordEnvironment su quel thread e mantiene il dispatcher attivo.
    /// </summary>
    private Task startWordStaThreadAndSetupAsync(string filename, CancellationToken ct) {

        _wordStaReadyTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _wordStaThread = new Thread(() => {
            try {
                _wordDispatcher = Dispatcher.CurrentDispatcher;
                setupWordEnvironment(filename);
                _wordStaReadyTcs.TrySetResult();
                Dispatcher.Run();
            }
            catch (Exception ex) {
                _wordStaReadyTcs.TrySetException(ex);
            }
        });

        _wordStaThread.SetApartmentState(ApartmentState.STA);
        _wordStaThread.IsBackground = true;
        _wordStaThread.Start();

        if (ct.CanBeCanceled) {
            ct.Register(() => _wordStaReadyTcs.TrySetCanceled());
        }

        return _wordStaReadyTcs.Task;
    }

    /// <summary>
    /// Termina il dispatcher/thread STA di Word in modo ordinato.
    /// </summary>
    private void shutdownWordStaThread() {

        if (_wordDispatcher != null) {
            try {
                _wordDispatcher.BeginInvokeShutdown(DispatcherPriority.Normal);
            }
            catch { }
        }
        if (_wordStaThread != null) {
            try {
                _wordStaThread.Join(2000);
            }
            catch { }
        }

        _wordDispatcher = null;
        _wordStaReadyTcs = null;
        _wordStaThread = null;
    }

    // --- Metodi CRUD per il Database ---

    /// <summary>
    /// Restituisce tutti i template (stub).
    /// </summary>
    public Task<List<WordTemplate>> GetAllTemplatesAsync() {
        return Task.FromResult(new List<WordTemplate>());
    }

    /// <summary>
    /// Elimina un template (stub).
    /// </summary>
    public Task DeleteTemplateAsync(int id) {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Duplica un template (stub).
    /// </summary>
    public Task DuplicateTemplateAsync(int id, string newName) {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Aggiorna i dettagli di un template (stub).
    /// </summary>
    public Task UpdateTemplateDetailsAsync(int id, string newName, string newDescription) {
        return Task.CompletedTask;
    }
}
