# Samples.AsmxWebService.Client

## What This Is

A .NET console client consuming `Samples.AsmxWebService` via a `wsdl.exe`-generated proxy class -- the standard decades-old pattern for calling any WSDL-publishing SOAP service from .NET.

---

## When to Use This Pattern

Only when consuming an existing ASMX or other WSDL-publishing SOAP service. Same "maintenance only" framing as ASMX itself.

---

## How It Works

The proxy class (`WebService/ExampleWebService.cs`) was generated from the WSDL with:

```
wsdl.exe /l:cs ExampleWebService.wsdl /o:ExampleWebService.cs
```

Instantiate it, set the URL, and call its methods directly -- the proxy handles the SOAP envelope behind the scenes:

```csharp
var client = new ExampleWebService { Url = "https://localhost:44355/ExampleWebService.asmx" };
var result = client.Ping();
var location = client.LookupLocation(new LocationLookupRequest { ZipCode = "75067" });
```

The generated class and the original WSDL are kept unmodified in `WebService/` so you can see genuine `wsdl.exe` output.

---

## Creating a Similar Project

### Visual Studio

**File > New > Project**, select "Console App (.NET Framework)". After the project is created, regenerate the proxy from a live service with: **Project > Add Service Reference** (or right-click > Add > Service Reference) and point at the `.asmx?wsdl` URL. Alternatively, run `wsdl.exe` from a Visual Studio Developer Command Prompt and add the resulting file manually.

### VS Code

```powershell
dotnet new console -n MyAsmxClient -f net48
```

Run `wsdl.exe` from a Visual Studio Developer Command Prompt (not available in VS Code directly) to generate the proxy file, then add it to the project manually.

---

## Running This Project

1. Start `Samples.AsmxWebService` (F5 in Visual Studio) and leave it running.
2. Run this project. It calls Ping, TestService, and LookupLocation in turn.

---

## Related Projects

- `Samples.AsmxWebService` -- the service this client consumes.
- `Samples.AsmxWebService.WebClient` -- a browser AJAX client for comparison.
