using CommandLine;

namespace SFA.DAS.Commitments.EFCoreTester.CommandLine
{
    [Verb("read", HelpText = "Runs queries against the database")]
    public class ReadCommandLine
    {
        [Option('t', "tableName", HelpText = "Execute a query against this table")]
        public string TableName { get; set; }
    }
}
