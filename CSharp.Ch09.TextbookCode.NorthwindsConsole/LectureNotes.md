# Ch09 Textbook Code: Northwinds Console

## What This Is, and Why It's Adapted Rather Than Raw-Preserved

Unlike most `TextbookCode.*` ports in this training set, this one is **not** a byte-for-byte preservation of the original download. The original used an EDMX-based, Database First EF6 model, a `.edmx` XML file plus roughly 35 T4-template-generated files covering the *entire* Northwind schema (every table, every database view, every stored procedure's result shape), most of which nothing in the actual demonstrated code ever touches. `Program.cs` itself only ever uses `Categories`, `Products`, and one stored procedure, `CustOrderHist`.

Given that, and given a deliberate choice (confirmed directly rather than assumed) to port only what's actually exercised, this project's model was rebuilt as a small, hand-written Code First model against an existing database, `Category`, `Product`, and a `CustOrderHistResult` class for the stored procedure's output, following the exact same pattern already established in `CSharp.Ch09.Supplemental.01.AdoNetAndEntityFramework`. This also has a real practical benefit: EDMX/T4 generation depends on classic Visual Studio tooling that doesn't reliably work in SDK-style projects, avoiding it entirely is what makes this project buildable and runnable via `LessonRunner`/`dotnet run` at all, rather than needing Visual Studio the way `CSharp.Ch09.TextbookCode.NorthwindsWCFDataService` does.

**See `README.md`** for the simplified `Northwinds` database schema (Categories, Products, Customers, Orders, Order Details) and the `CustOrderHist` stored procedure this project needs.

---

## The Five Operations, Preserved in Intent

Every operation from the original download is here, doing the same thing, just against the smaller model:

- **Simple Select**: `from c in db.Categories select c`
- **Select with a join**: `Categories` joined to `Products` on `CategoryID`
- **Add**: create a new `Category`, `Add()` + `SaveChanges()`
- **Update**: fetch an existing `Category`, change a property, `SaveChanges()`
- **Delete**: fetch, `Remove()`, `SaveChanges()`
- **Call a stored procedure**: `CustOrderHist("ALFKI")`

---

## One Real Adaptation: Calling `CustOrderHist`

```csharp
var custOrderHist = db.Database.SqlQuery<CustOrderHistResult>(
    "EXEC CustOrderHist @CustomerID", new SqlParameter("@CustomerID", "ALFKI")).ToList();
```

The original EDMX-generated context exposed this stored procedure as a strongly-typed method, `db.CustOrderHist("ALFKI")`, EDMX's "function import" feature, generated automatically from the `.edmx` file's own metadata. The Code First model here has no EDMX to generate a function import from, so this calls the same procedure the same way `CSharp.Ch09.Supplemental.01.AdoNetAndEntityFramework`'s `EfCallStoredProcedure()` does, via `Database.SqlQuery<T>()`. Functionally identical result, different mechanism, worth comparing directly against that Supplemental if the contrast is useful.

---

## A Real Bug Found While Testing: Lazy Loading Mid-Enumeration

```csharp
var products = from c in db.Categories
                join p in db.Products on c.CategoryID equals p.CategoryID
                select p;

foreach (Product product in products)
{
    Console.WriteLine($" - ProductName: {product.ProductName}, CategoryName: {product.Category.CategoryName}");
}
```

This threw `InvalidOperationException: There is already an open DataReader associated with this Command which must be closed first.` `Category` is a `virtual` navigation property, so `product.Category` triggers **lazy loading**, EF runs a brand-new query the moment that property is read, if it wasn't already loaded. The problem: that read happens *inside* the `foreach` loop, while the *outer* `products` query's own `DataReader` is still open and mid-enumeration on the same connection. Two queries can't share one open reader on the same connection without `MultipleActiveResultSets=True` explicitly enabled, and even with that enabled, firing a fresh query per row inside a loop (the classic N+1 problem) is exactly what eager loading exists to avoid in the first place.

**Fixed** two ways, both applied: `App.config`'s connection string now includes `MultipleActiveResultSets=True` (good general practice for EF6 against SQL Server), and, more importantly, the query itself now eager-loads `Category` and materializes before iterating:

```csharp
var products = (from c in db.Categories
                join p in db.Products.Include(p => p.Category) on c.CategoryID equals p.CategoryID
                select p).ToList();
```

`.Include(p => p.Category)` tells EF to fetch each product's category as part of the *same* query (a SQL join under the hood), so `product.Category.CategoryName` inside the loop reads already-loaded data instead of triggering a new round trip at all. `.ToList()` additionally forces the whole result set to be pulled and the reader closed *before* the loop even starts, belt-and-suspenders on top of the eager load. Worth remembering as a general EF pattern: reading a lazy-loaded navigation property while still enumerating the query that produced the parent object is a reliable way to hit exactly this error, `Include()` (or materializing with `.ToList()`/`.ToArray()` first) is the fix.

---

## Worth Reading Alongside `CSharp.Ch09.TextbookCode.NorthwindsWCFDataService`

That project's `NorthwindsService` exposes exactly the same `Categories` entity set over HTTP/OData, `EntitySetRights.AllRead | EntitySetRights.AllWrite`, matching what this console project does directly through EF. Worth reading both, one shows Entity Framework used locally, the other shows the identical data exposed as a web service that `CSharp.Ch09.TextbookCode.NorthwindsClient` then consumes remotely.
