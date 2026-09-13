using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobPortPro.Data;
using JobPortPro.Models;

namespace JobPortPro.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
                .Include(c => c.Jobs.Where(j => j.IsActive))
                .ToListAsync();

            var featuredJobs = await _context.Jobs
                .Include(j => j.Category)
                .Include(j => j.Employer)
                    .ThenInclude(e => e!.CompanyProfile)
                .Where(j => j.IsActive)
                .OrderByDescending(j => j.CreatedAt)
                .Take(6)
                .ToListAsync();

            var recentJobs = await _context.Jobs
                .Include(j => j.Category)
                .Include(j => j.Employer)
                    .ThenInclude(e => e!.CompanyProfile)
                .Where(j => j.IsActive)
                .OrderByDescending(j => j.CreatedAt)
                .Take(4)
                .ToListAsync();

            var model = new HomeIndexViewModel
            {
                Categories = categories,
                FeaturedJobs = featuredJobs,
                RecentJobs = recentJobs,
                TotalJobs = await _context.Jobs.CountAsync(j => j.IsActive),
                TotalCompanies = await _context.CompanyProfiles.CountAsync(),
                TotalCandidates = await _context.Users.CountAsync(u => u.Role == "JobSeeker"),
                TotalApplications = await _context.JobApplications.CountAsync()
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
