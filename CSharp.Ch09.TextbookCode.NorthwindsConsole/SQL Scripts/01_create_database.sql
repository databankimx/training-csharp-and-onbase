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