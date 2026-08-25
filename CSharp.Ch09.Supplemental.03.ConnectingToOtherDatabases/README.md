# Setting Up the Other Database Providers

Only SQLite in this lesson runs without any setup, it's file-based and serverless. Every other provider needs a real server to actually connect to. None of this is required to read or compile the project, only to see a given provider's method actually succeed instead of printing a "could not connect" message.

---

## MySQL

1. Download the free **MySQL Community Server** from <https://dev.mysql.com/downloads/mysql/>, or run it via Docker: `docker run --name mysql-training -e MYSQL_ROOT_PASSWORD=yourpassword -p 3306:3306 -d mysql:latest`.
2. Create a database and table:
   ```sql
   CREATE DATABASE ExternalData;
   USE ExternalData;
   CREATE TABLE MurphysLaws (LawName VARCHAR(50), LawText VARCHAR(250));
   INSERT INTO MurphysLaws VALUES ("Murphy's Law", "Anything that can go wrong will go wrong.");
   ```
3. Update the connection string in `UsingMySql()` with your actual server, username, and password.

## PostgreSQL

1. Download **PostgreSQL** from <https://www.postgresql.org/download/>, or run it via Docker: `docker run --name postgres-training -e POSTGRES_PASSWORD=yourpassword -p 5432:5432 -d postgres:latest`.
2. Create a database and table (note PostgreSQL folds unquoted identifiers to lowercase):
   ```sql
   CREATE DATABASE "ExternalData";
   \c ExternalData
   CREATE TABLE murphyslaws (lawname VARCHAR(50), lawtext VARCHAR(250));
   INSERT INTO murphyslaws VALUES ('Murphy''s Law', 'Anything that can go wrong will go wrong.');
   ```
3. Update the connection string in `UsingPostgreSql()` with your actual server, username, and password.

## Oracle

1. Download **Oracle Database Free** (formerly Express Edition) from <https://www.oracle.com/database/free/>, or run it via Docker: `docker run --name oracle-training -p 1521:1521 -e ORACLE_PWD=yourpassword -d gvenzl/oracle-free`.
2. Create the table:
   ```sql
   CREATE TABLE MurphysLaws (LawName VARCHAR2(50), LawText VARCHAR2(250));
   INSERT INTO MurphysLaws VALUES ('Murphy''s Law', 'Anything that can go wrong will go wrong.');
   COMMIT;
   ```
3. Update the connection string in `UsingOracle()` with your actual username, password, and the correct `Data Source` (host:port/service name, `XEPDB1` is the default pluggable database name for Oracle Database Free).

## ODBC

ODBC is a generic bridge, not tied to one specific database, it's the right choice for connecting to something that doesn't have its own modern .NET driver: Microsoft Access, Excel files, or older legacy systems.

1. Install an ODBC driver for whatever you're connecting to (Microsoft Access Database Engine, for example, is available from <https://www.microsoft.com/en-us/download/details.aspx?id=54920>).
2. Create a **Data Source Name (DSN)** via Windows' "ODBC Data Sources" administrative tool (search for it in the Start menu), pointing at your actual data source.
3. Update the connection string in `UsingOdbc()` with your DSN's actual name.

## MongoDB

1. Download **MongoDB Community Server** from <https://www.mongodb.com/try/download/community>, or run it via Docker: `docker run --name mongo-training -p 27017:27017 -d mongo:latest`.
2. No table/schema creation needed, MongoDB creates the `ExternalData` database and `MurphysLaws` collection automatically the first time `UsingMongoDb()` inserts a document into them.
3. The default connection string (`mongodb://localhost:27017`) should work as-is against a local, unauthenticated MongoDB instance.

---

## A Note on Running This More Than Once

`UsingSqlite()` creates and deletes its own temporary database file on every run, so it's always safe to re-run. The other methods (once you've set up a real server) will insert a new row/document every time they run, that's fine and expected for this lesson, but worth knowing if you're checking the data afterward and see duplicates.
