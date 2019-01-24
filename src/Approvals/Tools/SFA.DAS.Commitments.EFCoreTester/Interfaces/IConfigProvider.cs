using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.Commitments.EFCoreTester.Interfaces
{
    /// <summary>
    ///     Returns the specified config type. 
    /// </summary>
    public interface IConfigProvider
    {
        TConfigType Get<TConfigType>() where TConfigType : class, new();
    }
}
