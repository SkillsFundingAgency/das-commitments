using CommandLine;

namespace SFA.DAS.Commitments.EFCoreTester.CommandLine
{
    public enum ReadMode
    {
        EF,
        Dapper,
        AllTables
    }

    [Verb("read", HelpText = "Runs queries against the database")]
    public class ReadCommandLine : CommonCommandLine
    {
        [Option('m', "mode", HelpText = "Read mode (EF, Dapper or AllTables)", Default = ReadMode.EF)]
        public ReadMode Mode { get; set; }

        [Option('n', "notrack", HelpText = "Runs EF in no track mode (faster)", Default = false)]
        public bool NoTracking { get; set; }
    }
}
