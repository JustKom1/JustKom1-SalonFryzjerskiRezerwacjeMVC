using Rezerwacje.Models;
using System.ComponentModel.DataAnnotations;

public class Slot
{
    public int Id { get; set; }

    [Required]
    public DateTime Date { get; set; }

    public bool IsBooked { get; set; } = false;

    public int ServiceId { get; set; }
    public Service Service { get; set; }

    public Reservation? Reservation { get; set; }
}
