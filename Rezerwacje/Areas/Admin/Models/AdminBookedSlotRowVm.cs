namespace Rezerwacje.Areas.Admin.Models
{
    public class AdminBookedSlotRowVm
    {
        public int ReservationId { get; set; }
        public DateTime Date { get; set; }

        public string? UserEmail { get; set; }
        public string? ServiceName { get; set; }

        public int DurationMinutes { get; set; }
        public string Status { get; set; } = "";
    }
}
