# JobPortPro - Full-Featured Job Portal Web Application

JobPortPro is a modern, full-featured Job Portal web application built with **ASP.NET Core MVC (.NET 8)**, **Entity Framework Core**, **Microsoft SQL Server**, and **Bootstrap 5**.

---

## 🌟 Key Features

### For Job Seekers
- **Search & Filter Openings**: Search by keyword, industry category, job type (Full-Time, Remote, Part-Time, Contract, Internship), location, experience level, and salary range.
- **Job Details & Bookmarking**: Read comprehensive role requirements, save favorite jobs to bookmarks.
- **Application Submission**: Apply directly with tailored cover letters and resume uploads (or use saved profile resume).
- **Application Tracking**: Real-time status tracker (*Pending*, *Reviewed*, *Shortlisted*, *Accepted*, *Rejected*) and view feedback notes from employers.
- **Candidate Profile**: Custom portfolio page with skills tags, experience, education, GitHub, and LinkedIn links.

### For Employers
- **Employer Dashboard**: Visual analytics for active postings, total applications received, and shortlisted talent.
- **Job Postings Management**: Create, edit, activate/deactivate, and delete job postings.
- **Applicant Tracking System (ATS)**: Filter candidates by job and status, view candidate profile dossiers, download resumes, update pipeline stages, and add recruiter notes.
- **Company Profile**: Customize company branding, website, size, and mission.

---

## 🛠️ Technology Stack

- **Backend**: ASP.NET Core MVC (.NET 8)
- **Database & ORM**: Microsoft SQL Server Express (`DESKTOP-MMR6QJM\SQLEXPRESS`) + Entity Framework Core 8
- **Security**: Cookie Authentication & BCrypt Password Hashing
- **Frontend**: Bootstrap 5, FontAwesome 6, Google Fonts (Plus Jakarta Sans & Inter), Custom CSS

---

## 🚀 Getting Started

### 1. Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Microsoft SQL Server (e.g. SQL Server Express `DESKTOP-MMR6QJM\SQLEXPRESS`)

### 2. Configuration
Update the connection string in `appsettings.json` if needed:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=DESKTOP-MMR6QJM\\SQLEXPRESS;Database=JobPortPro;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;"
}
```

### 3. Run Application
The database and sample seed data are created automatically on startup:
```bash
dotnet run
```
Navigate to: `http://localhost:5246`

---

## 🔑 Demo Login Credentials

| Role | Email | Password |
|---|---|---|
| **Employer** | `employer@techsolutions.com` | `Password123!` |
| **Job Seeker** | `rahul.verma@example.com` | `Password123!` |
