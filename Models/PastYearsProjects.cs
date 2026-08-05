using System.ComponentModel.DataAnnotations;

namespace Pathify.Models
{
    public class PastYearsProjects
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Description { get; set; }

        [Required]
        public int Year { get; set; }
    }
}