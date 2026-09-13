using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace JobPortPro.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(50)]
        public string? IconClass { get; set; } = "fa-briefcase";

        public string? Description { get; set; }

        public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();
    }
}
