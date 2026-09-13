using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobPortPro.Data;
using JobPortPro.Models;

namespace JobPortPro.Controllers
{
    public class JobsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public JobsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
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
            var query = _context.Jobs
                .Include(j => j.Category)
                .Include(j => j.Employer)
                    .ThenInclude(e => e!.CompanyProfile)
                .Where(j => j.IsActive)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                string searchLower = q.Trim().ToLower();
                query = query.Where(j =>
                    j.Title.ToLower().Contains(searchLower) ||
                    j.Description.ToLower().Contains(searchLower) ||
                    (j.Requirements != null && j.Requirements.ToLower().Contains(searchLower)) ||
                    (j.Employer != null && j.Employer.CompanyProfile != null && j.Employer.CompanyProfile.CompanyName.ToLower().Contains(searchLower)));
            }

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(j => j.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(jobType))
            {
                query = query.Where(j => j.JobType == jobType);
            }

            if (!string.IsNullOrWhiteSpace(location))
            {
                query = query.Where(j => j.Location.ToLower().Contains(location.Trim().ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(experienceLevel))
            {
                query = query.Where(j => j.ExperienceLevel == experienceLevel);
            }

            if (minSalary.HasValue && minSalary.Value > 0)
            {
                query = query.Where(j => j.SalaryMax >= minSalary.Value || j.SalaryMin >= minSalary.Value);
            }

            // Sorting
            query = sortBy switch
            {
                "salary_high" => query.OrderByDescending(j => j.SalaryMax ?? j.SalaryMin ?? 0),
                "salary_low" => query.OrderBy(j => j.SalaryMin ?? j.SalaryMax ?? 0),
                _ => query.OrderByDescending(j => j.CreatedAt)
            };

            int pageSize = 9;
            int totalItems = await query.CountAsync();
            var jobs = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
            var locations = await _context.Jobs
                .Where(j => j.IsActive && !string.IsNullOrEmpty(j.Location))
                .Select(j => j.Location)
                .Distinct()
                .Take(15)
                .ToListAsync();

            var model = new JobFilterViewModel
            {
                Query = q,
                CategoryId = categoryId,
                JobType = jobType,
                Location = location,
                ExperienceLevel = experienceLevel,
                MinSalary = minSalary,
                SortBy = sortBy,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                Jobs = jobs,
                Categories = categories,
                Locations = locations
            };

            return View(model);
        }

        // GET: /Jobs/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var job = await _context.Jobs
                .Include(j => j.Category)
                .Include(j => j.Employer)
                    .ThenInclude(e => e!.CompanyProfile)
                .Include(j => j.Applications)
                .FirstOrDefaultAsync(j => j.Id == id);

            if (job == null)
            {
                return NotFound();
            }

            bool hasApplied = false;
            bool isSaved = false;

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                int userId = GetCurrentUserId();
                hasApplied = await _context.JobApplications.AnyAsync(a => a.JobId == id && a.JobSeekerId == userId);
                isSaved = await _context.SavedJobs.AnyAsync(s => s.JobId == id && s.JobSeekerId == userId);
            }

            ViewBag.HasApplied = hasApplied;
            ViewBag.IsSaved = isSaved;

            // Related jobs in same category
            var relatedJobs = await _context.Jobs
                .Include(j => j.Employer)
                    .ThenInclude(e => e!.CompanyProfile)
                .Where(j => j.CategoryId == job.CategoryId && j.Id != job.Id && j.IsActive)
                .OrderByDescending(j => j.CreatedAt)
                .Take(3)
                .ToListAsync();

            ViewBag.RelatedJobs = relatedJobs;

            return View(job);
        }

        // GET: /Jobs/Apply/5
        [Authorize(Roles = "JobSeeker")]
        [HttpGet]
        public async Task<IActionResult> Apply(int id)
        {
            var job = await _context.Jobs
                .Include(j => j.Employer)
                    .ThenInclude(e => e!.CompanyProfile)
                .FirstOrDefaultAsync(j => j.Id == id && j.IsActive);

            if (job == null)
            {
                return NotFound();
            }

            int userId = GetCurrentUserId();
            bool alreadyApplied = await _context.JobApplications.AnyAsync(a => a.JobId == id && a.JobSeekerId == userId);
            if (alreadyApplied)
            {
                TempData["WarningMessage"] = "You have already applied for this job.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var seekerProfile = await _context.JobSeekerProfiles.FirstOrDefaultAsync(p => p.UserId == userId);

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
            var job = await _context.Jobs
                .Include(j => j.Employer)
                    .ThenInclude(e => e!.CompanyProfile)
                .FirstOrDefaultAsync(j => j.Id == model.JobId && j.IsActive);

            if (job == null)
            {
                return NotFound();
            }

            int userId = GetCurrentUserId();
            bool alreadyApplied = await _context.JobApplications.AnyAsync(a => a.JobId == model.JobId && a.JobSeekerId == userId);
            if (alreadyApplied)
            {
                TempData["WarningMessage"] = "You have already submitted an application for this position.";
                return RedirectToAction(nameof(Details), new { id = model.JobId });
            }

            var seekerProfile = await _context.JobSeekerProfiles.FirstOrDefaultAsync(p => p.UserId == userId);

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

                // Update profile resume if not already set
                if (seekerProfile != null && string.IsNullOrEmpty(seekerProfile.ResumeFilePath))
                {
                    seekerProfile.ResumeFilePath = resumePath;
                    seekerProfile.ResumeFileName = resumeFileName;
                }
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

            var application = new JobApplication
            {
                JobId = model.JobId,
                JobSeekerId = userId,
                CoverLetter = model.CoverLetter?.Trim(),
                ResumePath = resumePath,
                ResumeFileName = resumeFileName,
                AppliedAt = DateTime.UtcNow,
                Status = "Pending"
            };

            _context.JobApplications.Add(application);
            await _context.SaveChangesAsync();

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
            var saved = await _context.SavedJobs.FirstOrDefaultAsync(s => s.JobId == id && s.JobSeekerId == userId);

            if (saved != null)
            {
                _context.SavedJobs.Remove(saved);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Job removed from your saved bookmarks.";
            }
            else
            {
                var newSave = new SavedJob
                {
                    JobId = id,
                    JobSeekerId = userId,
                    SavedAt = DateTime.UtcNow
                };
                _context.SavedJobs.Add(newSave);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Job saved to your bookmarks!";
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
