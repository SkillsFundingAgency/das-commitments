﻿using System.Collections.Generic;
using System.IO;

namespace SFA.DAS.CommitmentsV2.Shared.Interfaces
{
    public interface ICreateCsvService
    {
        MemoryStream GenerateCsvContent<T>(IEnumerable<T> results, bool hasHeader);
        void Dispose();
    }
}