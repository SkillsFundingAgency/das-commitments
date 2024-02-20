using System;
using System.Threading.Tasks;
using CommandLine;
using SFA.DAS.CommitmentsV2.TestSubscriber.CommandLines;

namespace SFA.DAS.CommitmentsV2.TestSubscriber;

public class Program
{
    private static Task Main(string[] args)
    {
        Task task = null;
        Console.Title = Constants.AppName;

        Parser.Default.ParseArguments<StartSubscriberCommandLineArgs>(args)
            .WithParsed(commandLineArguments => task = StartSubscriber(commandLineArguments))
            .WithNotParsed(parserResult =>
            {
                Console.WriteLine("The command line is incorrect:");
                foreach (var error in parserResult)
                {
                    Console.WriteLine(error.Tag);
                }
            });

        return task ?? Task.CompletedTask;
    }

    private static Task StartSubscriber(StartSubscriberCommandLineArgs args)
    {
        var runner = new NServiceBusRunner();
        return runner.StartNServiceBusBackgroundTask(args.ConnectionString);
    }
}