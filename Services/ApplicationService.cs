using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using JobPortPro.Data;
using JobPortPro.Models;

namespace JobPortPro.Services
{
    public class ApplicationService : IApplicationService
    {
        private readonly ApplicationDbContext _context;

        public ApplicationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<JobApplication> SubmitApplicationAsync(int seekerId, ApplyJobViewModel model, string resumePath, string resumeFileName)
        {
            var application = new JobApplication
            {
                JobId = model.JobId,
                JobSeekerId = seekerId,
                CoverLetter = model.CoverLetter?.Trim(),
                ResumePath = resumePath,
                ResumeFileName = resumeFileName,
                AppliedAt = DateTime.UtcNow,
                Status = "Pending"
            };

            _context.JobApplications.Add(application);
            await _context.SaveChangesAsync();
            return application;
        }

        public async Task<bool> HasUserAppliedAsync(int seekerId, int jobId)
        {
            return await _context.JobApplications.AnyAsync(a => a.JobId == jobId && a.JobSeekerId == seekerId);
        }

        public async Task<List<JobApplication>> GetApplicationsBySeekerAsync(int seekerId, string? status = null)
        {
            var q = _context.JobApplications
                .Include(a => a.Job)
                    .ThenInclude(j => j!.Employer)
                        .ThenInclude(e => e!.CompanyProfile)
                .Include(a => a.Job)
                    .ThenInclude(j => j!.Category)
                .Where(a => a.JobSeekerId == seekerId);

            if (!string.IsNullOrWhiteSpace(status))
            {
                q = q.Where(a => a.Status == status);
            }

            return await q.OrderByDescending(a => a.AppliedAt).ToListAsync();
        }

        public async Task<List<JobApplication>> GetApplicationsByEmployerAsync(int employerId, int? jobId = null, string? status = null)
        {
            var q = _context.JobApplications
                .Include(a => a.Job)
                .Include(a => a.JobSeeker)
                    .ThenInclude(u => u!.JobSeekerProfile)
                .Where(a => a.Job != null && a.Job.EmployerId == employerId);

            if (jobId.HasValue && jobId.Value > 0)
            {
                q = q.Where(a => a.JobId == jobId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                q = q.Where(a => a.Status == status);
            }

            return await q.OrderByDescending(a => a.AppliedAt).ToListAsync();
        }

        public async Task<JobApplication?> GetApplicationDetailAsync(int employerId, int applicationId)
        {
            var app = await _context.JobApplications
                .Include(a => a.Job)
                .Include(a => a.JobSeeker)
                    .ThenInclude(u => u!.JobSeekerProfile)
                .FirstOrDefaultAsync(a => a.Id == applicationId && a.Job != null && a.Job.EmployerId == employerId);

            if (app != null && app.Status == "Pending")
            {
                app.Status = "Reviewed";
                await _context.SaveChangesAsync();
            }

            return app;
        }

        public async Task<bool> UpdateApplicationStatusAsync(int employerId, int applicationId, string status, string? notes)
        {
            var app = await _context.JobApplications
                .Include(a => a.Job)
                .FirstOrDefaultAsync(a => a.Id == applicationId && a.Job != null && a.Job.EmployerId == employerId);

            if (app == null) return false;

            app.Status = status;
            if (!string.IsNullOrEmpty(notes))
            {
                app.EmployerNotes = notes;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> WithdrawApplicationAsync(int seekerId, int applicationId)
        {
            var app = await _context.JobApplications
                .FirstOrDefaultAsync(a => a.Id == applicationId && a.JobSeekerId == seekerId);

            if (app == null) return false;

            _context.JobApplications.Remove(app);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
