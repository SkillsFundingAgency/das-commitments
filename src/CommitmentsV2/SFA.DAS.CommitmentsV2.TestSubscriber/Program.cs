using System;
using System.Threading.Tasks;
using CommandLine;
using SFA.DAS.CommitmentsV2.TestSubscriber.CommandLines;

namespace SFA.DAS.CommitmentsV2.TestSubscriber
{
    class Program
    {
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
        }

        private Task StartSubscriber(StartSubscriberCommandLineArgs args)
        {
            var runner = new NServiceBusRunner();
            return runner.StartNServiceBusBackgroundTask(args.ConnectionString);
        }
    }
}
