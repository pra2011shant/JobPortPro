# JobPortPro - Security & Production Deployment Guide

This guide covers the security architecture of **JobPortPro** and step-by-step instructions for deploying to **IIS (Internet Information Services)** and **Microsoft Azure**.

---

## 🔒 Security Architecture & Hardening

### 1. Authentication & Authorization
- **Cookie Authentication**: Configured with `SlidingExpiration = true`, 7-day persistent lifetime, and `Cookie.HttpOnly = true`.
- **Role-Based Access Control (RBAC)**:
  - `[Authorize(Roles = "JobSeeker")]` restricts candidate portal pages.
  - `[Authorize(Roles = "Employer")]` restricts job postings, editing, and candidate review ATS portals.
  - Unauthenticated access automatically redirects to `/Account/Login?ReturnUrl=...`.
  - Forbidden roles redirect to `/Account/AccessDenied`.

### 2. SQL Injection Prevention
- All database queries are constructed using **Entity Framework Core parameterized LINQ queries**.
- No raw string concatenations or dynamic SQL are used, ensuring complete immunity against SQL injection attacks.

### 3. CSRF & XSS Protection
- **Anti-Forgery Tokens**: Every mutating POST request utilizes `@Html.AntiForgeryToken()` and `[ValidateAntiForgeryToken]`.
- **Automatic HTML Encoding**: Razor view engine automatically encodes user inputs to mitigate Cross-Site Scripting (XSS).

### 4. File Upload Validation
- Uploaded resumes and avatars are restricted to safe extensions (`.pdf`, `.doc`, `.docx`, `.png`, `.jpg`).
- Uploaded files are stored with cryptographically unique filenames (`Guid.NewGuid()_filename`) to prevent path traversal.

---

## 🖥️ Deploying to IIS (Windows Server / Local IIS)

### Step 1: Install Prerequisites on Windows Server
1. Enable **IIS (Internet Information Services)** via Windows Features.
2. Download and install the [.NET 8 Hosting Bundle](https://dotnet.microsoft.com/download/dotnet/8.0) (includes the .NET Runtime and `AspNetCoreModuleV2`).

### Step 2: Publish the Application
Run the publish command from the project root:
```powershell
dotnet publish -c Release -o ./publish
```

### Step 3: Configure IIS Site
1. Open **IIS Manager** (`inetmgr`).
2. Right-click **Sites** -> **Add Website...**
   - **Site name**: `JobPortPro`
   - **Physical path**: `E:\Work\JobPortPro\JobPortPro\publish` (or your destination folder)
   - **Binding**: `http` on Port `80` (or `8080`, or configure SSL with port `443`).
3. Click **Application Pools**:
   - Find the `JobPortPro` pool.
   - Click **Basic Settings** -> Set **.NET CLR Version** to **No Managed Code**.
4. Set Folder Permissions:
   - Grant `IIS_IUSRS` and `IUSR` **Read & Execute** permissions on the publish folder.
   - Grant `Write` permissions to `publish/wwwroot/uploads` for resume uploads.

### Step 4: SQL Server Permissions
Ensure the IIS Application Pool identity (e.g. `IIS AppPool\JobPortPro` or `NT AUTHORITY\SYSTEM` or SQL Auth user) has `db_datareader`, `db_datawriter`, and `db_ddladmin` permissions on the `JobPortPro` database.

---

## ☁️ Deploying to Microsoft Azure

### Option A: Azure App Service (Recommended)
1. **Create Azure Resources**:
   - Create an **Azure App Service** (Linux or Windows, .NET 8 runtime stack).
   - Create an **Azure SQL Database** and obtain the connection string.
2. **Configure App Settings in Azure Portal**:
   - Navigate to your App Service -> **Configuration** -> **Connection strings**.
   - Add name: `DefaultConnection`, Value: `Server=tcp:yourserver.database.windows.net,1433;Database=JobPortPro;User ID=...;Password=...;Encrypt=True;`
3. **Deploy via GitHub Actions or Visual Studio**:
   - Link your GitHub repository `https://github.com/pra2011shant/JobPortPro` directly to Azure Deployment Center for automated CI/CD.

---

## 🧪 Verification & Health Check

1. **Verify Unauthenticated Redirect**:
   - Open browser in Incognito mode.
   - Type: `http://localhost:5246/Employer/Dashboard`
   - **Result**: Automatically redirected to `http://localhost:5246/Account/Login?ReturnUrl=%2FEmployer%2FDashboard`.

2. **Verify Role Security**:
   - Log in as Job Seeker (`rahul.verma@example.com` / `Password123!`).
   - Try navigating to `/Employer/PostJob`.
   - **Result**: Automatically redirected to `/Account/AccessDenied`.
