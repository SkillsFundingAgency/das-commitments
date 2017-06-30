using System.Collections.Generic;

using Newtonsoft.Json;

namespace SFA.DAS.Commitments.Infrastructure.Models
{
    public class UserResponse
    {
        [JsonProperty("name.familyname")]
        public List<string> FamilyNames { get; set; }

        [JsonProperty("name.givenname")]
        public List<string> GivenNames { get; set; }

        [JsonProperty("emails")]
        public List<string> Emails { get; set; }

        [JsonProperty("Title")]
        public List<string> Titles { get; set; }
    }
}
