using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Office.Interop.Word;

namespace StudioHub.Services;


/// <summary>
/// Gestisce l'apertura e l'attesa di un documento Word per la creazione o modifica di un modello. Copre i punti 4.1 e
/// 4.2 del manifesto. DIPENDENZE: Microsoft.Office.Interop.Word (COM), .NET 10, C# 14. NOTA: Non usa Dependency
/// Injection — istanziato direttamente dal ViewModel.
/// </summary>
public sealed class ClaudeSonnet : IDisposable {

    // -------------------------------------------------------------------------
    // Stato interno
    // -------------------------------------------------------------------------

    private Application? _wordApp;
    private Document? _document;
    private string? _tempFolder;
    private string? _docPath;
    private string? _sessionGuid;
    private bool _disposed;

    // Completato dal gestore DocumentBeforeClose quando il GUID corrisponde.
    private TaskCompletionSource<bool>? _closeTcs;

    // Impostato dal ViewModel quando l'utente preme Annulla.
    private CancellationToken _cancellationToken;

    // -------------------------------------------------------------------------
    // Costanti
    // -------------------------------------------------------------------------

    private const string APP_NAME = "StudioHub";
    private const string HeadersFile = "headers.tsv";
    private const string NewDocName = "newdoc.docx";
    private const string EditDocName = "modello.docx";

    // -------------------------------------------------------------------------
    // Entry point pubblico
    // -------------------------------------------------------------------------

