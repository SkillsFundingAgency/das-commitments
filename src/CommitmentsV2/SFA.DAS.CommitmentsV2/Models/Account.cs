using System;

namespace SFA.DAS.CommitmentsV2.Models
{
    public class Account
    {
        public long Id { get; private set; }
        public string HashedId { get; private set; }
        public string PublicHashedId { get; private set; }
        public string Name { get; private set; }
        public DateTime Created { get; private set; }
        public DateTime? Updated { get; private set; }

        public Account(long id, string hashedId, string publicHashedId, string name, DateTime created)
        {
            Id = id;
            HashedId = hashedId;
            PublicHashedId = publicHashedId;
            Name = name;
            Created = created;
        }

        private Account()
        {
        }

        public void UpdateName(string name, DateTime updated)
        {
            if (IsUpdatedNameDateChronological(updated) && IsUpdatedNameDifferent(name))
            {
                Name = name;
                Updated = updated;
            }
        }

        private bool IsUpdatedNameDateChronological(DateTime updated)
        {
            return Updated == null || updated > Updated.Value;
        }

        private bool IsUpdatedNameDifferent(string name)
        {
            return name != Name;
        }
    }

}
