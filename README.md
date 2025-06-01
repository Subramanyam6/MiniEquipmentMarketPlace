# Mini Equipment Marketplace

A comprehensive ASP.NET Core 9.0 MVC application for equipment marketplace with vendor, shopper, and admin functionality. Deployed on Google Cloud Platform with PostgreSQL database and SendGrid email integration.

## 🚀 Live Application

**Application URL**: https://equipment-marketplace-app-457614448632.us-central1.run.app

### Quick Access:
- **Admin Login**: admin@demo.com / P@ssw0rd!
- **Vendor Registration**: [Register as Vendor](https://equipment-marketplace-app-457614448632.us-central1.run.app/Identity/Account/Register?userType=Vendor)
- **Shopper Registration**: [Register as Shopper](https://equipment-marketplace-app-457614448632.us-central1.run.app/Identity/Account/Register?userType=Shopper)

## 🛠️ Deployment

### Automated Deployment Script

The project includes a comprehensive deployment script that handles building and deploying to Google Cloud Run:

```bash
# Set required environment variables
export DB_PASSWORD='your_database_password'
export SENDGRID_API_KEY='your_sendgrid_api_key'  # Optional

# Run deployment
./gcp-deploy.sh
```

The script will:
- ✅ Validate environment variables and gcloud authentication
- ✅ Build and push Docker image to Google Container Registry
- ✅ Deploy to Cloud Run with proper configuration
- ✅ Test the deployment and provide useful endpoints

### Manual Deployment

If you prefer manual deployment:

```bash
# Build and push Docker image
docker buildx build --platform linux/amd64 \
  -t us-central1-docker.pkg.dev/equipment-marketplace/equipment-marketplace-repo/equipment-marketplace-app:latest \
  --push .

# Deploy to Cloud Run
gcloud run deploy equipment-marketplace-app \
  --image us-central1-docker.pkg.dev/equipment-marketplace/equipment-marketplace-repo/equipment-marketplace-app:latest \
  --platform managed \
  --region us-central1 \
  --add-cloudsql-instances equipment-marketplace:us-central1:equipment-marketplace-db \
  --set-env-vars DB_PASSWORD='your_password',SENDGRID_API_KEY='your_api_key' \
  --port 8080 \
  --allow-unauthenticated
```

## 🏗️ Architecture

### Technology Stack
- **Framework**: ASP.NET Core 9.0 MVC
- **Authentication**: ASP.NET Core Identity with roles (Admin, Vendor, Shopper)
- **Database**: PostgreSQL on Google Cloud SQL
- **Email Service**: SendGrid
- **Hosting**: Google Cloud Run
- **Container**: Docker

### Project Structure

The application follows the MVC (Model-View-Controller) architectural pattern:

- **Models**: Represent the data structures and business logic
- **Views**: Responsible for rendering the UI using Razor syntax
- **Controllers**: Handle user requests, process data, and return responses

The project is structured to separate concerns and promote maintainability while keeping the codebase clean and organized.

## Key Features

- **User Role Management**: Support for multiple user roles (Admin, Vendor, Shopper) with role-specific functionalities and access control
- **Equipment Listings**: Vendors can create, edit, and manage equipment listings with images, descriptions, and pricing
- **User Authentication**: Secure authentication system with email verification and password reset functionality
- **Responsive Design**: Mobile-friendly interface that works across devices of all sizes
- **Real-time Visual Feedback**: Interactive UI elements with animations and transitions for improved user experience
- **Data Persistence**: SQL Server database with Entity Framework Core for reliable data storage and retrieval
- **Secure Communication**: Email notifications for account activities and transaction updates

## Technology Stack

- **Backend**:
  - ASP.NET Core MVC (.NET 9 & C#)
  - Entity Framework Core
  - ASP.NET Core Identity
  - SQL Server

- **Frontend**:
  - HTML5/CSS3
  - JavaScript
  - Bootstrap
  - jQuery
  - Particles.js
  - Font Awesome

- **DevOps & Infrastructure**:
  - Docker
  - Azure App Service & Azure SQL DB
  - Git/GitHub

## Getting Started

### Prerequisites

- .NET 9 SDK
- SQL Server (or SQL Server Express)
- Visual Studio 2022 or Visual Studio Code

### Installation

1. Clone the repository:
   ```
   git clone https://github.com/Subramanyam6/MiniEquipmentMarketplace.git
   ```

2. Navigate to the project directory:
   ```
   cd MiniEquipmentMarketplace
   ```

3. Restore dependencies:
   ```
   dotnet restore
   ```

4. Update the database connection string in `appsettings.json` to point to your SQL Server instance.

5. Apply database migrations:
   ```
   dotnet ef database update
   ```

6. Run the application:
   ```
   dotnet run
   ```

7. Open your browser and navigate to `https://localhost:5001` or `http://localhost:5000`.

## Usage

The application provides different functionalities based on user roles:

- **Admin**: Can manage all equipment listings, vendors, and users
- **Vendor**: Can create and manage their equipment listings
- **Shopper**: Can browse and purchase equipment

### Demo Accounts

For testing purposes, you can register new accounts or use these demo credentials:

- **Admin**: admin@example.com / Password123!
- **Vendor**: vendor@example.com / Password123!
- **Shopper**: shopper@example.com / Password123!

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

- Sandhills Global for the inspiration
- All the open-source libraries and tools that made this project possible

## Contact

Bala Subramanyam - bduggirala2@huskers.unl.edu

Project Link: [https://github.com/Subramanyam6/MiniEquipmentMarketplace](https://github.com/Subramanyam6/MiniEquipmentMarketplace) 