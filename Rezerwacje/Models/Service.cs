using System.ComponentModel.DataAnnotations;

public class Service
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; }

    public string Description { get; set; }

    [Range(0, 999999)]
    public decimal Price { get; set; }

    public int DurationMinutes { get; set; }

    public List<Slot> Slots { get; set; } = new();
}
