using System.ComponentModel.DataAnnotations;

namespace WebQLPT.Models
{
    public class Payment
    {
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; }
        public int? DangTinId { get; set; }
        [Required]
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
