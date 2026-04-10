using System.ComponentModel.DataAnnotations;

namespace WebQLPT.Models
{
    public class ChuTroRequest
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        public ApplicationUser User { get; set; }

        public string Message { get; set; }

        [Required]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
