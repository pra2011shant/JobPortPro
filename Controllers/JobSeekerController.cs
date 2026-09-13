using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using JobPortPro.Models;
using JobPortPro.Services;

namespace JobPortPro.Controllers
{
    [Authorize(Roles = "JobSeeker")]
    public class JobSeekerController : Controller
    {
        private readonly IJobService _jobService;
        private readonly IApplicationService _applicationService;
        private readonly IAuthService _authService;

        public JobSeekerController(
            IJobService jobService, 
            IApplicationService applicationService,
            IAuthService authService)
        {
            _jobService = jobService;
            _applicationService = applicationService;
            _authService = authService;
        }

        // GET: /JobSeeker/ResumeBuilder
        [HttpGet]
        public async Task<IActionResult> ResumeBuilder()
        {
            int seekerId = GetCurrentUserId();
            var user = await _authService.GetUserByIdAsync(seekerId);
            if (user == null) return NotFound();

            var model = new ResumeBuilderViewModel
            {
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Headline = user.JobSeekerProfile?.Headline ?? "Software Professional",
                Summary = user.Bio ?? "Experienced software engineer dedicated to building scalable and robust web applications.",
                Skills = user.JobSeekerProfile?.Skills ?? "C#, ASP.NET Core, SQL Server, Entity Framework, JavaScript, HTML5, CSS3, REST APIs, Git",
                EducationDetails = user.JobSeekerProfile?.Education ?? "Bachelor of Technology in Computer Science",
                ExperienceDetails = $"• Software Engineer at Enterprise Tech (2022 - Present)\n  - Developed high-throughput REST APIs and MVC web applications.\n  - Optimized SQL Server stored procedures and database queries.\n• Associate Developer (2020 - 2022)\n  - Built responsive UI components using Bootstrap 5 and JavaScript.",
                ProjectsDetails = "• JobPortPro: Enterprise Job Portal built with ASP.NET Core MVC & SQL Server Stored Procedures.\n• Cloud Inventory System: Distributed microservices application with Azure deployment.",
                Certifications = "Microsoft Certified: Azure Fundamentals (AZ-900)\nASP.NET Core Architecture Specialization",
                GitHubUrl = user.JobSeekerProfile?.GitHubUrl ?? "https://github.com/",
                LinkedInUrl = user.JobSeekerProfile?.LinkedInUrl ?? "https://linkedin.com/in/"
            };

            return View(model);
        }

        // POST: /JobSeeker/ResumeBuilder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResumeBuilder(ResumeBuilderViewModel model)
        {
            if (ModelState.IsValid)
            {
                int seekerId = GetCurrentUserId();
                var profileModel = new ProfileViewModel
                {
                    UserId = seekerId,
                    FullName = model.FullName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    Bio = model.Summary,
                    Headline = model.Headline,
                    Skills = model.Skills,
                    Education = model.EducationDetails,
                    GitHubUrl = model.GitHubUrl,
                    LinkedInUrl = model.LinkedInUrl
                };

                await _authService.UpdateProfileAsync(seekerId, profileModel, null, null, null);
                TempData["SuccessMessage"] = "Your resume and profile skills have been synchronized successfully!";
            }
            return View(model);
        }

        // GET: /JobSeeker/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            int seekerId = GetCurrentUserId();

            var applications = await _applicationService.GetApplicationsBySeekerAsync(seekerId);
            var savedJobs = await _jobService.GetSavedJobsAsync(seekerId);
            var featuredJobs = await _jobService.GetFeaturedJobsAsync(4);

            var model = new JobSeekerDashboardViewModel
            {
                TotalApplications = applications.Count,
                PendingApplications = applications.FindAll(a => a.Status == "Pending" || a.Status == "Reviewed").Count,
                ShortlistedApplications = applications.FindAll(a => a.Status == "Shortlisted" || a.Status == "Accepted").Count,
                SavedJobsCount = savedJobs.Count,
                RecentApplications = applications.Count > 5 ? applications.GetRange(0, 5) : applications,
                SavedJobs = savedJobs.Count > 4 ? savedJobs.GetRange(0, 4) : savedJobs,
                RecommendedJobs = featuredJobs
            };

            return View(model);
        }

        // GET: /JobSeeker/AppliedJobs
        public async Task<IActionResult> AppliedJobs(string? status)
        {
            int seekerId = GetCurrentUserId();
            var applications = await _applicationService.GetApplicationsBySeekerAsync(seekerId, status);
            ViewBag.CurrentStatus = status;
            return View(applications);
        }

        // GET: /JobSeeker/SavedJobs
        public async Task<IActionResult> SavedJobs()
        {
            int seekerId = GetCurrentUserId();
            var savedJobs = await _jobService.GetSavedJobsAsync(seekerId);
            return View(savedJobs);
        }

        // POST: /JobSeeker/WithdrawApplication/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> WithdrawApplication(int id)
        {
            int seekerId = GetCurrentUserId();
            bool withdrawn = await _applicationService.WithdrawApplicationAsync(seekerId, id);

            if (withdrawn)
            {
                TempData["SuccessMessage"] = "Your application has been withdrawn.";
            }

            return RedirectToAction(nameof(AppliedJobs));
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdClaim, out int userId) ? userId : 0;
        }
    }
}
