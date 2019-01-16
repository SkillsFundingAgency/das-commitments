using System;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using SFA.DAS.Commitments.EFCoreTester.CommandLine;
using SFA.DAS.Commitments.EFCoreTester.Commands;
using SFA.DAS.Commitments.EFCoreTester.Config;
using SFA.DAS.Commitments.EFCoreTester.Interfaces;
using StructureMap;

namespace SFA.DAS.Commitments.EFCoreTester
{
    internal class Program
    {
        private readonly IContainer _container;

        private static void Main(string[] args)
        {
            Parser.Default.ParseArguments<ReadCommandLine, WriteCommandLine>(args)
                .WithParsed<ReadCommandLine>(commandLine => new Program(string.Empty).RunReads(commandLine))
                .WithParsed<WriteCommandLine>(commandLine => new Program(string.Empty).RunWrites(commandLine));
        }

        public Program(string configLocation)
        {
            _container = IoC.IoC.InitialiseIoC(configLocation);
        }

        private void RunReads(ReadCommandLine args)
        {
            SetConfigOverrides<ReadConfig>(config => config.TableName = args.TableName);
            RunCommand<ReadCommand>(args.ShowTimings, args.Runs);
        }

        private void RunWrites(WriteCommandLine args)
        {
            SetConfigOverrides<WriteConfig>(config => config.SingleApprenticeshipPerCommitment = args.SingleApprenticeshipPerCommitment);
            SetConfigOverrides<WriteConfig>(config => config.DraftCount = args.DraftCount);
            SetConfigOverrides<WriteConfig>(config => config.ConfirmedCount = args.ConfirmedCount);
            RunCommand<WriteCommand>(args.ShowTimings, args.Runs);
        }

        private void RunCommand<TCommand>(bool showTimings, int runs) where TCommand : ICommand
        {
            var cancellationTokenSource = new CancellationTokenSource();

            var task = StartCommand<TCommand>(cancellationTokenSource.Token, runs);

            WaitForCommandToCompleteOrCancel(task, cancellationTokenSource);

            if (showTimings)
            {
                ShowTimings(task.Result);
            }
        }

        private void ShowTimings(IOperation operation)
        {
            var outputter = _container.GetInstance<IOperationTimingOutputter>();
            outputter.Show(operation);
        }

        private void SetConfigOverrides<TConfigType>(Action<TConfigType> setter) where TConfigType : class, new()
        {
            SetConfigOverrides(setter, true);
        }

        private void SetConfigOverrides<TConfigType>(Action<TConfigType> setter, bool setCondition) where TConfigType : class, new()
        {
            if (setCondition)
            {
                var configProvider = _container.GetInstance<IConfigProvider>();
                var config = configProvider.Get<TConfigType>();
                setter(config);
            }
        }

        private Task<IOperation> StartCommand<TCommand>(CancellationToken cancellationToken, int runs) where TCommand : ICommand
        {
            var command = _container.GetInstance<TCommand>();
            var timer = _container.GetInstance<ITimer>();

            var task = StartCommandWithTimer(command, timer, runs, cancellationToken);

            Console.WriteLine("Task executing - waiting for it to finish");
            return task;
        }

        private Task<IOperation> StartCommandWithTimer(ICommand command, ITimer timer, int repeat, CancellationToken cancellationToken)
        {
            timer.StartCommand();
            return RunCommandANumberOfTimes(command, repeat, cancellationToken).ContinueWith(t => timer.EndCommand());
        }

        private async Task RunCommandANumberOfTimes(ICommand command, int repeat, CancellationToken cancellationToken)
        {
            for (int i = 0; i < repeat; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                await command.DoAsync(cancellationToken);
            }
        }

        private void WaitForCommandToCompleteOrCancel(Task commandTask, CancellationTokenSource cancellationTokenSource)
        {
            StartWaitingForManualCancelAsync(cancellationTokenSource);

            commandTask.Wait(cancellationTokenSource.Token);

            cancellationTokenSource.Cancel(false);
        }

        private Task StartWaitingForManualCancelAsync(CancellationTokenSource cancellationTokenSource)
        {
            return Task.Run((Action)(() =>
            {
                Console.WriteLine("press escape to cancel command");
                while (Console.ReadKey(true).Key != ConsoleKey.Escape && !cancellationTokenSource.IsCancellationRequested)
                    Console.WriteLine("Key ignored - press escape to quit");
                cancellationTokenSource.Cancel(false);
            }), cancellationTokenSource.Token);
        }
    }
}
