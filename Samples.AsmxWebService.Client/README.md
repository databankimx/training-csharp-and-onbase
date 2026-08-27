# Samples.AsmxWebService.Client

> **Looking for implementation details or porting notes?** See `LectureNotes.md` in this folder.

## What This Is

A console application that consumes the `Samples.AsmxWebService` ASMX service, using a genuinely generated proxy class rather than hand-written HTTP calls. This is the standard, decades-old pattern for consuming any WSDL-publishing SOAP service from .NET:

1. **Generate a proxy class from the WSDL**, using the Visual Studio command-line tool:
   ```
   wsdl.exe /l:cs <path to input WSDL file> /o:<path to output .cs file>
   ```
2. **Instantiate the generated class** like any other object, setting its `Url`:
   ```csharp
   var client = new ExampleWebService { Url = "https://localhost:44355/ExampleWebService.asmx" };
   ```
3. **Call the exposed methods** directly, the proxy handles SOAP envelope construction and parsing entirely behind the scenes:
   ```csharp
   var result = client.Ping();
   ```

`WebService/ExampleWebService.cs` in this project is exactly that generated class, and `WebService/ExampleWebService.wsdl` is the WSDL document it was generated from, both kept unmodified so you can see genuine `wsdl.exe` output, not a hand-written approximation of it.

---

## When to Use This Pattern

Only when consuming an ASMX (or other WSDL-publishing SOAP) service that already exists, exactly the same "never for new development, but you'll maintain it" framing as ASMX itself. See `Samples.AsmxWebService`'s own `README.md` for the fuller pros/cons discussion of the technology this client is consuming.

---

## What's in This Project

| Path | Purpose |
|---|---|
| `Program.cs` | Walks through `Ping()`, `TestService()`, and `LookupLocation()` interactively |
| `WebService/ExampleWebService.cs` | The `wsdl.exe`-generated proxy class and its data contracts |
| `WebService/ExampleWebService.wsdl` | The WSDL document the proxy was generated from (reference only) |
| `Models/Configuration/WebServiceSettings.cs` | Strongly-typed `App.config` section (the service URL) |
| `App.config` | Where the actual service URL is configured |

---

## How to Run

1. Run `Samples.AsmxWebService` first (F5 in Visual Studio, IIS Express), and leave it running.
2. Run this project. It calls each of the three service methods in turn, pausing after each so you can read the result before continuing.

---

## Related Samples

- **`Samples.AsmxWebService`** — the ASMX service this client consumes.
- **`Samples.AsmxWebService.WebClient`** — a browser-based (HTML/JavaScript) client calling the same service directly via AJAX, a very different consumption style worth comparing against this one.
