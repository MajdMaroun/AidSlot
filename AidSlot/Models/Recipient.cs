namespace AidSlot.Models;

public class Recipient
{
    public int Id { get; set; }

    public int CampaignId { get; set; }

    public Campaign Campaign { get; set; } = null!;

    public int RecordNumber { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string? SpouseFullName { get; set; }

    public string ResidentialArea { get; set; } = string.Empty;

    public int FamilyMemberCount { get; set; }

    public string HeadOfHouseholdGender { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string Nationality { get; set; } = string.Empty;

    public string DocumentType { get; set; } = string.Empty;

    public string DocumentNumber { get; set; } = string.Empty;

    public TimeSpan? AssignedSlotTime { get; set; }

    // Indicates whether the recipient has received the aid.
    public bool HasReceived { get; set; } = false;

    // Stores the date and time when the aid was received.
    public DateTime? ReceivedAt { get; set; }
}