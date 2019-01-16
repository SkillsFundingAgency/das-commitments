using System;
using System.Threading.Tasks;

namespace SFA.DAS.Commitments.EFCoreTester.Interfaces
{
    public interface ITimer
    {
        void StartCommand();
        void StartOperation(string title);
        void Time(string title, Action timeAction);
        T Time<T>(string title, Func<T> timeAction);
        Task<T> TimeAsync<T>(string title, Func<Task<T>> timeAction);
        IOperation EndOperation();
        IOperation EndCommand();
    }
}
