using System;
using System.Threading.Tasks;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipUpdate;
using SFA.DAS.CommitmentsV2.Mapping.ResponseMappers;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Mapping.ResponseMappers
{
    [TestFixture]
    public class UpdateDraftApprenticeshipRequestToUpdateDraftApprenticeshipCommandMapperTests :
    MapperTester<GetApprenticeshipUpdateMapper, GetApprenticeshipUpdateQueryResult, GetApprenticeshipUpdateResponse>
    {
        [Test]
        public Task Map_Id_ShouldBeSet()
        {
            return AssertPropertySet(from => from.Id, (long)2);
        }

        [Test]
        public Task Map_ApprenticeshipId_ShouldBeSet()
        {
            return AssertPropertySet(from => from.ApprenticeshipId, (long)2090);
        }

        [TestCase(Originator.Employer, Party.Employer)]
        [TestCase(Originator.Provider, Party.Provider)]
        public Task Map_Originator_ShouldBeSetToParty(Originator fromValue, Party expectedToValue)
        {
            return AssertPropertySet(from => from.Originator = fromValue, to => to.OriginatingParty == expectedToValue);
        }

        [TestCase(null)]
        [TestCase(123.32)]
        public Task Map_Cost_ShouldBeSet(decimal? value)
        {
            return AssertPropertySet(from => from.Cost, value);
        }

        [Test]
        public Task Map_FirstName_ShouldBeSet()
        {
            return AssertPropertySet(from => from.FirstName, "FirstName");
        }

        [Test]
        public Task Map_LastName_ShouldBeSet()
        {
            return AssertPropertySet(from => from.FirstName, "LastName");
        }

        [TestCase(ProgrammeType.Standard)]
        [TestCase(null)]
        public Task Map_TrainingType_ShouldBeSet(ProgrammeType? value)
        {
            return AssertPropertySet(from => from.TrainingType, value);
        }

        [Test]
        public Task Map_TrainingCode_ShouldBeSet()
        {
            return AssertPropertySet(from => from.TrainingCode, "XASA");
        }

        [Test]
        public Task Map_TrainingName_ShouldBeSet()
        {
            return AssertPropertySet(from => from.TrainingName, "Training name of course");
        }

        [Test]
        public Task Map_StartDateWithoutValue_ShouldBeSet()
        {
            return AssertPropertySet(from => from.StartDate, (DateTime?)null);
        }

        [Test]
        public Task Map_StartDateWithValue_ShouldBeSet()
        {
            return AssertPropertySet(from => from.StartDate, (DateTime?)DateTime.Now);
        }

        [Test]
        public Task Map_EndDateWithoutValue_ShouldBeSet()
        {
            return AssertPropertySet(from => from.EndDate, (DateTime?)null);
        }

        [Test]
        public Task Map_EndDateWithValue_ShouldBeSet()
        {
            return AssertPropertySet(from => from.EndDate, (DateTime?)DateTime.Now);
        }

        [Test]
        public Task Map_DateOfBirthWithoutValue_ShouldBeSet()
        {
            return AssertPropertySet(from => from.DateOfBirth, (DateTime?)null);
        }

        [Test]
        public Task Map_DateOfBirthWithValue_ShouldBeSet()
        {
            return AssertPropertySet(from => from.DateOfBirth, (DateTime?)DateTime.Now);
        }
    }
}