    /// <summary>
    /// Avvia l'intera sequenza 4.1–4.2: - crea cartella temporanea con GUID - prepara il documento (nuovo o copia) -
    /// genera headers.tsv - avvia Word, inietta variabile, associa mail merge - rende Word visibile - restituisce un
    /// Task che si completa alla chiusura del documento Il Task restituito si completa con: true → l'utente ha chiuso
    /// il documento normalmente (Ramo A) false → annullamento tramite CancellationToken (Ramo B)
    /// </summary>
    /// <param name="existingDocBytes">
    /// null per "Nuovo modello"; byte[] del documento esistente per "Modifica".
    /// </param>
    /// <param name="fieldHeaders">
    /// Nomi delle colonne per il file headers.tsv (intestazioni mail merge).
    /// </param>
    /// <param name="cancellationToken">
    /// Token collegato al pulsante Annulla nella finestra modale.
    /// </param>
    public async Task<bool> StartAndWaitAsync(byte[]? existingDocBytes, string[] fieldHeaders, CancellationToken cancellationToken) {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _cancellationToken = cancellationToken;

        // ------------------------------------------------------------------
        // 4.1 — Genera GUID e crea cartella temporanea
        // ------------------------------------------------------------------
        _sessionGuid = Guid.NewGuid().ToString("D");
        _tempFolder = Path.Combine(Path.GetTempPath(), _sessionGuid);
        Directory.CreateDirectory(_tempFolder);

        try {
            // prepara il documento nella cartella temp
            _docPath = existingDocBytes is null
                ? await createEmptyDocumentAsync(_tempFolder)
                : await copyExistingDocumentAsync(existingDocBytes, _tempFolder);

            // --------------------------------------------------------------
            // 4.2 — Genera headers.tsv
            // --------------------------------------------------------------
            string headersTsvPath = generateHeadersTSV(_tempFolder, fieldHeaders);

            // --------------------------------------------------------------
            // 4.2 — Avvia Word
            // --------------------------------------------------------------
            _wordApp = new Application {
                // Non mostrare subito: prima configuriamo tutto, poi rendiamo visibile.
                Visible = false
            };

            // Disabilita AutoSave (OneDrive) per impedire salvataggi fuori cartella.
            // Nota: AutoSaveOn è una proprietà di Document, la impostiamo dopo Open().

            // apre il documento
            _document = _wordApp.Documents.Open(
                FileName: _docPath,
                ConfirmConversions: false,
                ReadOnly: false,
                AddToRecentFiles: false);

            // impedisce il salvataggio automatico su OneDrive / SharePoint.
            _document.AutoSaveOn = false;

            // inietta variabile con il GUID dell'operazione
            _document.Variables[APP_NAME].Value = _sessionGuid;

            // associa headers.tsv come origine dati mail merge
            _document.MailMerge.OpenDataSource(
                Name: headersTsvPath,
                ConfirmConversions: false,
                ReadOnly: true,
                LinkToSource: false);

            // salva per rendere persistenti variabile e mail merge.
            _document.Save();

            // iscrive gli eventi globali dell'applicazione Word
            _wordApp.DocumentBeforeSave += onDocumentBeforeSave;
            _wordApp.DocumentBeforeClose += onDocumentBeforeClose;

            // prepara il TCS prima di rendere Word visibile
            _closeTcs = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously);

            // registra la cancellazione: se l'utente preme Annulla, il documento viene chiuso forzatamente
            cancellationToken.Register(handleCancellation, useSynchronizationContext: true);

            // rende Word visibile e in primo piano
            _wordApp.Visible = true;
            _wordApp.Activate();

            // Attesa asincrona: si sblocca su BeforeClose o su cancellazione
            bool completedNormally = await _closeTcs.Task;
            return completedNormally;
        }
        catch {
            // Qualsiasi eccezione durante la preparazione: pulizia e rethrow.
            cleanupComObjects();
            deleteTempFolder();
            throw;
        }
    }

    // -------------------------------------------------------------------------
    // Gestori eventi Word
    // -------------------------------------------------------------------------

    /// <summary>
    /// Punto 4.2.12 — DocumentBeforeSave. Impedisce "Salva con nome" verificando che il percorso non cambi.
    /// </summary>
    private void onDocumentBeforeSave(
        Document doc,
        ref bool saveAsUi,
        ref bool cancel) {
        // se l'utente ha aperto il dialogo "Salva con nome", annulliamo.
        if (saveAsUi) {
            cancel = true;

            // messaggio su thread UI tramite Dispatcher.
            System.Windows.Application.Current?.Dispatcher.Invoke(() => {
                Dialog.Show(DialogType.Info, APP_NAME,
                "Usa il pulsante Salva nell'applicazione al termine della modifica.\nIl salvataggio con nome non è consentito.");
            });
            return;
        }

        // Verifica che il percorso del documento non sia cambiato.
        if (!string.Equals(doc.FullName, _docPath, StringComparison.OrdinalIgnoreCase)) {
            cancel = true;
        }
    }

    /// <summary>
    /// Punto 4.2.12 — DocumentBeforeClose. Completa il TCS solo se la variabile GUID corrisponde all'operazione
    /// corrente.
    /// </summary>
    private void onDocumentBeforeClose(Document doc, ref bool cancel) {
        // verifica GUID: ignora chiusure di altri documenti aperti in Word.
        string? docGuid = null;
        try { docGuid = doc.Variables[APP_NAME]?.Value; } catch { }

        if (!string.Equals(docGuid, _sessionGuid, StringComparison.Ordinal)) {
            return;
        }

        // Completa il TCS sul thread di sincronizzazione UI.
        // I cleanup COM avvengono DOPO (punto 4.4), non qui.
        _closeTcs?.TrySetResult(true);
    }

    // -------------------------------------------------------------------------
    // Ramo B — Annullamento da CancellationToken
    // -------------------------------------------------------------------------

    /// <summary>
    /// Invocato quando il CancellationToken viene cancellato (pulsante Annulla).
    /// </summary>
    private void handleCancellation() {
        if (_document is null || _wordApp is null) {
            _closeTcs?.TrySetResult(false);
            return;
        }

        try {
            // Rimuove gli handler prima di chiudere per evitare rientri.
            _wordApp.DocumentBeforeSave -= onDocumentBeforeSave;
            _wordApp.DocumentBeforeClose -= onDocumentBeforeClose;

            _document.Close(SaveChanges: WdSaveOptions.wdDoNotSaveChanges);
            _wordApp.Quit(SaveChanges: WdSaveOptions.wdDoNotSaveChanges);
        }
        catch { }
        finally {
            cleanupComObjects();
            deleteTempFolder();
            _closeTcs?.TrySetResult(false);
        }
    }

    // -------------------------------------------------------------------------
    // Metodi di supporto — preparazione file
    // -------------------------------------------------------------------------

    /// <summary>
    /// Crea un documento Word vuoto nella cartella temp senza aprire Word. Usa una copia del Normal.dotm come base
    /// minima.
    /// </summary>
    private static Task<string> createEmptyDocumentAsync(string folder) {
        string path = Path.Combine(folder, NewDocName);
        return System.Threading.Tasks.Task.FromResult(path);
    }

    /// <summary>
    /// Scrive i byte del documento esistente nella cartella temp.
    /// </summary>
    private static async Task<string> copyExistingDocumentAsync(byte[] docBytes, string folder) {
        string path = Path.Combine(folder, EditDocName);
        await File.WriteAllBytesAsync(path, docBytes);
        return path;
    }

    /// <summary>
    /// Genera il file headers.tsv con le sole intestazioni dei campi, separate da tabulazione, su una singola riga.
    /// </summary>
    private static string generateHeadersTSV(string folder, string[] headers) {
        string tsvPath = Path.Combine(folder, HeadersFile);
        string schemaPath = Path.Combine(folder, "schema.ini");

        File.WriteAllText(tsvPath, string.Join('\t', headers), System.Text.Encoding.UTF8);
        File.WriteAllText(schemaPath,
           $"""
            [{HeadersFile}]
            Format=TabDelimited
            CharacterSet=65001
            ColNameHeader=True
            """,
           System.Text.Encoding.ASCII);

        return tsvPath;
    }

    // -------------------------------------------------------------------------
    // Pulizia COM e file system
    // -------------------------------------------------------------------------

    /// <summary>
    /// Rilascia tutti i riferimenti COM in modo esplicito. Deve essere chiamato dopo aver già invocato Document.Close()
    /// e Application.Quit().
    /// </summary>
    private void cleanupComObjects() {
        if (_document is not null) {
            Marshal.ReleaseComObject(_document);
            _document = null;
        }

        if (_wordApp is not null) {
            Marshal.ReleaseComObject(_wordApp);
            _wordApp = null;
        }

        // Forza il GC a raccogliere i wrapper RCW rimasti.
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }

    /// <summary>
    /// Elimina la cartella temporanea con tutti i file creati durante l'operazione. In caso di lock residui, fa un
    /// singolo tentativo silenzioso. La logica di retry completa con dialog è nel ViewModel (punto 4.5).
    /// </summary>
    private void deleteTempFolder() {
        if (_tempFolder is null || !Directory.Exists(_tempFolder)) {
            return;
        }

        try {
            Directory.Delete(_tempFolder, recursive: true);
        }
        catch {
            // Il polling del ViewModel gestirà il cleanup differito (punto 4.5).
        }
    }

    // -------------------------------------------------------------------------
    // Cleanup pubblico (da chiamare dopo il polling del file — punto 4.4/4.5)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Completa il cleanup COM dopo che il TCS si è completato (Ramo A). Rimuove variabile e mail merge, chiude
    /// documento e Word. Da chiamare nel ViewModel DOPO il completamento del Task di StartAndWaitAsync.
    /// </summary>
    public void FinalizeWordSession() {
        if (_document is null || _wordApp is null) {
            return;
        }

        // Rimuove handler prima di chiudere.
        try { _wordApp.DocumentBeforeSave -= onDocumentBeforeSave; } catch { }
        try { _wordApp.DocumentBeforeClose -= onDocumentBeforeClose; } catch { }

        // rimuove variabile dell'operazione.
        try { _document.Variables[APP_NAME].Delete(); } catch { }

        // rimuove associazione mail merge.
        try { _document.MailMerge.MainDocumentType = WdMailMergeMainDocType.wdNotAMergeDocument; } catch { }

        try {
            _document.Close(SaveChanges: WdSaveOptions.wdDoNotSaveChanges);
            _wordApp.Quit(SaveChanges: WdSaveOptions.wdDoNotSaveChanges);
        }
        catch { }
        finally {
            cleanupComObjects();
        }
    }

    // -------------------------------------------------------------------------
    // Proprietà di supporto per il ViewModel
    // -------------------------------------------------------------------------

    /// <summary>Percorso del file .docx nella cartella temp.</summary>
    public string? DocumentPath {
        get { return _docPath; }
    }

    /// <summary>Percorso della cartella temporanea corrente.</summary>
    public string? TempFolder {
        get { return _tempFolder; }
    }

    // -------------------------------------------------------------------------
    // IDisposable
    // -------------------------------------------------------------------------

    public void Dispose() {
        if (_disposed) {
            return;
        }

        _disposed = true;

        // Tenta un cleanup difensivo se il ViewModel non ha chiamato FinalizeWordSession.
        try { FinalizeWordSession(); } catch { }
        deleteTempFolder();
    }
}
