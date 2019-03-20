namespace SFA.DAS.CommitmentsV2.Configuration
{
    public class HashingConfiguration
    {
        public string Alphabet { get; set; }
        public string Salt { get; set; }
    }

    public class CommitmentIdHashingConfiguration : HashingConfiguration
    {

    }
}
