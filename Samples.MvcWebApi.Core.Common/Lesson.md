# Samples.MvcWebApi.Core.Common

## What This Is

Shared request/response types for `Samples.MvcWebApi.Core` and `Samples.MvcWebApi.Core.Client`. Same role as `Samples.MvcWebApi.Common` -- filling the gap left by the absence of a built-in contract format in Web API -- but using C# `record` types instead of plain mutable classes.

---

## When to Use This Pattern

Same reasoning as `Samples.MvcWebApi.Common`: whenever a .NET client needs to consume an ASP.NET Core Web API in a type-safe way without duplicating or hand-writing the response shapes.

---

## Records vs. Classes

```csharp
// Samples.MvcWebApi.Common (classic) -- mutable class
public class Location
{
    public string City { get; set; }
    public string State { get; set; }
}

// Samples.MvcWebApi.Core.Common (modern) -- immutable record
public record Location(string State, string County, string City, string ZipCode);
```

`record` types are immutable by default, provide value-based equality (`location1 == location2` compares field values, not reference identity), and are the idiomatic C# choice for data-holder types that don't need mutation. Both approaches work equally well for JSON deserialization.

---

## Creating a Similar Project

### Visual Studio

**File > New > Project**, select "Class Library", choose the appropriate .NET version. Add `record` definitions. Add a project reference from the API and client projects.

### VS Code

```powershell
dotnet new classlib -n MyApi.Core.Common -f net10.0
dotnet add ../MyApi.Core/MyApi.Core.csproj reference MyApi.Core.Common.csproj
dotnet add ../MyApi.Core.Client/MyApi.Core.Client.csproj reference MyApi.Core.Common.csproj
```

---

## Related Projects

- `Samples.MvcWebApi.Core` -- the API that uses these types.
- `Samples.MvcWebApi.Core.Client` -- the .NET client that references this library.
- `Samples.MvcWebApi.Common` -- the classic equivalent using plain mutable classes.
