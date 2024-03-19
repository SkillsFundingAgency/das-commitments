using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Support.SubSite.Enums;
using SFA.DAS.Commitments.Support.SubSite.Mappers;
using SFA.DAS.Commitments.Support.SubSite.Models;
using SFA.DAS.Commitments.Support.SubSite.Orchestrators;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Queries.GetSupportApprenticeship;
using System.Threading;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.Encoding;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipUpdate;
using AutoFixture.Kernel;
using System.Reflection;
using AutoFixture;
using System.Collections.ObjectModel;

namespace SFA.DAS.Commitments.Support.SubSite.UnitTests.Orchestrators.ApprenticeshipOrchestratorTests
{
    [TestFixture]
    public class ReviewApprenticeshipUpdatesRequestToViewModelMapperTests
    {
        ReviewApprenticeshipUpdatesRequestToViewModelMapperTestsFixture fixture;
        [SetUp]
        public void Arrange()
        {
            fixture = new ReviewApprenticeshipUpdatesRequestToViewModelMapperTestsFixture();
        }

        [Test]
        public void FirstName_IsMapped()
        {
            var viewModel =  fixture.Map();

            Assert.That(viewModel.FirstName, Is.EqualTo(fixture.ApprenticeshipUpdate.FirstName));
        }

        [Test]
        public void LastName_IsMapped()
        {
            var viewModel =  fixture.Map();

            Assert.That(viewModel.LastName, Is.EqualTo(fixture.ApprenticeshipUpdate.LastName));
        }

        [Test]
        public void DateOfBirth_IsMapped()
        {
            var viewModel =  fixture.Map();

            Assert.That(viewModel.DateOfBirth, Is.EqualTo(fixture.ApprenticeshipUpdate.DateOfBirth));
        }

        [Test]
        public void Cost_IsMapped()
        {
            var viewModel =  fixture.Map();

            Assert.That(viewModel.Cost, Is.EqualTo(fixture.ApprenticeshipUpdate.Cost));
        }

        [Test]
        public void StartDate_IsMapped()
        {
            var viewModel =  fixture.Map();

            Assert.That(viewModel.StartDate, Is.EqualTo(fixture.ApprenticeshipUpdate.StartDate));
        }

        [Test]
        public void EndDate_IsMapped()
        {
            var viewModel =  fixture.Map();

            Assert.That(viewModel.EndDate, Is.EqualTo(fixture.ApprenticeshipUpdate.EndDate));
        }

        [Test]
        public void DeliveryModel_IsMapped()
        {
            var viewModel =  fixture.Map();

            Assert.That(viewModel.DeliveryModel, Is.EqualTo(fixture.ApprenticeshipUpdate.DeliveryModel));
        }

        [Test]
        public void EmploymentEndDate_IsMapped()
        {
            var viewModel =  fixture.Map();

            Assert.That(viewModel.EmploymentEndDate, Is.EqualTo(fixture.ApprenticeshipUpdate.EmploymentEndDate));
        }

        [Test]
        public void EmploymentPrice_IsMapped()
        {
            var viewModel =  fixture.Map();

            Assert.That(viewModel.EmploymentPrice, Is.EqualTo(fixture.ApprenticeshipUpdate.EmploymentPrice));
        }

        [Test]
        public void CourseCode_IsMapped()
        {
            var viewModel =  fixture.Map();

            Assert.That(viewModel.CourseCode, Is.EqualTo(fixture.ApprenticeshipUpdate.TrainingCode));
        }

        [Test]
        public void Email_IsMapped()
        {
            var viewModel =  fixture.Map();

            Assert.That(viewModel.Email, Is.EqualTo(fixture.ApprenticeshipUpdate.Email));
        }

        [Test]
        public void TrainingName_IsMapped()
        {
            var viewModel =  fixture.Map();

            Assert.That(viewModel.CourseName, Is.EqualTo(fixture.ApprenticeshipUpdate.TrainingName));
        }

