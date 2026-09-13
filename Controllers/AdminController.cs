using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobPortPro.Data;
using JobPortPro.Models;
using JobPortPro.Services;

namespace JobPortPro.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IJobService _jobService;
        private readonly ILookupService _lookupService;

        public AdminController(
            ApplicationDbContext context,
            IJobService jobService,
            ILookupService lookupService)
        {
            _context = context;
            _jobService = jobService;
            _lookupService = lookupService;
        }

        // GET: /Admin/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var model = new AdminDashboardViewModel
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalJobSeekers = await _context.Users.CountAsync(u => u.Role == "JobSeeker"),
                TotalEmployers = await _context.Users.CountAsync(u => u.Role == "Employer"),
                TotalJobs = await _context.Jobs.CountAsync(),
                ActiveJobs = await _context.Jobs.CountAsync(j => j.IsActive),
                TotalApplications = await _context.JobApplications.CountAsync(),
                RecentUsers = await _context.Users.OrderByDescending(u => u.CreatedAt).Take(5).ToListAsync(),
                RecentJobs = await _context.Jobs
                    .Include(j => j.Category)
                    .Include(j => j.Employer)
                        .ThenInclude(e => e!.CompanyProfile)
                    .OrderByDescending(j => j.CreatedAt)
                    .Take(5)
                    .ToListAsync(),
                RecentApplications = await _context.JobApplications
                    .Include(a => a.Job)
                    .Include(a => a.JobSeeker)
                    .OrderByDescending(a => a.AppliedAt)
                    .Take(5)
                    .ToListAsync()
            };

            return View(model);
        }

        // GET: /Admin/Users
        public async Task<IActionResult> Users(string? search, string? role)
        {
            var query = _context.Users
                .Include(u => u.CompanyProfile)
                .Include(u => u.JobSeekerProfile)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                string s = search.Trim().ToLower();
                query = query.Where(u => u.FullName.ToLower().Contains(s) || u.Email.ToLower().Contains(s));
            }

            if (!string.IsNullOrWhiteSpace(role) && role != "All")
            {
                query = query.Where(u => u.Role == role);
            }

            var users = await query.OrderByDescending(u => u.CreatedAt).ToListAsync();

            var model = new AdminUsersViewModel
            {
                SearchQuery = search,
                RoleFilter = role ?? "All",
                Users = users
            };

            return View(model);
        }

        // GET: /Admin/Jobs
        public async Task<IActionResult> Jobs(string? search, int? categoryId)
        {
            var query = _context.Jobs
                .Include(j => j.Category)
                .Include(j => j.JobTypeEntity)
                .Include(j => j.Employer)
                    .ThenInclude(e => e!.CompanyProfile)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                string s = search.Trim().ToLower();
                query = query.Where(j => j.Title.ToLower().Contains(s) || j.Location.ToLower().Contains(s));
            }

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(j => j.CategoryId == categoryId.Value);
            }

            var jobs = await query.OrderByDescending(j => j.CreatedAt).ToListAsync();
            var categories = await _lookupService.GetCategoriesAsync();

            var model = new AdminJobsViewModel
            {
                SearchQuery = search,
                CategoryId = categoryId,
                Jobs = jobs,
                Categories = categories
            };

            return View(model);
        }

        // POST: /Admin/ToggleJobStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleJobStatus(int id)
        {
            var job = await _context.Jobs.FindAsync(id);
            if (job != null)
            {
                job.IsActive = !job.IsActive;
                job.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Job '{job.Title}' status toggled to {(job.IsActive ? "Active" : "Closed")}.";
            }
            return RedirectToAction(nameof(Jobs));
        }

        // POST: /Admin/DeleteJob/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteJob(int id)
        {
            var job = await _context.Jobs.FindAsync(id);
            if (job != null)
            {
                _context.Jobs.Remove(job);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Job has been permanently deleted.";
            }
            return RedirectToAction(nameof(Jobs));
        }

        // POST: /Admin/DeleteUser/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                if (user.Role == "Admin" && await _context.Users.CountAsync(u => u.Role == "Admin") <= 1)
                {
                    TempData["ErrorMessage"] = "Cannot delete the sole Administrator account.";
                    return RedirectToAction(nameof(Users));
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"User '{user.FullName}' ({user.Email}) has been deleted.";
            }
            return RedirectToAction(nameof(Users));
        }
    }
}
