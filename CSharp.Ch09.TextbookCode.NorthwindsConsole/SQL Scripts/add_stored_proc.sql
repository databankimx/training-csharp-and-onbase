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