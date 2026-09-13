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

        public JobSeekerController(IJobService jobService, IApplicationService applicationService)
        {
            _jobService = jobService;
            _applicationService = applicationService;
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
