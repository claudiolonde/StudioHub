using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading;
using System.Threading.Tasks;
using StudioHub.Models;
using Word = Microsoft.Office.Interop.Word;
namespace StudioHub.Services;

public class ManageWordTemplatesService {

    private Word.Application? _wordApp;
    private Word.Document? _wordDoc;
    private TaskCompletionSource<bool>? _tcs;
    private string _currentSessionGuid = string.Empty;
    private SynchronizationContext? _syncContext;
    private bool _isCanceledByUser;

    // Costruttore senza parametri (niente DI)
    public ManageWordTemplatesService() {
        // Qui in futuro potremmo inizializzare la stringa di connessione a RepoDb
    }

    // --- SEZIONE 1: Gestione Database e Concorrenza ---

    public async Task<bool> IsTemplateLockedAsync(int templateId) {
        // TODO con RepoDb: SELECT Locked FROM WordTemplates WHERE Id = @Id
        // Simuliamo che non sia bloccato
        return false;
    }

    public async Task SetTemplateLockAsync(int templateId, bool isLocked) {
        // TODO con RepoDb: UPDATE WordTemplates SET Locked = @isLocked WHERE Id = @Id
    }

    // --- SEZIONE 2: Preparazione File System ---

    /// <summary>
    /// Crea un ambiente isolato in %TEMP%\StudioHub\<GUID> contenente il file docx (vuoto o ripristinato), headers.tsv
    /// e schema.ini.
    /// </summary>
    /// <summary>
    /// Crea un ambiente isolato in %TEMP%\StudioHub\<GUID> contenente il file docx (vuoto o ripristinato), headers.tsv
    /// (dinamico) e schema.ini.
    /// </summary>
    private async Task<(string TempFolder, string DocPath, string HeadersPath)> PrepareTempEnvironmentAsync(
        List<string> headers,
        byte[]? existingContent = null) {

        var sessionGuid = Guid.NewGuid().ToString();
        var tempFolder = Path.Combine(Path.GetTempPath(), "StudioHub", sessionGuid);

        Directory.CreateDirectory(tempFolder);

        var docName = existingContent == null ? "NuovoModello.docx" : "Modello.docx";
        var docPath = Path.Combine(tempFolder, docName);
        var headersPath = Path.Combine(tempFolder, "headers.tsv");
        var schemaPath = Path.Combine(tempFolder, "schema.ini");

        // 1. Ripristino o creazione file Word
        if (existingContent != null) {
            await File.WriteAllBytesAsync(docPath, existingContent);
        }
        else {
            await File.WriteAllBytesAsync(docPath, []); // File vuoto
        }

        // 2. Creazione dinamica di headers.tsv (Regola: Intestazioni + Ritorno a capo + (N-1) TAB)
        if (headers == null || headers.Count == 0) {
            // Fallback base se non vengono fornite intestazioni
            headers = ["Campo1"];
        }

        var headerLine = string.Join("\t", headers);
        var emptyLineTabs = new string('\t', headers.Count - 1);
        var tsvContent = $"{headerLine}\r\n{emptyLineTabs}";

        await File.WriteAllTextAsync(headersPath, tsvContent);

        // 3. Creazione schema.ini
        var schemaContent =
            $"[headers.tsv]\n" +
            $"Format=TabDelimited\n" +
            $"ColNameHeader=True\n" +
            $"MaxScanRows=0\n" +
            $"CharacterSet=UTF8";
        await File.WriteAllTextAsync(schemaPath, schemaContent);

        return (tempFolder, docPath, headersPath);
    }



    // --- SEZIONE 3: Automazione Word ---
    /// <summary>
    /// Avvia Word, attende la chiusura da parte dell'utente e ritorna il file modificato in byte.
    /// </summary>
    public async Task<byte[]> EditTemplateContentAsync(
        List<string> headers,
        byte[]? existingContent,
        CancellationToken ct) {
        // 1. Salva il contesto di sincronizzazione corrente (thread UI)
        _syncContext = SynchronizationContext.Current;
        _tcs = new TaskCompletionSource<bool>();
        _isCanceledByUser = false;

        // 2. Registra l'annullamento da parte dell'utente tramite la UI (es. pulsante "Annulla")
        using var ctr = ct.Register(() => {
            _isCanceledByUser = true;
            ForceCloseWord();
            _tcs.TrySetCanceled();
        });

        // 3. Prepara la cartella temporanea
        var (tempFolder, docPath, headersPath) = await PrepareTempEnvironmentAsync(headers, existingContent);

        try {
            // 4. Avvia COM Word su un thread separato per non frizzare la UI
            await Task.Run(() => SetupWordEnvironment(docPath, headersPath, Path.GetFileName(tempFolder)), ct);

            // 5. Attesa (asincrona) finché il documento non viene chiuso
            // Il _tcs verrà completato nell'evento DocumentBeforeClose o tramite l'annullamento
            await _tcs.Task;

            // 6. Pulizia di Word (se non annullato)
            if (!_isCanceledByUser) {
                CleanAndCloseDocument();

                // Opzionale: Se il sistema operativo trattiene ancora il file, 
                // qui andrebbe il Polling (Step 4) che avevamo messo in pausa.
                // Per ora assumiamo che sia sbloccato.
            }

            // 7. Leggi il risultato finale
            var resultBytes = await File.ReadAllBytesAsync(docPath);
            return resultBytes;
        }
        finally {
            // 8. Rilascia sempre i riferimenti COM ed elimina i file
            ReleaseComObjects();
            CleanupTempEnvironment(tempFolder);
        }
    }

