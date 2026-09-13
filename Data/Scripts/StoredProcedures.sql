-- ====================================================================
-- JobPortPro Complete Database Setup & Stored Procedures Script
-- Database: JobPortPro | SQL Server Express
-- Safe for fresh databases or existing databases (Idempotent & Self-Healing)
-- ====================================================================

USE [JobPortPro];
GO

-- --------------------------------------------------------------------
-- 1. TABLE CREATION & SCHEMA MIGRATION (Ensuring all columns exist)
-- --------------------------------------------------------------------

-- 1.1 Categories Table
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Categories')
BEGIN
    CREATE TABLE dbo.Categories (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL,
        IconClass NVARCHAR(50) NULL,
        Description NVARCHAR(250) NULL,
        DisplayOrder INT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    );
END
ELSE
BEGIN
    IF COL_LENGTH('dbo.Categories', 'DisplayOrder') IS NULL ALTER TABLE dbo.Categories ADD DisplayOrder INT NOT NULL DEFAULT 0;
    IF COL_LENGTH('dbo.Categories', 'IsActive') IS NULL ALTER TABLE dbo.Categories ADD IsActive BIT NOT NULL DEFAULT 1;
    IF COL_LENGTH('dbo.Categories', 'CreatedAt') IS NULL ALTER TABLE dbo.Categories ADD CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME();
    IF COL_LENGTH('dbo.Categories', 'UpdatedAt') IS NULL ALTER TABLE dbo.Categories ADD UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME();
    IF COL_LENGTH('dbo.Categories', 'IconClass') IS NULL ALTER TABLE dbo.Categories ADD IconClass NVARCHAR(50) NULL;
    IF COL_LENGTH('dbo.Categories', 'Description') IS NULL ALTER TABLE dbo.Categories ADD Description NVARCHAR(250) NULL;
END
GO

-- 1.2 JobTypes Table
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'JobTypes')
BEGIN
    CREATE TABLE dbo.JobTypes (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(50) NOT NULL,
        Code NVARCHAR(50) NOT NULL,
        BadgeClass NVARCHAR(50) NULL,
        DisplayOrder INT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    );
END
GO

-- 1.3 ExperienceLevels Table
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ExperienceLevels')
BEGIN
    CREATE TABLE dbo.ExperienceLevels (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Title NVARCHAR(100) NOT NULL,
        Code NVARCHAR(50) NOT NULL,
        MinYears INT NOT NULL DEFAULT 0,
        MaxYears INT NULL,
        DisplayOrder INT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    );
END
GO

-- 1.4 Users Table
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE dbo.Users (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        FullName NVARCHAR(100) NOT NULL,
        Email NVARCHAR(256) NOT NULL UNIQUE,
        PasswordHash NVARCHAR(500) NOT NULL,
        Role NVARCHAR(50) NOT NULL,
        PhoneNumber NVARCHAR(20) NULL,
        Bio NVARCHAR(500) NULL,
        ProfilePicture NVARCHAR(500) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    );
END
ELSE
BEGIN
    IF COL_LENGTH('dbo.Users', 'PhoneNumber') IS NULL ALTER TABLE dbo.Users ADD PhoneNumber NVARCHAR(20) NULL;
    IF COL_LENGTH('dbo.Users', 'Bio') IS NULL ALTER TABLE dbo.Users ADD Bio NVARCHAR(500) NULL;
    IF COL_LENGTH('dbo.Users', 'ProfilePicture') IS NULL ALTER TABLE dbo.Users ADD ProfilePicture NVARCHAR(500) NULL;
    IF COL_LENGTH('dbo.Users', 'CreatedAt') IS NULL ALTER TABLE dbo.Users ADD CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME();
    IF COL_LENGTH('dbo.Users', 'UpdatedAt') IS NULL ALTER TABLE dbo.Users ADD UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME();
END
GO

-- 1.5 CompanyProfiles Table
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CompanyProfiles')
BEGIN
    CREATE TABLE dbo.CompanyProfiles (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL UNIQUE FOREIGN KEY REFERENCES dbo.Users(Id) ON DELETE CASCADE,
        CompanyName NVARCHAR(200) NOT NULL,
        Description NVARCHAR(MAX) NULL,
        Website NVARCHAR(200) NULL,
        Location NVARCHAR(150) NULL,
        Industry NVARCHAR(100) NULL,
        CompanySize NVARCHAR(50) NULL,
        LogoUrl NVARCHAR(500) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    );
END
ELSE
BEGIN
    IF COL_LENGTH('dbo.CompanyProfiles', 'CreatedAt') IS NULL ALTER TABLE dbo.CompanyProfiles ADD CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME();
    IF COL_LENGTH('dbo.CompanyProfiles', 'UpdatedAt') IS NULL ALTER TABLE dbo.CompanyProfiles ADD UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME();
END
GO