        [Test]
        public void Version_IsMapped()
        {
            var viewModel =  fixture.Map();

            Assert.That(viewModel.Version, Is.EqualTo(fixture.ApprenticeshipUpdate.TrainingCourseVersion));
        }

        [Test]
        public void Option_IsMapped()
        {
            var viewModel =  fixture.Map();

            Assert.That(viewModel.Option, Is.EqualTo(fixture.ApprenticeshipUpdate.TrainingCourseOption));
        }

        [Test]
        public void OriginalApprenticeship_FirstName_IsMapped()
        {
            var viewModel =  fixture.Map();

            Assert.That(viewModel.FirstName, Is.EqualTo(fixture.ApprenticeshipUpdate.FirstName));
        }

        [Test]
        public void OriginalApprenticeship_LastName_IsMapped()
        {
            var viewModel =  fixture.Map();

            Assert.That(viewModel.LastName, Is.EqualTo(fixture.ApprenticeshipUpdate.LastName));
        }

        [Test]
        public void OriginalApprenticeship_DateOfBirth_IsMapped()
        {
            var viewModel =  fixture.Map();

            Assert.That(viewModel.DateOfBirth, Is.EqualTo(fixture.ApprenticeshipUpdate.DateOfBirth));
        }

        [Test]
        public void OriginalApprenticeship_Cost_IsMapped()
        {
            var viewModel =  fixture.Map();

            Assert.That(viewModel.Cost, Is.EqualTo(fixture.ApprenticeshipUpdate.Cost));
        }

        [Test]
        public void OriginalApprenticeship_DeliveryModel_IsMapped()
        {
            var viewModel =  fixture.Map();

            Assert.That(viewModel.DeliveryModel, Is.EqualTo(fixture.ApprenticeshipUpdate.DeliveryModel));
        }

        [Test]
        public void OriginalApprenticeship_EmploymentEndDate_IsMapped()
        {
            var viewModel =  fixture.Map();

            Assert.That(viewModel.EmploymentEndDate, Is.EqualTo(fixture.ApprenticeshipUpdate.EmploymentEndDate));
        }

        [Test]
        public void OriginalApprenticeship_EmploymentPrice_IsMapped()
        {
            var viewModel =  fixture.Map();

            Assert.That(viewModel.EmploymentPrice, Is.EqualTo(fixture.ApprenticeshipUpdate.EmploymentPrice));
        }

        [Test]
        public void OriginalApprenticeship_StartDate_IsMapped()
        {
            var viewModel =  fixture.Map();

            Assert.That(viewModel.StartDate, Is.EqualTo(fixture.ApprenticeshipUpdate.StartDate));
        }

        [Test]
        public void OriginalApprenticeship_EndDate_IsMapped()
        {
            var viewModel =  fixture.Map();

            Assert.That(viewModel.EndDate, Is.EqualTo(fixture.ApprenticeshipUpdate.EndDate));
        }

        [Test]
        public void OriginalApprenticeship_CourseCode_IsMapped()
        {
            var viewModel =  fixture.Map();

            Assert.That(viewModel.CourseCode, Is.EqualTo(fixture.ApprenticeshipUpdate.TrainingCode));
        }

        [Test]
        public void OriginalApprenticeship_TrainingName_IsMapped()
        {
            var viewModel =  fixture.Map();

            Assert.That(viewModel.CourseName, Is.EqualTo(fixture.ApprenticeshipUpdate.TrainingName));
        }

        [Test]
        public void OriginalApprenticeship_Version_IsMapped()
        {
            var viewModel =  fixture.Map();

            Assert.That(viewModel.Version, Is.EqualTo(fixture.ApprenticeshipUpdate.TrainingCourseVersion));
        }

        [Test]
        public void OriginalApprenticeship_Option_IsMapped()
        {
            var viewModel =  fixture.Map();

            Assert.That(viewModel.Option, Is.EqualTo(fixture.ApprenticeshipUpdate.TrainingCourseOption));
        }

