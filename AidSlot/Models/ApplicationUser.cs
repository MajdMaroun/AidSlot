using Microsoft.AspNetCore.Identity;

namespace AidSlot.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public int? OrganizationId { get; set; }
        public Organization? Organization { get; set; }
    }
}
