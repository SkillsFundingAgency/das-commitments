using System;
using System.Collections.Generic;
using SFA.DAS.Commitments.EFCoreTester.Interfaces;
using System.Threading.Tasks;

namespace SFA.DAS.Commitments.EFCoreTester.Timing
{
    public class Timer : ITimer
    {
        private readonly Stack<Operation> _operationStack = new Stack<Operation>();
        

        public void StartCommand()
        {
            _operationStack.Clear();
            StartOperation("Root");
        }

        public void StartOperation(string title)
        {
            var newOperation = new Operation(title, _operationStack.Count);

            // Add this to parent's child-operations
            if (_operationStack.Count > 0)
            {
                var existingCurrentOperation = _operationStack.Peek();
                existingCurrentOperation.AddChildOperation(newOperation);
            }

            // Make this the new current operation
            _operationStack.Push(newOperation);
        }

        public void Time(string title, Action timeAction)
        {
            StartOperation(title);
            try
            {
                timeAction();
            }
            finally
            {
                EndOperation();
            }
        }

        public T Time<T>(string title, Func<T> timeAction)
        {
            StartOperation(title);
            try
            {
                return timeAction();
            }
            finally
            {
                EndOperation();
            }
        }

        public Task<T> TimeAsync<T>(string title, Func<Task<T>> timeAction)
        {
            StartOperation(title);
            return timeAction().ContinueWith(t =>
            {
                EndOperation();
                return t.Result;
            });
        }

        public IOperation EndOperation()
        {
            if (_operationStack.Count == 0)
            {
                throw new InvalidOperationException(
                    $"{nameof(EndOperation)} called but there was no corresponding {nameof(StartOperation)}");
            }

            var operation = _operationStack.Pop();
            operation.Ended = DateTime.Now;
            return operation;
        }

        public IOperation EndCommand()
        {
            if (_operationStack.Count > 1)
            {
                Console.WriteLine($"The command is ending but not all sub-operations have ended");
                while (_operationStack.Count > 1)
                {
                    _operationStack.Pop();
                }
            }

            return EndOperation();
        }
    }
}