    private void CleanAndCloseDocument() {
        if (_wordDoc == null) return;
        try {
            // Disconnette l'origine dati e pulisce la variabile
            _wordDoc.MailMerge.MainDocumentType = Word.WdMailMergeMainDocType.wdNotAMergeDocument;
            _wordDoc.Variables["MiaApp"].Delete();

            // Salva e chiude
            _wordDoc.Save();
            _wordDoc.Close(); // Chiude il documento
        }
        catch { }
        finally {
            _wordApp?.Quit(); // Chiude l'applicazione Word
        }
    }

    private void ForceCloseWord() {
        // Chiude forzatamente senza salvare in caso di "Annulla"
        if (_wordDoc != null) {
            try { _wordDoc.Close(Word.WdSaveOptions.wdDoNotSaveChanges); } catch { }
        }
        if (_wordApp != null) {
            try { _wordApp.Quit(); } catch { }
        }
    }
    private void SetupWordEnvironment(string docPath, string headersPath, string sessionGuid) {
        _currentSessionGuid = sessionGuid;
        _wordApp = new Word.Application { Visible = false };
        _wordDoc = _wordApp.Documents.Open(docPath);

        // Inietta la variabile per riconoscere il nostro documento alla chiusura
        _wordDoc.Variables.Add("MiaApp", _currentSessionGuid);

        // Collega il file TSV creato prima per la stampa unione
        _wordDoc.MailMerge.OpenDataSource(headersPath);

        // Salva per consolidare i collegamenti
        _wordDoc.Save();

        // Iscrizione agli eventi critici
        _wordApp.DocumentBeforeSave += WordApp_DocumentBeforeSave;
        _wordApp.DocumentBeforeClose += WordApp_DocumentBeforeClose;

        // Mostra l'interfaccia all'utente
        _wordApp.Visible = true;
        _wordApp.Activate();
    }

    private void WordApp_DocumentBeforeSave(Word.Document Doc, ref bool SaveAsUI, ref bool Cancel) {
        // Impedisce "Salva con nome" per evitare che l'utente cambi percorso
        if (SaveAsUI) Cancel = true;
    }

    private void WordApp_DocumentBeforeClose(Word.Document Doc, ref bool Cancel) {
        if (_isCanceledByUser) return;

        bool isOurDoc = false;
        try {
            var variable = Doc.Variables["MiaApp"];
            if (variable != null && variable.Value == _currentSessionGuid) isOurDoc = true;
        }
        catch { /* Variabile mancante, ignoriamo */ }

        if (isOurDoc) {
            // Annulla la chiusura nativa per gestirla noi in modo asincrono
            Cancel = true;

            // Torna al thread UI per completare l'operazione
            _syncContext?.Post(_ => _tcs?.TrySetResult(true), null);
        }
    }

    private void ReleaseComObjects() {
        if (_wordApp != null) {
            try {
                _wordApp.DocumentBeforeSave -= WordApp_DocumentBeforeSave;
                _wordApp.DocumentBeforeClose -= WordApp_DocumentBeforeClose;
            }
            catch { }
        }

        if (_wordDoc != null) Marshal.ReleaseComObject(_wordDoc);
        if (_wordApp != null) Marshal.ReleaseComObject(_wordApp);

        _wordDoc = null;
        _wordApp = null;

        GC.Collect();
        GC.WaitForPendingFinalizers();
    }

    /// <summary>
    /// Rimuove la cartella temporanea e il suo contenuto in modo sicuro.
    /// </summary>
    private void CleanupTempEnvironment(string tempFolder) {
        try {
            if (Directory.Exists(tempFolder)) {
                Directory.Delete(tempFolder, true);
            }
        }
        catch {
            // Ignoriamo le eccezioni di file in uso residui (eventualmente verranno 
            // sovrascritti o puliti dal sistema operativo successivamente).
        }
    }

    // --- Metodi CRUD per il Database ---
    public async Task<List<WordTemplate>> GetAllTemplatesAsync() {
        // Da implementare con RepoDb
        return [];
    }

    public async Task DeleteTemplateAsync(Guid id) {
        // Da implementare con RepoDb
    }

    public async Task DuplicateTemplateAsync(Guid id, string newName) {
        // Da implementare con RepoDb: legge, cambia Id e Nome, salva
    }

    public async Task UpdateTemplateDetailsAsync(Guid id, string newName, string newDescription) {
        // Da implementare con RepoDb: aggiorna solo Nome e Descrizione
    }

    // --- Metodi per il Flusso Word (Step 3) ---
    public async Task<WordTemplate> CreateNewTemplateContentAsync(CancellationToken ct) {
        // Da implementare: crea file vuoto, avvia Word, attende chiusura, ritorna il file binario
        return null!;
    }

    public async Task<byte[]> EditTemplateContentAsync(Guid templateId, byte[] currentContent, CancellationToken ct) {
        // Da implementare: ripristina file, avvia Word, attende chiusura, ritorna nuovo binario
        return [];
    }
}
