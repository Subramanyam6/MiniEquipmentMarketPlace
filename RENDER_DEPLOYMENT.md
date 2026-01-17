# Render.com Deployment Guide

## 🚀 Quick Start - Deploy to Render.com

This application is configured to deploy to Render.com using Docker and Infrastructure-as-Code via `render.yaml`.

### Prerequisites
- GitHub account
- Render.com account (free tier available)
- Git repository with this code

---

## 📋 Step-by-Step Deployment

### 1. Push Code to GitHub

```bash
git add .
git commit -m "Prepare for Render deployment"
git push origin main
```

### 2. Connect to Render.com

1. Go to [Render Dashboard](https://dashboard.render.com/)
2. Click **"New +"** → **"Blueprint"**
3. Connect your GitHub repository
4. Render will automatically detect `render.yaml`

### 3. Configure Environment Variables

Render will create two services automatically:
- **Web Service**: `equipment-marketplace`
- **PostgreSQL Database**: `equipment-marketplace-db`

#### Required Environment Variable to Add Manually:

**POSTMARK_SERVER_TOKEN** (Email Service)
- Navigate to your web service in Render dashboard
- Go to **Environment** tab
- Add environment variable:
  - **Key**: `POSTMARK_SERVER_TOKEN`
  - **Value**: Your Postmark API token (get from [Postmark](https://postmarkapp.com/))
  - Click **"Save Changes"**

> **Note**: If you don't have a Postmark account yet, you can skip this initially. The app will work without email functionality, but user registration emails won't be sent.

### 4. Deploy

1. Click **"Apply"** to provision all resources
2. Render will:
   - Create PostgreSQL database
   - Build Docker image from your Dockerfile
   - Deploy web service
   - Connect services automatically
3. Wait 5-10 minutes for initial deployment

### 5. Access Your Application

Once deployed, Render provides a URL like:
```
https://equipment-marketplace-xxxx.onrender.com
```

---

## 🔧 Configuration Details

### Architecture Overview

```
┌─────────────────────────────────────┐
│   Render Web Service (Free Tier)   │
│   - Docker Container                │
│   - ASP.NET Core 9.0                │
│   - Port 8080                       │
│   - Auto-deploy on git push         │
└──────────────┬──────────────────────┘
               │
               │ DATABASE_URL
               │ (Auto-configured)
               ▼
┌─────────────────────────────────────┐
│  Render PostgreSQL (Free Tier)     │
│  - Managed Database                 │
│  - Automatic backups                │
│  - 90-day retention (free tier)     │
└─────────────────────────────────────┘
```

### Environment Variables

The application automatically reads these environment variables:

| Variable | Source | Description |
|----------|--------|-------------|
| `DATABASE_URL` | Auto-set by Render | PostgreSQL connection string |
| `ASPNETCORE_ENVIRONMENT` | Set in render.yaml | Set to "Production" |
| `POSTMARK_SERVER_TOKEN` | **You must set manually** | Email service API key |

### Database Connection

The app automatically parses Render's `DATABASE_URL` format:
```
postgres://user:password@host:port/database
```

And converts it to Npgsql format with SSL enabled.

---

## 📧 Setting Up Email (Postmark)

### Option 1: Use Postmark (Recommended)

1. Sign up at [Postmark](https://postmarkapp.com/)
2. Get your **Server API Token**
3. Add to Render environment variables:
   - Key: `POSTMARK_SERVER_TOKEN`
   - Value: Your token

### Option 2: Skip Email Setup

The app will work without email, but:
- User registration will still work
- Password reset won't work
- No email notifications

---

## 🎯 Default Admin Account

After first deployment, the app automatically creates:

- **Email**: `admin@demo.com`
- **Password**: `P@ssw0rd!`

**⚠️ IMPORTANT**: Change this password immediately after first login!

---

## 🔄 Continuous Deployment

Every time you push to the `main` branch:
1. Render automatically detects changes
2. Builds new Docker image
3. Deploys updated application
4. Zero-downtime deployment

To disable auto-deploy:
- Edit `render.yaml` and set `autoDeploy: false`

---

## 💰 Free Tier Limitations

### Web Service (Free)
- ✅ 750 hours/month (enough for 24/7 if single project)
- ⚠️ Spins down after 15 minutes of inactivity
- ⚠️ Cold start: 30-60 seconds to wake up
- ⚠️ 512 MB RAM limit

### PostgreSQL (Free)
- ✅ 1 GB storage
- ✅ Automatic backups
- ⚠️ **Expires after 90 days** (must upgrade or migrate)
- ⚠️ Limited concurrent connections

### Upgrade Options
- **Starter Plan**: $7/month (no cold starts, more resources)
- **PostgreSQL Starter**: $7/month (persistent database)

---

## 🐛 Troubleshooting

### Application Won't Start

1. Check logs in Render dashboard
2. Verify `DATABASE_URL` is set (should be automatic)
3. Check Docker build succeeded

### Database Connection Errors

```bash
# In Render Shell (Web Service → Shell tab):
echo $DATABASE_URL
```

Should output something like:
```
postgres://user:pass@host:5432/dbname
```

### Cold Start Issues

Free tier spins down after 15 minutes. First request after idle will be slow (30-60s).

**Solutions**:
- Upgrade to paid plan ($7/month)
- Use a uptime monitoring service to ping every 10 minutes
- Accept the limitation for development/demo projects

### Email Not Sending

1. Verify `POSTMARK_SERVER_TOKEN` is set in Render dashboard
2. Check Postmark dashboard for send attempts
3. Verify sender email is verified in Postmark

---

## 📊 Monitoring & Logs

### View Logs
1. Go to Render Dashboard
2. Select your web service
3. Click **"Logs"** tab
4. Real-time log streaming

### Metrics
- CPU usage
- Memory usage
- Request count
- Response times

Available in the **"Metrics"** tab.

---

## 🔒 Security Recommendations

### Before Going to Production:

1. **Change Default Admin Password**
   - Login with `admin@demo.com` / `P@ssw0rd!`
   - Change password immediately

2. **Set Strong Database Password**
   - Render generates this automatically
   - Don't share `DATABASE_URL`

3. **Enable HTTPS** (Automatic)
   - Render provides free SSL certificates
   - All traffic is encrypted

4. **Environment Variables**
   - Never commit secrets to Git
   - Use Render's environment variable system

---

## 🔄 Database Migrations

Migrations run automatically on startup. The app will:
1. Check for pending migrations
2. Apply them automatically
3. Seed initial data (vendors, equipment, admin user)

To manually run migrations:
```bash
# In Render Shell:
dotnet ef database update
```

---

## 📦 Local Development

### Run Locally with Docker

```bash
# Start PostgreSQL
docker run -d \
  --name postgres \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_DB=equipmentmarketplace \
  -p 5432:5432 \
  postgres:15

# Run application
dotnet run
```

### Run Locally without Docker

1. Install PostgreSQL locally
2. Update `appsettings.json` connection string
3. Run: `dotnet run`

---

## 🆘 Support & Resources

- **Render Documentation**: https://render.com/docs
- **Render Community**: https://community.render.com/
- **ASP.NET Core Docs**: https://docs.microsoft.com/aspnet/core/
- **PostgreSQL Docs**: https://www.postgresql.org/docs/

---

## 📝 Summary Checklist

- [ ] Push code to GitHub
- [ ] Connect repository to Render
- [ ] Apply Blueprint (render.yaml)
- [ ] Add `POSTMARK_SERVER_TOKEN` environment variable
- [ ] Wait for deployment to complete
- [ ] Access application URL
- [ ] Login with admin account
- [ ] Change default password
- [ ] Test functionality

---

**🎉 Your application is now live on Render.com!**

For questions or issues, check the Render logs first, then consult the troubleshooting section above.
