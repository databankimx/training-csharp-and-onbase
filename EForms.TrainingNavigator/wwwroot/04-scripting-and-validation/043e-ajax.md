# AJAX

AJAX lets a script call a web service from the browser without reloading the page or navigating anywhere - the mechanism behind looking up data from an external system while a form is still open and being filled out.

## The Name Is Historical

"AJAX" originally stood for "Asynchronous JavaScript And XML." In practice today, a modern AJAX call almost always sends and receives JSON rather than XML - the name stuck around long after the "X" stopped being accurate for most real usage.

## A Real Example: Location Lookup by ZIP Code

This lesson's own page calls a real, working service - a local REST API implemented in ASP.NET Core with Entity Framework Core - to look up a location by ZIP code. The service runs on the same server as this training page and queries a SQL Server database of US locations.

```javascript
$.ajax({
    url: "/api/location/lookup",
    type: "POST",
    contentType: "application/json",
    data: JSON.stringify({ zipCode: zipCode, requestId: requestId }),
    xhrFields: {
        withCredentials: true
    },
    success: function (result) {
        // runs once the service responds successfully
    },
    error: function (jqXHR, textStatus, errorThrown) {
        // runs if the request fails instead
    },
});
```

A few things worth understanding about this call:

- **The URL is now local:** Instead of calling an external WCF service or public API, this AJAX call goes to `/api/location/lookup` on the same server. This minimizes network latency and avoids external dependencies.
- **The request body uses camelCase property names:** `zipCode` and `requestId` (not `ZipCode` and `RequestId`). This is the standard JSON naming convention for REST APIs.
- **No need for `crossDomain: true`:** The request stays on the same server, so cross-domain CORS handling is not needed.
- **Two separate callbacks handle the two possible outcomes** - `success` if the service responds normally, `error` if the request fails (network error, server error, etc.).

## Setting Up Your Database

The location lookup service queries a SQL Server database for ZIP code information. To use this service:

### 1. Download the Database Backup

Visit the home page and click "Try It: A Real Location Lookup", then click "Change Connection String". You'll find a link to download `ExternalData.bak`.

### 2. Restore the Database

Use SQL Server Management Studio or the command line to restore the backup to your SQL Server instance:

```sql
RESTORE DATABASE ExternalData 
FROM DISK = 'C:\backups\ExternalData.bak' 
WITH REPLACE, 
RECOVERY
```

Replace `'C:\backups\ExternalData.bak'` with the actual path to your downloaded backup file.

### 3. Configure the Connection String

The default connection string is already configured in `appsettings.json` to connect to `OnBaseSandboxVM`. 

If your database is elsewhere (Azure SQL Database, local machine, cloud provider, different server), click "Change Connection String" on the demo page to generate a new connection string. You can either:

- **Temporarily override** by using the modal form (regenerate the string each time)
- **Permanently override** by setting the `ConnectionStrings__DefaultConnection` environment variable in your deployment environment

Environment variable example (Windows):
```powershell
$env:ConnectionStrings__DefaultConnection="Server=myserver.database.windows.net;Database=ExternalData;User Id=admin;Password=MyPassword123;Encrypt=true"
```

Environment variable example (Linux/macOS):
```bash
export ConnectionStrings__DefaultConnection="Server=myserver.postgres.database.azure.com;Database=ExternalData;User Id=admin;Password=MyPassword123"
```

### For Different Database Platforms

**SQL Server:**
```
Server=myserver.database.windows.net;Database=ExternalData;User Id=admin;Password=pass;Encrypt=true
```

**Azure SQL Database:**
```
Server=myserver.database.windows.net;Database=ExternalData;User Id=admin@myserver;Password=pass;Encrypt=true;TrustServerCertificate=false
```

**Local Development:**
```
Server=.\SQLEXPRESS;Database=ExternalData;Integrated Security=true;TrustServerCertificate=true
```

## AJAX Is Asynchronous

The "A" is the important part in practice: the call doesn't pause the script and wait for a response. `$.ajax()` sends the request and returns immediately - whatever code comes right after it keeps running before any response has actually come back. The `success`/`error` callbacks run later, whenever the response actually arrives, which could be milliseconds or seconds later, on its own separate timeline from the rest of the script.

> **Why this matters for error handling:** a plain `try`/`catch` wrapped around an AJAX call will *not* catch a failure coming back from the service - by the time the response arrives (success or failure), the `try` block has already finished running and moved on. This is exactly why AJAX calls need their own dedicated `error` callback, rather than relying on the same `try`/`catch` pattern used everywhere else in this training set for handling errors.

## How This Training Project Differs from the AuthGateway

This training project is a **self-contained, standalone version** of the location lookup service:

- **No authentication required** - all endpoints are publicly accessible for student use
- **Same database and schema** - both use the same `ExternalData` database and `ZipCodes` table
- **Same REST endpoints** - `/api/location/lookup` (POST) and `/api/download/externaldata-backup` (GET)
- **Same modal configuration UI** - students can configure their own database connection
- **Same styling and experience** - the HTML, CSS, and JavaScript are consistent with the main AuthGateway

Students can use this project independently to learn AJAX patterns without needing access to the main AuthGateway application.

