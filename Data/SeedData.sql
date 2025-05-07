-- SQL Script to seed the database with existing data
-- Run this script on the target database after migrations

-- Disable constraints for bulk insert
EXEC sp_MSforeachtable "ALTER TABLE ? NOCHECK CONSTRAINT all"

-- Clear existing data (in reverse order of dependencies)
DELETE FROM [Equipment];
DELETE FROM [Vendors];
-- Uncomment the following if you want to reset identity users 
-- DELETE FROM [AspNetUserRoles];
-- DELETE FROM [AspNetRoles];
-- DELETE FROM [AspNetUsers];

-- Reset identity columns
DBCC CHECKIDENT ('[Equipment]', RESEED, 0);
DBCC CHECKIDENT ('[Vendors]', RESEED, 0);

-- Insert Vendors data
-- Replace the values below with your actual data from the existing database
-- You can get this data by running: SELECT * FROM [Vendors] in your local SQL Server
INSERT INTO [Vendors] ([Name], [Email], [CreatedAt])
VALUES 
('Vendor 1', 'vendor1@example.com', '2025-05-07 00:00:00'),
('Vendor 2', 'vendor2@example.com', '2025-05-07 00:00:00');
-- Add more vendors as needed

-- Insert Equipment data
-- Replace the values below with your actual data from the existing database
-- You can get this data by running: SELECT * FROM [Equipment] in your local SQL Server
INSERT INTO [Equipment] ([Title], [Description], [Price], [VendorId], [CreatedAt])
VALUES 
('Equipment 1', 'Description for equipment 1', 199.99, 1, '2025-05-07 00:00:00'),
('Equipment 2', 'Description for equipment 2', 299.99, 1, '2025-05-07 00:00:00'),
('Equipment 3', 'Description for equipment 3', 399.99, 2, '2025-05-07 00:00:00');
-- Add more equipment as needed

-- Re-enable constraints
EXEC sp_MSforeachtable "ALTER TABLE ? CHECK CONSTRAINT all"

-- Note: This is a template. You should generate the actual data by running the following commands in SSMS or Azure Data Studio:
-- 
-- SELECT 'INSERT INTO [Vendors] ([Name], [Email], [CreatedAt]) VALUES ' + 
--        CHAR(13) + CHAR(10) + 
--        STRING_AGG('(''' + REPLACE([Name], '''', '''''') + ''', ''' + REPLACE([Email], '''', '''''') + ''', ''' + CONVERT(VARCHAR, [CreatedAt], 120) + ''')', ',' + CHAR(13) + CHAR(10))
-- FROM [Vendors];
--
-- SELECT 'INSERT INTO [Equipment] ([Title], [Description], [Price], [VendorId], [CreatedAt]) VALUES ' + 
--        CHAR(13) + CHAR(10) + 
--        STRING_AGG('(''' + REPLACE([Title], '''', '''''') + ''', ''' + REPLACE([Description], '''', '''''') + ''', ' + CAST([Price] AS VARCHAR) + ', ' + CAST([VendorId] AS VARCHAR) + ', ''' + CONVERT(VARCHAR, [CreatedAt], 120) + ''')', ',' + CHAR(13) + CHAR(10))
-- FROM [Equipment]; 