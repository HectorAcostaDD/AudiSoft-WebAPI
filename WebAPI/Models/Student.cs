using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace WebAPI.Models
{
    public class Student
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        // Navigation property
        public ICollection<Score> Scores { get; set; } = new List<Score>();
    }
}