-- 1.6 JobSeekerProfiles Table
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'JobSeekerProfiles')
BEGIN
    CREATE TABLE dbo.JobSeekerProfiles (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL UNIQUE FOREIGN KEY REFERENCES dbo.Users(Id) ON DELETE CASCADE,
        Headline NVARCHAR(200) NULL,
        Skills NVARCHAR(MAX) NULL,
        ExperienceYears INT NULL,
        Education NVARCHAR(200) NULL,
        ResumeFilePath NVARCHAR(500) NULL,
        ResumeFileName NVARCHAR(255) NULL,
        GitHubUrl NVARCHAR(200) NULL,
        LinkedInUrl NVARCHAR(200) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    );
END
ELSE
BEGIN
    IF COL_LENGTH('dbo.JobSeekerProfiles', 'CreatedAt') IS NULL ALTER TABLE dbo.JobSeekerProfiles ADD CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME();
    IF COL_LENGTH('dbo.JobSeekerProfiles', 'UpdatedAt') IS NULL ALTER TABLE dbo.JobSeekerProfiles ADD UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME();
END
GO

-- 1.7 Jobs Table
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Jobs')
BEGIN
    CREATE TABLE dbo.Jobs (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        EmployerId INT NOT NULL FOREIGN KEY REFERENCES dbo.Users(Id),
        Title NVARCHAR(200) NOT NULL,
        CategoryId INT NOT NULL FOREIGN KEY REFERENCES dbo.Categories(Id),
        JobTypeId INT NULL FOREIGN KEY REFERENCES dbo.JobTypes(Id),
        JobType NVARCHAR(50) NOT NULL DEFAULT 'Full-Time',
        Location NVARCHAR(150) NOT NULL,
        SalaryMin DECIMAL(18,2) NULL,
        SalaryMax DECIMAL(18,2) NULL,
        ExperienceLevelId INT NULL FOREIGN KEY REFERENCES dbo.ExperienceLevels(Id),
        ExperienceLevel NVARCHAR(50) NULL,
        Description NVARCHAR(MAX) NOT NULL,
        Requirements NVARCHAR(MAX) NULL,
        Responsibilities NVARCHAR(MAX) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        Deadline DATETIME2 NULL,
        ViewsCount INT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    );
END
ELSE
BEGIN
    IF COL_LENGTH('dbo.Jobs', 'JobTypeId') IS NULL ALTER TABLE dbo.Jobs ADD JobTypeId INT NULL;
    IF COL_LENGTH('dbo.Jobs', 'ExperienceLevelId') IS NULL ALTER TABLE dbo.Jobs ADD ExperienceLevelId INT NULL;
    IF COL_LENGTH('dbo.Jobs', 'ViewsCount') IS NULL ALTER TABLE dbo.Jobs ADD ViewsCount INT NOT NULL DEFAULT 0;
    IF COL_LENGTH('dbo.Jobs', 'CreatedAt') IS NULL ALTER TABLE dbo.Jobs ADD CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME();
    IF COL_LENGTH('dbo.Jobs', 'UpdatedAt') IS NULL ALTER TABLE dbo.Jobs ADD UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME();
END
GO

-- 1.8 JobApplications Table
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'JobApplications')
BEGIN
    CREATE TABLE dbo.JobApplications (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        JobId INT NOT NULL FOREIGN KEY REFERENCES dbo.Jobs(Id) ON DELETE CASCADE,
        JobSeekerId INT NOT NULL FOREIGN KEY REFERENCES dbo.Users(Id),
        CoverLetter NVARCHAR(MAX) NULL,
        ResumePath NVARCHAR(500) NOT NULL,
        ResumeFileName NVARCHAR(255) NOT NULL,
        AppliedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        Status NVARCHAR(30) NOT NULL DEFAULT 'Pending',
        EmployerNotes NVARCHAR(MAX) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    );
END
ELSE
BEGIN
    IF COL_LENGTH('dbo.JobApplications', 'CreatedAt') IS NULL ALTER TABLE dbo.JobApplications ADD CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME();
    IF COL_LENGTH('dbo.JobApplications', 'UpdatedAt') IS NULL ALTER TABLE dbo.JobApplications ADD UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME();
END
GO

-- 1.9 SavedJobs Table
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'SavedJobs')
BEGIN
    CREATE TABLE dbo.SavedJobs (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        JobId INT NOT NULL FOREIGN KEY REFERENCES dbo.Jobs(Id) ON DELETE CASCADE,
        JobSeekerId INT NOT NULL FOREIGN KEY REFERENCES dbo.Users(Id),
        SavedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    );
END
ELSE
BEGIN
    IF COL_LENGTH('dbo.SavedJobs', 'CreatedAt') IS NULL ALTER TABLE dbo.SavedJobs ADD CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME();
    IF COL_LENGTH('dbo.SavedJobs', 'UpdatedAt') IS NULL ALTER TABLE dbo.SavedJobs ADD UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME();
END
GO

-- --------------------------------------------------------------------
-- 2. SEED DEFAULT MASTER LOOKUPS (If empty)
-- --------------------------------------------------------------------

