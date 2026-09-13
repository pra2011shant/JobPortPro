using Microsoft.EntityFrameworkCore;
using JobPortPro.Models;

namespace JobPortPro.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<CompanyProfile> CompanyProfiles { get; set; }
        public DbSet<JobSeekerProfile> JobSeekerProfiles { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<JobApplication> JobApplications { get; set; }
        public DbSet<SavedJob> SavedJobs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User - CompanyProfile (1:1)
            modelBuilder.Entity<CompanyProfile>()
                .HasOne(cp => cp.User)
                .WithOne(u => u.CompanyProfile)
                .HasForeignKey<CompanyProfile>(cp => cp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User - JobSeekerProfile (1:1)
            modelBuilder.Entity<JobSeekerProfile>()
                .HasOne(jp => jp.User)
                .WithOne(u => u.JobSeekerProfile)
                .HasForeignKey<JobSeekerProfile>(jp => jp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User - Job (Employer -> Jobs)
            modelBuilder.Entity<Job>()
                .HasOne(j => j.Employer)
                .WithMany(u => u.PostedJobs)
                .HasForeignKey(j => j.EmployerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Category - Job (1:N)
            modelBuilder.Entity<Job>()
                .HasOne(j => j.Category)
                .WithMany(c => c.Jobs)
                .HasForeignKey(j => j.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // JobApplication
            modelBuilder.Entity<JobApplication>()
                .HasOne(ja => ja.Job)
                .WithMany(j => j.Applications)
                .HasForeignKey(ja => ja.JobId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<JobApplication>()
                .HasOne(ja => ja.JobSeeker)
                .WithMany(u => u.Applications)
                .HasForeignKey(ja => ja.JobSeekerId)
                .OnDelete(DeleteBehavior.Restrict);

            // SavedJob
            modelBuilder.Entity<SavedJob>()
                .HasOne(sj => sj.Job)
                .WithMany(j => j.SavedByUsers)
                .HasForeignKey(sj => sj.JobId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SavedJob>()
                .HasOne(sj => sj.JobSeeker)
                .WithMany(u => u.SavedJobs)
                .HasForeignKey(sj => sj.JobSeekerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
