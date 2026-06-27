-- Parking Lot*******
-- *                *
-- *                *
--- *****************



-- Comment can be done single line with --
-- Comment can be done multi line with /* */

/*
DQL - Data Query Language
Keywords:

SELECT - retrieve data, select the columns from the resulting set
FROM - the table(s) to retrieve data from
WHERE - a conditional filter of the data
GROUP BY - group the data based on one or more columns
HAVING - a conditional filter of the grouped data
ORDER BY - sort the data
*/

USE Chinook_AutoIncrement;
GO
-- BASIC CHALLENGES
-- List all customers (full name, customer id, and country) who are not in the USA
SELECT FirstName + ' ' + LastName AS FullName, CustomerId, Country FROM dbo.Customer WHERE Country NOT IN ('USA');
-- List all customers from Brazil
SELECT *
FROM dbo.Customer
WHERE Country IN ('Brazil');

-- List all sales agents
SELECT * FROM dbo.Employee WHERE Title LIKE '%Sales%';

-- SELECT * FROM employee WHERE title LIKE '%Agent%;


-- Retrieve a list of all countries in billing addresses on invoices
SELECT DISTINCT BillingCountry FROM dbo.Invoice;


-- Retrieve how many invoices there were in 2009, and what was the sales total for that year?
SELECT COUNT(*) AS Invoices, SUM(Total) AS SalesTotal FROM dbo.Invoice WHERE YEAR(InvoiceDate) = 2021;

-- (challenge: find the invoice count sales total for every year using one query)
SELECT YEAR(InvoiceDate), SUM(Total) AS SalesYear  FROM dbo.Invoice GROUP BY YEAR(InvoiceDate) ORDER BY YEAR(InvoiceDate) ASC;

-- how many line items were there for invoice #37
SELECT COUNT(*) AS LineItems FROM dbo.InvoiceLine WHERE InvoiceId = 37; 

-- how many invoices per country? BillingCountry  # of invoices 
SELECT BillingCountry, COUNT(*) AS CountInvoices FROM Invoice GROUP BY BillingCountry;

-- Retrieve the total sales per country, ordered by the highest total sales first.
SELECT BillingCountry, SUM(Total) AS CountrySales FROM Invoice GROUP BY BillingCountry ORDER BY SUM(Total) DESC;

-- JOINS CHALLENGES
-- Every Album by Artist
SELECT ar.Name Artist, al.Title Album 
FROM Album al
JOIN Artist ar ON al.ArtistId = ar.ArtistId ORDER BY ar.Name; 

-- (inner keyword is optional for inner join)

-- All songs of the rock genre
SELECT s.Name 
FROM dbo.Track s 
JOIN dbo.Genre g ON s.GenreId = g.GenreId
WHERE g.Name LIKE 'Rock';

-- Show all invoices of customers from brazil (mailing address not billing)
SELECT i.InvoiceId, i.CustomerId, c.Country FROM Customer c
JOIN Invoice i ON c.CustomerId = i.CustomerId
WHERE c.Country LIKE 'Brazil';

-- Show all invoices together with the name of the sales agent for each one
SELECT i.InvoiceId, e.FirstName
FROM dbo.Invoice i 
JOIN Customer c ON i.CustomerId = c.CustomerId
JOIN Employee e ON c.SupportRepId = e.EmployeeId
WHERE e.Title LIKE '%Sales%'
ORDER BY i.InvoiceId;

-- Which sales agent made the most sales in 2021?
SELECT TOP 1 e.FirstName, SUM(i.Total) AS TOtalSales
FROM dbo.Invoice i
    JOIN Customer c ON i.CustomerId = c.CustomerId
    JOIN Employee e ON c.SupportRepId = e.EmployeeId
WHERE e.Title LIKE '%Sales%' AND YEAR(i.InvoiceDate) = 2021
GROUP BY e.FirstName
ORDER BY SUM(i.Total) DESC;

-- How many customers are assigned to each sales agent?
SELECT e.FirstName, COUNT(*) AS Customers
FROM Employee e 
JOIN Customer c ON e.EmployeeId = c.SupportRepId
GROUP BY e.FirstName;

-- Which track was purchased the most in 2021?
SELECT TOP 1 t.Name, SUM(il.Quantity) Purchases
FROM Track t 
JOIN InvoiceLine il ON t.TrackId = il.TrackId
JOIN Invoice i ON il.InvoiceId = i.InvoiceId
WHERE YEAR(i.InvoiceDate) = 2021
GROUP BY t.Name
ORDER BY SUM(il.Quantity) DESC;


-- Show the top three best selling artists.
SELECT TOP 3
    a.Name, SUM(il.Quantity) ArtistSales
FROM Track t
    JOIN Album al ON t.AlbumId = al.AlbumId
    JOIN Artist a ON al.ArtistId = a.ArtistId 
    JOIN InvoiceLine il ON t.TrackId = il.TrackId
    JOIN Invoice i ON il.InvoiceId = i.InvoiceId
GROUP BY a.Name
ORDER BY SUM(il.Quantity) DESC;

-- Which customers have the same initials as at least one other customer?
SELECT 
c1.FirstName + ' ' + c1.LastName AS Customer1,
(SUBSTRING(c1.FirstName,1,1) + SUBSTRING(c1.LastName,1,1)) AS Initials, 
c2.FirstName + ' ' + c2.LastName  AS Customer2
FROM Customer c1
CROSS JOIN Customer c2
WHERE 
(SUBSTRING(c1.FirstName,1,1) + SUBSTRING(c1.LastName,1,1)) = 
(SUBSTRING(c2.FirstName,1,1) + SUBSTRING(c2.LastName,1,1)) 
AND c1.CustomerId  != c2.CustomerId;


