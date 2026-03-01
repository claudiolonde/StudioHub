using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace StudioHub.Helpers;

/// <summary>
/// Classe di utilità che fornisce metodi helper per la gestione delle finestre WPF.
/// </summary>
public class Win {

    /// <summary>
    /// Restituisce la finestra attiva dell'applicazione corrente, se presente.
    /// </summary>
    /// <returns>
    /// L'istanza <see cref="Window"/> attualmente attiva, oppure <c> null</c> se nessuna finestra è attiva o se non
    /// sono presenti finestre.
    /// </returns>
    public static Window? GetActiveWindow() {
        WindowCollection? windows = Application.Current?.Windows;
        if (windows is null) {
            return null;
        }
        foreach (Window window in windows) {
            if (window.IsActive) {
                return window;
            }
        }
        return null;
    }

    /// <summary>
    /// Disabilita l'icona della finestra specificata.
    /// </summary>
    /// <param name="window">Istanza della finestra WPF di cui disabilitare l'icona.</param>
    /// <remarks>
    /// Inserire la funzione all'interno del metodo <c> OnSourceInitialized</c> della finestra per assicurarsi che venga
    /// eseguita dopo l'inizializzazione. <code> protected override void OnSourceInitialized(EventArgs e) {
    /// base.OnSourceInitialized(e); DisableWindowIcon(this); } </code>
    /// </remarks>
    public static void DisableWindowIcon(Window window) {
        IntPtr hwnd = new WindowInteropHelper(window).Handle;
        int extendedStyle = GetWindowLong(hwnd, -20);
        _ = SetWindowLong(hwnd, -20, extendedStyle | 0x0001);
        SendMessage(hwnd, 0x0080, new IntPtr(1), IntPtr.Zero);
        SendMessage(hwnd, 0x0080, IntPtr.Zero, IntPtr.Zero);
    }
#pragma warning disable SYSLIB1054
    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hwnd, int index);
    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);
    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam);
#pragma warning restore SYSLIB1054

}