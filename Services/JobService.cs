using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using JobPortPro.Models;

namespace JobPortPro.Services
{
    /// <summary>
    /// Handles business logic and data access for jobs and bookmarks via SQL Server Stored Procedures.
    /// </summary>
    public class JobService : IJobService
    {
        private readonly IStoredProcedureExecutor _spExecutor;
        private readonly ILookupService _lookupService;

        public JobService(IStoredProcedureExecutor spExecutor, ILookupService lookupService)
        {
            _spExecutor = spExecutor;
            _lookupService = lookupService;
        }

        public async Task<JobFilterViewModel> GetFilteredJobsAsync(
            string? query,
            int? categoryId,
            string? jobType,
            string? location,
            string? experienceLevel,
            decimal? minSalary,
            string? sortBy,
            int page,
            int pageSize)
        {
            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@SearchQuery", (object?)query ?? DBNull.Value),
                new SqlParameter("@CategoryId", (object?)categoryId ?? DBNull.Value),
                new SqlParameter("@JobTypeId", DBNull.Value),
                new SqlParameter("@JobType", (object?)jobType ?? DBNull.Value),
                new SqlParameter("@Location", (object?)location ?? DBNull.Value),
                new SqlParameter("@ExperienceLevelId", DBNull.Value),
                new SqlParameter("@MinSalary", (object?)minSalary ?? DBNull.Value),
                new SqlParameter("@SortBy", (object?)sortBy ?? "newest"),
                new SqlParameter("@PageNumber", page),
                new SqlParameter("@PageSize", pageSize)
            };

            var (jobs, totalItems) = await _spExecutor.ExecuteStoredProcedurePagedListAsync(
                "dbo.sp_GetFilteredJobs",
                parameters,
                "@TotalCount",
                MapJobFromReader);

            var categories = await _lookupService.GetCategoriesAsync();
            var jobTypes = await _lookupService.GetJobTypesAsync();
            var expLevels = await _lookupService.GetExperienceLevelsAsync();
            var locations = await _lookupService.GetPopularLocationsAsync();

            return new JobFilterViewModel
            {
                Query = query,
                CategoryId = categoryId,
                JobType = jobType,
                Location = location,
                ExperienceLevel = experienceLevel,
                MinSalary = minSalary,
                SortBy = sortBy ?? "newest",
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                Jobs = jobs,
                Categories = categories,
                DynamicJobTypes = jobTypes,
                DynamicExperienceLevels = expLevels,
                Locations = locations
            };
        }

        public async Task<Job?> GetJobByIdAsync(int id)
        {
            var parameters = new[]
            {
                new SqlParameter("@JobId", id),
                new SqlParameter("@IncrementViews", 1)
            };

            return await _spExecutor.ExecuteStoredProcedureSingleAsync(
                "dbo.sp_GetJobById",
                parameters,
                MapJobDetailsFromReader);
        }

        public async Task<List<Job>> GetJobsByEmployerAsync(int employerId, string? status = null)
        {
            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@SearchQuery", DBNull.Value),
                new SqlParameter("@CategoryId", DBNull.Value),
                new SqlParameter("@JobTypeId", DBNull.Value),
                new SqlParameter("@JobType", DBNull.Value),
                new SqlParameter("@Location", DBNull.Value),
                new SqlParameter("@ExperienceLevelId", DBNull.Value),
                new SqlParameter("@MinSalary", DBNull.Value),
                new SqlParameter("@SortBy", "newest"),
                new SqlParameter("@PageNumber", 1),
                new SqlParameter("@PageSize", 100)
            };

            var (jobs, _) = await _spExecutor.ExecuteStoredProcedurePagedListAsync(
                "dbo.sp_GetFilteredJobs",
                parameters,
                "@TotalCount",
                MapJobFromReader);