IF NOT EXISTS (SELECT 1 FROM dbo.JobTypes)
BEGIN
    INSERT INTO dbo.JobTypes (Name, Code, BadgeClass, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
    VALUES 
    ('Full-Time', 'full-time', 'badge-soft-primary', 1, 1, SYSUTCDATETIME(), SYSUTCDATETIME()),
    ('Remote', 'remote', 'badge-soft-success', 2, 1, SYSUTCDATETIME(), SYSUTCDATETIME()),
    ('Part-Time', 'part-time', 'badge-soft-warning', 3, 1, SYSUTCDATETIME(), SYSUTCDATETIME()),
    ('Contract', 'contract', 'badge-soft-info', 4, 1, SYSUTCDATETIME(), SYSUTCDATETIME()),
    ('Internship', 'internship', 'badge-soft-secondary', 5, 1, SYSUTCDATETIME(), SYSUTCDATETIME());
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.ExperienceLevels)
BEGIN
    INSERT INTO dbo.ExperienceLevels (Title, Code, MinYears, MaxYears, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
    VALUES 
    ('Entry Level', 'entry-level', 0, 2, 1, 1, SYSUTCDATETIME(), SYSUTCDATETIME()),
    ('Mid Level', 'mid-level', 2, 5, 2, 1, SYSUTCDATETIME(), SYSUTCDATETIME()),
    ('Senior Level', 'senior-level', 5, 10, 3, 1, SYSUTCDATETIME(), SYSUTCDATETIME()),
    ('Director / Executive', 'director', 10, NULL, 4, 1, SYSUTCDATETIME(), SYSUTCDATETIME());
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Categories)
BEGIN
    INSERT INTO dbo.Categories (Name, IconClass, Description, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
    VALUES 
    ('Software & IT', 'fa-solid fa-code', 'Web Development, Mobile Apps, Cloud, DevOps, AI & Data Science', 1, 1, SYSUTCDATETIME(), SYSUTCDATETIME()),
    ('Design & Creative', 'fa-solid fa-palette', 'UI/UX, Graphic Design, Product Design, 3D Animation', 2, 1, SYSUTCDATETIME(), SYSUTCDATETIME()),
    ('Marketing & Sales', 'fa-solid fa-bullhorn', 'Digital Marketing, SEO, Social Media, Content, B2B Sales', 3, 1, SYSUTCDATETIME(), SYSUTCDATETIME()),
    ('Finance & Accounting', 'fa-solid fa-chart-pie', 'Auditing, Financial Analysis, Taxation, Banking', 4, 1, SYSUTCDATETIME(), SYSUTCDATETIME()),
    ('Customer Support', 'fa-solid fa-headset', 'Technical Support, Customer Success, Helpdesk', 5, 1, SYSUTCDATETIME(), SYSUTCDATETIME()),
    ('Human Resources', 'fa-solid fa-users-gear', 'Talent Acquisition, HR Operations, Payroll, Employee Relations', 6, 1, SYSUTCDATETIME(), SYSUTCDATETIME()),
    ('Healthcare', 'fa-solid fa-heart-pulse', 'Nursing, Clinical Research, Pharmacy, Medical Tech', 7, 1, SYSUTCDATETIME(), SYSUTCDATETIME()),
    ('Engineering', 'fa-solid fa-gears', 'Mechanical, Civil, Electrical, Robotics', 8, 1, SYSUTCDATETIME(), SYSUTCDATETIME());
END
GO

-- --------------------------------------------------------------------
-- 3. STORED PROCEDURES (18 High-Performance SPs)
-- --------------------------------------------------------------------

-- 1. Get All Categories
CREATE OR ALTER PROCEDURE dbo.sp_GetCategories
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        c.Id,
        c.Name,
        c.IconClass,
        c.Description,
        c.DisplayOrder,
        c.IsActive,
        c.CreatedAt,
        c.UpdatedAt,
        COUNT(j.Id) AS ActiveJobsCount
    FROM dbo.Categories c WITH (NOLOCK)
    LEFT JOIN dbo.Jobs j WITH (NOLOCK) ON c.Id = j.CategoryId AND j.IsActive = 1
    WHERE c.IsActive = 1
    GROUP BY c.Id, c.Name, c.IconClass, c.Description, c.DisplayOrder, c.IsActive, c.CreatedAt, c.UpdatedAt
    ORDER BY c.DisplayOrder ASC, c.Name ASC;
END;
GO

-- 2. Get All Job Types
CREATE OR ALTER PROCEDURE dbo.sp_GetJobTypes
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        Id,
        Name,
        Code,
        BadgeClass,
        DisplayOrder,
        IsActive,
        CreatedAt,
        UpdatedAt
    FROM dbo.JobTypes WITH (NOLOCK)
    WHERE IsActive = 1
    ORDER BY DisplayOrder ASC, Name ASC;
END;
GO

-- 3. Get All Experience Levels
CREATE OR ALTER PROCEDURE dbo.sp_GetExperienceLevels
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        Id,
        Title,
        Code,
        MinYears,
        MaxYears,
        DisplayOrder,
        IsActive,
        CreatedAt,
        UpdatedAt
    FROM dbo.ExperienceLevels WITH (NOLOCK)
    WHERE IsActive = 1
    ORDER BY DisplayOrder ASC, MinYears ASC;
END;
GO

-- 4. Get Filtered Jobs with Pagination & Aggregation
CREATE OR ALTER PROCEDURE dbo.sp_GetFilteredJobs
    @SearchQuery NVARCHAR(200) = NULL,
    @CategoryId INT = NULL,
    @JobTypeId INT = NULL,
    @JobType NVARCHAR(50) = NULL,
    @Location NVARCHAR(150) = NULL,
    @ExperienceLevelId INT = NULL,
    @MinSalary DECIMAL(18,2) = NULL,
    @SortBy NVARCHAR(50) = 'newest',
    @PageNumber INT = 1,
    @PageSize INT = 9,
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SET @SearchQuery = LTRIM(RTRIM(@SearchQuery));
    SET @Location = LTRIM(RTRIM(@Location));
    IF @PageNumber < 1 SET @PageNumber = 1;
    IF @PageSize < 1 SET @PageSize = 9;

    SELECT @TotalCount = COUNT(1)
    FROM dbo.Jobs j WITH (NOLOCK)
    LEFT JOIN dbo.Categories c WITH (NOLOCK) ON j.CategoryId = c.Id
    LEFT JOIN dbo.JobTypes jt WITH (NOLOCK) ON j.JobTypeId = jt.Id
    LEFT JOIN dbo.ExperienceLevels el WITH (NOLOCK) ON j.ExperienceLevelId = el.Id
    LEFT JOIN dbo.Users u WITH (NOLOCK) ON j.EmployerId = u.Id
    LEFT JOIN dbo.CompanyProfiles cp WITH (NOLOCK) ON u.Id = cp.UserId
    WHERE j.IsActive = 1
      AND (@CategoryId IS NULL OR j.CategoryId = @CategoryId)
      AND (@JobTypeId IS NULL OR j.JobTypeId = @JobTypeId)
      AND (@JobType IS NULL OR j.JobType = @JobType OR jt.Code = @JobType)
      AND (@ExperienceLevelId IS NULL OR j.ExperienceLevelId = @ExperienceLevelId)
      AND (@MinSalary IS NULL OR j.SalaryMax >= @MinSalary OR (j.SalaryMin >= @MinSalary AND j.SalaryMax IS NULL))
      AND (@Location IS NULL OR j.Location LIKE '%' + @Location + '%')
      AND (@SearchQuery IS NULL OR 
           j.Title LIKE '%' + @SearchQuery + '%' OR 
           j.Description LIKE '%' + @SearchQuery + '%' OR 
           j.Requirements LIKE '%' + @SearchQuery + '%' OR 
           cp.CompanyName LIKE '%' + @SearchQuery + '%');

    SELECT 
        j.Id,
        j.Title,
        j.EmployerId,
        j.CategoryId,
        j.JobTypeId,
        j.JobType,
        j.Location,
        j.SalaryMin,
        j.SalaryMax,
        j.ExperienceLevelId,
        j.ExperienceLevel,
        j.Description,
        j.Requirements,
        j.Responsibilities,
        j.IsActive,
        j.Deadline,
        j.ViewsCount,
        j.CreatedAt,
        j.UpdatedAt,
        c.Name AS CategoryName,
        c.IconClass AS CategoryIcon,
        jt.Name AS JobTypeName,
        jt.BadgeClass AS JobTypeBadge,
        el.Title AS ExperienceLevelTitle,
        cp.CompanyName,
        cp.Location AS CompanyLocation,
        cp.Website AS CompanyWebsite,
        (SELECT COUNT(1) FROM dbo.JobApplications ja WITH (NOLOCK) WHERE ja.JobId = j.Id) AS ApplicationCount
    FROM dbo.Jobs j WITH (NOLOCK)
    LEFT JOIN dbo.Categories c WITH (NOLOCK) ON j.CategoryId = c.Id
    LEFT JOIN dbo.JobTypes jt WITH (NOLOCK) ON j.JobTypeId = jt.Id
    LEFT JOIN dbo.ExperienceLevels el WITH (NOLOCK) ON j.ExperienceLevelId = el.Id
    LEFT JOIN dbo.Users u WITH (NOLOCK) ON j.EmployerId = u.Id
    LEFT JOIN dbo.CompanyProfiles cp WITH (NOLOCK) ON u.Id = cp.UserId
    WHERE j.IsActive = 1
      AND (@CategoryId IS NULL OR j.CategoryId = @CategoryId)
      AND (@JobTypeId IS NULL OR j.JobTypeId = @JobTypeId)
      AND (@JobType IS NULL OR j.JobType = @JobType OR jt.Code = @JobType)
      AND (@ExperienceLevelId IS NULL OR j.ExperienceLevelId = @ExperienceLevelId)
      AND (@MinSalary IS NULL OR j.SalaryMax >= @MinSalary OR (j.SalaryMin >= @MinSalary AND j.SalaryMax IS NULL))
      AND (@Location IS NULL OR j.Location LIKE '%' + @Location + '%')
      AND (@SearchQuery IS NULL OR 
           j.Title LIKE '%' + @SearchQuery + '%' OR 
           j.Description LIKE '%' + @SearchQuery + '%' OR 
           j.Requirements LIKE '%' + @SearchQuery + '%' OR 
           cp.CompanyName LIKE '%' + @SearchQuery + '%')
    ORDER BY 
        CASE WHEN @SortBy = 'salary_desc' THEN j.SalaryMax END DESC,
        CASE WHEN @SortBy = 'salary_asc' THEN j.SalaryMin END ASC,
        CASE WHEN @SortBy = 'oldest' THEN j.CreatedAt END ASC,
        j.CreatedAt DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END;
GO

-- 5. Get Featured Jobs
CREATE OR ALTER PROCEDURE dbo.sp_GetFeaturedJobs
    @Count INT = 6
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (@Count)
        j.Id,
        j.Title,
        j.EmployerId,
        j.CategoryId,
        j.JobTypeId,
        j.JobType,
        j.Location,
        j.SalaryMin,
        j.SalaryMax,
        j.ExperienceLevelId,
        j.ExperienceLevel,
        j.Description,
        j.Requirements,
        j.Responsibilities,
        j.IsActive,
        j.Deadline,
        j.ViewsCount,
        j.CreatedAt,
        j.UpdatedAt,
        c.Name AS CategoryName,
        c.IconClass AS CategoryIcon,
        jt.Name AS JobTypeName,
        jt.BadgeClass AS JobTypeBadge,
        el.Title AS ExperienceLevelTitle,
        cp.CompanyName,
        cp.Location AS CompanyLocation,
        cp.Website AS CompanyWebsite
    FROM dbo.Jobs j WITH (NOLOCK)
    LEFT JOIN dbo.Categories c WITH (NOLOCK) ON j.CategoryId = c.Id
    LEFT JOIN dbo.JobTypes jt WITH (NOLOCK) ON j.JobTypeId = jt.Id
    LEFT JOIN dbo.ExperienceLevels el WITH (NOLOCK) ON j.ExperienceLevelId = el.Id
    LEFT JOIN dbo.Users u WITH (NOLOCK) ON j.EmployerId = u.Id
    LEFT JOIN dbo.CompanyProfiles cp WITH (NOLOCK) ON u.Id = cp.UserId
    WHERE j.IsActive = 1
    ORDER BY j.ViewsCount DESC, j.CreatedAt DESC;
END;
GO

-- 6. Get Recent Jobs
CREATE OR ALTER PROCEDURE dbo.sp_GetRecentJobs
    @Count INT = 4
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (@Count)
        j.Id,
        j.Title,
        j.EmployerId,
        j.CategoryId,
        j.JobTypeId,
        j.JobType,
        j.Location,
        j.SalaryMin,
        j.SalaryMax,
        j.ExperienceLevelId,
        j.ExperienceLevel,
        j.Description,
        j.Requirements,
        j.Responsibilities,
        j.IsActive,
        j.Deadline,
        j.ViewsCount,
        j.CreatedAt,
        j.UpdatedAt,
        c.Name AS CategoryName,
        c.IconClass AS CategoryIcon,
        jt.Name AS JobTypeName,
        jt.BadgeClass AS JobTypeBadge,
        el.Title AS ExperienceLevelTitle,
        cp.CompanyName,
        cp.Location AS CompanyLocation,
        cp.Website AS CompanyWebsite
    FROM dbo.Jobs j WITH (NOLOCK)
    LEFT JOIN dbo.Categories c WITH (NOLOCK) ON j.CategoryId = c.Id
    LEFT JOIN dbo.JobTypes jt WITH (NOLOCK) ON j.JobTypeId = jt.Id
    LEFT JOIN dbo.ExperienceLevels el WITH (NOLOCK) ON j.ExperienceLevelId = el.Id
    LEFT JOIN dbo.Users u WITH (NOLOCK) ON j.EmployerId = u.Id
    LEFT JOIN dbo.CompanyProfiles cp WITH (NOLOCK) ON u.Id = cp.UserId
    WHERE j.IsActive = 1
    ORDER BY j.CreatedAt DESC;
END;
GO

-- 7. Get Job Details by ID
CREATE OR ALTER PROCEDURE dbo.sp_GetJobById
    @JobId INT,
    @IncrementViews BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    IF @IncrementViews = 1
    BEGIN
        UPDATE dbo.Jobs SET ViewsCount = ViewsCount + 1 WHERE Id = @JobId;
    END

    SELECT 
        j.Id,
        j.Title,
        j.EmployerId,
        j.CategoryId,
        j.JobTypeId,
        j.JobType,
        j.Location,
        j.SalaryMin,
        j.SalaryMax,
        j.ExperienceLevelId,
        j.ExperienceLevel,
        j.Description,
        j.Requirements,
        j.Responsibilities,
        j.IsActive,
        j.Deadline,
        j.ViewsCount,
        j.CreatedAt,
        j.UpdatedAt,
        c.Name AS CategoryName,
        c.IconClass AS CategoryIcon,
        jt.Name AS JobTypeName,
        jt.BadgeClass AS JobTypeBadge,
        el.Title AS ExperienceLevelTitle,
        u.FullName AS EmployerName,
        u.Email AS EmployerEmail,
        cp.CompanyName,
        cp.Description AS CompanyDescription,
        cp.Website AS CompanyWebsite,
        cp.Location AS CompanyLocation,
        cp.CompanySize
    FROM dbo.Jobs j WITH (NOLOCK)
    LEFT JOIN dbo.Categories c WITH (NOLOCK) ON j.CategoryId = c.Id
    LEFT JOIN dbo.JobTypes jt WITH (NOLOCK) ON j.JobTypeId = jt.Id
    LEFT JOIN dbo.ExperienceLevels el WITH (NOLOCK) ON j.ExperienceLevelId = el.Id
    LEFT JOIN dbo.Users u WITH (NOLOCK) ON j.EmployerId = u.Id
    LEFT JOIN dbo.CompanyProfiles cp WITH (NOLOCK) ON u.Id = cp.UserId
    WHERE j.Id = @JobId;
END;
GO

-- 8. Create Job Procedure
CREATE OR ALTER PROCEDURE dbo.sp_CreateJob
    @EmployerId INT,
    @Title NVARCHAR(200),
    @CategoryId INT,
    @JobTypeId INT = NULL,
    @JobType NVARCHAR(50),
    @Location NVARCHAR(150),
    @SalaryMin DECIMAL(18,2) = NULL,
    @SalaryMax DECIMAL(18,2) = NULL,
    @ExperienceLevelId INT = NULL,
    @ExperienceLevel NVARCHAR(50) = NULL,
    @Description NVARCHAR(MAX),
    @Requirements NVARCHAR(MAX) = NULL,
    @Responsibilities NVARCHAR(MAX) = NULL,
    @Deadline DATETIME2 = NULL,
    @NewJobId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF @JobTypeId IS NOT NULL AND (@JobType IS NULL OR @JobType = '')
    BEGIN
        SELECT @JobType = Name FROM dbo.JobTypes WHERE Id = @JobTypeId;
    END

    IF @ExperienceLevelId IS NOT NULL AND (@ExperienceLevel IS NULL OR @ExperienceLevel = '')
    BEGIN
        SELECT @ExperienceLevel = Title FROM dbo.ExperienceLevels WHERE Id = @ExperienceLevelId;
    END

    INSERT INTO dbo.Jobs 
    (
        EmployerId, Title, CategoryId, JobTypeId, JobType, Location, 
        SalaryMin, SalaryMax, ExperienceLevelId, ExperienceLevel, 
        Description, Requirements, Responsibilities, IsActive, Deadline, 
        ViewsCount, CreatedAt, UpdatedAt
    )
    VALUES 
    (
        @EmployerId, @Title, @CategoryId, @JobTypeId, @JobType, @Location, 
        @SalaryMin, @SalaryMax, @ExperienceLevelId, @ExperienceLevel, 
        @Description, @Requirements, @Responsibilities, 1, @Deadline, 
        0, SYSUTCDATETIME(), SYSUTCDATETIME()
    );

    SET @NewJobId = SCOPE_IDENTITY();
END;
GO

-- 9. Update Job Procedure
CREATE OR ALTER PROCEDURE dbo.sp_UpdateJob
    @JobId INT,
    @EmployerId INT,
    @Title NVARCHAR(200),
    @CategoryId INT,
    @JobTypeId INT = NULL,
    @JobType NVARCHAR(50),
    @Location NVARCHAR(150),
    @SalaryMin DECIMAL(18,2) = NULL,
    @SalaryMax DECIMAL(18,2) = NULL,
    @ExperienceLevelId INT = NULL,
    @ExperienceLevel NVARCHAR(50) = NULL,
    @Description NVARCHAR(MAX),
    @Requirements NVARCHAR(MAX) = NULL,
    @Responsibilities NVARCHAR(MAX) = NULL,
    @IsActive BIT = 1,
    @Deadline DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @JobTypeId IS NOT NULL AND (@JobType IS NULL OR @JobType = '')
    BEGIN
        SELECT @JobType = Name FROM dbo.JobTypes WHERE Id = @JobTypeId;
    END

    IF @ExperienceLevelId IS NOT NULL AND (@ExperienceLevel IS NULL OR @ExperienceLevel = '')
    BEGIN
        SELECT @ExperienceLevel = Title FROM dbo.ExperienceLevels WHERE Id = @ExperienceLevelId;
    END

    UPDATE dbo.Jobs
    SET Title = @Title,
        CategoryId = @CategoryId,
        JobTypeId = @JobTypeId,
        JobType = @JobType,
        Location = @Location,
        SalaryMin = @SalaryMin,
        SalaryMax = @SalaryMax,
        ExperienceLevelId = @ExperienceLevelId,
        ExperienceLevel = @ExperienceLevel,
        Description = @Description,
        Requirements = @Requirements,
        Responsibilities = @Responsibilities,
        IsActive = @IsActive,
        Deadline = @Deadline,
        UpdatedAt = SYSUTCDATETIME()
    WHERE Id = @JobId AND EmployerId = @EmployerId;
END;
GO

-- 10. Delete Job Procedure
CREATE OR ALTER PROCEDURE dbo.sp_DeleteJob
    @JobId INT,
    @EmployerId INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM dbo.Jobs WHERE Id = @JobId AND EmployerId = @EmployerId;
END;
GO

-- 11. Submit Job Application Procedure
CREATE OR ALTER PROCEDURE dbo.sp_SubmitJobApplication
    @JobId INT,
    @JobSeekerId INT,
    @CoverLetter NVARCHAR(MAX) = NULL,
    @ResumePath NVARCHAR(500) = NULL,
    @ResumeFileName NVARCHAR(255) = NULL,
    @ApplicationId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.JobApplications WHERE JobId = @JobId AND JobSeekerId = @JobSeekerId)
    BEGIN
        SELECT @ApplicationId = Id FROM dbo.JobApplications WHERE JobId = @JobId AND JobSeekerId = @JobSeekerId;
        RETURN;
    END

    INSERT INTO dbo.JobApplications
    (
        JobId, JobSeekerId, CoverLetter, ResumePath, ResumeFileName, 
        AppliedAt, Status, CreatedAt, UpdatedAt
    )
    VALUES
    (
        @JobId, @JobSeekerId, @CoverLetter, @ResumePath, @ResumeFileName, 
        SYSUTCDATETIME(), 'Pending', SYSUTCDATETIME(), SYSUTCDATETIME()
    );

    SET @ApplicationId = SCOPE_IDENTITY();
