# Istruzioni di Coding per Copilot

## 1. Tecnologie e Linguaggi
- **Stack principale:** C#, .NET e SQL.
- Per SQL, prediligi leggibilità ed efficienza standard.

## 2. Stile di Codifica (Mandatorio)
- **Tipi Espliciti:** NON usare mai 'var'. Usa SEMPRE il tipo esplicito (es. `int x = 5;`).
- **Nessun 'this':** NON qualificare i membri della classe con 'this.', salvo ambiguità tecniche.
- **Parentesi (1TBS):** Usa lo stile "One True Brace Style" (parentesi sulla stessa riga) SEMPRE, anche per blocchi a singola riga.
- **Asincronia:** Usa 'Async/Await' SOLO per vantaggi prestazionali reali. Se presente, il suffisso 'Async' è obbligatorio.
- **Semplificazione:** Evita espressioni eccessivamente contratte o zucchero sintattico estremo se riducono la leggibilità del tipo esplicito.

## 3. Naming Convention
- **Entità Private:** camelCase.
- **CommunityToolkit.Mvvm:** _camelCase SOLO per i campi generati dal toolkit.
- **Interfacce:** IPascalCase.
- **Costanti:** UPPER_CASE (anche per variabili readonly con funzione di costante).

## 4. Architettura e Struttura
- **Approccio:** Separazione delle responsabilità e uso di 'Record' per immutabilità, senza fanatismo.
- **Errori:** Bilancia try-catch e Result Pattern in base alla complessità.
- **Logging:** NON includere ILogger se non richiesto esplicitamente.

## 5. Documentazione e Lingua
- **Lingua:** Usa l'italiano per la prosa dei commenti (descrizioni e spiegazioni).
- **Lessico Tecnico:** Mantieni in inglese i termini tecnici e le keyword (es. try-catch, task, loop). NON tradurre mai i costrutti del codice.
- **XML Documentation:** Commenti `///` in italiano per TUTTI i metodi (pubblici e privati), completi ma concisi.
- **Commenti In-line:** `//` in italiano SOLO per passaggi critici o poco intuitivi.
- **Test:** NON generare Unit Test.

## 6. Messaggi di Commit Git
- **Struttura:** Genera messaggi molto sintetici in italiano seguendo rigorosamente questo schema:
    Aggiunto: [elenco breve]
    Modificato: [elenco breve]
    Eliminato: [elenco breve]
- **Nota:** Se una categoria è vuota, omettila pure per mantenere la sintesi.

## 7. Convenzioni XAML
- **Namespace Prefixes:** Quando suggerisci o generi codice XAML, usa questi prefissi specifici:
    - 'a:' per il namespace radice del progetto (App/Assembly). NON usare mai 'local'.
    - 'c:' per le classi nella cartella Controls.
    - 'h:' per le classi nella cartella Helpers.
    - 'vm:' per le classi nella cartella ViewModels.
- **Sintassi:** Mantieni i prefissi brevi e assicurati che le dichiarazioni xmlns siano coerenti con questa mappatura.