using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.CommitmentsV2.Mapping
{
    public interface IMapper<in TFrom, out TTo> where TFrom: class where TTo: class
    {
        TTo Map(TFrom source);
    }
}
