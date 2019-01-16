using CommandLine;

namespace SFA.DAS.Commitments.EFCoreTester.CommandLine
{
    [Verb("write", HelpText = "Populates the database")]
    public class WriteCommandLine : CommonCommandLine
    {
        [Option('d', "draftCount", HelpText = "The number of drafts that are to be created", Default = 10)]
        public int DraftCount { get; set; }

        [Option('c', "confirmedCount", HelpText = "The number of confirmed apprenticeships that are to be created", Default = 10)]
        public int ConfirmedCount { get; set; }

        [Option('s', "single", HelpText = "Create one apprenticeship per cohort (default is to create all apprenticeships in the same cohort)", Default = false)]
        public bool SingleApprenticeshipPerCommitment { get; set; }
    }
}
