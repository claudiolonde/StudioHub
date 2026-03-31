# Copilot Custom Instructions

## 1. General Principles

* **Core Stack:** C#, .NET, WPF-UI, SQL, e **RepoDb.SqlServer**.
* **High Confidence:** Fornisci solo suggerimenti ad alta confidenza. Se non sei sicuro, chiedi chiarimenti.
* **Testing:** **EVITA** qualsiasi tipo di test (inclusi unit test, test di integrazione o end-to-end) a meno che non sia esplicitamente richiesto.
* **Clean Code:** Dai priorità alla manutenibilità e leggibilità. Evita sintassi troppo complessa che riduce la chiarezza.
* **NativeAOT:** Scrivi codice compatibile con NativeAOT. Evita la generazione dinamica di codice o l'uso intensivo di reflection. Evidenzia le incompatibilità di **RepoDb.SqlServer** con NativeAOT e suggerisci alternative quando necessario.
* **System Files:** Non modificare `global.json`, `package.json`, `package-lock.json` o `NuGet.config` a meno che non sia esplicitamente richiesto.
* **POCO WordTemplate:** Implementa i seguenti campi: `Guid Id` (utilizzato per la cartella temporanea e StudioHubKey), `string Name`, `string Description`, `byte[] Content`, `string TargetApp`, `DateTime Created`, `DateTime Modified`. Preferisci usare questo modello per il salvataggio di blob nel DB; considera varianti come `ContentLength`, `ContentHash`, `MimeType`, `RowVersion` e storage separato/FILESTREAM per grandi BLOB.

## 2. Naming & Formatting

* **EditorConfig:** Segui rigorosamente gli stili definiti in `.editorconfig`.
* **Braces (Mandatory):** Usa sempre le parentesi graffe `{}` per tutte le istruzioni di controllo (`if`, `else`, `for`, `foreach`, `while`), anche per le istruzioni su una sola riga. **NESSUNA ECCEZIONE.**
* **Braces Style (1TBS):** Usa lo stile **1TBS** (la parentesi graffa di apertura sulla stessa riga dell'istruzione).
* **Variables:** Preferisci tipi **espliciti** rispetto a `var` (a meno che il tipo non sia ovvio dall'assegnazione, ad esempio `new()`).
* **CommunityToolkit.Mvvm:** Usa `_camelCase` **SOLO** per i campi privati destinati alla generazione di codice del toolkit.
* **Constants:** Usa `UPPER_CASE` per le costanti e le variabili `static readonly` che agiscono come costanti.
* **Private Methods:** Usa `camelCase` per i metodi privati per distinguerli da quelli pubblici.
* **Modern C#:** Usa costruttori primari, spazi dei nomi a livello di file e **using directives** su una sola riga.
* **Language Helpers:** Usa `ArgumentNullException.ThrowIfNull` e `ObjectDisposedException.ThrowIf` dove applicabile.

## 3. Architecture & Data Access

* **Service-Centric Design:** Non creare classi separate per Repository o DAO. L'accesso ai dati è parte integrante del Service.
* **RepoDb:** Implementa **RepoDb.SqlServer** direttamente nelle classi situate nella cartella `Services`. Evidenzia eventuali incompatibilità con NativeAOT.
* **Immutability:** Usa tipi `record` per il trasferimento dei dati e l'immutabilità, senza sovra-ingegnerizzare.
* **Error Handling:** Bilancia i blocchi `try-catch` e il pattern Result in base alla complessità logica. Evita di catturare eccezioni senza rilanciarle, a meno che non vengano gestite o loggate.
* **Logging:** Non includere `ILogger` o logica di logging a meno che non sia esplicitamente richiesto. Suggerisci approcci per gestire eccezioni critiche senza logging, ma senza essere invasivo.

## 4. Asynchronous Programming

* Fornisci versioni sia sincrone che asincrone dei metodi dove appropriato.
* Usa sempre il suffisso `Async` per i metodi asincroni.
* Ritorna `Task` o `ValueTask`. Evita `async void` (eccetto per gli event handler).
* Usa parametri `CancellationToken` per supportare la cancellazione.
* Usa `ConfigureAwait(false)` solo nel codice di libreria; generalmente non è necessario nelle app desktop moderne o ASP.NET Core.

## 5. XAML Conventions (WPF-UI)

Quando generi XAML, usa rigorosamente questi prefissi di namespace:

* `a:` per il namespace radice/assembly (non usare mai `local:`).
* `c:` per le classi nella cartella `Controls`.
* `h:` per le classi nella cartella `Helpers`.
* `vm:` per le classi nella cartella `ViewModels`.
* `ui:` per i componenti **WPF-UI**.
Assicurati che le dichiarazioni `xmlns` siano coerenti con questa mappatura.

## 6. Documentation & Language

* **Prose Language:** Usa **Italiano** per tutti i commenti, descrizioni e spiegazioni.
* **Technical Terms:** Mantieni in **Inglese** le parole chiave e i termini tecnici (es. *try-catch*, *task*, *loop*, *hot path*). Non tradurre i costrutti di codice.
* **XML Documentation:** Fornisci commenti `///` in **Italiano** per **TUTTI i metodi** (sia pubblici che privati). Mantienili semplici e descrittivi. Usa `<see langword="*" />` per parole chiave come `null`, `true` o `false`.
* **In-line Comments:** Usa `//` in **Italiano** solo per logica critica o non intuitiva.

## 7. Git & Markdown

* **Commit Messages:** Genera messaggi sintetici in **Italiano** usando questo schema (omettendo le categorie vuote):
* `Aggiunto: [elenco sintetico]`
* `Modificato: [elenco sintetico]`
* `Eliminato: [elenco sintetico]`

* **Markdown Blocks:** Specifica il linguaggio per i blocchi di codice (es. `csharp`, `json`, `bash`).

## 8. Lingua Preferita

* Usa **Italiano** per la comunicazione, tranne che per i termini tecnici o relativi al linguaggio di programmazione, che devono rimanere in **Inglese**.
* Non chiudere **MAI** le risposte con domande o proposte di cortesia, a meno che non siano strettamente necessarie a perfezionare la comprensione della domanda dell'utente e la relativa risposta.
