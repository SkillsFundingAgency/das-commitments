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

            Assert.AreEqual(fixture.ApprenticeshipUpdate.FirstName, viewModel.FirstName);
        }

        [Test]
        public void LastName_IsMapped()
        {
            var viewModel =  fixture.Map();

            Assert.AreEqual(fixture.ApprenticeshipUpdate.LastName, viewModel.LastName);
        }

        [Test]
        public void DateOfBirth_IsMapped()
        {
            var viewModel =  fixture.Map();

            Assert.AreEqual(fixture.ApprenticeshipUpdate.DateOfBirth, viewModel.DateOfBirth);
        }

        [Test]
        public void Cost_IsMapped()
        {
            var viewModel =  fixture.Map();

            Assert.AreEqual(fixture.ApprenticeshipUpdate.Cost, viewModel.Cost);
        }

        [Test]
        public void StartDate_IsMapped()
        {
            var viewModel =  fixture.Map();

            Assert.AreEqual(fixture.ApprenticeshipUpdate.StartDate, viewModel.StartDate);
        }

        [Test]
        public void EndDate_IsMapped()
        {
            var viewModel =  fixture.Map();

            Assert.AreEqual(fixture.ApprenticeshipUpdate.EndDate, viewModel.EndDate);
        }

        [Test]
        public void DeliveryModel_IsMapped()
        {
            var viewModel =  fixture.Map();

            Assert.AreEqual(fixture.ApprenticeshipUpdate.DeliveryModel, viewModel.DeliveryModel);
        }

        [Test]
        public void EmploymentEndDate_IsMapped()
        {
            var viewModel =  fixture.Map();

            Assert.AreEqual(fixture.ApprenticeshipUpdate.EmploymentEndDate, viewModel.EmploymentEndDate);
        }

        [Test]
        public void EmploymentPrice_IsMapped()
        {
            var viewModel =  fixture.Map();

            Assert.AreEqual(fixture.ApprenticeshipUpdate.EmploymentPrice, viewModel.EmploymentPrice);
        }

        [Test]
        public void CourseCode_IsMapped()
        {
            var viewModel =  fixture.Map();

            Assert.AreEqual(fixture.ApprenticeshipUpdate.TrainingCode, viewModel.CourseCode);
        }

        [Test]
        public void Email_IsMapped()
        {
            var viewModel =  fixture.Map();

            Assert.AreEqual(fixture.ApprenticeshipUpdate.Email, viewModel.Email);
        }

        [Test]
        public void TrainingName_IsMapped()
        {
            var viewModel =  fixture.Map();

            Assert.AreEqual(fixture.ApprenticeshipUpdate.TrainingName, viewModel.CourseName);
        }

        [Test]
        public void Version_IsMapped()
        {
            var viewModel =  fixture.Map();

            Assert.AreEqual(fixture.ApprenticeshipUpdate.TrainingCourseVersion, viewModel.Version);
        }

        [Test]
        public void Option_IsMapped()
        {
            var viewModel =  fixture.Map();

            Assert.AreEqual(fixture.ApprenticeshipUpdate.TrainingCourseOption, viewModel.Option);
        }

        [Test]
        public void OriginalApprenticeship_FirstName_IsMapped()
        {
            var viewModel =  fixture.Map();

            Assert.AreEqual(fixture.ApprenticeshipUpdate.FirstName, viewModel.FirstName);
        }

        [Test]
        public void OriginalApprenticeship_LastName_IsMapped()
        {
            var viewModel =  fixture.Map();

            Assert.AreEqual(fixture.ApprenticeshipUpdate.LastName, viewModel.LastName);
        }

        [Test]
        public void OriginalApprenticeship_DateOfBirth_IsMapped()
        {
            var viewModel =  fixture.Map();

            Assert.AreEqual(fixture.ApprenticeshipUpdate.DateOfBirth, viewModel.DateOfBirth);
        }

        [Test]
        public void OriginalApprenticeship_Cost_IsMapped()
        {
            var viewModel =  fixture.Map();

            Assert.AreEqual(fixture.ApprenticeshipUpdate.Cost, viewModel.Cost);
        }

        [Test]
        public void OriginalApprenticeship_DeliveryModel_IsMapped()
        {
            var viewModel =  fixture.Map();

            Assert.AreEqual(fixture.ApprenticeshipUpdate.DeliveryModel, viewModel.DeliveryModel);
        }

        [Test]
        public void OriginalApprenticeship_EmploymentEndDate_IsMapped()
        {
            var viewModel =  fixture.Map();

            Assert.AreEqual(fixture.ApprenticeshipUpdate.EmploymentEndDate, viewModel.EmploymentEndDate);
        }

        [Test]
        public void OriginalApprenticeship_EmploymentPrice_IsMapped()
        {
            var viewModel =  fixture.Map();

            Assert.AreEqual(fixture.ApprenticeshipUpdate.EmploymentPrice, viewModel.EmploymentPrice);
        }

        [Test]
        public void OriginalApprenticeship_StartDate_IsMapped()
        {
            var viewModel =  fixture.Map();

            Assert.AreEqual(fixture.ApprenticeshipUpdate.StartDate, viewModel.StartDate);
        }

        [Test]
        public void OriginalApprenticeship_EndDate_IsMapped()
        {
            var viewModel =  fixture.Map();

            Assert.AreEqual(fixture.ApprenticeshipUpdate.EndDate, viewModel.EndDate);
        }

        [Test]
        public void OriginalApprenticeship_CourseCode_IsMapped()
        {
            var viewModel =  fixture.Map();

            Assert.AreEqual(fixture.ApprenticeshipUpdate.TrainingCode, viewModel.CourseCode);
        }

        [Test]
        public void OriginalApprenticeship_TrainingName_IsMapped()
        {
            var viewModel =  fixture.Map();

            Assert.AreEqual(fixture.ApprenticeshipUpdate.TrainingName, viewModel.CourseName);
        }

        [Test]
        public void OriginalApprenticeship_Version_IsMapped()
        {
            var viewModel =  fixture.Map();

            Assert.AreEqual(fixture.ApprenticeshipUpdate.TrainingCourseVersion, viewModel.Version);
        }

        [Test]
        public void OriginalApprenticeship_Option_IsMapped()
        {
            var viewModel =  fixture.Map();

            Assert.AreEqual(fixture.ApprenticeshipUpdate.TrainingCourseOption, viewModel.Option);
        }

        [Test]
        public void If_FirstName_Only_Updated_Map_FirstName_From_OriginalApprenticeship()
        {
            fixture.ApprenticeshipUpdate.LastName = null;

            var viewModel =  fixture.Map();

            Assert.AreEqual(viewModel.FirstName + " " + fixture.ApprenticeshipDetails.LastName, viewModel.DisplayNameForUpdate);
        }

        [Test]
        public void If_LastName_Only_Updated_Map_FirstName_From_OriginalApprenticeship()
        {
            fixture.ApprenticeshipUpdate.FirstName = null;

            var viewModel =  fixture.Map();

            Assert.AreEqual(fixture.ApprenticeshipDetails.FirstName + " " + fixture.ApprenticeshipUpdate.LastName, viewModel.DisplayNameForUpdate);
        }

        [Test]
        public void If_BothNames_Updated_Map_BothNames_From_Update()
        {
            var viewModel =  fixture.Map();

            Assert.AreEqual(fixture.ApprenticeshipUpdate.FirstName, viewModel.FirstName);
            Assert.AreEqual(fixture.ApprenticeshipUpdate.LastName, viewModel.LastName);
        }

        [Test]
        public void OriginalApprenticeship_Email_IsMapped()
        {
            var viewModel = fixture.Map();

            Assert.AreEqual(fixture.ApprenticeshipUpdate.Email, viewModel.Email);
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
