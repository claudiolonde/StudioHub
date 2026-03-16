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
        if (!Hub.Initialize()) {
            Application.Current.Shutdown();
            return;
        }
    }
    private void button_Click(object sender, RoutedEventArgs e) {
        Dialog.Show("Titolo",
            DialogType.Warning,
            @"Giusto un semplice messaggio!
            Crea un metodo che accetta un array si stringhe e:
- elimina le stringhe vuote e le stringhe composte da solo un carattere fra questi -> ~!*
- cerca una stringa che inizia con '*' e salva l'indice in defaultIndex e cancelIndex, altrimenti
- cerca una stringa che inizia con '!' e salva l'indice in defaultIndex
- cerca una stringa che inizia con '~' e salva l'indice in cancelIndex
- crea un Button per ogni stringa con queste proprietà:
- Margin = new Thickness(10, 0, 10, 0)
- MinWidth = 100
- Tag = l'indice della stringa
- Content = il testo della stringa senza il prefisso se uno fra ~!*
successivamente imposta:
- prefisso *
newButton.IsDefault = true
newButton.IsCancel = true
newButton.Appearance = ControlAppearance.Primary
newButton.ToolTip = Invio/Esc
- prefisso !
newButton.IsDefault = true
newButton.Appearance = ControlAppearance.Primary
newButton.ToolTip = Invio
- prefisso ~
newButton.IsCancel = true
newButton.Appearance = ControlAppearance.Secondary
newButton.ToolTip = Esc
- oppure
Appearance = ControlAppearance.Transparent

concludi aggiungendo il Button alla raccolta: Buttons.Children.Add(newButton)",
            ["Salva", "*", "!Chiudi"]
        );

        return;
        DialogOLD.Show(@"In WPF.", DialogType.Warning, ["!Claudio", "Sandro", "~Nicholas"], null);
        ManageWordTemplatesView.Open(App.Name, ["Nome", "Cognome", "Indirizzo"]);
    }

}
