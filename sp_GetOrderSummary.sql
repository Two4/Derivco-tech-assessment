CREATE PROCEDURE pr_GetOrderSummary
    @StartDate DATETIME,
    @EndDate DATETIME,
    @EmployeeID INT,
    @CustomerID NCHAR(5)
AS
SELECT
    CONCAT(E.TitleOfCourtesy, ' ', E.FirstName, ' ', E.LastName) AS EmployeeFullName,
    S.CompanyName AS ShipperCompanyName,
    C.CompanyName AS CustomerCompanyName,
    COUNT(O.OrderID) AS OrderCount,
    O.OrderDate AS [Date],
    SUM(O.Freight) AS TotalFreightCost,
    COUNT(DISTINCT OD.ProductID) AS NumberOfDifferentProducts,
    SUM(OS.Subtotal) AS TotalOrderValue
FROM
    Orders O
    INNER JOIN Employees E ON O.EmployeeID = E.EmployeeID
    INNER JOIN Customers C ON O.CustomerID = C.CustomerID
    INNER JOIN Shippers S ON O.ShipVia = S.ShipperID
    INNER JOIN [Order Details] OD ON O.OrderID = OD.OrderID
    INNER JOIN [Order Subtotals] OS ON O.OrderID = OS.OrderID
WHERE
    O.EmployeeID = ISNULL(NULLIF(@EmployeeID, ''), O.EmployeeID) AND
    O.CustomerID = ISNULL(NULLIF(@CustomerID, ''), O.CustomerID) AND
    O.OrderDate BETWEEN @StartDate AND @EndDate
GROUP BY
    O.OrderDate,
    CONCAT(E.TitleOfCourtesy, ' ', E.FirstName, ' ', E.LastName),
    C.CompanyName,
    S.CompanyName
go