END;
GO

-- 12. Get Applications for an Employer
CREATE OR ALTER PROCEDURE dbo.sp_GetApplicationsByEmployer
    @EmployerId INT,
    @JobId INT = NULL,
    @Status NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        ja.Id,
        ja.JobId,
        ja.JobSeekerId,
        ja.CoverLetter,
        ja.ResumePath,
        ja.ResumeFileName,
        ja.AppliedAt,
        ja.Status,
        ja.EmployerNotes,
        ja.CreatedAt,
        ja.UpdatedAt,
        j.Title AS JobTitle,
        j.Location AS JobLocation,
        u.FullName AS CandidateName,
        u.Email AS CandidateEmail,
        u.PhoneNumber AS CandidatePhone,
        jsp.Headline AS CandidateHeadline,
        jsp.Skills AS CandidateSkills,
        jsp.ExperienceYears AS CandidateExperience
    FROM dbo.JobApplications ja WITH (NOLOCK)
    INNER JOIN dbo.Jobs j WITH (NOLOCK) ON ja.JobId = j.Id
    INNER JOIN dbo.Users u WITH (NOLOCK) ON ja.JobSeekerId = u.Id
    LEFT JOIN dbo.JobSeekerProfiles jsp WITH (NOLOCK) ON u.Id = jsp.UserId
    WHERE j.EmployerId = @EmployerId
      AND (@JobId IS NULL OR ja.JobId = @JobId)
      AND (@Status IS NULL OR ja.Status = @Status)
    ORDER BY ja.AppliedAt DESC;
