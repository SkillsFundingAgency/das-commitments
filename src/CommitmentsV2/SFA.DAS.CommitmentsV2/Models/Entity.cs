using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SFA.DAS.UnitOfWork.Context;

namespace SFA.DAS.CommitmentsV2.Models
{
    public abstract class Entity
    {
        protected void Publish<T>(Func<T> action) where T : class
        {
            UnitOfWorkContext.AddEvent(action);
        }
    }
}