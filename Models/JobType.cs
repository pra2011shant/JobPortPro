using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using JobPortPro.Models.Common;

namespace JobPortPro.Models
{
    /// <summary>
    /// Master entity for Job Types (Full-Time, Remote, Part-Time, Contract, Internship) stored in DB.
    /// </summary>
    public class JobType : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty; // "full-time", "remote", "part-time", etc.

        [StringLength(50)]
        public string BadgeClass { get; set; } = "badge-soft-primary";

        public int DisplayOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();
    }
}
