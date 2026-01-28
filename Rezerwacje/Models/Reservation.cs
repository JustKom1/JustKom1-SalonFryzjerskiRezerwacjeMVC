using Rezerwacje.Models;
using System.ComponentModel.DataAnnotations;

    public class Reservation
    {
        public int Id { get; set; }

        [Required]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected, Cancelled

        public string UserId { get; set; }
        public AppUser User { get; set; }

        public int SlotId { get; set; }
        public Slot Slot { get; set; }
    }