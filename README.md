# Mini Equipment Marketplace

A web application for an equipment marketplace built with ASP.NET Core MVC.

## Database Migration and Deployment Guide

This guide explains how to preserve your database data when pushing to GitHub and deploying to Azure.

### Option 1: Export SQL Data and Use the SQL Script

1. Export your current database data:
   - Use SQL Server Management Studio (SSMS) or Azure Data Studio
   - Connect to your local database
   - Right-click on your database and select "Tasks" > "Generate Scripts"
   - Follow the wizard, make sure to select "Schema and data" when asked what to script
   - Save the script to your project folder (e.g., as `DatabaseExport.sql`)

   Alternatively, you can execute the query in `Data/SeedData.sql` to generate INSERT statements for your data.

2. Push to GitHub:
   - Commit all your changes including the SQL script
   - Push to your GitHub repository

3. Deploy to Azure:
   - Deploy your application to Azure App Service
   - Connect to your Azure SQL Database
   - Run the SQL script to populate the database

### Option 2: Use DbInitializer for Programmatic Seeding

1. Update the DbInitializer.cs:
   - Open `Data/DbInitializer.cs`
   - Replace the sample data with your actual data in the vendors and equipment arrays
   - For more complex data, export it from your database to SQL (as in Option 1) and write the equivalent C# code

2. Push to GitHub:
   - Commit all your changes
   - Push to your GitHub repository

3. Deploy to Azure:
   - Deploy your application to Azure App Service
   - The DbInitializer will automatically run on application startup
   - If the database is empty, it will be populated with your data

### Option 3: Use Azure SQL Database Migrations

1. Ensure your local database schema matches your model:
   ```
   dotnet ef database update
   ```

2. Publish your database to Azure SQL:
   - Use SQL Server Management Studio
   - Right-click on your local database
   - Select "Tasks" > "Deploy Database to Microsoft Azure SQL Database"
   - Follow the wizard to deploy your database with data

3. Update your Azure App Service Connection String:
   - In the Azure Portal, go to your App Service
   - Navigate to Configuration > Connection Strings
   - Update the connection string to match your Azure SQL Database

## Useful Commands

- Generate a SQL script for your database:
  ```
  sqlcmd -S YOUR_SERVER -d YOUR_DATABASE -U YOUR_USERNAME -P YOUR_PASSWORD -Q "SELECT * FROM YOUR_TABLE" -o output.sql
  ```

- View database migration status:
  ```
  dotnet ef migrations list
  ```

- Create a new migration:
  ```
  dotnet ef migrations add MigrationName
  ```

- Apply migrations:
  ```
  dotnet ef database update
  ```

## Azure Deployment Checklist

- [ ] Update connection strings in appsettings.json or Azure App Service settings
- [ ] Set environment variables for sensitive information
- [ ] Configure authentication settings
- [ ] Setup HTTPS and SSL certificates
- [ ] Set up monitoring and logging
- [ ] Configure Azure SQL Database firewall rules
- [ ] Test the application after deployment 