    using System.Globalization;
    using System.Text;
    using AidSlot.Csv;
    using AidSlot.DTOs;
    using CsvHelper;

    namespace AidSlot.Services;

    public class RecipientCsvParser
    {
        public List<RecipientCsvRow> Parse(Stream fileStream)
        {
            using var reader = new StreamReader(
                fileStream,
                Encoding.UTF8,
                detectEncodingFromByteOrderMarks: true
            );

            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            csv.Context.RegisterClassMap<RecipientCsvMap>();

            return csv.GetRecords<RecipientCsvRow>().ToList();
        }
    }