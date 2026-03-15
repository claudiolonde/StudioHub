using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace StudioHub.Services;


internal class ManageWordTemplatesService : IDisposable {
    public event EventHandler DirectoryChanged;

    private FileSystemWatcher watcher;
    private CancellationTokenSource retryCancellationTokenSource;
    private string currentPath;
    private bool isDisposed;

    public ManageWordTemplatesService() {
    }

    public void Start(string path) {
        currentPath = path;
        initializeWatcher();
    }

    private void initializeWatcher() {
        if (watcher != null) {
            watcher.Dispose();
        }

        try {
            watcher = new FileSystemWatcher(currentPath);

            watcher.Filters.Add("*.doc");
            watcher.Filters.Add("*.docx");

            watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite;

            watcher.Created += onFileSystemEvent;
            watcher.Deleted += onFileSystemEvent;
            watcher.Renamed += onFileSystemEvent;
            watcher.Error += onError;

            watcher.EnableRaisingEvents = true;
        }
        catch (Exception) {
            handleNetworkDrop();
        }
    }

    private void onFileSystemEvent(object sender, FileSystemEventArgs e) {
        DirectoryChanged?.Invoke(sender, EventArgs.Empty);
    }

    private void onError(object sender, ErrorEventArgs e) {
        handleNetworkDrop();
    }

    private void handleNetworkDrop() {
        if (watcher != null) {
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
            watcher = null;
        }

        if (retryCancellationTokenSource != null) {
            retryCancellationTokenSource.Cancel();
            retryCancellationTokenSource.Dispose();
        }

        retryCancellationTokenSource = new CancellationTokenSource();
        CancellationToken token = retryCancellationTokenSource.Token;

        // Tentativi di riconnessione asincroni in caso di caduta della rete
        Task.Run(async () => {
            while (!token.IsCancellationRequested) {
                try {
                    if (Directory.Exists(currentPath)) {
                        initializeWatcher();
                        DirectoryChanged?.Invoke(null, EventArgs.Empty);
                        break;
                    }
                }
                catch (Exception) {
                }

                await Task.Delay(5000, token);
            }
        }, token);
    }

    public void Dispose() {
        if (isDisposed) {
            return;
        }

        if (retryCancellationTokenSource != null) {
            retryCancellationTokenSource.Cancel();
            retryCancellationTokenSource.Dispose();
        }

        if (watcher != null) {
            watcher.EnableRaisingEvents = false;
            watcher.Created -= onFileSystemEvent;
            watcher.Deleted -= onFileSystemEvent;
            watcher.Renamed -= onFileSystemEvent;
            watcher.Error -= onError;
            watcher.Dispose();
        }

        isDisposed = true;
    }
}
