# Mini Equipment Marketplace

A comprehensive ASP.NET Core 9.0 MVC application for equipment marketplace with vendor, shopper, and admin functionality. Deployed on Render.com with PostgreSQL database and Postmark email integration.

## 🚀 Quick Deploy to Render.com

[![Deploy to Render](https://render.com/images/deploy-to-render-button.svg)](https://render.com)

**See [RENDER_DEPLOYMENT.md](RENDER_DEPLOYMENT.md) for complete deployment guide.**

### Quick Start:
1. Push this repo to GitHub
2. Connect to Render.com
3. Render auto-detects `render.yaml`
4. Add `POSTMARK_SERVER_TOKEN` environment variable
5. Deploy! 🎉

---

## 🏗️ Architecture

### Technology Stack
- **Framework**: ASP.NET Core 9.0 MVC
- **Authentication**: ASP.NET Core Identity with roles (Admin, Vendor, Shopper)
- **Database**: PostgreSQL 15
- **Email Service**: Postmark
- **Hosting**: Render.com (Docker)
- **Container**: Multi-stage Docker build

### Project Structure

```
MiniEquipmentMarketplace/
├── Controllers/          # MVC Controllers
│   ├── Api/             # API endpoints
│   ├── Equipment/       # Equipment CRUD
│   ├── Vendors/         # Vendor management
│   └── Home/            # Public pages
├── Models/              # Data models
├── Views/               # Razor views
├── Data/                # Database context
├── Services/            # Email service
├── Areas/Identity/      # Authentication pages
├── wwwroot/             # Static files (CSS, JS, images)
├── Dockerfile           # Docker configuration
└── render.yaml          # Render.com Blueprint
```

---

## ✨ Key Features

### User Management
- **Role-Based Access Control**: Admin, Vendor, Shopper roles
- **Secure Authentication**: ASP.NET Core Identity
- **Email Verification**: Postmark integration
- **Password Reset**: Secure token-based system

### Equipment Marketplace
- **CRUD Operations**: Create, Read, Update, Delete equipment
- **Vendor Management**: Vendors can manage their listings
- **Search & Filter**: Find equipment easily
- **Responsive Design**: Works on all devices

### Admin Features
- Manage all equipment listings
- Manage vendors and users
- View system statistics
- Role assignment

### Technical Features
- **RESTful API**: Swagger documentation included
- **Database Migrations**: Automatic on deployment
- **Seed Data**: Auto-populated demo data
- **Logging**: Console logging for debugging
- **Health Checks**: Built-in monitoring

---

## 🛠️ Local Development

### Prerequisites
- .NET 9 SDK
- PostgreSQL 15+
- Docker (optional)

### Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/Subramanyam6/MiniEquipmentMarketplace.git
   cd MiniEquipmentMarketplace
   ```

2. **Start PostgreSQL**
   ```bash
   # Using Docker
   docker run -d \
     --name postgres \
     -e POSTGRES_PASSWORD=postgres \
     -e POSTGRES_DB=equipmentmarketplace \
     -p 5432:5432 \
     postgres:15
   
   # Or install PostgreSQL locally
   ```

3. **Restore dependencies**
   ```bash
   dotnet restore
   ```

4. **Run migrations**
   ```bash
   dotnet ef database update
   ```

5. **Run the application**
   ```bash
   dotnet run
   ```

6. **Access the app**
   - Navigate to `https://localhost:5001` or `http://localhost:5000`

---

## 🔑 Default Credentials

After first run, use these credentials:

- **Admin**: `admin@demo.com` / `P@ssw0rd!`
- **Dual Role User**: `dual@demo.com` / `P@ssw0rd!` (Vendor + Shopper)

**⚠️ Change these passwords in production!**

---

## 📧 Email Configuration

### Where to Add Your Postmark Token

**File**: `appsettings.json` (for local development)

```json
{
  "EmailSettings": {
    "ServerToken": "YOUR_POSTMARK_TOKEN_HERE",
    "FromName": "Equipment Marketplace",
    "FromEmail": "your-verified-email@example.com"
  }
}
```

**For Render.com Production**:
- Add `POSTMARK_SERVER_TOKEN` as environment variable in Render dashboard
- See [RENDER_DEPLOYMENT.md](RENDER_DEPLOYMENT.md) for details

### Get Postmark Token
1. Sign up at [Postmark](https://postmarkapp.com/)
2. Create a server
3. Copy the **Server API Token**
4. Verify your sender email address

---

## 🐳 Docker

### Build and Run Locally

```bash
# Build image
docker build -t equipment-marketplace .

# Run container
docker run -d \
  -p 8080:8080 \
  -e DATABASE_URL="postgres://user:pass@host:5432/db" \
  -e POSTMARK_SERVER_TOKEN="your-token" \
  equipment-marketplace
```

### Docker Compose (Coming Soon)

---

## 🧪 API Documentation

Swagger UI is available at:
- **Local**: `http://localhost:5000/swagger`
- **Production**: `https://your-app.onrender.com/swagger`

### Sample API Endpoints

```
GET    /api/equipment          # List all equipment
GET    /api/equipment/{id}     # Get equipment by ID
POST   /api/equipment          # Create equipment (Auth required)
PUT    /api/equipment/{id}     # Update equipment (Auth required)
DELETE /api/equipment/{id}     # Delete equipment (Auth required)
```

---

## 📊 Database Schema

### Main Tables

**Equipment**
- Id, Title, Description, Price, VendorId, CreatedAt

**Vendors**
- Id, Name, Email, CreatedAt

**AspNetUsers** (Identity)
- Id, UserName, Email, PasswordHash, etc.

**AspNetRoles** (Identity)
- Id, Name (Admin, Vendor, Shopper)

---

## 🚀 Deployment

### Render.com (Recommended)

See [RENDER_DEPLOYMENT.md](RENDER_DEPLOYMENT.md) for complete guide.

**TL;DR**:
1. Push to GitHub
2. Connect to Render
3. Apply Blueprint
4. Done!

### Other Platforms

The Docker setup works on:
- **Railway**: Use Dockerfile
- **Fly.io**: Use Dockerfile
- **Heroku**: Use Dockerfile + Heroku Postgres
- **DigitalOcean App Platform**: Use Dockerfile

---

## 🔧 Configuration Files

### `render.yaml`
Infrastructure-as-Code for Render.com deployment. Defines:
- Web service configuration
- PostgreSQL database
- Environment variables
- Auto-deploy settings

### `appsettings.json`
Local development configuration:
- Database connection string
- Logging levels
- Email settings

### `appsettings.Production.json`
Production overrides:
- Production logging levels
- Email configuration (token from env var)

### `Dockerfile`
Multi-stage Docker build:
1. Build stage: Compile .NET application
2. Runtime stage: Run with minimal ASP.NET runtime

---

## 🧹 Project Cleanup

This project has been cleaned of:
- ❌ GCP Cloud Run configurations
- ❌ Azure App Service configurations
- ❌ Unused deployment scripts
- ❌ Legacy connection string formats

Now optimized for:
- ✅ Render.com deployment
- ✅ Docker-first approach
- ✅ Standard PostgreSQL connections
- ✅ Environment variable configuration

---

## 🤝 Contributing

Contributions are welcome! Please:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

---

## 📝 License

This project is licensed under the MIT License - see the LICENSE file for details.

---

## 🙏 Acknowledgments

- Sandhills Global for the inspiration
- ASP.NET Core team for the excellent framework
- Render.com for free hosting
- All open-source contributors

---

## 📞 Contact

**Bala Subramanyam**
- Email: bduggirala2@huskers.unl.edu
- GitHub: [@Subramanyam6](https://github.com/Subramanyam6)

**Project Link**: [https://github.com/Subramanyam6/MiniEquipmentMarketplace](https://github.com/Subramanyam6/MiniEquipmentMarketplace)

---

## 🎯 Roadmap

- [ ] Add shopping cart functionality
- [ ] Implement payment processing
- [ ] Add image upload for equipment
- [ ] Real-time chat between vendors and shoppers
- [ ] Advanced search and filtering
- [ ] Equipment reviews and ratings
- [ ] Mobile app (React Native)

---

**Made with ❤️ using ASP.NET Core 9.0**
