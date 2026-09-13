using System;
using System.Collections.Generic;
using System.Linq;
using JobPortPro.Models;

namespace JobPortPro.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            // Ensure database is created
            context.Database.EnsureCreated();

            // Execute Stored Procedures Creation / Update
            ApplyStoredProcedures(context);

            // 1. Seed Master Job Types if not present
            if (!context.JobTypes.Any())
            {
                var jobTypes = new List<JobType>
                {
                    new JobType { Name = "Full-Time", Code = "full-time", BadgeClass = "badge-soft-primary", DisplayOrder = 1, IsActive = true },
                    new JobType { Name = "Remote", Code = "remote", BadgeClass = "badge-soft-success", DisplayOrder = 2, IsActive = true },
                    new JobType { Name = "Part-Time", Code = "part-time", BadgeClass = "badge-soft-warning", DisplayOrder = 3, IsActive = true },
                    new JobType { Name = "Contract", Code = "contract", BadgeClass = "badge-soft-info", DisplayOrder = 4, IsActive = true },
                    new JobType { Name = "Internship", Code = "internship", BadgeClass = "badge-soft-secondary", DisplayOrder = 5, IsActive = true }
                };
                context.JobTypes.AddRange(jobTypes);
                context.SaveChanges();
            }

            // 2. Seed Master Experience Levels if not present
            if (!context.ExperienceLevels.Any())
            {
                var expLevels = new List<ExperienceLevel>
                {
                    new ExperienceLevel { Title = "Entry Level", Code = "entry-level", MinYears = 0, MaxYears = 2, DisplayOrder = 1, IsActive = true },
                    new ExperienceLevel { Title = "Mid Level", Code = "mid-level", MinYears = 2, MaxYears = 5, DisplayOrder = 2, IsActive = true },
                    new ExperienceLevel { Title = "Senior Level", Code = "senior-level", MinYears = 5, MaxYears = 10, DisplayOrder = 3, IsActive = true },
                    new ExperienceLevel { Title = "Director / Executive", Code = "director", MinYears = 10, MaxYears = null, DisplayOrder = 4, IsActive = true }
                };
                context.ExperienceLevels.AddRange(expLevels);
                context.SaveChanges();
            }

            // Look for any users
            if (context.Users.Any())
            {
                return; // Core users & jobs already seeded
            }

            // 3. Seed Categories
            var categories = new List<Category>
            {
                new Category { Name = "Software & IT", IconClass = "fa-solid fa-code", Description = "Web Development, Mobile Apps, Cloud, DevOps, AI & Data Science", DisplayOrder = 1, IsActive = true },
                new Category { Name = "Design & Creative", IconClass = "fa-solid fa-palette", Description = "UI/UX, Graphic Design, Product Design, 3D Animation", DisplayOrder = 2, IsActive = true },
                new Category { Name = "Marketing & Sales", IconClass = "fa-solid fa-bullhorn", Description = "Digital Marketing, SEO, Social Media, Content, B2B Sales", DisplayOrder = 3, IsActive = true },
                new Category { Name = "Finance & Accounting", IconClass = "fa-solid fa-chart-pie", Description = "Auditing, Financial Analysis, Taxation, Banking", DisplayOrder = 4, IsActive = true },
                new Category { Name = "Customer Support", IconClass = "fa-solid fa-headset", Description = "Technical Support, Customer Success, Helpdesk", DisplayOrder = 5, IsActive = true },
                new Category { Name = "Human Resources", IconClass = "fa-solid fa-users-gear", Description = "Talent Acquisition, HR Operations, Payroll, Employee Relations", DisplayOrder = 6, IsActive = true },
                new Category { Name = "Healthcare", IconClass = "fa-solid fa-heart-pulse", Description = "Nursing, Clinical Research, Pharmacy, Medical Tech", DisplayOrder = 7, IsActive = true },
                new Category { Name = "Engineering", IconClass = "fa-solid fa-gears", Description = "Mechanical, Civil, Electrical, Robotics", DisplayOrder = 8, IsActive = true }
            };
            context.Categories.AddRange(categories);
            context.SaveChanges();

            // 4. Seed Users
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword("Password123!");

            var employer1 = new User
            {
                FullName = "Vikram Malhotra",
                Email = "employer@techsolutions.com",
                PasswordHash = hashedPassword,
                Role = "Employer",
                PhoneNumber = "+91 9876543210",
                Bio = "HR Director at TechSolutions Global. Hiring top tech talent.",
                CreatedAt = DateTime.UtcNow.AddMonths(-3)
            };

            var employer2 = new User
            {
                FullName = "Ananya Sharma",
                Email = "hr@innovatedigital.io",
                PasswordHash = hashedPassword,
                Role = "Employer",
                PhoneNumber = "+91 9812345678",
                Bio = "Head of People & Culture at Innovate Digital.",
                CreatedAt = DateTime.UtcNow.AddMonths(-2)
            };

            var seeker1 = new User
            {
                FullName = "Rahul Verma",
                Email = "rahul.verma@example.com",
                PasswordHash = hashedPassword,
                Role = "JobSeeker",
                PhoneNumber = "+91 9123456780",
                Bio = "Passionate .NET Full Stack Developer with 4 years experience in building high-performance web applications.",
                CreatedAt = DateTime.UtcNow.AddMonths(-1)
            };

            var seeker2 = new User
            {
                FullName = "Priya Nair",
                Email = "priya.nair@example.com",
                PasswordHash = hashedPassword,
                Role = "JobSeeker",
                PhoneNumber = "+91 9234567891",
                Bio = "UI/UX Designer who loves crafting intuitive user journeys and modern SaaS dashboards.",
                CreatedAt = DateTime.UtcNow.AddMonths(-1)
            };

            context.Users.AddRange(employer1, employer2, seeker1, seeker2);
            context.SaveChanges();

            // 5. Seed Company Profiles
            var company1 = new CompanyProfile
            {
                UserId = employer1.Id,
                CompanyName = "TechSolutions Global",
                Description = "A premier software engineering and cloud consulting organization helping global enterprises scale digital experiences.",
                Website = "https://techsolutions.example.com",
                Location = "Bengaluru, Karnataka, India",
                Industry = "Information Technology",
                CompanySize = "500-1000 Employees"
            };

            var company2 = new CompanyProfile
            {
                UserId = employer2.Id,
                CompanyName = "Innovate Digital Labs",
                Description = "Fast-growing creative tech agency specializing in modern web applications, AI integration, and UI/UX design.",
                Website = "https://innovatedigital.example.com",
                Location = "Mumbai, Maharashtra, India",
                Industry = "Digital Agency & Software",
                CompanySize = "50-200 Employees"
            };

            context.CompanyProfiles.AddRange(company1, company2);

            // 6. Seed Job Seeker Profiles
            var seekerProfile1 = new JobSeekerProfile
            {
                UserId = seeker1.Id,
                Headline = "Senior .NET Core & Full Stack Engineer",
                Skills = "C#, ASP.NET Core MVC, SQL Server, Entity Framework, REST APIs, JavaScript, Bootstrap 5, Azure",
                ExperienceYears = 4,
                Education = "B.Tech in Computer Science, NIT Karnataka",
                GitHubUrl = "https://github.com/rahulverma-demo",
                LinkedInUrl = "https://linkedin.com/in/rahulverma-demo",
                ResumeFileName = "Rahul_Verma_Resume.pdf",
                ResumeFilePath = "/resumes/sample_resume_rahul.pdf"
            };

            var seekerProfile2 = new JobSeekerProfile
            {
                UserId = seeker2.Id,
                Headline = "Lead UI/UX & Product Designer",
                Skills = "Figma, Adobe XD, Wireframing, Prototyping, Design Systems, HTML5, CSS3, User Research",
                ExperienceYears = 3,
                Education = "Bachelor of Design, NID Ahmedabad",
                GitHubUrl = "https://github.com/priyanair-design",
                LinkedInUrl = "https://linkedin.com/in/priyanair-design",
                ResumeFileName = "Priya_Nair_Portfolio_Resume.pdf",
                ResumeFilePath = "/resumes/sample_resume_priya.pdf"
            };

            context.JobSeekerProfiles.AddRange(seekerProfile1, seekerProfile2);
            context.SaveChanges();

            // 7. Seed Jobs
            var itCategory = categories.First(c => c.Name == "Software & IT");
            var designCategory = categories.First(c => c.Name == "Design & Creative");
            var marketingCategory = categories.First(c => c.Name == "Marketing & Sales");
            var financeCategory = categories.First(c => c.Name == "Finance & Accounting");

            var fullTimeType = context.JobTypes.FirstOrDefault(jt => jt.Code == "full-time");
            var remoteType = context.JobTypes.FirstOrDefault(jt => jt.Code == "remote");

            var midLevel = context.ExperienceLevels.FirstOrDefault(el => el.Code == "mid-level");
            var seniorLevel = context.ExperienceLevels.FirstOrDefault(el => el.Code == "senior-level");
            var entryLevel = context.ExperienceLevels.FirstOrDefault(el => el.Code == "entry-level");

            var jobs = new List<Job>
            {
                new Job
                {
                    EmployerId = employer1.Id,
                    CategoryId = itCategory.Id,
                    JobTypeId = fullTimeType?.Id,
                    JobType = "Full-Time",
                    ExperienceLevelId = seniorLevel?.Id,
                    ExperienceLevel = "Senior Level",
                    Title = "Senior ASP.NET Core Full Stack Developer",
                    Location = "Bengaluru, Karnataka (Hybrid)",
                    SalaryMin = 1200000,
                    SalaryMax = 1800000,
                    Description = "We are seeking a talented Senior .NET Developer with deep expertise in ASP.NET Core MVC, Entity Framework Core, SQL Server, and frontend technologies.",
                    Requirements = "• 4+ years of hands-on experience in C# and ASP.NET Core\n• Strong SQL Server database design, stored procedures, and query optimization\n• Experience with RESTful APIs, Git, and Azure deployment\n• Excellent problem-solving skills and team collaboration",
                    Responsibilities = "• Architect and build scalable web applications\n• Collaborate with cross-functional product and design teams\n• Write clean, well-tested, maintainable code\n• Mentor junior developers and participate in code reviews",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-10),
                    Deadline = DateTime.UtcNow.AddDays(25)
                },
                new Job
                {
                    EmployerId = employer1.Id,
                    CategoryId = itCategory.Id,
                    JobTypeId = remoteType?.Id,
                    JobType = "Remote",
                    ExperienceLevelId = midLevel?.Id,
                    ExperienceLevel = "Mid Level",
                    Title = "Cloud DevOps Engineer (Azure / AWS)",
                    Location = "Remote (India)",
                    SalaryMin = 1400000,
                    SalaryMax = 2200000,
                    Description = "Looking for a proactive DevOps Engineer to automate CI/CD pipelines, manage Kubernetes clusters, and scale cloud infrastructure.",
                    Requirements = "• 3+ years experience with Azure or AWS cloud platforms\n• Strong Docker, Kubernetes, Terraform, and GitHub Actions knowledge\n• Linux administration and scripting (Bash / PowerShell)",
                    Responsibilities = "• Design and maintain robust CI/CD pipelines\n• Ensure high system reliability and monitoring with Grafana/Prometheus\n• Implement security best practices across cloud environments",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-6),
                    Deadline = DateTime.UtcNow.AddDays(20)
                },
                new Job
                {
                    EmployerId = employer2.Id,
                    CategoryId = designCategory.Id,
                    JobTypeId = fullTimeType?.Id,
                    JobType = "Full-Time",
                    ExperienceLevelId = midLevel?.Id,
                    ExperienceLevel = "Mid Level",
                    Title = "UI/UX & Product Designer",
                    Location = "Mumbai, Maharashtra (On-site)",
                    SalaryMin = 800000,
                    SalaryMax = 1400000,
                    Description = "Innovate Digital is looking for an imaginative UI/UX Designer to build beautiful, intuitive mobile and web app interfaces for enterprise clients.",
                    Requirements = "• Proficient in Figma, Sketch, and Adobe Creative Cloud\n• Strong portfolio demonstrating user journey maps and interactive prototypes\n• Good understanding of design systems and responsive design",
                    Responsibilities = "• Create wireframes, storyboards, and high-fidelity mockups\n• Conduct user research and usability testing\n• Collaborate closely with frontend engineering teams",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-4),
                    Deadline = DateTime.UtcNow.AddDays(30)
                },
                new Job
                {
                    EmployerId = employer2.Id,
                    CategoryId = marketingCategory.Id,
                    JobTypeId = remoteType?.Id,
                    JobType = "Remote",
                    ExperienceLevelId = entryLevel?.Id,
                    ExperienceLevel = "Entry Level",
                    Title = "Growth Marketing & SEO Specialist",
                    Location = "Remote",
                    SalaryMin = 600000,
                    SalaryMax = 1000000,
                    Description = "Drive organic traffic and lead acquisition across multiple digital channels and international campaigns.",
                    Requirements = "• Hands-on experience with Google Analytics, SEMrush, Ahrefs\n• Solid understanding of technical SEO and content marketing\n• Strong analytical and copywriting skills",
                    Responsibilities = "• Execute data-driven SEO and growth marketing strategies\n• Optimize landing pages and conversion funnels\n• Track and report weekly KPIs and campaign ROI",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    Deadline = DateTime.UtcNow.AddDays(15)
                },
                new Job
                {
                    EmployerId = employer1.Id,
                    CategoryId = financeCategory.Id,
                    JobTypeId = fullTimeType?.Id,
                    JobType = "Full-Time",
                    ExperienceLevelId = seniorLevel?.Id,
                    ExperienceLevel = "Senior Level",
                    Title = "Senior Financial Analyst",
                    Location = "Bengaluru, Karnataka",
                    SalaryMin = 1000000,
                    SalaryMax = 1600000,
                    Description = "Lead financial forecasting, variance analysis, and strategic budgeting for high-growth tech projects.",
                    Requirements = "• CA / MBA in Finance with 3+ years in corporate finance\n• Advanced MS Excel & financial modeling skills\n• Strong business acumen and presentation skills",
                    Responsibilities = "• Prepare quarterly financial models and board reports\n• Partner with department heads to manage operational budgets\n• Identify revenue growth and cost-optimization opportunities",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-8),
                    Deadline = DateTime.UtcNow.AddDays(18)
                }
            };

            context.Jobs.AddRange(jobs);
            context.SaveChanges();

            // 8. Seed Sample Applications
            var app1 = new JobApplication
            {
                JobId = jobs[0].Id,
                JobSeekerId = seeker1.Id,
                CoverLetter = "Hello Hiring Team,\n\nI am thrilled to apply for the Senior ASP.NET Core Developer role at TechSolutions Global. With over 4 years of robust experience in building scalable .NET systems and SQL Server databases, I am confident in adding immediate value to your team.\n\nBest regards,\nRahul Verma",
                ResumePath = "/resumes/sample_resume_rahul.pdf",
                ResumeFileName = "Rahul_Verma_Resume.pdf",
                AppliedAt = DateTime.UtcNow.AddDays(-3),
                Status = "Shortlisted",
                EmployerNotes = "Strong technical background in ASP.NET Core. Candidate shortlisted for round 1 technical interview."
            };

            var app2 = new JobApplication
            {
                JobId = jobs[2].Id,
                JobSeekerId = seeker2.Id,
                CoverLetter = "Dear Innovate Digital Team,\n\nI would love the opportunity to contribute my product design and Figma design systems expertise to your dynamic projects. My portfolio highlights several successful SaaS dashboard redesigns.\n\nWarm regards,\nPriya Nair",
                ResumePath = "/resumes/sample_resume_priya.pdf",
                ResumeFileName = "Priya_Nair_Portfolio_Resume.pdf",
                AppliedAt = DateTime.UtcNow.AddDays(-1),
                Status = "Pending",
                EmployerNotes = null
            };

            context.JobApplications.AddRange(app1, app2);

            // 9. Seed Saved Jobs
            var savedJob = new SavedJob
            {
                JobId = jobs[1].Id,
                JobSeekerId = seeker1.Id,
                SavedAt = DateTime.UtcNow.AddDays(-2)
            };
            context.SavedJobs.Add(savedJob);

            context.SaveChanges();
        }

        private static void ApplyStoredProcedures(ApplicationDbContext context)
        {
            try
            {
                var sqlScriptPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Scripts", "StoredProcedures.sql");
                if (!System.IO.File.Exists(sqlScriptPath))
                {
                    // Fallback to project folder
                    sqlScriptPath = System.IO.Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Data", "Scripts", "StoredProcedures.sql");
                }

                if (System.IO.File.Exists(sqlScriptPath))
                {
                    var sql = System.IO.File.ReadAllText(sqlScriptPath);
                    var batches = System.Text.RegularExpressions.Regex.Split(sql, @"^\s*GO\s*$", System.Text.RegularExpressions.RegexOptions.Multiline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                    var connection = Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.GetDbConnection(context.Database);
                    if (connection.State != System.Data.ConnectionState.Open)
                    {
                        connection.Open();
                    }

                    foreach (var batch in batches)
                    {
                        var trimmed = batch.Trim();
                        if (!string.IsNullOrWhiteSpace(trimmed))
                        {
                            using var cmd = connection.CreateCommand();
                            cmd.CommandText = trimmed;
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Silently continue or log - SPs are also created via raw scripts
            }
        }
    }
}
