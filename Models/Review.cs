using System.ComponentModel.DataAnnotations;

namespace RolesAuth.Models
{
    public class Review
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string UserName { get; set; }

        [Required]
        [StringLength(500)]
        public string Feedback { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; } // Rating between 1 and 5

        public DateTime Date { get; set; } = DateTime.Now;
    }
}
