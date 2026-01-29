using System;

namespace Rezerwacje.Areas.Admin.Models
{
    public class BookedSlotVm
    {
        public int SlotId { get; set; }
        public DateTime Date { get; set; }

        public string? ServiceName { get; set; }
        public int DurationMinutes { get; set; }

        public int? ReservationId { get; set; }
        public string? ReservationStatus { get; set; }
        public string? UserEmail { get; set; }
    }
}
