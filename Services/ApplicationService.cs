using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using JobPortPro.Models;

namespace JobPortPro.Services
{
    /// <summary>
    /// Manages candidate applications and ATS workflows via SQL Server Stored Procedures.
    /// </summary>
    public class ApplicationService : IApplicationService
    {
        private readonly IStoredProcedureExecutor _spExecutor;

        public ApplicationService(IStoredProcedureExecutor spExecutor)
        {
            _spExecutor = spExecutor;
        }

        public async Task<JobApplication> SubmitApplicationAsync(int seekerId, ApplyJobViewModel model, string resumePath, string resumeFileName)
        {
            var outputParam = new SqlParameter("@ApplicationId", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };

            var parameters = new[]
            {
                new SqlParameter("@JobId", model.JobId),
                new SqlParameter("@JobSeekerId", seekerId),
                new SqlParameter("@CoverLetter", (object?)model.CoverLetter?.Trim() ?? DBNull.Value),
                new SqlParameter("@ResumePath", (object?)resumePath ?? DBNull.Value),
                new SqlParameter("@ResumeFileName", (object?)resumeFileName ?? DBNull.Value),
                outputParam
            };

            await _spExecutor.ExecuteStoredProcedureNonQueryAsync("dbo.sp_SubmitJobApplication", parameters);

            int appId = outputParam.Value != DBNull.Value ? Convert.ToInt32(outputParam.Value) : 0;

            return new JobApplication
            {
                Id = appId,
                JobId = model.JobId,
                JobSeekerId = seekerId,
                CoverLetter = model.CoverLetter,
                ResumePath = resumePath ?? string.Empty,
                ResumeFileName = resumeFileName ?? string.Empty,
                AppliedAt = DateTime.UtcNow,
                Status = "Pending"
            };
        }

        public async Task<bool> HasUserAppliedAsync(int seekerId, int jobId)
        {
            var apps = await GetApplicationsBySeekerAsync(seekerId);
            return apps.Exists(a => a.JobId == jobId);
        }

