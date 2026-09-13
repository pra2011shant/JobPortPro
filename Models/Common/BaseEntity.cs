using System;
using System.ComponentModel.DataAnnotations;

namespace JobPortPro.Models.Common
{
    /// <summary>
    /// Base class for all domain entities providing common auditing attributes (OOPs - Inheritance).
    /// </summary>
    public abstract class BaseEntity
    {
        [Key]
        public int Id { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
