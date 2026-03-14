# Copilot Custom Instructions

## 1. General Principles

* **Core Stack:** C#, .NET, WPF-UI, SQL, and **RepoDb.SqlServer**.
* **High Confidence:** Make only high-confidence suggestions. If unsure, ask for clarification.
* **Testing:** **DO NOT** generate Unit Tests unless explicitly requested.
* **Clean Code:** Prioritize maintainability and readability. Avoid extreme syntactic sugar if it reduces clarity.
* **NativeAOT:** Write NativeAOT-compatible code. Avoid dynamic code generation or heavy reflection. Mark incompatible code with appropriate annotations or exceptions.
* **System Files:** Never modify `global.json`, `package.json`, `package-lock.json`, or `NuGet.config` unless explicitly asked.

## 2. Naming & Formatting

* **EditorConfig:** Strictly follow the styles defined in `.editorconfig`.
* **Braces (Mandatory):** Always use curly braces `{}` for all control flow statements (`if`, `else`, `for`, `foreach`, `while`), even for single-line statements. **NO EXCEPTIONS.**
* **Braces Style (1TBS):** Use the **1TBS style** (opening curly brace on the same line as the statement).
* **Variables:** Prefer **explicit types** over `var` (unless the type is obvious from the assignment, e.g., `new()`).
* **CommunityToolkit.Mvvm:** Use `_camelCase` **ONLY** for private fields intended for toolkit source generation.
* **Constants:** Use `UPPER_CASE` for constants and `static readonly` variables acting as constants.
* **Private Methods:** Use `camelCase` for private methods to distinguish them from public ones.
* **Modern C#:** Use Primary Constructors, file-scoped namespaces, and **single-line using directives** (using declarations).
* **Language Helpers:** Use `ArgumentNullException.ThrowIfNull` and `ObjectDisposedException.ThrowIf` where applicable.

## 3. Architecture & Data Access

* **Service-Centric Design:** Do not create separate Repository or DAO classes. Data access is an integral part of the Service.
* **RepoDb:** Implement **RepoDb.SqlServer** directly within classes located in the `Services` folder.
* **Immutability:** Use `record` types for data transfer and immutability where appropriate, without over-engineering.
* **Error Handling:** Balance `try-catch` blocks and the Result Pattern based on logic complexity. Avoid catching exceptions without rethrowing them unless logging/handling.
* **Logging:** Do **NOT** include `ILogger` or logging logic unless explicitly requested.

## 4. Asynchronous Programming

* Provide both synchronous and asynchronous versions of methods where appropriate.
* Always use the `Async` suffix for asynchronous methods.
* Return `Task` or `ValueTask`. Avoid `async void` (except for event handlers).
* Use `CancellationToken` parameters to support cancellation.
* Use `ConfigureAwait(false)` only in library code; it is generally unnecessary in modern desktop/ASP.NET Core apps.

## 5. XAML Conventions (WPF-UI)

When generating XAML, strictly use these namespace prefixes:

* `a:` for the root/assembly namespace (Never use `local:`).
* `c:` for classes in the `Controls` folder.
* `h:` for classes in the `Helpers` folder.
* `vm:` for classes in the `ViewModels` folder.
* `ui:` for **WPF-UI** components.
Ensure `xmlns` declarations are consistent with this mapping.

## 6. Documentation & Language

* **Prose Language:** Use **Italian** for all prose in comments, descriptions, and explanations.
* **Technical Terms:** Keep technical keywords and terms in **English** (e.g., *try-catch*, *task*, *loop*, *hot path*). Never translate code constructs.
* **XML Documentation:** Provide `///` comments in **Italian** for **ALL methods** (both public and private). Keep them complete but concise. Use `<see langword="*" />` for keywords like `null`, `true`, or `false`.
* **In-line Comments:** Use `//` in **Italian** only for critical or non-intuitive logic.

## 7. Git & Markdown

* **Commit Messages:** Generate synthetic messages in **Italian** using this exact schema (omit empty categories):
* `Aggiunto: [brief list]`
* `Modificato: [brief list]`
* `Eliminato: [brief list]`

* **Markdown Blocks:** Specify the language for code blocks (e.g., `csharp, `json, ```bash).