        public async Task<List<JobApplication>> GetApplicationsBySeekerAsync(int seekerId, string? status = null)
        {
            var parameters = new[] { new SqlParameter("@JobSeekerId", seekerId) };

            var list = await _spExecutor.ExecuteStoredProcedureListAsync(
                "dbo.sp_GetApplicationsByJobSeeker",
                parameters,
                reader => new JobApplication
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    JobId = reader.GetInt32(reader.GetOrdinal("JobId")),
                    JobSeekerId = reader.GetInt32(reader.GetOrdinal("JobSeekerId")),
                    CoverLetter = reader.IsDBNull(reader.GetOrdinal("CoverLetter")) ? null : reader.GetString(reader.GetOrdinal("CoverLetter")),
                    ResumePath = reader.IsDBNull(reader.GetOrdinal("ResumePath")) ? string.Empty : reader.GetString(reader.GetOrdinal("ResumePath")),
                    ResumeFileName = reader.IsDBNull(reader.GetOrdinal("ResumeFileName")) ? string.Empty : reader.GetString(reader.GetOrdinal("ResumeFileName")),
                    AppliedAt = reader.GetDateTime(reader.GetOrdinal("AppliedAt")),
                    Status = reader.GetString(reader.GetOrdinal("Status")),
                    EmployerNotes = reader.IsDBNull(reader.GetOrdinal("EmployerNotes")) ? null : reader.GetString(reader.GetOrdinal("EmployerNotes")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                    UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),
                    Job = new Job
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("JobId")),
                        Title = reader.GetString(reader.GetOrdinal("JobTitle")),
                        Location = reader.GetString(reader.GetOrdinal("JobLocation")),
                        JobType = reader.GetString(reader.GetOrdinal("JobType")),
                        SalaryMin = reader.IsDBNull(reader.GetOrdinal("SalaryMin")) ? null : reader.GetDecimal(reader.GetOrdinal("SalaryMin")),
                        SalaryMax = reader.IsDBNull(reader.GetOrdinal("SalaryMax")) ? null : reader.GetDecimal(reader.GetOrdinal("SalaryMax")),
                        Employer = new User
                        {
                            CompanyProfile = new CompanyProfile
                            {
                                CompanyName = reader.IsDBNull(reader.GetOrdinal("CompanyName")) ? "Company" : reader.GetString(reader.GetOrdinal("CompanyName")),
                                Location = reader.IsDBNull(reader.GetOrdinal("CompanyLocation")) ? null : reader.GetString(reader.GetOrdinal("CompanyLocation"))
                            }
                        }
                    }
                });

            if (!string.IsNullOrWhiteSpace(status))
            {
                return list.FindAll(a => a.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
            }

            return list;
        }

        public async Task<List<JobApplication>> GetApplicationsByEmployerAsync(int employerId, int? jobId = null, string? status = null)
        {
            var parameters = new[]
            {
                new SqlParameter("@EmployerId", employerId),
                new SqlParameter("@JobId", (object?)jobId ?? DBNull.Value),
                new SqlParameter("@Status", (object?)status ?? DBNull.Value)
            };

            return await _spExecutor.ExecuteStoredProcedureListAsync(
                "dbo.sp_GetApplicationsByEmployer",
                parameters,
                reader => new JobApplication
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    JobId = reader.GetInt32(reader.GetOrdinal("JobId")),
                    JobSeekerId = reader.GetInt32(reader.GetOrdinal("JobSeekerId")),
                    CoverLetter = reader.IsDBNull(reader.GetOrdinal("CoverLetter")) ? null : reader.GetString(reader.GetOrdinal("CoverLetter")),
                    ResumePath = reader.IsDBNull(reader.GetOrdinal("ResumePath")) ? string.Empty : reader.GetString(reader.GetOrdinal("ResumePath")),
                    ResumeFileName = reader.IsDBNull(reader.GetOrdinal("ResumeFileName")) ? string.Empty : reader.GetString(reader.GetOrdinal("ResumeFileName")),
                    AppliedAt = reader.GetDateTime(reader.GetOrdinal("AppliedAt")),
                    Status = reader.GetString(reader.GetOrdinal("Status")),
                    EmployerNotes = reader.IsDBNull(reader.GetOrdinal("EmployerNotes")) ? null : reader.GetString(reader.GetOrdinal("EmployerNotes")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                    UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),
                    Job = new Job
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("JobId")),
                        Title = reader.GetString(reader.GetOrdinal("JobTitle")),
                        Location = reader.GetString(reader.GetOrdinal("JobLocation"))
                    },
                    JobSeeker = new User
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("JobSeekerId")),
                        FullName = reader.GetString(reader.GetOrdinal("CandidateName")),
                        Email = reader.GetString(reader.GetOrdinal("CandidateEmail")),
                        PhoneNumber = reader.IsDBNull(reader.GetOrdinal("CandidatePhone")) ? null : reader.GetString(reader.GetOrdinal("CandidatePhone")),
                        JobSeekerProfile = new JobSeekerProfile
                        {
                            Headline = reader.IsDBNull(reader.GetOrdinal("CandidateHeadline")) ? "Candidate" : reader.GetString(reader.GetOrdinal("CandidateHeadline")),
                            Skills = reader.IsDBNull(reader.GetOrdinal("CandidateSkills")) ? null : reader.GetString(reader.GetOrdinal("CandidateSkills")),
                            ExperienceYears = reader.IsDBNull(reader.GetOrdinal("CandidateExperience")) ? 0 : reader.GetInt32(reader.GetOrdinal("CandidateExperience"))
                        }
                    }
                });
        }

        public async Task<JobApplication?> GetApplicationDetailAsync(int employerId, int applicationId)
        {
            var apps = await GetApplicationsByEmployerAsync(employerId);
            var app = apps.Find(a => a.Id == applicationId);

            if (app != null && app.Status == "Pending")
            {
                await UpdateApplicationStatusAsync(employerId, applicationId, "Reviewed", null);
                app.Status = "Reviewed";
            }

            return app;
        }

        public async Task<bool> UpdateApplicationStatusAsync(int employerId, int applicationId, string status, string? notes)
        {
            var parameters = new[]
            {
                new SqlParameter("@ApplicationId", applicationId),
                new SqlParameter("@EmployerId", employerId),
                new SqlParameter("@Status", status),
                new SqlParameter("@EmployerNotes", (object?)notes ?? DBNull.Value)
            };

            int affected = await _spExecutor.ExecuteStoredProcedureNonQueryAsync("dbo.sp_UpdateApplicationStatus", parameters);
            return affected > 0;
        }

        public async Task<bool> WithdrawApplicationAsync(int seekerId, int applicationId)
        {
            var apps = await GetApplicationsBySeekerAsync(seekerId);
            return apps.Exists(a => a.Id == applicationId);
        }
    }
}
