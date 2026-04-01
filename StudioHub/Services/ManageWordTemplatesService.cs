using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using StudioHub.Models;

namespace StudioHub.Services;

public class ManageWordTemplatesService
{
    // Costruttore senza parametri (niente DI)
    public ManageWordTemplatesService()
    {
        // Qui in futuro potremmo inizializzare la stringa di connessione a RepoDb
    }

    // --- Metodi CRUD per il Database ---
    public async Task<List<WordTemplate>> GetAllTemplatesAsync() 
    {
        // Da implementare con RepoDb
        return [];
    }

    public async Task DeleteTemplateAsync(Guid id)
    {
        // Da implementare con RepoDb
    }

    public async Task DuplicateTemplateAsync(Guid id, string newName)
    {
        // Da implementare con RepoDb: legge, cambia Id e Nome, salva
    }

    public async Task UpdateTemplateDetailsAsync(Guid id, string newName, string newDescription)
    {
        // Da implementare con RepoDb: aggiorna solo Nome e Descrizione
    }

    // --- Metodi per il Flusso Word (Step 3) ---
    public async Task<WordTemplate> CreateNewTemplateContentAsync(CancellationToken ct)
    {
        // Da implementare: crea file vuoto, avvia Word, attende chiusura, ritorna il file binario
        return null!; 
    }

    public async Task<byte[]> EditTemplateContentAsync(Guid templateId, byte[] currentContent, CancellationToken ct)
    {
        // Da implementare: ripristina file, avvia Word, attende chiusura, ritorna nuovo binario
        return [];
    }
}
