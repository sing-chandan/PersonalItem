# InventoryProc - Quick Start Guide

## 🚀 Get Started in 3 Steps

### Prerequisites
- ✅ .NET 9.0 SDK installed
- ✅ Node.js 22.x installed
- ✅ SQL Server LocalDB (comes with Visual Studio)

---

## Step 1: Start the Backend API

Open a terminal and run:

```bash
cd c:\PersonalItem\InventoryProc\backend\src\Host\InventoryProc.API
dotnet run
```

**Expected Output:**
```
[INF] InventoryProc API starting...
[INF] Now listening on: http://localhost:5005
[INF] Application started.
```

✅ Backend running at: **http://localhost:5005**

---

## Step 2: Start the Frontend

Open a **NEW terminal** and run:

```bash
cd c:\PersonalItem\InventoryProc\frontend
npm start
```

**Expected Output:**
```
✔ Building...
➜ Local:   http://localhost:4200/
```

✅ Frontend running at: **http://localhost:4200**

---

## Step 3: Create Your Account

1. **Open your browser**: http://localhost:4200

2. **Click "Sign Up"** (or navigate to `/register`)

3. **Fill in the registration form:**
   ```
   Company Name:    My Business Inc
   First Name:      John
   Last Name:       Doe
   Email:           john@mybusiness.com
   Mobile:          +1234567890 (optional)
   Password:        MySecure@123
   Confirm Password: MySecure@123
   ```

4. **Click "Create Account"**

5. **You're in!** 🎉
   - Automatically logged in
   - Redirected to dashboard
   - Your tenant and admin user are created

---

## 🎮 Using the Application

### Login Next Time
1. Go to http://localhost:4200/login
2. Enter your email and password
3. Click "Sign In"

### Dashboard
- View your account information
- See your tenant ID
- Check your role (Admin)
- Logout button in top-right

### Logout
- Click "Logout" button in dashboard
- Tokens cleared automatically
- Redirected to login page

---

## 🔧 Troubleshooting

### Backend Issues

**Problem:** `Cannot open database "InventoryProc"`
```bash
# Solution: Apply migrations
cd c:\PersonalItem\InventoryProc\backend\src\BuildingBlocks\InventoryProc.Infrastructure
dotnet ef database update --startup-project "../../Host/InventoryProc.API/InventoryProc.API.csproj"
```

**Problem:** Port 5005 already in use
```bash
# Solution: Stop other applications using port 5005
# Or change port in appsettings.json
```

### Frontend Issues

**Problem:** `Port 4200 is already in use`
```bash
# Solution: Kill existing process or use different port
npm start -- --port 4300
```

**Problem:** `Cannot connect to backend`
- Check backend is running on http://localhost:5005
- Check CORS is enabled in backend
- Check firewall/antivirus not blocking

### Login Issues

**Problem:** "Email already registered"
- Email must be unique globally
- Try a different email address

**Problem:** "Invalid credentials"
- Check email and password are correct
- Password is case-sensitive

---

## 📚 API Documentation

### Swagger UI
Once backend is running, visit:
**http://localhost:5005/swagger**

### Available Endpoints

```
POST /api/auth/register
POST /api/auth/login
POST /api/auth/refresh
POST /api/auth/logout
GET  /api/auth/me
```

---

## 🗄️ Database

### View Database
1. Open Visual Studio or Azure Data Studio
2. Connect to: `(localdb)\mssqllocaldb`
3. Database: `InventoryProc`
4. Tables: `Tenants`, `Users`

### Reset Database
```bash
cd c:\PersonalItem\InventoryProc\backend\src\BuildingBlocks\InventoryProc.Infrastructure
dotnet ef database drop --startup-project "../../Host/InventoryProc.API/InventoryProc.API.csproj"
dotnet ef database update --startup-project "../../Host/InventoryProc.API/InventoryProc.API.csproj"
```

---

## 🧪 Test Accounts

After you register, you can use these credentials:

```
Email:    john@mybusiness.com
Password: MySecure@123
Role:     Admin
```

Create more users by registering with different emails and company names. Each registration creates a new tenant!

---

## 🎯 What's Next?

Phase 1 (Authentication) is complete. Next features coming:

- ✅ User Authentication
- ✅ Multi-tenant Support
- ⏳ Inventory Management
- ⏳ Product Management
- ⏳ Sales Orders
- ⏳ Purchase Orders
- ⏳ Reports & Analytics

---

## 📁 Project Files

### Backend
```
c:\PersonalItem\InventoryProc\backend\
```

### Frontend
```
c:\PersonalItem\InventoryProc\frontend\
```

### Documentation
```
c:\PersonalItem\InventoryProc\Documentation\
```

---

## 💡 Tips

1. **Keep both terminals open** (backend + frontend)
2. **Use Chrome DevTools** to see API calls (Network tab)
3. **Check browser console** for frontend errors (F12)
4. **Check backend logs** in terminal for API errors
5. **Tokens stored in LocalStorage** - can view in DevTools (Application tab)

---

## 🆘 Need Help?

### Check Logs
- **Backend logs**: Terminal running `dotnet run`
- **Frontend logs**: Browser console (F12)
- **Database logs**: Enable in appsettings.json

### Common Commands

```bash
# Backend - Rebuild
cd c:\PersonalItem\InventoryProc\backend
dotnet clean
dotnet build

# Frontend - Reinstall dependencies
cd c:\PersonalItem\InventoryProc\frontend
rm -rf node_modules package-lock.json
npm install

# Backend - Run migrations
cd c:\PersonalItem\InventoryProc\backend\src\BuildingBlocks\InventoryProc.Infrastructure
dotnet ef database update --startup-project "../../Host/InventoryProc.API/InventoryProc.API.csproj"
```

---

## ✅ Checklist

- [ ] Backend running on http://localhost:5005
- [ ] Frontend running on http://localhost:4200
- [ ] Database created and migrated
- [ ] Registered a test account
- [ ] Logged in successfully
- [ ] Dashboard loading correctly
- [ ] Logout working

---

**Enjoy building with InventoryProc!** 🎉

For detailed documentation, see:
- `PHASE1_COMPLETE.md` - Full implementation details
- `frontend/README_IMPLEMENTATION.md` - Frontend documentation
- `Documentation/` - Project requirements and architecture