END;
GO

-- 13. Get Applications for a Job Seeker
CREATE OR ALTER PROCEDURE dbo.sp_GetApplicationsByJobSeeker
    @JobSeekerId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        ja.Id,
        ja.JobId,
        ja.JobSeekerId,
        ja.CoverLetter,
        ja.ResumePath,
        ja.ResumeFileName,
        ja.AppliedAt,
        ja.Status,
        ja.EmployerNotes,
        ja.CreatedAt,
        ja.UpdatedAt,
        j.Title AS JobTitle,
        j.Location AS JobLocation,
        j.JobType,
        j.SalaryMin,
        j.SalaryMax,
        cp.CompanyName,
        cp.Location AS CompanyLocation
    FROM dbo.JobApplications ja WITH (NOLOCK)
    INNER JOIN dbo.Jobs j WITH (NOLOCK) ON ja.JobId = j.Id
    INNER JOIN dbo.Users u WITH (NOLOCK) ON j.EmployerId = u.Id
    LEFT JOIN dbo.CompanyProfiles cp WITH (NOLOCK) ON u.Id = cp.UserId
    WHERE ja.JobSeekerId = @JobSeekerId
    ORDER BY ja.AppliedAt DESC;
END;
GO

-- 14. Update Application Status & Feedback
CREATE OR ALTER PROCEDURE dbo.sp_UpdateApplicationStatus
    @ApplicationId INT,
    @EmployerId INT,
    @Status NVARCHAR(50),
    @EmployerNotes NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE ja
    SET ja.Status = @Status,
        ja.EmployerNotes = COALESCE(@EmployerNotes, ja.EmployerNotes),
        ja.UpdatedAt = SYSUTCDATETIME()
    FROM dbo.JobApplications ja
    INNER JOIN dbo.Jobs j ON ja.JobId = j.Id
    WHERE ja.Id = @ApplicationId AND j.EmployerId = @EmployerId;
