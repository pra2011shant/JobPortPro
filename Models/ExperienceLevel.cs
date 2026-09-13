using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using JobPortPro.Models.Common;

namespace JobPortPro.Models
{
    /// <summary>
    /// Master entity for Experience Levels (Entry Level, Mid Level, Senior Level, Director) stored in DB.
    /// </summary>
    public class ExperienceLevel : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty;

        public int MinYears { get; set; } = 0;

        public int? MaxYears { get; set; }

        public int DisplayOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();
    }
}
