using CommandLine;

namespace SFA.DAS.Commitments.EFCoreTester.CommandLine
{
    public enum TimingsMode
    {
        None,
        Full,
        Summary
    }

    public class CommonCommandLine
    {
        [Option('r', "runs", HelpText = "Repeat the runs this number of times", Default = 1)]
        public int Runs { get; set; }

        [Option('t', "time", HelpText = "Show timings (None, Full, Summary) - default None", Default = TimingsMode.None)]
        public TimingsMode TimingsMode { get; set; }

    }
}