using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Shared.Services
{
    public class CreateCsvService : ICreateCsvService
    {
        public byte[] GenerateCsvContent<T>(IEnumerable<T> results)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var streamWriter = new StreamWriter(memoryStream))
                {
                    using (var csvWriter = new CsvWriter(streamWriter))
                    {
                        csvWriter.WriteRecords(results);
                        streamWriter.Flush();
                        memoryStream.Position = 0;
                        return memoryStream.ToArray();
                    }
                }
            }
        }
    }
}