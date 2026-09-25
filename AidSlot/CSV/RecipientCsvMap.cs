using AidSlot.DTOs;
using CsvHelper.Configuration;

namespace AidSlot.Csv;

public sealed class RecipientCsvMap : ClassMap<RecipientCsvRow>
{
    public RecipientCsvMap()
    {
        Map(m => m.RecordNumber).Name("الرقم");
        Map(m => m.FullName).Name("الاسم الثلاثي");
        Map(m => m.SpouseFullName).Name("اسم الزوج الزوجة الثلاثي");
        Map(m => m.ResidentialArea).Name("المنطقة السكنية");
        Map(m => m.FamilyMemberCount).Name("عدد أفراد الأسرة");
        Map(m => m.HeadOfHouseholdGender).Name("جنس رب الأسرة");
        Map(m => m.PhoneNumber).Name("رقم الهاتف");
        Map(m => m.Nationality).Name("الجنسية");
        Map(m => m.DocumentType).Name("نوع الوثيقة التي يحملها");
        Map(m => m.DocumentNumber).Name("رقم الوثيقة التي يحملها");
    }
}