            return jobs.FindAll(j => j.EmployerId == employerId);
        }

        public async Task<List<Job>> GetFeaturedJobsAsync(int count = 6)
        {
            var parameters = new[] { new SqlParameter("@Count", count) };

            return await _spExecutor.ExecuteStoredProcedureListAsync(
                "dbo.sp_GetFeaturedJobs",
                parameters,
                MapJobFromReader);
        }

        public async Task<List<Job>> GetRecentJobsAsync(int count = 4)
        {
            var parameters = new[] { new SqlParameter("@Count", count) };

            return await _spExecutor.ExecuteStoredProcedureListAsync(
                "dbo.sp_GetRecentJobs",
                parameters,
                MapJobFromReader);
        }

        public async Task<List<Job>> GetRelatedJobsAsync(int categoryId, int excludeJobId, int count = 3)
        {
            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@SearchQuery", DBNull.Value),
                new SqlParameter("@CategoryId", categoryId),
                new SqlParameter("@JobTypeId", DBNull.Value),
                new SqlParameter("@JobType", DBNull.Value),
                new SqlParameter("@Location", DBNull.Value),
                new SqlParameter("@ExperienceLevelId", DBNull.Value),
                new SqlParameter("@MinSalary", DBNull.Value),
                new SqlParameter("@SortBy", "newest"),
                new SqlParameter("@PageNumber", 1),
                new SqlParameter("@PageSize", count + 1)
            };

            var (jobs, _) = await _spExecutor.ExecuteStoredProcedurePagedListAsync(
                "dbo.sp_GetFilteredJobs",
                parameters,
                "@TotalCount",
                MapJobFromReader);

            return jobs.FindAll(j => j.Id != excludeJobId).GetRange(0, Math.Min(count, jobs.FindAll(j => j.Id != excludeJobId).Count));
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await _lookupService.GetCategoriesAsync();
        }

        public async Task<Job> CreateJobAsync(int employerId, PostJobViewModel model)
        {
            var outputParam = new SqlParameter("@NewJobId", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };

            var parameters = new[]
            {
                new SqlParameter("@EmployerId", employerId),
                new SqlParameter("@Title", model.Title.Trim()),
                new SqlParameter("@CategoryId", model.CategoryId),
                new SqlParameter("@JobTypeId", (object?)model.JobTypeId ?? DBNull.Value),
                new SqlParameter("@JobType", (object?)model.JobType ?? "Full-Time"),
                new SqlParameter("@Location", model.Location.Trim()),
                new SqlParameter("@SalaryMin", (object?)model.SalaryMin ?? DBNull.Value),
                new SqlParameter("@SalaryMax", (object?)model.SalaryMax ?? DBNull.Value),
                new SqlParameter("@ExperienceLevelId", (object?)model.ExperienceLevelId ?? DBNull.Value),
                new SqlParameter("@ExperienceLevel", (object?)model.ExperienceLevel ?? "Mid Level"),
                new SqlParameter("@Description", model.Description.Trim()),
                new SqlParameter("@Requirements", (object?)model.Requirements?.Trim() ?? DBNull.Value),
                new SqlParameter("@Responsibilities", (object?)model.Responsibilities?.Trim() ?? DBNull.Value),
                new SqlParameter("@Deadline", (object?)model.Deadline ?? DBNull.Value),
                outputParam
            };

            await _spExecutor.ExecuteStoredProcedureNonQueryAsync("dbo.sp_CreateJob", parameters);

            int newId = outputParam.Value != DBNull.Value ? Convert.ToInt32(outputParam.Value) : 0;

            return new Job
            {
                Id = newId,
                EmployerId = employerId,
                Title = model.Title,
                CategoryId = model.CategoryId,
                JobTypeId = model.JobTypeId,
                JobType = model.JobType ?? "Full-Time",
                Location = model.Location,
                SalaryMin = model.SalaryMin,
                SalaryMax = model.SalaryMax,
                ExperienceLevelId = model.ExperienceLevelId,
                ExperienceLevel = model.ExperienceLevel,
                Description = model.Description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
        }

        public async Task<bool> UpdateJobAsync(int employerId, PostJobViewModel model)
        {
            if (!model.Id.HasValue) return false;

            var parameters = new[]
            {
                new SqlParameter("@JobId", model.Id.Value),
                new SqlParameter("@EmployerId", employerId),
                new SqlParameter("@Title", model.Title.Trim()),
                new SqlParameter("@CategoryId", model.CategoryId),
                new SqlParameter("@JobTypeId", (object?)model.JobTypeId ?? DBNull.Value),
                new SqlParameter("@JobType", (object?)model.JobType ?? "Full-Time"),
                new SqlParameter("@Location", model.Location.Trim()),
                new SqlParameter("@SalaryMin", (object?)model.SalaryMin ?? DBNull.Value),
                new SqlParameter("@SalaryMax", (object?)model.SalaryMax ?? DBNull.Value),
                new SqlParameter("@ExperienceLevelId", (object?)model.ExperienceLevelId ?? DBNull.Value),
                new SqlParameter("@ExperienceLevel", (object?)model.ExperienceLevel ?? "Mid Level"),
                new SqlParameter("@Description", model.Description.Trim()),
                new SqlParameter("@Requirements", (object?)model.Requirements?.Trim() ?? DBNull.Value),
                new SqlParameter("@Responsibilities", (object?)model.Responsibilities?.Trim() ?? DBNull.Value),
                new SqlParameter("@IsActive", model.IsActive),
                new SqlParameter("@Deadline", (object?)model.Deadline ?? DBNull.Value)
            };

            int affected = await _spExecutor.ExecuteStoredProcedureNonQueryAsync("dbo.sp_UpdateJob", parameters);
            return affected > 0;
        }

        public async Task<bool> ToggleJobStatusAsync(int employerId, int jobId)
        {
            var job = await GetJobByIdAsync(jobId);
            if (job == null || job.EmployerId != employerId) return false;

            var model = new PostJobViewModel
            {
                Id = job.Id,
                Title = job.Title,
                CategoryId = job.CategoryId,
                JobTypeId = job.JobTypeId,
                JobType = job.JobType,
                Location = job.Location,
                SalaryMin = job.SalaryMin,
                SalaryMax = job.SalaryMax,
                ExperienceLevelId = job.ExperienceLevelId,
                ExperienceLevel = job.ExperienceLevel,
                Description = job.Description,
                Requirements = job.Requirements,
                Responsibilities = job.Responsibilities,
                IsActive = !job.IsActive,
                Deadline = job.Deadline
            };

            return await UpdateJobAsync(employerId, model);
        }

        public async Task<bool> DeleteJobAsync(int employerId, int jobId)
        {
            var parameters = new[]
            {
                new SqlParameter("@JobId", jobId),
                new SqlParameter("@EmployerId", employerId)
            };

            int affected = await _spExecutor.ExecuteStoredProcedureNonQueryAsync("dbo.sp_DeleteJob", parameters);
            return affected > 0;
        }

        public async Task<bool> ToggleSavedJobAsync(int seekerId, int jobId)
        {
            var outputParam = new SqlParameter("@IsSaved", SqlDbType.Bit)
            {
                Direction = ParameterDirection.Output
            };

            var parameters = new[]
            {
                new SqlParameter("@JobId", jobId),
                new SqlParameter("@JobSeekerId", seekerId),
                outputParam
            };

            await _spExecutor.ExecuteStoredProcedureNonQueryAsync("dbo.sp_ToggleSaveJob", parameters);
            return outputParam.Value != DBNull.Value && Convert.ToBoolean(outputParam.Value);
        }

        public async Task<List<SavedJob>> GetSavedJobsAsync(int seekerId)
        {
            var parameters = new[] { new SqlParameter("@JobSeekerId", seekerId) };

            return await _spExecutor.ExecuteStoredProcedureListAsync(
                "dbo.sp_GetSavedJobsByUser",
                parameters,
                reader => new SavedJob
                {
                    Id = reader.GetInt32(reader.GetOrdinal("SavedJobId")),
                    SavedAt = reader.GetDateTime(reader.GetOrdinal("SavedAt")),
                    JobId = reader.GetInt32(reader.GetOrdinal("JobId")),
                    JobSeekerId = seekerId,
                    Job = new Job
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("JobId")),
                        Title = reader.GetString(reader.GetOrdinal("Title")),
                        JobType = reader.GetString(reader.GetOrdinal("JobType")),
                        Location = reader.GetString(reader.GetOrdinal("Location")),
                        SalaryMin = reader.IsDBNull(reader.GetOrdinal("SalaryMin")) ? null : reader.GetDecimal(reader.GetOrdinal("SalaryMin")),
                        SalaryMax = reader.IsDBNull(reader.GetOrdinal("SalaryMax")) ? null : reader.GetDecimal(reader.GetOrdinal("SalaryMax")),
                        Deadline = reader.IsDBNull(reader.GetOrdinal("Deadline")) ? null : reader.GetDateTime(reader.GetOrdinal("Deadline")),
                        IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                        Category = new Category { Name = reader.GetString(reader.GetOrdinal("CategoryName")) },
                        Employer = new User
                        {
                            CompanyProfile = new CompanyProfile
                            {
                                CompanyName = reader.IsDBNull(reader.GetOrdinal("CompanyName")) ? "Company" : reader.GetString(reader.GetOrdinal("CompanyName"))
                            }
                        }
                    }
                });
        }

        public async Task<bool> IsJobSavedAsync(int seekerId, int jobId)
        {
            var savedList = await GetSavedJobsAsync(seekerId);
            return savedList.Exists(s => s.JobId == jobId);
        }

        private static Job MapJobFromReader(DbDataReader reader)
        {
            return new Job
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Title = reader.GetString(reader.GetOrdinal("Title")),
                EmployerId = reader.GetInt32(reader.GetOrdinal("EmployerId")),
                CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
                JobTypeId = reader.IsDBNull(reader.GetOrdinal("JobTypeId")) ? null : reader.GetInt32(reader.GetOrdinal("JobTypeId")),
                JobType = reader.GetString(reader.GetOrdinal("JobType")),
                Location = reader.GetString(reader.GetOrdinal("Location")),
                SalaryMin = reader.IsDBNull(reader.GetOrdinal("SalaryMin")) ? null : reader.GetDecimal(reader.GetOrdinal("SalaryMin")),
                SalaryMax = reader.IsDBNull(reader.GetOrdinal("SalaryMax")) ? null : reader.GetDecimal(reader.GetOrdinal("SalaryMax")),
                ExperienceLevelId = reader.IsDBNull(reader.GetOrdinal("ExperienceLevelId")) ? null : reader.GetInt32(reader.GetOrdinal("ExperienceLevelId")),
                ExperienceLevel = reader.IsDBNull(reader.GetOrdinal("ExperienceLevel")) ? null : reader.GetString(reader.GetOrdinal("ExperienceLevel")),
                Description = reader.GetString(reader.GetOrdinal("Description")),
                Requirements = reader.IsDBNull(reader.GetOrdinal("Requirements")) ? null : reader.GetString(reader.GetOrdinal("Requirements")),
                Responsibilities = reader.IsDBNull(reader.GetOrdinal("Responsibilities")) ? null : reader.GetString(reader.GetOrdinal("Responsibilities")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                Deadline = reader.IsDBNull(reader.GetOrdinal("Deadline")) ? null : reader.GetDateTime(reader.GetOrdinal("Deadline")),
                ViewsCount = reader.GetInt32(reader.GetOrdinal("ViewsCount")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),
                Category = new Category
                {
                    Id = reader.GetInt32(reader.GetOrdinal("CategoryId")),
                    Name = reader.IsDBNull(reader.GetOrdinal("CategoryName")) ? "General" : reader.GetString(reader.GetOrdinal("CategoryName")),
                    IconClass = reader.IsDBNull(reader.GetOrdinal("CategoryIcon")) ? "fa-solid fa-briefcase" : reader.GetString(reader.GetOrdinal("CategoryIcon"))
                },
                JobTypeEntity = reader.IsDBNull(reader.GetOrdinal("JobTypeId")) ? null : new JobType
                {
                    Id = reader.GetInt32(reader.GetOrdinal("JobTypeId")),
                    Name = reader.IsDBNull(reader.GetOrdinal("JobTypeName")) ? reader.GetString(reader.GetOrdinal("JobType")) : reader.GetString(reader.GetOrdinal("JobTypeName")),
                    BadgeClass = reader.IsDBNull(reader.GetOrdinal("JobTypeBadge")) ? "badge-soft-primary" : reader.GetString(reader.GetOrdinal("JobTypeBadge"))
                },
                ExperienceLevelEntity = reader.IsDBNull(reader.GetOrdinal("ExperienceLevelId")) ? null : new ExperienceLevel
                {
                    Id = reader.GetInt32(reader.GetOrdinal("ExperienceLevelId")),
                    Title = reader.IsDBNull(reader.GetOrdinal("ExperienceLevelTitle")) ? (reader.IsDBNull(reader.GetOrdinal("ExperienceLevel")) ? "Any" : reader.GetString(reader.GetOrdinal("ExperienceLevel"))) : reader.GetString(reader.GetOrdinal("ExperienceLevelTitle"))
                },
                Employer = new User
                {
                    Id = reader.GetInt32(reader.GetOrdinal("EmployerId")),
                    CompanyProfile = new CompanyProfile
                    {
                        CompanyName = reader.IsDBNull(reader.GetOrdinal("CompanyName")) ? "Company" : reader.GetString(reader.GetOrdinal("CompanyName")),
                        Location = reader.IsDBNull(reader.GetOrdinal("CompanyLocation")) ? null : reader.GetString(reader.GetOrdinal("CompanyLocation")),
                        Website = reader.IsDBNull(reader.GetOrdinal("CompanyWebsite")) ? null : reader.GetString(reader.GetOrdinal("CompanyWebsite"))
                    }
                }
            };
        }

        private static Job MapJobDetailsFromReader(DbDataReader reader)
        {
            var job = MapJobFromReader(reader);
            if (job.Employer != null)
            {
                job.Employer.FullName = reader.IsDBNull(reader.GetOrdinal("EmployerName")) ? "Employer" : reader.GetString(reader.GetOrdinal("EmployerName"));
                job.Employer.Email = reader.IsDBNull(reader.GetOrdinal("EmployerEmail")) ? "" : reader.GetString(reader.GetOrdinal("EmployerEmail"));
                if (job.Employer.CompanyProfile != null)
                {
                    job.Employer.CompanyProfile.Description = reader.IsDBNull(reader.GetOrdinal("CompanyDescription")) ? null : reader.GetString(reader.GetOrdinal("CompanyDescription"));
                    job.Employer.CompanyProfile.CompanySize = reader.IsDBNull(reader.GetOrdinal("CompanySize")) ? null : reader.GetString(reader.GetOrdinal("CompanySize"));
                }
            }
            return job;
        }
    }
}
