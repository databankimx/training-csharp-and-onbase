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