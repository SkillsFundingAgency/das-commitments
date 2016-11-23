namespace SFA.DAS.Commitments.Application.UnitTests.Rules.CommitmentRules
{
    using System.Collections.Generic;

    using FluentAssertions;

    using NUnit.Framework;

    using Ploeh.AutoFixture;

    using SFA.DAS.Commitments.Application.Rules;
    using SFA.DAS.Commitments.Domain.Entities;

    [TestFixture]
    public class WhenDetermineAgreementStatus
    {
        [TestCase(AgreementStatus.NotAgreed)]
        [TestCase(AgreementStatus.BothAgreed)]
        [TestCase(AgreementStatus.ProviderAgreed)]
        [TestCase(AgreementStatus.EmployerAgreed)]
        public void ShouldReturnSameAsAllApprenticeships(AgreementStatus agreementStatus)
        {
            var _sut = new CommitmentRules();

            var fixture = new Fixture();

            fixture.Customize<Apprenticeship>(ob => ob
                .With(x => x.AgreementStatus, agreementStatus));

            var apprenticeships = new List<Apprenticeship>
            {
                fixture.Create<Apprenticeship>(),
                fixture.Create<Apprenticeship>(),
                fixture.Create<Apprenticeship>()
            };

            _sut.DetermineAgreementStatus(apprenticeships).ShouldBeEquivalentTo(agreementStatus);
        }
    }
}
