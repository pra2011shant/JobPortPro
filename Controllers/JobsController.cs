using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using JobPortPro.Models;
using JobPortPro.Services;

namespace JobPortPro.Controllers
{
    public class JobsController : Controller
    {
        private readonly IJobService _jobService;
        private readonly IApplicationService _applicationService;
        private readonly IAuthService _authService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public JobsController(
            IJobService jobService,
            IApplicationService applicationService,
            IAuthService authService,
            IWebHostEnvironment webHostEnvironment)
        {
            _jobService = jobService;
            _applicationService = applicationService;
            _authService = authService;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: /Jobs
        public async Task<IActionResult> Index(
            string? q,
            int? categoryId,
            string? jobType,
            string? location,
            string? experienceLevel,
            decimal? minSalary,
            string? sortBy = "newest",
            int page = 1)
        {
            var model = await _jobService.GetFilteredJobsAsync(
                q, categoryId, jobType, location, experienceLevel, minSalary, sortBy, page, 9);

            return View(model);
        }

        // GET: /Jobs/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var job = await _jobService.GetJobByIdAsync(id);
            if (job == null)
            {
                return NotFound();
            }

            bool hasApplied = false;
            bool isSaved = false;

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                int userId = GetCurrentUserId();
                hasApplied = await _applicationService.HasUserAppliedAsync(userId, id);
                isSaved = await _jobService.IsJobSavedAsync(userId, id);
            }

            ViewBag.HasApplied = hasApplied;
            ViewBag.IsSaved = isSaved;
            ViewBag.RelatedJobs = await _jobService.GetRelatedJobsAsync(job.CategoryId, job.Id, 3);

            return View(job);
        }

        // GET: /Jobs/Apply/5
        [Authorize(Roles = "JobSeeker")]
        [HttpGet]
        public async Task<IActionResult> Apply(int id)
        {
            var job = await _jobService.GetJobByIdAsync(id);
            if (job == null || !job.IsActive)
            {
                return NotFound();
            }

            int userId = GetCurrentUserId();
            bool alreadyApplied = await _applicationService.HasUserAppliedAsync(userId, id);
            if (alreadyApplied)
            {
                TempData["WarningMessage"] = "You have already applied for this job.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var user = await _authService.GetUserByIdAsync(userId);
            var seekerProfile = user?.JobSeekerProfile;

            var model = new ApplyJobViewModel
            {
                JobId = job.Id,
                JobTitle = job.Title,
                CompanyName = job.Employer?.CompanyProfile?.CompanyName ?? "Company",
                Location = job.Location,
                ExistingResumeFileName = seekerProfile?.ResumeFileName,
                UseProfileResume = !string.IsNullOrEmpty(seekerProfile?.ResumeFilePath)
            };

            return View(model);
        }

        // POST: /Jobs/Apply/5
        [Authorize(Roles = "JobSeeker")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(ApplyJobViewModel model)
        {
            var job = await _jobService.GetJobByIdAsync(model.JobId);
            if (job == null || !job.IsActive)
            {
                return NotFound();
            }

            int userId = GetCurrentUserId();
            bool alreadyApplied = await _applicationService.HasUserAppliedAsync(userId, model.JobId);
            if (alreadyApplied)
            {
                TempData["WarningMessage"] = "You have already submitted an application for this position.";
                return RedirectToAction(nameof(Details), new { id = model.JobId });
            }

            var user = await _authService.GetUserByIdAsync(userId);
            var seekerProfile = user?.JobSeekerProfile;

            string resumePath = string.Empty;
            string resumeFileName = string.Empty;

            if (model.UseProfileResume && seekerProfile != null && !string.IsNullOrEmpty(seekerProfile.ResumeFilePath))
            {
                resumePath = seekerProfile.ResumeFilePath;
                resumeFileName = seekerProfile.ResumeFileName ?? "Profile_Resume.pdf";
            }
            else if (model.ResumeFile != null && model.ResumeFile.Length > 0)
            {
                string resumeFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "resumes");
                Directory.CreateDirectory(resumeFolder);
                string uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(model.ResumeFile.FileName)}";
                string fullPath = Path.Combine(resumeFolder, uniqueFileName);
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await model.ResumeFile.CopyToAsync(stream);
                }
                resumePath = $"/uploads/resumes/{uniqueFileName}";
                resumeFileName = model.ResumeFile.FileName;
            }
            else
            {
                ModelState.AddModelError("ResumeFile", "Please upload a resume or select your profile resume.");
                model.ExistingResumeFileName = seekerProfile?.ResumeFileName;
                model.JobTitle = job.Title;
                model.CompanyName = job.Employer?.CompanyProfile?.CompanyName ?? "Company";
                model.Location = job.Location;
                return View(model);
            }

            await _applicationService.SubmitApplicationAsync(userId, model, resumePath, resumeFileName);

            TempData["SuccessMessage"] = $"Your application for '{job.Title}' has been submitted successfully!";
            return RedirectToAction("AppliedJobs", "JobSeeker");
        }

        // POST: /Jobs/ToggleSave/5
        [Authorize(Roles = "JobSeeker")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleSave(int id, string? returnUrl = null)
        {
            int userId = GetCurrentUserId();
            bool added = await _jobService.ToggleSavedJobAsync(userId, id);

            if (added)
            {
                TempData["SuccessMessage"] = "Job saved to your bookmarks!";
            }
            else
            {
                TempData["SuccessMessage"] = "Job removed from your saved bookmarks.";
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdClaim, out int userId) ? userId : 0;
        }
    }
}
