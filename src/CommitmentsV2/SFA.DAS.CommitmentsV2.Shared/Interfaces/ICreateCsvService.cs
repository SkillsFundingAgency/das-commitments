using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Shared.Interfaces
{
    public interface ICreateCsvService
    {
        byte[] GenerateCsvContent<T>(IEnumerable<T> results);
    }
}