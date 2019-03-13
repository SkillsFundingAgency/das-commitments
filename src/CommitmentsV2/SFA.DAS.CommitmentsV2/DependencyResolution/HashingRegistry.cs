using Microsoft.Extensions.Configuration;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.ProviderCommitments.HashingTemp;
using StructureMap;

namespace SFA.DAS.CommitmentsV2.DependencyResolution
{
    public class HashingRegistry : Registry
    {
        public HashingRegistry()
        {
            For<IHashingService>()
                .Add("", ctx =>
                {
                    var config = ctx.GetInstance<CommitmentIdHashingConfiguration>();
                    return new HashingService(config.Alphabet, config.Salt);
                })
                .Singleton();
        }
    }
}
