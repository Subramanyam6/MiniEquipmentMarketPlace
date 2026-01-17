# Quick Start Guide

## 🚀 Deploy to Render.com in 5 Minutes

### Step 1: Push to GitHub
```bash
git add .
git commit -m "Deploy to Render"
git push origin main
```

### Step 2: Connect to Render
1. Go to https://dashboard.render.com/
2. Click **"New +"** → **"Blueprint"**
3. Connect your GitHub repository
4. Render detects `render.yaml` automatically

### Step 3: Add Email Token (Optional)
1. In Render dashboard, go to your web service
2. Click **"Environment"** tab
3. Add: `POSTMARK_SERVER_TOKEN` = `your-token-here`
4. Click **"Save Changes"**

### Step 4: Deploy
1. Click **"Apply"** in Render
2. Wait 5-10 minutes
3. Access your app URL!

---

## 📧 Where to Add Email Token

### For Local Development
**File**: `appsettings.json` (line 14)

```json
{
  "EmailSettings": {
    "ServerToken": "PUT_YOUR_POSTMARK_TOKEN_HERE",
    "FromName": "Equipment Marketplace",
    "FromEmail": "your-verified-email@example.com"
  }
}
```

### For Render Production
**Location**: Render Dashboard → Your Web Service → Environment Tab

Add environment variable:
- **Key**: `POSTMARK_SERVER_TOKEN`
- **Value**: Your Postmark API token

### Get Postmark Token
1. Sign up: https://postmarkapp.com/
2. Create a server
3. Copy **Server API Token**
4. Verify your sender email

---

## 🔑 Default Login

After deployment, login with:
- **Email**: `admin@demo.com`
- **Password**: `P@ssw0rd!`

**⚠️ Change this password immediately!**

---

## 📚 Full Documentation

- **[RENDER_DEPLOYMENT.md](RENDER_DEPLOYMENT.md)** - Complete deployment guide
- **[README.md](README.md)** - Project documentation
- **[MIGRATION_SUMMARY.md](MIGRATION_SUMMARY.md)** - All changes made
- **[USAGE_GUIDE.md](USAGE_GUIDE.md)** - User guide

---

## 🆘 Troubleshooting

### Build Failed?
Check Render logs for errors. Common issues:
- Missing environment variables
- Database connection issues

### Can't Login?
- Use `admin@demo.com` / `P@ssw0rd!`
- Check database was seeded (see logs)

### Email Not Working?
- Add `POSTMARK_SERVER_TOKEN` in Render dashboard
- Verify sender email in Postmark

---

## ✅ Checklist

- [ ] Code pushed to GitHub
- [ ] Connected to Render
- [ ] Applied Blueprint
- [ ] Added POSTMARK_SERVER_TOKEN (optional)
- [ ] Deployment successful
- [ ] Logged in with admin account
- [ ] Changed default password

---

**That's it! Your app is live! 🎉**
