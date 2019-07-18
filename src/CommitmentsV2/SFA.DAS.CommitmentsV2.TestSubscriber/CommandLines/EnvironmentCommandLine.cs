using System;
using System.Collections.Generic;
using System.Text;
using CommandLine;

namespace SFA.DAS.CommitmentsV2.TestSubscriber.CommandLines
{
    public class StartSubscriberCommandLineArgs
    {
        [Option('c', "connectionstring", HelpText = "Specifies the AZ SB connection. If not specified then the learning environment will be used", Required = false)]
        public string ConnectionString { get; set; }
    }
}