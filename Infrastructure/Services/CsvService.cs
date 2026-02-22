using System.Globalization;
using Application.Interfaces;
using CsvHelper;
using Domain.Entities;

namespace Infrastructure.Services;

public class CsvService : ICsvService
{
    public byte[] WriteRows<T>(List<T> rows)
    {
        using (var memoryStream = new MemoryStream())
        {
            using (var writer = new StreamWriter(memoryStream))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteHeader<T>();
                csv.NextRecord();
                foreach (var record in rows)
                {
                    csv.WriteRecord(record);
                    csv.NextRecord();
                }
            }

            return memoryStream.ToArray();
        }
    }
}
