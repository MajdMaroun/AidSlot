using System.ComponentModel.DataAnnotations;

namespace AidSlot.Models;

public class Organization
{
    public int Id { get; set; }

    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;
}