END;
GO

-- 15. Get User by Email for Authentication
CREATE OR ALTER PROCEDURE dbo.sp_GetUserByEmail
    @Email NVARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        u.Id,
        u.FullName,
        u.Email,
        u.PasswordHash,
        u.Role,
        u.PhoneNumber,
        u.Bio,
        u.CreatedAt,
        u.UpdatedAt,
        cp.Id AS CompanyProfileId,
        cp.CompanyName,
        cp.Description AS CompanyDescription,
        cp.Website AS CompanyWebsite,
        jsp.Id AS JobSeekerProfileId,
        jsp.Headline AS SeekerHeadline,
        jsp.Skills AS SeekerSkills,
        jsp.ResumeFilePath AS SeekerResumePath
    FROM dbo.Users u WITH (NOLOCK)
    LEFT JOIN dbo.CompanyProfiles cp WITH (NOLOCK) ON u.Id = cp.UserId
    LEFT JOIN dbo.JobSeekerProfiles jsp WITH (NOLOCK) ON u.Id = jsp.UserId
    WHERE u.Email = @Email;
END;
GO

-- 16. Create User & Profile
CREATE OR ALTER PROCEDURE dbo.sp_CreateUser
    @FullName NVARCHAR(150),
    @Email NVARCHAR(256),
    @PasswordHash NVARCHAR(500),
    @Role NVARCHAR(50),
    @PhoneNumber NVARCHAR(20) = NULL,
    @CompanyName NVARCHAR(200) = NULL,
    @UserId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.Users WHERE Email = @Email)
    BEGIN
        SET @UserId = -1;
        RETURN;
    END

    INSERT INTO dbo.Users (FullName, Email, PasswordHash, Role, PhoneNumber, CreatedAt, UpdatedAt)
    VALUES (@FullName, @Email, @PasswordHash, @Role, @PhoneNumber, SYSUTCDATETIME(), SYSUTCDATETIME());

    SET @UserId = SCOPE_IDENTITY();

    IF @Role = 'Employer'
    BEGIN
        INSERT INTO dbo.CompanyProfiles (UserId, CompanyName, CreatedAt, UpdatedAt)
        VALUES (@UserId, COALESCE(@CompanyName, @FullName + ' Company'), SYSUTCDATETIME(), SYSUTCDATETIME());
    END
    ELSE IF @Role = 'JobSeeker'
    BEGIN
        INSERT INTO dbo.JobSeekerProfiles (UserId, Headline, CreatedAt, UpdatedAt)
        VALUES (@UserId, 'Candidate Profile', SYSUTCDATETIME(), SYSUTCDATETIME());
    END
