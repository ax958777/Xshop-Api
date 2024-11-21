using System.ComponentModel.DataAnnotations;

namespace Api.Model
{
    public class Order
    {
        [Key]
        public Guid Id { get; set; }

        public string PaymentIntentId { get; set; }

        public AppUser User { get; set; }

        public Boolean IsPaid { get; set; }

        public decimal Amount { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime LastUpdatedAt { get; set;} = DateTime.UtcNow;
    }
}
