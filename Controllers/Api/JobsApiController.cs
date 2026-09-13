using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using JobPortPro.Models;
using JobPortPro.Services;

namespace JobPortPro.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobsApiController : ControllerBase
    {
        private readonly IJobService _jobService;

        public JobsApiController(IJobService jobService)
        {
            _jobService = jobService;
        }

        // GET: /api/jobsapi
        [HttpGet]
        public async Task<IActionResult> GetJobs(
            [FromQuery] string? q,
            [FromQuery] int? categoryId,
            [FromQuery] string? jobType,
            [FromQuery] string? location,
            [FromQuery] string? experienceLevel,
            [FromQuery] decimal? minSalary,
            [FromQuery] string? sortBy,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _jobService.GetFilteredJobsAsync(
                q, categoryId, jobType, location, experienceLevel, minSalary, sortBy, page, pageSize);

            var jobsData = result.Jobs.Select(j => new
            {
                j.Id,
                j.Title,
                Category = j.Category?.Name,
                Company = j.Employer?.CompanyProfile?.CompanyName,
                j.JobType,
                j.Location,
                j.SalaryMin,
                j.SalaryMax,
                j.ExperienceLevel,
                j.Description,
                j.CreatedAt,
                j.Deadline
            });

            return Ok(new
            {
                status = "success",
                totalItems = result.TotalItems,
                totalPages = result.TotalPages,
                page,
                pageSize,
                data = jobsData
            });
        }

        // GET: /api/jobsapi/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetJobById(int id)
        {
            var job = await _jobService.GetJobByIdAsync(id);
            if (job == null)
            {
                return NotFound(new { status = "error", message = "Job not found" });
            }

            return Ok(new
            {
                status = "success",
                data = new
                {
                    job.Id,
                    job.Title,
                    Category = job.Category?.Name,
                    Company = job.Employer?.CompanyProfile?.CompanyName,
                    CompanyDescription = job.Employer?.CompanyProfile?.Description,
                    CompanyWebsite = job.Employer?.CompanyProfile?.Website,
                    job.JobType,
                    job.Location,
                    job.SalaryMin,
                    job.SalaryMax,
                    job.ExperienceLevel,
                    job.Description,
                    job.Requirements,
                    job.Responsibilities,
                    job.CreatedAt,
                    job.Deadline
                }
            });
        }

        // GET: /api/jobsapi/categories
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _jobService.GetAllCategoriesAsync();
            var data = categories.Select(c => new
            {
                c.Id,
                c.Name,
                c.IconClass,
                c.Description,
                ActiveJobsCount = c.Jobs?.Count ?? 0
            });

            return Ok(new { status = "success", data });
        }
    }
}
