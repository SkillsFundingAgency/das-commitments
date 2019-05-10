using System;
using System.Threading.Tasks;
using CommandLine;
using SFA.DAS.CommitmentsV2.TestSubscriber.CommandLines;
using StructureMap;

namespace SFA.DAS.CommitmentsV2.TestSubscriber
{
    class Program
    {
        private readonly IContainer _container;

        private static Task Main(string[] args)
        {
            Task task = null;

            Parser.Default.ParseArguments<StartSubscriberCommandLineArgs>(args)
                .WithParsed(commandLineArguments => task = new Program().StartSubscriber(commandLineArguments))
                .WithNotParsed(parserResult =>
                {
                    Console.WriteLine("The command line is incorrect:");
                    foreach (Error error in parserResult)
                    {
                        Console.WriteLine((object)error.Tag);
                    }
                });

            return task ?? Task.CompletedTask;
        }

        public Program()
        {
            Console.Title = Constants.AppName;
            _container = IoC.InitializeIoC();
        }

        private Task StartSubscriber(StartSubscriberCommandLineArgs args)
        {
            var runner = _container.GetInstance<INServiceBusRunner>();

            return runner.StartNServiceBusBackgroundTask(args.ConnectionString);
        }
    }
}