-- Which countries have the most invoices?
SELECT TOP 5 BillingCountry, COUNT(Total) AS Invoices
FROM dbo.Invoice
GROUP BY BillingCountry
ORDER BY COUNT(Total) DESC


-- Which city has the customer with the highest sales total?
SELECT City 
FROM dbo.Customer 
WHERE CustomerId = (
    SELECT TOP 1 i.CustomerId
    FROM dbo.Invoice i
    GROUP BY i.CustomerId
    ORDER BY MAX(i.Total) DESC
);
-- Who is the highest spending customer?
SELECT FirstName + ' ' + LastName AS Customer
FROM dbo.Customer
WHERE CustomerId = (
    SELECT TOP 1
    i.CustomerId
    FROM dbo.Invoice i
    GROUP BY i.CustomerId
    ORDER BY SUM(i.Total) DESC
);

-- Return the email and full name of of all customers who listen to Rock.
SELECT DISTINCT c.Email, (c.FirstName + ' ' + c.LastName) AS FullName
FROM dbo.Customer c
JOIN dbo.Invoice i ON c.CustomerId = i.CustomerId
JOIN dbo.InvoiceLine il ON i.InvoiceId = il.InvoiceId
JOIN dbo.Track t ON il.TrackId = t.TrackId
JOIN dbo.Genre g ON t.GenreId = g.GenreId
WHERE g.Name LIKE 'Rock';

-- Which artist has written the most Rock songs?
SELECT TOP 1 a.Name, COUNT(*) AS RockSongs
FROM Artist a
JOIN Album ab ON a.ArtistId = ab.ArtistId
JOIN dbo.Track t ON ab.AlbumId = t.AlbumId
JOIN dbo.Genre g ON t.GenreId = g.GenreId
WHERE g.Name LIKE 'Rock'
GROUP BY a.Name
ORDER BY COUNT(*) DESC;

-- Which artist has generated the most revenue?
SELECT TOP 1
    a.Name, SUM(i.Total) Revenue
FROM Artist a
    JOIN Album al ON a.ArtistId = al.ArtistId
    JOIN Track t ON al.AlbumId = t.AlbumId
    JOIN InvoiceLine il ON t.TrackId = il.TrackId
    JOIN Invoice i ON il.InvoiceId = i.InvoiceId
GROUP BY a.Name
ORDER BY SUM(i.Total) DESC;



-- ADVANCED CHALLENGES
-- solve these with a mixture of joins, subqueries, CTE, and set operators.
-- solve at least one of them in two different ways, and see if the execution
-- plan for them is the same, or different.

-- 1. which artists did not make any albums at all?
SELECT a.ArtistId, a.Name
FROM dbo.Artist a
WHERE a.ArtistId NOT IN
(SELECT DISTINCT a.ArtistId
FROM dbo.Artist a
    JOIN Album ab ON a.ArtistId = ab.ArtistId);

SELECT a.ArtistId, a.Name
FROM Artist a
LEFT JOIN dbo.Album ab ON a.ArtistId = ab.ArtistId
WHERE ab.AlbumId IS NULL;

-- 2. which artists did not record any tracks of the Latin genre?
SELECT DISTINCT a.Name, g.Name
FROM dbo.Artist a
JOIN dbo.Album ab ON a.ArtistId = ab.ArtistId
JOIN dbo.Track t ON ab.AlbumId = t.AlbumId
JOIN dbo.Genre g ON t.GenreId = g.GenreId
WHERE g.Name NOT LIKE 'Latin';

-- 3. which video track has the longest length? (use media type table)
SELECT TOP 1 sq.TrackId, sq.Milliseconds  
FROM 
(SELECT t.TrackId, t.Milliseconds 
FROM dbo.Track t
JOIN dbo.MediaType mt ON t.MediaTypeId = mt.MediaTypeId
WHERE mt.Name LIKE '%Video%') AS sq
ORDER BY sq.Milliseconds DESC;
-- 4. boss employee (the one who reports to nobody)
SELECT EmployeeId, LastName, FirstName  FROM dbo.Employee WHERE ReportsTo IS NULL;

-- 5. how many audio tracks were bought by German customers, and what was
--    the total price paid for them?
SELECT COUNT (*) AS German_Customers_Audio_Total_Purchases
FROM dbo.Invoice i
    JOIN dbo.Customer c ON i.CustomerId = c.CustomerId
    JOIN dbo.InvoiceLine il ON i.InvoiceId = il.InvoiceId
    JOIN dbo.Track t ON il.TrackId = t.TrackId
    JOIN dbo.MediaType mt ON t.MediaTypeId = mt.MediaTypeId
WHERE c.Country LIKE 'Germany' AND mt.Name LIKE '%Audio%'


-- 6. list the names and countries of the customers supported by an employee
--    who was hired younger than 45.
SELECT c.FirstName, c.LastName, c.Country
FROM dbo.Customer c
    JOIN dbo.Employee e ON c.SupportRepId = e.EmployeeId
WHERE DATEDIFF(year, e.BirthDate, GETDATE()) < 55

-- DML exercises

-- 1. insert two new records into the employee table.

-- 2. insert two new records into the tracks table.

-- 3. update customer Aaron Mitchell's name to Robert Walter

-- 4. delete one of the employees you inserted.

-- 5. delete customer Robert Walter.