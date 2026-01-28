namespace Rezerwacje.Areas.Admin.Models
{
    public class AdminDashboardViewModel
    {
        public int ServicesCount { get; set; }
        public int SlotsCount { get; set; }

        public int PendingCount { get; set; }
        public int ApprovedCount { get; set; }
        public int RejectedCount { get; set; }
        public int CancelledCount { get; set; }

        public List<AdminPendingRowVm> LatestPending { get; set; } = new();
    }
}