        [Test]
        public void If_FirstName_Only_Updated_Map_FirstName_From_OriginalApprenticeship()
        {
            fixture.ApprenticeshipUpdate.LastName = null;

            var viewModel =  fixture.Map();

            Assert.That(viewModel.DisplayNameForUpdate, Is.EqualTo(viewModel.FirstName + " " + fixture.ApprenticeshipDetails.LastName));
        }

        [Test]
        public void If_LastName_Only_Updated_Map_FirstName_From_OriginalApprenticeship()
        {
            fixture.ApprenticeshipUpdate.FirstName = null;

            var viewModel =  fixture.Map();

            Assert.That(viewModel.DisplayNameForUpdate, Is.EqualTo(fixture.ApprenticeshipDetails.FirstName + " " + fixture.ApprenticeshipUpdate.LastName));
        }

        [Test]
        public void If_BothNames_Updated_Map_BothNames_From_Update()
        {
            var viewModel =  fixture.Map();

            Assert.That(viewModel.FirstName, Is.EqualTo(fixture.ApprenticeshipUpdate.FirstName));
            Assert.That(viewModel.LastName, Is.EqualTo(fixture.ApprenticeshipUpdate.LastName));
        }

        [Test]
        public void OriginalApprenticeship_Email_IsMapped()
        {
            var viewModel = fixture.Map();

            Assert.That(viewModel.Email, Is.EqualTo(fixture.ApprenticeshipUpdate.Email));
        }

        public class ReviewApprenticeshipUpdatesRequestToViewModelMapperTestsFixture
        {
            public SupportApprenticeshipDetails ApprenticeshipDetails;
            public GetApprenticeshipUpdateQueryResult.ApprenticeshipUpdate ApprenticeshipUpdate;
            public GetApprenticeshipUpdateQueryResult ApprenticeshipUpdateQueryResult;
            private ApprenticeshipMapper _sut;

            public long ApprenticeshipId = 1;

            public ReviewApprenticeshipUpdatesRequestToViewModelMapperTestsFixture()
            {
                var autoFixture = new Fixture();
                autoFixture.Customizations.Add(new DateTimeSpecimenBuilder());

                ApprenticeshipUpdate = autoFixture.Create<GetApprenticeshipUpdateQueryResult.ApprenticeshipUpdate>();
                ApprenticeshipUpdateQueryResult = new GetApprenticeshipUpdateQueryResult();
                ApprenticeshipUpdateQueryResult.ApprenticeshipUpdates = (new List<GetApprenticeshipUpdateQueryResult.ApprenticeshipUpdate> { ApprenticeshipUpdate }).AsReadOnly();

                ApprenticeshipDetails = autoFixture.Create<SupportApprenticeshipDetails>();
                _sut = new ApprenticeshipMapper(Mock.Of<IEncodingService>());
            }

            public ApprenticeshipUpdateViewModel Map()
            {
                return _sut.MapToUpdateApprenticeshipViewModel(ApprenticeshipUpdateQueryResult, ApprenticeshipDetails);
            }

        }

        public class DateTimeSpecimenBuilder : ISpecimenBuilder
        {
            public object Create(object request, ISpecimenContext context)
            {
                var pi = request as PropertyInfo;
                if (pi == null || pi.PropertyType != typeof(DateTime?))
                    return new NoSpecimen();

                else
                {
                    DateTime dt;
                    var randomDateTime = context.Create<DateTime>();

                    if (pi.Name == "DateOfBirth")
                    {
                        dt = new DateTime(randomDateTime.Year, randomDateTime.Month, randomDateTime.Day);
                    }
                    else
                    {
                        dt = new DateTime(randomDateTime.Year, randomDateTime.Month, 1);
                    }

                    return dt;
                }
            }
        }
    }
}