END;
GO

-- 17. Toggle Save Job Procedure
CREATE OR ALTER PROCEDURE dbo.sp_ToggleSaveJob
    @JobId INT,
    @JobSeekerId INT,
    @IsSaved BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.SavedJobs WHERE JobId = @JobId AND JobSeekerId = @JobSeekerId)
    BEGIN
        DELETE FROM dbo.SavedJobs WHERE JobId = @JobId AND JobSeekerId = @JobSeekerId;
        SET @IsSaved = 0;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.SavedJobs (JobId, JobSeekerId, SavedAt, CreatedAt, UpdatedAt)
        VALUES (@JobId, @JobSeekerId, SYSUTCDATETIME(), SYSUTCDATETIME(), SYSUTCDATETIME());
        SET @IsSaved = 1;
    END
END;
GO

-- 18. Get Saved Jobs for User
CREATE OR ALTER PROCEDURE dbo.sp_GetSavedJobsByUser
    @JobSeekerId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        sj.Id AS SavedJobId,
        sj.SavedAt,
        j.Id AS JobId,
        j.Title,
        j.JobType,
        j.Location,
        j.SalaryMin,
        j.SalaryMax,
        j.Deadline,
        j.IsActive,
        c.Name AS CategoryName,
        cp.CompanyName
    FROM dbo.SavedJobs sj WITH (NOLOCK)
    INNER JOIN dbo.Jobs j WITH (NOLOCK) ON sj.JobId = j.Id
    LEFT JOIN dbo.Categories c WITH (NOLOCK) ON j.CategoryId = c.Id
    LEFT JOIN dbo.Users u WITH (NOLOCK) ON j.EmployerId = u.Id
    LEFT JOIN dbo.CompanyProfiles cp WITH (NOLOCK) ON u.Id = cp.UserId
    WHERE sj.JobSeekerId = @JobSeekerId
    ORDER BY sj.SavedAt DESC;
END;
GO
