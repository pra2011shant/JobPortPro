using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using JobPortPro.Models;
using JobPortPro.Services;

namespace JobPortPro.Controllers
{
    [Authorize(Roles = "Employer")]
    public class EmployerController : Controller
    {
        private readonly IJobService _jobService;
        private readonly IApplicationService _applicationService;

        public EmployerController(IJobService jobService, IApplicationService applicationService)
        {
            _jobService = jobService;
            _applicationService = applicationService;
        }

        // GET: /Employer/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            int employerId = GetCurrentUserId();

            var postedJobs = await _jobService.GetJobsByEmployerAsync(employerId);
            var applications = await _applicationService.GetApplicationsByEmployerAsync(employerId);

            var model = new EmployerDashboardViewModel
            {
                TotalJobsPosted = postedJobs.Count,
                ActiveJobs = postedJobs.FindAll(j => j.IsActive).Count,
                TotalApplications = applications.Count,
                ShortlistedApplications = applications.FindAll(a => a.Status == "Shortlisted" || a.Status == "Accepted").Count,
                RecentJobs = postedJobs.Count > 5 ? postedJobs.GetRange(0, 5) : postedJobs,
                RecentApplications = applications.Count > 5 ? applications.GetRange(0, 5) : applications
            };

            return View(model);
        }

        // GET: /Employer/ManageJobs
        public async Task<IActionResult> ManageJobs(string? status)
        {
            int employerId = GetCurrentUserId();
            var jobs = await _jobService.GetJobsByEmployerAsync(employerId, status);
            ViewBag.CurrentStatus = status;
            return View(jobs);
        }

        // GET: /Employer/PostJob
        [HttpGet]
        public async Task<IActionResult> PostJob()
        {
            var categories = await _jobService.GetAllCategoriesAsync();
            var model = new PostJobViewModel
            {
                AvailableCategories = categories,
                Deadline = DateTime.UtcNow.AddDays(30)
            };
            return View(model);
        }

        // POST: /Employer/PostJob
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostJob(PostJobViewModel model)
        {
            int employerId = GetCurrentUserId();

            if (ModelState.IsValid)
            {
                var job = await _jobService.CreateJobAsync(employerId, model);
                TempData["SuccessMessage"] = $"Job posting '{job.Title}' has been published successfully!";
                return RedirectToAction(nameof(ManageJobs));
            }

            model.AvailableCategories = await _jobService.GetAllCategoriesAsync();
            return View(model);
        }

        // GET: /Employer/EditJob/5
        [HttpGet]
        public async Task<IActionResult> EditJob(int id)
        {
            int employerId = GetCurrentUserId();
            var job = await _jobService.GetJobByIdAsync(id);

            if (job == null || job.EmployerId != employerId)
            {
                return NotFound();
            }

            var categories = await _jobService.GetAllCategoriesAsync();
            var model = new PostJobViewModel
            {
                Id = job.Id,
                Title = job.Title,
                CategoryId = job.CategoryId,
                JobType = job.JobType,
                Location = job.Location,
                SalaryMin = job.SalaryMin,
                SalaryMax = job.SalaryMax,
                ExperienceLevel = job.ExperienceLevel,
                Description = job.Description,
                Requirements = job.Requirements,
                Responsibilities = job.Responsibilities,
                IsActive = job.IsActive,
                Deadline = job.Deadline,
                AvailableCategories = categories
            };

            return View(model);
        }

        // POST: /Employer/EditJob/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditJob(PostJobViewModel model)
        {
            if (!model.Id.HasValue) return NotFound();

            int employerId = GetCurrentUserId();

            if (ModelState.IsValid)
            {
                bool updated = await _jobService.UpdateJobAsync(employerId, model);
                if (!updated) return NotFound();

                TempData["SuccessMessage"] = $"Job posting '{model.Title}' updated successfully!";
                return RedirectToAction(nameof(ManageJobs));
            }

            model.AvailableCategories = await _jobService.GetAllCategoriesAsync();
            return View(model);
        }

        // POST: /Employer/ToggleJobStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleJobStatus(int id)
        {
            int employerId = GetCurrentUserId();
            bool toggled = await _jobService.ToggleJobStatusAsync(employerId, id);
            if (toggled)
            {
                TempData["SuccessMessage"] = "Job status updated successfully.";
            }

            return RedirectToAction(nameof(ManageJobs));
        }

        // POST: /Employer/DeleteJob/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteJob(int id)
        {
            int employerId = GetCurrentUserId();
            bool deleted = await _jobService.DeleteJobAsync(employerId, id);
            if (deleted)
            {
                TempData["SuccessMessage"] = "Job posting has been deleted.";
            }

            return RedirectToAction(nameof(ManageJobs));
        }

        // GET: /Employer/Applicants
        public async Task<IActionResult> Applicants(int? jobId, string? status)
        {
            int employerId = GetCurrentUserId();
            var applications = await _applicationService.GetApplicationsByEmployerAsync(employerId, jobId, status);
            var employerJobs = await _jobService.GetJobsByEmployerAsync(employerId);

            ViewBag.EmployerJobs = employerJobs;
            ViewBag.SelectedJobId = jobId;
            ViewBag.SelectedStatus = status;

            return View(applications);
        }

        // GET: /Employer/ApplicantDetail/5
        public async Task<IActionResult> ApplicantDetail(int id)
        {
            int employerId = GetCurrentUserId();
            var app = await _applicationService.GetApplicationDetailAsync(employerId, id);

            if (app == null)
            {
                return NotFound();
            }

            var model = new ApplicantDetailViewModel
            {
                ApplicationId = app.Id,
                JobId = app.JobId,
                JobTitle = app.Job?.Title ?? "Unknown Job",
                ApplicantName = app.JobSeeker?.FullName ?? "Unknown Applicant",
                ApplicantEmail = app.JobSeeker?.Email ?? "",
                ApplicantPhone = app.JobSeeker?.PhoneNumber,
                Headline = app.JobSeeker?.JobSeekerProfile?.Headline,
                Skills = app.JobSeeker?.JobSeekerProfile?.Skills,
                ExperienceYears = app.JobSeeker?.JobSeekerProfile?.ExperienceYears,
                Education = app.JobSeeker?.JobSeekerProfile?.Education,
                GitHubUrl = app.JobSeeker?.JobSeekerProfile?.GitHubUrl,
                LinkedInUrl = app.JobSeeker?.JobSeekerProfile?.LinkedInUrl,
                CoverLetter = app.CoverLetter,
                ResumePath = app.ResumePath,
                ResumeFileName = app.ResumeFileName,
                AppliedAt = app.AppliedAt,
                Status = app.Status,
                EmployerNotes = app.EmployerNotes
            };

            return View(model);
        }

        // POST: /Employer/UpdateApplicationStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateApplicationStatus(int applicationId, string status, string? notes)
        {
            int employerId = GetCurrentUserId();
            bool updated = await _applicationService.UpdateApplicationStatusAsync(employerId, applicationId, status, notes);

            if (updated)
            {
                TempData["SuccessMessage"] = $"Applicant status successfully updated to '{status}'!";
            }

            return RedirectToAction(nameof(ApplicantDetail), new { id = applicationId });
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdClaim, out int userId) ? userId : 0;
        }
    }
}
