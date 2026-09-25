using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace AidSlot.Models;

public class CreateCampaignViewModel
{
    [Required]
    public Campaign Campaign { get; set; } = new();

    [Required(ErrorMessage = "Please select a recipients CSV file.")]
    [Display(Name = "Recipients CSV file")]
    public IFormFile? RecipientsCsvFile { get; set; }
}
