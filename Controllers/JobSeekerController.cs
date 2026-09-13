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
    [Authorize(Roles = "JobSeeker")]
    public class JobSeekerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public JobSeekerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /JobSeeker/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            int seekerId = GetCurrentUserId();

            var applications = await _context.JobApplications
                .Include(a => a.Job)
                    .ThenInclude(j => j!.Employer)
                        .ThenInclude(e => e!.CompanyProfile)
                .Include(a => a.Job)
                    .ThenInclude(j => j!.Category)
                .Where(a => a.JobSeekerId == seekerId)
                .OrderByDescending(a => a.AppliedAt)
                .ToListAsync();

            var savedJobs = await _context.SavedJobs
                .Include(s => s.Job)
                    .ThenInclude(j => j!.Employer)
                        .ThenInclude(e => e!.CompanyProfile)
                .Include(s => s.Job)
                    .ThenInclude(j => j!.Category)
                .Where(s => s.JobSeekerId == seekerId)
                .OrderByDescending(s => s.SavedAt)
                .ToListAsync();

            var appliedJobIds = applications.Select(a => a.JobId).ToList();

            var recommendedJobs = await _context.Jobs
                .Include(j => j.Category)
                .Include(j => j.Employer)
                    .ThenInclude(e => e!.CompanyProfile)
                .Where(j => j.IsActive && !appliedJobIds.Contains(j.Id))
                .OrderByDescending(j => j.CreatedAt)
                .Take(4)
                .ToListAsync();

            var model = new JobSeekerDashboardViewModel
            {
                TotalApplications = applications.Count,
                PendingApplications = applications.Count(a => a.Status == "Pending" || a.Status == "Reviewed"),
                ShortlistedApplications = applications.Count(a => a.Status == "Shortlisted" || a.Status == "Accepted"),
                SavedJobsCount = savedJobs.Count,
                RecentApplications = applications.Take(5).ToList(),
                SavedJobs = savedJobs.Take(4).ToList(),
                RecommendedJobs = recommendedJobs
            };

            return View(model);
        }

        // GET: /JobSeeker/AppliedJobs
        public async Task<IActionResult> AppliedJobs(string? status)
        {
            int seekerId = GetCurrentUserId();

            var query = _context.JobApplications
                .Include(a => a.Job)
                    .ThenInclude(j => j!.Employer)
                        .ThenInclude(e => e!.CompanyProfile)
                .Include(a => a.Job)
                    .ThenInclude(j => j!.Category)
                .Where(a => a.JobSeekerId == seekerId);

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(a => a.Status == status);
            }

            var applications = await query.OrderByDescending(a => a.AppliedAt).ToListAsync();
            ViewBag.CurrentStatus = status;

            return View(applications);
        }

        // GET: /JobSeeker/SavedJobs
        public async Task<IActionResult> SavedJobs()
        {
            int seekerId = GetCurrentUserId();

            var savedJobs = await _context.SavedJobs
                .Include(s => s.Job)
                    .ThenInclude(j => j!.Employer)
                        .ThenInclude(e => e!.CompanyProfile)
                .Include(s => s.Job)
                    .ThenInclude(j => j!.Category)
                .Where(s => s.JobSeekerId == seekerId)
                .OrderByDescending(s => s.SavedAt)
                .ToListAsync();

            return View(savedJobs);
        }

        // POST: /JobSeeker/WithdrawApplication/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> WithdrawApplication(int id)
        {
            int seekerId = GetCurrentUserId();

            var app = await _context.JobApplications
                .Include(a => a.Job)
                .FirstOrDefaultAsync(a => a.Id == id && a.JobSeekerId == seekerId);

            if (app != null)
            {
                _context.JobApplications.Remove(app);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Your application for '{app.Job?.Title}' has been withdrawn.";
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
