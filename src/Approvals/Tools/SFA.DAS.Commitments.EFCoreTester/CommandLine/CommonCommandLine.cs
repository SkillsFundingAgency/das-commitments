using CommandLine;

namespace SFA.DAS.Commitments.EFCoreTester.CommandLine
{
    public class CommonCommandLine
    {
        [Option('r', "runs", HelpText = "Repeat the runs this number of times", Default = 1)]
        public int Runs { get; set; }

        [Option('t', "time", HelpText = "Show detailed timings")]
        public bool ShowTimings { get; set; }

    }
}