using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SFA.DAS.Commitments.Support.SubSite.Configuration
{
    public class CommitmentSupportSiteConfiguartion
    {
        public string DatabaseConnectionString { get; set; }
        public string AllowedHashstringCharacters { get;  set; }
        public string Hashstring { get;  set; }
    }
}