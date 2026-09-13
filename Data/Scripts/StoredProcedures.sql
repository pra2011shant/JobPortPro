-- ==========================================================
-- JobPortPro Production Stored Procedures (SPs)
-- Database: JobPortPro | SQL Server
-- High-Performance, Parameterized & Clean Data Access
-- ==========================================================

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

    -- Normalize inputs
    SET @SearchQuery = LTRIM(RTRIM(@SearchQuery));
    SET @Location = LTRIM(RTRIM(@Location));
    IF @PageNumber < 1 SET @PageNumber = 1;
    IF @PageSize < 1 SET @PageSize = 9;

    -- Calculate Total Matching Count
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

    -- Return Paginated Data
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
        -- Category details
        c.Name AS CategoryName,
        c.IconClass AS CategoryIcon,
        -- JobType details
        jt.Name AS JobTypeName,
        jt.BadgeClass AS JobTypeBadge,
        -- ExperienceLevel details
        el.Title AS ExperienceLevelTitle,
        -- Company details
        cp.CompanyName,
        cp.Location AS CompanyLocation,
        cp.Website AS CompanyWebsite,
        -- Application Count
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

-- 7. Get Job Details by ID (and increment view count)
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

    -- Infer JobType name if null
    IF @JobTypeId IS NOT NULL AND (@JobType IS NULL OR @JobType = '')
    BEGIN
        SELECT @JobType = Name FROM dbo.JobTypes WHERE Id = @JobTypeId;
    END

    -- Infer ExperienceLevel name if null
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

    -- Infer JobType name if null
    IF @JobTypeId IS NOT NULL AND (@JobType IS NULL OR @JobType = '')
    BEGIN
        SELECT @JobType = Name FROM dbo.JobTypes WHERE Id = @JobTypeId;
    END

    -- Infer ExperienceLevel name if null
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

    -- Check if already applied
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
        -- Job info
        j.Title AS JobTitle,
        j.Location AS JobLocation,
        -- Job Seeker info
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
        -- Job info
        j.Title AS JobTitle,
        j.Location AS JobLocation,
        j.JobType,
        j.SalaryMin,
        j.SalaryMax,
        -- Company info
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

    -- Check duplicate email
    IF EXISTS (SELECT 1 FROM dbo.Users WHERE Email = @Email)
    BEGIN
        SET @UserId = -1;
        RETURN;
    END

    INSERT INTO dbo.Users (FullName, Email, PasswordHash, Role, PhoneNumber, CreatedAt, UpdatedAt)
    VALUES (@FullName, @Email, @PasswordHash, @Role, @PhoneNumber, SYSUTCDATETIME(), SYSUTCDATETIME());

    SET @UserId = SCOPE_IDENTITY();

    -- Automatically initialize corresponding profile
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
