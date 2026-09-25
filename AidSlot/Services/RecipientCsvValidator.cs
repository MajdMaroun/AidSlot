using AidSlot.DTOs;

namespace AidSlot.Services;

public class RecipientCsvValidator
{
    public List<string> Validate(List<RecipientCsvRow> rows)
    {
        var errors = new List<string>();
        var documentNumbers = new HashSet<string>();

        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i];

            // +2 because:
            // line 1 = CSV header
            // first data row = line 2
            int lineNumber = i + 2;

            if (string.IsNullOrWhiteSpace(row.RecordNumber))
            {
                errors.Add($"Line {lineNumber}: Record number is required.");
            }
            else if (!int.TryParse(row.RecordNumber, out int recordNumber))
            {
                errors.Add($"Line {lineNumber}: Record number must be a number.");
            }
            else if (recordNumber <= 0)
            {
                errors.Add($"Line {lineNumber}: Record number must be greater than 0.");
            }

            if (string.IsNullOrWhiteSpace(row.FullName))
            {
                errors.Add($"Line {lineNumber}: Full name is required.");
            }

            if (string.IsNullOrWhiteSpace(row.ResidentialArea))
            {
                errors.Add($"Line {lineNumber}: Residential area is required.");
            }

            if (string.IsNullOrWhiteSpace(row.FamilyMemberCount))
            {
                errors.Add($"Line {lineNumber}: Family member count is required.");
            }
            else if (!int.TryParse(row.FamilyMemberCount, out int familyMemberCount))
            {
                errors.Add($"Line {lineNumber}: Family member count must be a number.");
            }
            else if (familyMemberCount <= 0)
            {
                errors.Add($"Line {lineNumber}: Family member count must be greater than 0.");
            }

            if (string.IsNullOrWhiteSpace(row.HeadOfHouseholdGender))
            {
                errors.Add($"Line {lineNumber}: Head of household gender is required.");
            }

            if (string.IsNullOrWhiteSpace(row.PhoneNumber))
            {
                errors.Add($"Line {lineNumber}: Phone number is required.");
            }

            if (string.IsNullOrWhiteSpace(row.Nationality))
            {
                errors.Add($"Line {lineNumber}: Nationality is required.");
            }

            if (string.IsNullOrWhiteSpace(row.DocumentType))
            {
                errors.Add($"Line {lineNumber}: Document type is required.");
            }

            if (string.IsNullOrWhiteSpace(row.DocumentNumber))
            {
                errors.Add($"Line {lineNumber}: Document number is required.");
            }
            else if (!documentNumbers.Add(row.DocumentNumber.Trim()))
            {
                errors.Add($"Line {lineNumber}: Duplicate document number.");
            }
        }

        return errors;
    }
}