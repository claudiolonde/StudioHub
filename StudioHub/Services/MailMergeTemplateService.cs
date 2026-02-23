using Microsoft.Data.SqlClient;
using RepoDb;
using StudioHub.Models;

namespace StudioHub.Services;

public static class MailMergeTemplateService {

    // Recupera i metadati per l'app specifica
    public static async Task<IEnumerable<MailMergeTemplateInfo>> GetMailMergeTemplateInfoAsync(string appName) {
        using SqlConnection connection = new(ConnectionInfoService.PrimaryConnectionString);
        return await connection.QueryAsync<MailMergeTemplateInfo>(x => x.App == appName);
    }

    // Recupera il contenuto binario e le headers
    public static async Task<MailMergeTemplateData?> GetMailMergeTemplateDataAsync(int id) {
        using SqlConnection connection = new(ConnectionInfoService.PrimaryConnectionString);
        return (await connection.QueryAsync<MailMergeTemplateData>(x => x.Id == id)).FirstOrDefault();
    }

    public static async Task<byte[]> GetMailMergeTemplateFileContentAsync(int id) {
        using SqlConnection connection = new(ConnectionInfoService.PrimaryConnectionString);
        MailMergeTemplateData? data = (await connection.QueryAsync<MailMergeTemplateData>(x => x.Id == id)).FirstOrDefault();
        return data?.FileContent ?? [];
    }

    // Inserimento atomico di Info + Data
    public static async Task<int> InsertMailMergeTemplateAsync(MailMergeTemplateInfo info, byte[] content, string headers) {

        using SqlConnection connection = new(ConnectionInfoService.PrimaryConnectionString);
        await connection.OpenAsync();
        using SqlTransaction transaction = connection.BeginTransaction();

        try {
            int id = (int)await connection.InsertAsync(info, transaction: transaction);

            MailMergeTemplateData data = new() {
                Id = id,
                FileContent = content,
                Headers = headers
            };
            await connection.InsertAsync(data, transaction: transaction);

            transaction.Commit();
            return id;
        }
        catch {
            transaction.Rollback();
            throw;
        }
    }

    // Aggiornamento dei soli metadati (Nome/Descrizione)
    public static async Task UpdateMailMergeTemplateInfoAsync(int id, string name, string description) {

        using SqlConnection connection = new(ConnectionInfoService.PrimaryConnectionString);
        await connection.UpdateAsync<MailMergeTemplateInfo>(new() {
            Id = id,
            Name = name,
            Description = description,
            LastModified = DateTime.Now
        });
    }

    // Aggiornamento del file Word
    public static async Task UpdateMailMergeTemplateDataAsync(int id, byte[] content) {

        using SqlConnection connection = new(ConnectionInfoService.PrimaryConnectionString);
        await connection.OpenAsync();
        using SqlTransaction transaction = connection.BeginTransaction();

        try {
            await connection.UpdateAsync<MailMergeTemplateData>(
                new() {
                    Id = id,
                    FileContent = content
                },
                transaction: transaction);
            await connection.UpdateAsync<MailMergeTemplateInfo>(
                new() {
                    Id = id,
                    Size = content.Length,
                    LastModified = DateTime.Now
                },
                transaction: transaction);
            transaction.Commit();
        }
        catch {
            transaction.Rollback();
            throw;
        }
    }

    public static async Task UpdateMailMergeTemplateFileContentAsync(int id, byte[] newContent) {
        using SqlConnection connection = new(ConnectionInfoService.PrimaryConnectionString);
        await connection.OpenAsync();

        using SqlTransaction transaction = connection.BeginTransaction();
        try {
            // 1. Aggiorniamo il blob binario
            await connection.UpdateAsync<MailMergeTemplateData>(
                new() {
                    Id = id,
                    FileContent = newContent
                },
                transaction: transaction
            );

            // 2. Aggiorniamo metadati (dimensione e data) nella tabella Info
            await connection.UpdateAsync<MailMergeTemplateInfo>(
                new() {
                    Id = id,
                    Size = newContent.Length,
                    LastModified = DateTime.Now
                },
                transaction: transaction
            );

            transaction.Commit();
        }
        catch {
            transaction.Rollback();
            throw;
        }
    }
}
