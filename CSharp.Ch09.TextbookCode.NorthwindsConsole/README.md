# Setting Up the "Northwinds" Database

This lesson cluster (`CSharp.Ch09.TextbookCode.NorthwindsConsole`, `CSharp.Ch09.TextbookCode.NorthwindsWCFDataService`, `CSharp.Ch09.TextbookCode.NorthwindsClient`) runs against a database named **`Northwinds`** (with a trailing "s", matching the original textbook download's own connection strings and namespaces exactly, not the more commonly-seen "Northwind").

Rather than the full ~800-line Microsoft Northwind sample script, this is a **simplified subset**: just `Categories`, `Products`, `Customers`, `Orders`, `Order Details`, and the `CustOrderHist` stored procedure, the only parts the actual demonstrated code (`Program.cs` in each project) touches. See `LectureNotes.md` in `CSharp.Ch09.TextbookCode.NorthwindsConsole` for why the full ~35-table/view/procedure model wasn't ported.

---

## 1. Create the Database and Schema

Run this in SQL Server Management Studio, connected to your local SQL Server instance (see `CSharp.Ch09.Supplemental.01.AdoNetAndEntityFramework/README.md` if you don't have SQL Server installed yet):

```sql
CREATE DATABASE Northwinds;
GO

USE Northwinds;
GO

CREATE TABLE Categories (
    CategoryID   INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName NVARCHAR(15) NOT NULL,
    Description  NTEXT NULL,
    Picture      IMAGE NULL
);

CREATE TABLE Products (
    ProductID    INT IDENTITY(1,1) PRIMARY KEY,
    ProductName  NVARCHAR(40) NOT NULL,
    CategoryID   INT NULL REFERENCES Categories(CategoryID),
    UnitPrice    MONEY NULL,
    UnitsInStock SMALLINT NULL,
    Discontinued BIT NOT NULL DEFAULT 0
);

CREATE TABLE Customers (
    CustomerID   NCHAR(5) PRIMARY KEY,
    CompanyName  NVARCHAR(40) NOT NULL,
    ContactName  NVARCHAR(30) NULL,
    City         NVARCHAR(15) NULL,
    Country      NVARCHAR(15) NULL
);

CREATE TABLE Orders (
    OrderID      INT IDENTITY(1,1) PRIMARY KEY,
    CustomerID   NCHAR(5) NULL REFERENCES Customers(CustomerID),
    OrderDate    DATETIME NULL
);

CREATE TABLE [Order Details] (
    OrderID      INT NOT NULL REFERENCES Orders(OrderID),
    ProductID    INT NOT NULL REFERENCES Products(ProductID),
    UnitPrice    MONEY NOT NULL,
    Quantity     SMALLINT NOT NULL DEFAULT 1,
    Discount     REAL NOT NULL DEFAULT 0,
    PRIMARY KEY (OrderID, ProductID)
);
GO
```

## 2. Seed Some Data

```sql
USE Northwinds;
GO

INSERT INTO Categories (CategoryName, Description) VALUES
    ('Beverages', 'Soft drinks, coffees, teas, beers, and ales'),
    ('Condiments', 'Sweet and savory sauces, relishes, spreads, and seasonings'),
    ('Confections', 'Desserts, candies, and sweet breads');

INSERT INTO Products (ProductName, CategoryID, UnitPrice, UnitsInStock) VALUES
    ('Chai', 1, 18.00, 39),
    ('Chang', 1, 19.00, 17),
    ('Aniseed Syrup', 2, 10.00, 13),
    ('Chef Anton''s Cajun Seasoning', 2, 22.00, 53),
    ('Teatime Chocolate Biscuits', 3, 9.20, 25);

INSERT INTO Customers (CustomerID, CompanyName, ContactName, City, Country) VALUES
    ('ALFKI', 'Alfreds Futterkiste', 'Maria Anders', 'Berlin', 'Germany'),
    ('ANATR', 'Ana Trujillo Emparedados y helados', 'Ana Trujillo', 'México D.F.', 'Mexico');

INSERT INTO Orders (CustomerID, OrderDate) VALUES
    ('ALFKI', '2026-01-15'),
    ('ALFKI', '2026-02-03');

-- Order 1 (the first identity value inserted above) gets two line items,
-- order 2 gets one, adjust the OrderID literals below if your identity
-- values don't start at 1 (check with SELECT * FROM Orders first).
INSERT INTO [Order Details] (OrderID, ProductID, UnitPrice, Quantity) VALUES
    (1, 1, 18.00, 10),
    (1, 3, 10.00, 5),
    (2, 2, 19.00, 20);
GO
```

## 3. Create the `CustOrderHist` Stored Procedure

This is the one stored procedure the demonstrated code actually calls, matching the real Northwind sample's own definition:

```sql
USE Northwinds;
GO

CREATE PROCEDURE CustOrderHist
    @CustomerID NCHAR(5)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT P.ProductName, SUM(OD.Quantity) AS Total
    FROM Products P
    JOIN [Order Details] OD ON P.ProductID = OD.ProductID
    JOIN Orders O ON OD.OrderID = O.OrderID
    JOIN Customers C ON O.CustomerID = C.CustomerID
    WHERE C.CustomerID = @CustomerID
    GROUP BY P.ProductName;
END
GO
```

With the seed data above, `EXEC CustOrderHist 'ALFKI'` should return two rows: `Chai` (10) and `Aniseed Syrup` (5).

---

## 4. Running Each Project

- **`CSharp.Ch09.TextbookCode.NorthwindsConsole`**: an ordinary SDK-style console project, runs via `LessonRunner` or `dotnet run` like everything else in this training set, once the database above exists.
- **`CSharp.Ch09.TextbookCode.NorthwindsWCFDataService`**: **must be opened and run in Visual Studio**, not through `LessonRunner`. Open the project, press F5 (or right-click the `.svc` file → "View in Browser"), Visual Studio will start IIS Express and host the service automatically. See that project's own `LectureNotes.md` for why this can't be automated the way the rest of this training set is.
- **`CSharp.Ch09.TextbookCode.NorthwindsClient`**: an SDK-style console project, but only useful once `NorthwindsWCFDataService` is actually running (via Visual Studio, per above), it makes an HTTP request against that running service's URL.
