using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Office.Interop.Word;
using StudioHub.Models;
using StudioHub.Services;
using StudioHub.Controls;
using StudioHub.Views;
using Dialog = StudioHub.Controls.Dialog;
using Document = Microsoft.Office.Interop.Word.Document;
using Task = System.Threading.Tasks.Task;

namespace StudioHub.ViewModels;

/// <inheritdoc/>
public partial class MailMergeTemplatesViewModel() : ObservableObject {

    [ObservableProperty]
    private string _appName = string.Empty;

    [ObservableProperty]
    private string[] _headers = [];


    [ObservableProperty]
    private ObservableCollection<MailMergeTemplateInfo> _allTemplates = [];

    // Lista dei nomi esistenti (per il controllo duplicati nel dialogo)
    private IEnumerable<string> GetExistingNames() {
        return AllTemplates.Select(t => t.Name);
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(NewCommand))]
    private bool _isBusy;


    [RelayCommand]
    private async Task LoadTemplates() {

        IsBusy = true;
        try {
            IEnumerable<MailMergeTemplateInfo> results = await MailMergeTemplateService.GetMailMergeTemplateInfoAsync(AppName);

            AllTemplates.Clear();
            foreach (MailMergeTemplateInfo info in results) {
                AllTemplates.Add(info);
            }

            ApplyFilter(); // Il metodo che gestisce la ricerca/filtro sulla UI
        }
        catch (Exception ex) {
            Dialog.Show($"Errore durante il caricamento dei modelli: {ex.Message}", DialogIcon.Error);
        }
        finally {
            IsBusy = false;
        }
    }

    [ObservableProperty]
    private ObservableCollection<MailMergeTemplateInfo> _filteredTemplates = [];

    [ObservableProperty]
    private MailMergeTemplateInfo? _selectedTemplate = null;

    [ObservableProperty]
    private string? _textToSearch = string.Empty;

    /// <inheritdoc/>
    [ObservableProperty]
    public string? _totalItems = "0";

    /// <inheritdoc/>
    [ObservableProperty]
    public string? _showedItems = "0";

    [RelayCommand]
    private void Duplicate() {
    }

    [RelayCommand]
    private void EditDetails() {
    }

    [RelayCommand]
    private void Delete() {
    }

    partial void OnTextToSearchChanged(string? value) {
        ApplyFilter();
    }

    private void ApplyFilter() {

        if (string.IsNullOrWhiteSpace(TextToSearch)) {
            FilteredTemplates = new ObservableCollection<MailMergeTemplateInfo>(AllTemplates);
            UpdateCounts();
            return;
        }

        string query = TextToSearch.Trim();

        IEnumerable<MailMergeTemplateInfo> result;

        if (query.Contains('/')) {
            // Dividiamo la ricerca in due parti (max 2 pezzi)
            string[] parts = query.Split('/', 2);
            string namePart = parts[0].Trim();
            string descPart = parts[1].Trim();

            result = AllTemplates.Where(t =>
                (string.IsNullOrEmpty(namePart) || t.Name.Contains(namePart, StringComparison.InvariantCultureIgnoreCase)) &&
                (string.IsNullOrEmpty(descPart) || t.Description.Contains(descPart, StringComparison.InvariantCultureIgnoreCase))
            );
        }
        else {
            // Ricerca standard: se non c'è lo slash, cerca il testo ovunque (comportamento classico)
            result = AllTemplates.Where(t =>
                t.Name.Contains(query, StringComparison.CurrentCultureIgnoreCase) ||
                t.Description.Contains(query, StringComparison.CurrentCultureIgnoreCase)
            );
        }

        FilteredTemplates = new ObservableCollection<MailMergeTemplateInfo>(result);
        UpdateCounts();
    }

    private void UpdateCounts() {
        ShowedItems = FilteredTemplates.Count.ToString();
        TotalItems = AllTemplates.Count.ToString();
    }

    private bool CanExecuteNew() {
        return !IsBusy;
    }


    private async Task<byte[]?> OpenWordAndGetContentAsync(string templateName, byte[]? existingContent = null) {

        // crea la firma univoca
        string signature = Guid.NewGuid().ToString();

        // crea cartella e files temporanei
        string tempPath = Path.Combine(TEMP_PATH, signature);
        Directory.CreateDirectory(tempPath);
        string templateFullName = Path.Combine(tempPath, $"{templateName}.docx");
        string headersFullName = Path.Combine(tempPath, "datasource.tsv");
        string iniFullName = Path.Combine(tempPath, "schema.ini");
        File.WriteAllText(headersFullName, string.Join('\t', Headers) + '\n' + new string('\t', Headers.Length - 1), Encoding.UTF8);
        File.WriteAllText(iniFullName, $"[datasource.tsv]\nFormat=TabDelimited\nColNameHeader=True\nCharacterSet=65001");

        // se esiste già del contenuto (Edit), lo scriviamo su disco, altrimenti Word crea un doc vuoto (New)
        if (existingContent != null) {
            await File.WriteAllBytesAsync(templateFullName, existingContent);
        }

        Application wordApp = new() { Visible = true };
        Document doc = existingContent != null
                     ? wordApp.Documents.Open(templateFullName)
                     : wordApp.Documents.Add();

        doc.Variables.Add("StudioHubSignature", signature);
        if (existingContent == null) {
            doc.SaveAs2(templateFullName);
        }

        doc.MailMerge.OpenDataSource(
            Name: headersFullName,
            ConfirmConversions: false,
            ReadOnly: true,
            Connection: "SELECT * FROM [datasource.tsv]",
            Format: WdOpenFormat.wdOpenFormatText,
            SubType: WdMergeSubType.wdMergeSubTypeOther
        );

        TaskCompletionSource<string?> tcs = new();

        wordApp.DocumentBeforeClose += (closingDoc, ref cancel) => {
            try {
                Variable v = closingDoc.Variables["StudioHubSignature"];
                if (v != null && v.Value == signature) {
                    string temp = closingDoc.FullName;
                    v.Delete();
                    closingDoc.MailMerge.MainDocumentType = WdMailMergeMainDocType.wdNotAMergeDocument;
                    closingDoc.Save();
                    tcs.SetResult(temp);
                }
            }
            catch {
                tcs.SetResult(null);
            }
        };

        string? closedFileFullName = await tcs.Task;
        byte[]? newContent = null;

        newContent = await TryAcquireContentWithTimeoutAsync(closedFileFullName, tempPath);

        GC.KeepAlive(wordApp);
        return newContent;
    }


    private static async Task<byte[]?> TryAcquireContentWithTimeoutAsync(string? filePath, string tempPath) {

        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) {
            return null;
        }

        TimeSpan timeout = TimeSpan.FromSeconds(15);
        Stopwatch sw = Stopwatch.StartNew();
        byte[]? content = null;

        while (sw.Elapsed < timeout) {
            try {
                // Tentativo di apertura esclusiva: se riesce, Word ha rilasciato il file
                using FileStream fs = new(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
                content = new byte[fs.Length];
                await fs.ReadExactlyAsync(content, 0, (int)fs.Length);

                // Se siamo qui, la lettura è riuscita (Semaforo Verde)
                break;
            }
            catch (IOException) {
                // Il file è ancora bloccato, aspettiamo un breve intervallo prima di riprovare
                await Task.Delay(500);
            }
        }

        sw.Stop();

        if (content != null) {
            // Pulizia sicura poiché abbiamo acquisito i byte e il lock è libero
            try {
                File.Delete(filePath);
                if (Directory.Exists(tempPath)) {
                    Directory.Delete(tempPath, true);
                }
            }
            catch {}
            return content;
        }

        // Gestione del caso Timeout
        Dialog.Show("Il sistema non è riuscito ad acquisire il file da Word entro i tempi previsti.\n\nVerrà aperta la cartella temporanea per permettere il recupero manuale.", DialogIcon.Warning);

        try {
            Process.Start(new ProcessStartInfo {
                FileName = tempPath,
                UseShellExecute = true,
                Verb = "open"
            });
        }
        catch { }

        return null;
    }


    [RelayCommand(CanExecute = nameof(CanExecuteNew))]
    private async Task New() {

        IsBusy = true;

        // 1. Apri Word e attendi i byte
        byte[]? content = await OpenWordAndGetContentAsync("NuovoModello");

        if (content != null) {
            // 2. Chiedi i dettagli all'utente
            (string? Name, string? Description) details = EditMailMergeTemplateDetailsView.Open(AllTemplates.Select(t => t.Name));

            if (details.Name != null) {
                try {
                    MailMergeTemplateInfo newInfo = new() {
                        App = AppName,
                        Name = details.Name,
                        Description = details.Description ?? string.Empty,
                        Size = content.Length,
                        LastModified = DateTime.Now
                    };

                    // Dopo questa chiamata, newInfo.Id conterrà già il valore corretto del DB
                    await MailMergeTemplateService.InsertMailMergeTemplateAsync(newInfo, content, string.Join('\t', Headers));

                    // Aggiungiamo direttamente l'oggetto originale
                    AllTemplates.Add(newInfo);
                    ApplyFilter();

                    Dialog.Show("Modello creato e salvato con successo.", DialogIcon.Info);
                }
                catch (Exception ex) {
                    Dialog.Show($"Errore durante il salvataggio: {ex.Message}", DialogIcon.Error);
                }
            }
        }

        IsBusy = false;
    }

    [RelayCommand]
    private async Task EditContent() {
        if (SelectedTemplate == null) {
            return;
        }

        IsBusy = true;

        try {
            // Chiamata esplicita al Service
            byte[] currentContent = await MailMergeTemplateService.GetMailMergeTemplateFileContentAsync(SelectedTemplate.Id);

            if (currentContent.Length == 0) {
                Dialog.Show("Impossibile trovare il file nel database.", DialogIcon.Error);
                return;
            }

            byte[]? newContent = await OpenWordAndGetContentAsync(SelectedTemplate.Name, currentContent);

            if (newContent != null) {
                await MailMergeTemplateService.UpdateMailMergeTemplateFileContentAsync(SelectedTemplate.Id, newContent);

                // Aggiorniamo l'oggetto in memoria per la UI
                SelectedTemplate.Size = newContent.Length;
                SelectedTemplate.LastModified = DateTime.Now;

                Dialog.Show("Modello aggiornato.", DialogIcon.Info);
            }
        }
        catch (Exception ex) {
            Dialog.Show($"Errore: {ex.Message}", DialogIcon.Error);
        }
        finally {
            IsBusy = false;
        }
    }


}