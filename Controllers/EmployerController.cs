using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobPortPro.Data;
using JobPortPro.Models;

namespace JobPortPro.Controllers
{
    [Authorize(Roles = "Employer")]
    public class EmployerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmployerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Employer/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            int employerId = GetCurrentUserId();

            var postedJobs = await _context.Jobs
                .Include(j => j.Category)
                .Include(j => j.Applications)
                .Where(j => j.EmployerId == employerId)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();

            var applications = await _context.JobApplications
                .Include(a => a.Job)
                .Include(a => a.JobSeeker)
                    .ThenInclude(u => u!.JobSeekerProfile)
                .Where(a => a.Job != null && a.Job.EmployerId == employerId)
                .OrderByDescending(a => a.AppliedAt)
                .ToListAsync();

            var model = new EmployerDashboardViewModel
            {
                TotalJobsPosted = postedJobs.Count,
                ActiveJobs = postedJobs.Count(j => j.IsActive),
                TotalApplications = applications.Count,
                ShortlistedApplications = applications.Count(a => a.Status == "Shortlisted" || a.Status == "Accepted"),
                RecentJobs = postedJobs.Take(5).ToList(),
                RecentApplications = applications.Take(5).ToList()
            };

            return View(model);
        }

        // GET: /Employer/ManageJobs
        public async Task<IActionResult> ManageJobs(string? status)
        {
            int employerId = GetCurrentUserId();

            var query = _context.Jobs
                .Include(j => j.Category)
                .Include(j => j.Applications)
                .Where(j => j.EmployerId == employerId);

            if (status == "active")
            {
                query = query.Where(j => j.IsActive);
            }
            else if (status == "inactive")
            {
                query = query.Where(j => !j.IsActive);
            }

            var jobs = await query.OrderByDescending(j => j.CreatedAt).ToListAsync();
            ViewBag.CurrentStatus = status;
            return View(jobs);
        }

        // GET: /Employer/PostJob
        [HttpGet]
        public async Task<IActionResult> PostJob()
        {
            var categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
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
                var job = new Job
                {
                    EmployerId = employerId,
                    Title = model.Title.Trim(),
                    CategoryId = model.CategoryId,
                    JobType = model.JobType,
                    Location = model.Location.Trim(),
                    SalaryMin = model.SalaryMin,
                    SalaryMax = model.SalaryMax,
                    ExperienceLevel = model.ExperienceLevel,
                    Description = model.Description.Trim(),
                    Requirements = model.Requirements?.Trim(),
                    Responsibilities = model.Responsibilities?.Trim(),
                    IsActive = model.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    Deadline = model.Deadline
                };

                _context.Jobs.Add(job);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Job posting '{job.Title}' has been published successfully!";
                return RedirectToAction(nameof(ManageJobs));
            }

            model.AvailableCategories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
            return View(model);
        }

        // GET: /Employer/EditJob/5
        [HttpGet]
        public async Task<IActionResult> EditJob(int id)
        {
            int employerId = GetCurrentUserId();
            var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == id && j.EmployerId == employerId);

            if (job == null)
            {
                return NotFound();
            }

            var categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
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
            if (!model.Id.HasValue)
            {
                return NotFound();
            }

            int employerId = GetCurrentUserId();
            var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == model.Id.Value && j.EmployerId == employerId);

            if (job == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                job.Title = model.Title.Trim();
                job.CategoryId = model.CategoryId;
                job.JobType = model.JobType;
                job.Location = model.Location.Trim();
                job.SalaryMin = model.SalaryMin;
                job.SalaryMax = model.SalaryMax;
                job.ExperienceLevel = model.ExperienceLevel;
                job.Description = model.Description.Trim();
                job.Requirements = model.Requirements?.Trim();
                job.Responsibilities = model.Responsibilities?.Trim();
                job.IsActive = model.IsActive;
                job.Deadline = model.Deadline;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Job posting '{job.Title}' updated successfully!";
                return RedirectToAction(nameof(ManageJobs));
            }

            model.AvailableCategories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
            return View(model);
        }

        // POST: /Employer/ToggleJobStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleJobStatus(int id)
        {
            int employerId = GetCurrentUserId();
            var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == id && j.EmployerId == employerId);

            if (job != null)
            {
                job.IsActive = !job.IsActive;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Job '{job.Title}' status changed to {(job.IsActive ? "Active" : "Closed")}.";
            }

            return RedirectToAction(nameof(ManageJobs));
        }

        // POST: /Employer/DeleteJob/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteJob(int id)
        {
            int employerId = GetCurrentUserId();
            var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == id && j.EmployerId == employerId);

            if (job != null)
            {
                _context.Jobs.Remove(job);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Job '{job.Title}' has been deleted.";
            }

            return RedirectToAction(nameof(ManageJobs));
        }

        // GET: /Employer/Applicants
        public async Task<IActionResult> Applicants(int? jobId, string? status)
        {
            int employerId = GetCurrentUserId();

            var query = _context.JobApplications
                .Include(a => a.Job)
                .Include(a => a.JobSeeker)
                    .ThenInclude(u => u!.JobSeekerProfile)
                .Where(a => a.Job != null && a.Job.EmployerId == employerId);

            if (jobId.HasValue && jobId.Value > 0)
            {
                query = query.Where(a => a.JobId == jobId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(a => a.Status == status);
            }

            var applications = await query.OrderByDescending(a => a.AppliedAt).ToListAsync();

            var employerJobs = await _context.Jobs
                .Where(j => j.EmployerId == employerId)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();

            ViewBag.EmployerJobs = employerJobs;
            ViewBag.SelectedJobId = jobId;
            ViewBag.SelectedStatus = status;

            return View(applications);
        }

        // GET: /Employer/ApplicantDetail/5
        public async Task<IActionResult> ApplicantDetail(int id)
        {
            int employerId = GetCurrentUserId();

            var app = await _context.JobApplications
                .Include(a => a.Job)
                .Include(a => a.JobSeeker)
                    .ThenInclude(u => u!.JobSeekerProfile)
                .FirstOrDefaultAsync(a => a.Id == id && a.Job != null && a.Job.EmployerId == employerId);

            if (app == null)
            {
                return NotFound();
            }

            // Auto-mark as "Reviewed" if it was "Pending"
            if (app.Status == "Pending")
            {
                app.Status = "Reviewed";
                await _context.SaveChangesAsync();
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

            var app = await _context.JobApplications
                .Include(a => a.Job)
                .FirstOrDefaultAsync(a => a.Id == applicationId && a.Job != null && a.Job.EmployerId == employerId);

            if (app != null)
            {
                app.Status = status;
                if (!string.IsNullOrEmpty(notes))
                {
                    app.EmployerNotes = notes;
                }
                await _context.SaveChangesAsync();
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
