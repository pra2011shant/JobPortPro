using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using JobPortPro.Data;
using JobPortPro.Models;
using JobPortPro.Services;

namespace JobPortPro.Controllers
{
    public class HomeController : Controller
    {
        private readonly IJobService _jobService;
        private readonly ILookupService _lookupService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            IJobService jobService,
            ILookupService lookupService,
            ApplicationDbContext context,
            ILogger<HomeController> logger)
        {
            _jobService = jobService;
            _lookupService = lookupService;
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _lookupService.GetCategoriesAsync();
            var jobTypes = await _lookupService.GetJobTypesAsync();
            var featuredJobs = await _jobService.GetFeaturedJobsAsync(6);
            var recentJobs = await _jobService.GetRecentJobsAsync(4);

            var model = new HomeIndexViewModel
            {
                Categories = categories,
                JobTypes = jobTypes,
                FeaturedJobs = featuredJobs,
                RecentJobs = recentJobs,
                TotalJobs = await _jobService.GetFilteredJobsAsync(null, null, null, null, null, null, null, 1, 1).ContinueWith(t => t.Result.TotalItems),
                TotalCompanies = _context.CompanyProfiles.Count(),
                TotalCandidates = _context.Users.Count(u => u.Role == "JobSeeker"),
                TotalApplications = _context.JobApplications.Count()
            };

            return View(model);
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Contact(string name, string email, string subject, string message)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(message))
            {
                ModelState.AddModelError(string.Empty, "Please fill in all required fields.");
                return View();
            }

            TempData["SuccessMessage"] = "Thank you for reaching out! We have received your message and will respond shortly.";
            return RedirectToAction(nameof(Contact));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
