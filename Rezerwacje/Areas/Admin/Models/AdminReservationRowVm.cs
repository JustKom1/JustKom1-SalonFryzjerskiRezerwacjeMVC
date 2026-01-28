namespace Rezerwacje.Areas.Admin.Models
{
    public class AdminReservationRowVm
    {
        public int Id { get; set; }
        public string? UserEmail { get; set; }
        public string? ServiceName { get; set; }
        public DateTime? Date { get; set; }
        public string Status { get; set; } = "";
    }
}
