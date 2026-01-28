using System.ComponentModel.DataAnnotations;

namespace Rezerwacje.Models
{
    public class SalonDayOverride
    {
        public int Id { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        public bool IsClosed { get; set; }

        // jeśli IsClosed = false, możesz podać inne godziny otwarcia
        public TimeSpan? OpenTime { get; set; }
        public TimeSpan? CloseTime { get; set; }

        [StringLength(200)]
        public string? Note { get; set; }
    }
}
