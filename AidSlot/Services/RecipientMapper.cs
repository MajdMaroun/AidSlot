using AidSlot.DTOs;
using AidSlot.Models;

namespace AidSlot.Services;

public class RecipientMapper
{
    public Recipient MapToRecipient(RecipientCsvRow row, int campaignId)
    {
        return new Recipient
        {
            CampaignId = campaignId,
            RecordNumber = int.Parse(row.RecordNumber),
            FullName = row.FullName.Trim(),
            SpouseFullName = row.SpouseFullName.Trim(),
            ResidentialArea = row.ResidentialArea.Trim(),
            FamilyMemberCount = int.Parse(row.FamilyMemberCount),
            HeadOfHouseholdGender = row.HeadOfHouseholdGender.Trim(),
            PhoneNumber = row.PhoneNumber.Trim(),
            Nationality = row.Nationality.Trim(),
            DocumentType = row.DocumentType.Trim(),
            DocumentNumber = row.DocumentNumber.Trim()
        };
    }
}