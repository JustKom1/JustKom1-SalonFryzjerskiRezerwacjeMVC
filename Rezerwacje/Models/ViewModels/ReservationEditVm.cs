using System;
using System.ComponentModel.DataAnnotations;

namespace Rezerwacje.Models.ViewModels
{
    public class ReservationEditVm
    {
        [Required]
        public int Id { get; set; } // ReservationId

        [Required(ErrorMessage = "Wybierz usługę.")]
        public int ServiceId { get; set; }

        [Required(ErrorMessage = "Wybierz dzień.")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Wybierz godzinę.")]
        public string StartTime { get; set; } = ""; // "HH:mm"
    }
}
