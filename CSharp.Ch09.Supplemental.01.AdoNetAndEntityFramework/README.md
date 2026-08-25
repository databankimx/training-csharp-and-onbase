# Setting Up SQL Server for This Lesson

This lesson (and `CSharp.Ch09.Supplemental.02.SqlInjection`) run against a real SQL Server database, `ExternalData`, restored from a backup file. This is a one-time setup, once it's done, every lesson that uses `ExternalData` will work.

---

## 1. Install SQL Server (Free, Developer Edition)

If you don't already have a local SQL Server instance:

1. Go to <https://www.microsoft.com/en-us/sql-server/sql-server-downloads>.
2. Download **Developer Edition** (free, full-featured, licensed for non-production use, exactly right for a training environment).
3. Run the installer. Choose the **Basic** installation type, it's enough for everything in this training set.
4. Note the instance name shown at the end of setup. The default is usually `MSSQLSERVER` (the "default instance", reachable as just `.` or `localhost`), or `SQLEXPRESS` if you installed Express edition instead.

## 2. Install SQL Server Management Studio (SSMS)

SSMS is the graphical tool used to restore the database backup and browse the data afterward.

1. Go to <https://learn.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms>.
2. Download and run the installer (also free).
3. Launch SSMS and connect to your local instance (server name `.` or `localhost`, or `.\SQLEXPRESS` if that's what you installed, Windows Authentication is fine).

## 3. Restore the `ExternalData` Database

The backup file is at `Resources\Other\ExternalData.bak` in this repository.

1. In SSMS's Object Explorer, right-click **Databases** → **Restore Database...**
2. Under **Source**, choose **Device**, click the `...` button, click **Add**, and browse to `ExternalData.bak`.
3. Once selected, the **Destination** database name should auto-fill as `ExternalData`, leave it as-is.
4. Click **OK** to restore.
5. Once restored, expand **Databases** → **ExternalData** → **Tables** in Object Explorer to confirm you see `MurphysLaws`, `Numbers`, `Phrases`, `TestItems`, and `ZipCodes`.

If you'd rather do this with a script instead of the wizard, here's the T-SQL equivalent (adjust the file paths to match where you saved `ExternalData.bak`, and where you want the restored `.mdf`/`.ldf` files to live):

```sql
RESTORE DATABASE [ExternalData]
FROM DISK = N'C:\Path\To\ExternalData.bak'
WITH MOVE N'Externals' TO N'C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\DATA\ExternalData.mdf',
     MOVE N'Externals_log' TO N'C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\DATA\ExternalData_log.ldf',
     REPLACE;
```

`RESTORE FILELISTONLY FROM DISK = N'C:\Path\To\ExternalData.bak'` will show you the actual logical file names and their original paths if the `MOVE` clauses above don't match your SQL Server installation's data directory.

## 4. Create the Stored Procedure Used by This Lesson

The chapter's "Call a Stored Procedure" topic needs one to actually call. Run this once, in SSMS, against the `ExternalData` database:

```sql
USE [ExternalData]
GO

CREATE PROCEDURE [dbo].[GetZipCodesByState]
    @State VARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT [Id], [State], [County], [City], [ZipCode]
    FROM [dbo].[ZipCodes]
    WHERE [State] = @State
    ORDER BY [City];
END
GO
```

## 5. Point the Lesson at Your Server

Both `CSharp.Ch09.Supplemental.01.AdoNetAndEntityFramework` and `CSharp.Ch09.Supplemental.02.SqlInjection` read the connection's server name from `App.config`. Open `App.config` in each project and update the `Data Source` value to match your instance:

```xml
<connectionStrings>
  <add name="ExternalData" connectionString="Data Source=.;Initial Catalog=ExternalData;Integrated Security=True;" providerName="System.Data.SqlClient" />
</connectionStrings>
```

- `Data Source=.` targets your machine's default instance. If you installed SQL Server Express, use `Data Source=.\SQLEXPRESS` instead.
- `Integrated Security=True` uses your current Windows login, no separate SQL username/password needed for a local dev setup like this. If you created a SQL Server login instead of using Windows Authentication, replace it with `User Id=yourUsername;Password=yourPassword;` (and see `CSharp.Ch09.Supplemental.02.SqlInjection`'s lecture notes for why hardcoding real credentials in source code is exactly the kind of thing to avoid outside of a local training sandbox).

Once that's set, both lessons should run directly against your restored `ExternalData` database.
