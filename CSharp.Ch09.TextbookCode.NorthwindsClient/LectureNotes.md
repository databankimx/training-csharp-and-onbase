# Ch09 Textbook Code: Northwinds Client

## What This Is, and Why It's Adapted

This is the client side of `CSharp.Ch09.TextbookCode.NorthwindsWCFDataService`, consuming that service's `Categories` entity set two ways: through an OData client library (the "Create a Client Application That Uses WCF Data Services" chapter topic) and as raw JSON over HTTP (the "Request Data as JSON in a Client Application" topic). **Needs the WCF Data Service actually running first** (Visual Studio only, see that project's own `LectureNotes.md`), on `http://localhost:8999/`, the port the original download's own project settings already used.

Like the other two Northwind projects, this is an adapted port, not byte-for-byte preserved, but for a different reason than the EDMX simplification those two needed. Here, the issue was structural: the original download's active code created a `NorthwindsEntities` client from a Visual-Studio-generated Service Reference (`Service References\NorthwindsServiceReference\Reference.cs`), then left every actual *use* of that client, Select, Add, Update, Delete, commented out. Only the raw JSON `HttpWebRequest` at the very end of the file was genuinely active. Porting the file as literally downloaded would have meant one working demo and four inert comments referencing a type that doesn't exist in this migration at all.

---

## A Hand-Written Client, Standing In for Generated Tooling

```csharp
public class NorthwindsEntities : DataServiceContext
{
    public NorthwindsEntities(Uri serviceRoot) : base(serviceRoot, DataServiceProtocolVersion.V3) { }

    public DataServiceQuery<Category> Categories => CreateQuery<Category>("Categories");

    public void AddToCategories(Category category) => AddObject("Categories", category);
}
```

Visual Studio's "Add Service Reference" wizard would normally generate a file like this automatically, by reading the running service's `$metadata` endpoint. Since generating that requires the service to be running at project-creation time (a live dependency this migration process didn't have), `NorthwindsEntities.cs` here is hand-written instead, deliberately shaped to match what that tooling produces closely enough that the CRUD calls in `Program.cs` work exactly the way they would against a real generated proxy: `DataServiceContext` as the base class, one `DataServiceQuery<T>` property per entity set the service exposes. `DataServiceProtocolVersion.V3` matches the service's own `MaxProtocolVersion` setting in `NorthwindsService.svc.cs`'s `InitializeService()`.

---

## All Five Original Operations, Now All Active

```csharp
// Simple Select
var categories = from c in db.Categories select c;

// Add
db.AddToCategories(category);
db.SaveChanges();

// Update
db.UpdateObject(category);
db.SaveChanges();

// Delete
db.DeleteObject(category);
db.SaveChanges();

// Raw JSON request (this one WAS already active in the original)
var req = (HttpWebRequest)WebRequest.Create(".../Categories(1)?$select=...");
req.Accept = "application/json;odata=verbose";
```

Worth comparing this file directly against `CSharp.Ch09.TextbookCode.NorthwindsConsole`'s five operations: same intent (Select, Select-with-a-join's counterpart being skipped here since the service only exposes `Categories`, Add, Update, Delete), same data, but reached through an HTTP-based OData client instead of Entity Framework talking to SQL Server directly. `LINQ` over `db.Categories` looks nearly identical in both files, worth noticing that similarity is deliberate on Microsoft's part: `DataServiceQuery<T>` is built specifically to feel like querying a local `IQueryable<T>`, even though every query actually becomes an HTTP request under the hood.

---

## Worth Reading Alongside the Other Two Projects

Read all three Northwind projects together for the fullest picture: `NorthwindsConsole` reads `Categories`/`Products` directly through Entity Framework, `NorthwindsWCFDataService` exposes that same data as a real, running OData web service, and this project consumes it, both through a typed client library and as raw JSON. Same underlying data, three genuinely different ways of reaching it, each with real, different tradeoffs in coupling, performance, and how much infrastructure has to be running for it to work at all.
