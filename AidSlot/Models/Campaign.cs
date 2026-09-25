using System.ComponentModel.DataAnnotations;

namespace AidSlot.Models
{
    public class Campaign
    {
        public int Id { get; set; }
        public int? OrganizationId { get; set; }
        public Organization? Organization { get; set; }

        [Required]
        public string CampaignName { get; set; } = string.Empty;

        [Required]
        public string CampaignType { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public DateTime DistributionDate { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [Required]
        public string LocationName { get; set; } = string.Empty;

        [Required]
        public string Address { get; set; } = string.Empty;

        [Required]
        public string CoordinatorName { get; set; } = string.Empty;

        [Required]
        public string CoordinatorNumber { get; set; } = string.Empty;

        public string Status { get; set; } = "Draft";

        public string? CsvFileName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Recipient> Recipients { get; set; } = new List<Recipient>();
    }
}
