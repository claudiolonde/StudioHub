using System.Windows;
using StudioHub;
using StudioHub.Views;

namespace _test;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainView {

    public MainView() {
        InitializeComponent();
        Dialog.Show(@"Capisco perfettamente la frustrazione. Il problema del ""ritaglio"" (clipping) con SizeToContent in WPF è un classico, specialmente quando si usano librerie come wpf-ui che iniettano un ""Chrome"" personalizzato sopra la finestra standard. Se il framework non sa esattamente quanto spazio occupa la barra del titolo decorata, taglia il contenuto in basso.

Per forzare un ricalcolo corretto e risolvere il problema dell'area bianca/nera, dobbiamo agire su due fronti: invalidare il layout dopo il rendering iniziale e usare un container che non soffra di ""misurazioni pigre"".

Ecco la soluzione tecnica ""hardened"":

1. XAML: Il trucco del ""Margin Negativo"" e Grid Layout
Invece di lasciare che la finestra decida tutto, forziamo un'area di contenuto che spinga per le sue dimensioni.",
            "",
            ["\\dSalva", "Ignora modifiche", "\\cAnnulla"],
            MessageType.Info
        );
        return;
        if (!Hub.Initialize()) {
            Application.Current.Shutdown();
            return;
        }
    }
    private void button_Click(object sender, RoutedEventArgs e) {
        WordTemplatesView.Open(App.Name, ["Nome", "Cognome", "Indirizzo"]);
        return;
    }

}
