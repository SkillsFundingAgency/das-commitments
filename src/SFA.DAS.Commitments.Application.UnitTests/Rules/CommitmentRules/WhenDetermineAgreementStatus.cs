using System.Collections.Generic;
using AutoFixture;
using FluentAssertions;

using NUnit.Framework;

using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.UnitTests.Rules.CommitmentRules
{
    [TestFixture]
    public class WhenDetermineAgreementStatus
    {
        [TestCase(AgreementStatus.NotAgreed)]
        [TestCase(AgreementStatus.BothAgreed)]
        [TestCase(AgreementStatus.ProviderAgreed)]
        [TestCase(AgreementStatus.EmployerAgreed)]
        public void ShouldReturnSameAsAllApprenticeships(AgreementStatus agreementStatus)
        {
            var _sut = new Application.Rules.CommitmentRules();

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
