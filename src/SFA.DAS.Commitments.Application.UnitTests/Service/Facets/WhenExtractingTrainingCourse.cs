using System.Collections.Generic;
using System.Linq;

using FluentAssertions;

using NUnit.Framework;

using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.Commitments.Api.Types.Apprenticeship.Types;
using SFA.DAS.Commitments.Application.Services;

namespace SFA.DAS.Commitments.Application.UnitTests.Service.Facets
{
    [TestFixture]
    public class WhenExtractingTrainingCourse
    {
        private FacetMapper _sut;
        private List<Apprenticeship> _data;
        private ApprenticeshipSearchQuery _userQuery;

        [SetUp]
        public void SetUp()
        {
            _data = new List<Apprenticeship>();

            _userQuery = new ApprenticeshipSearchQuery();
            _sut = new FacetMapper();
        }

        [Test]
        public void ShouldHave2UniqueCourses()
        {
            _data.Add(new Apprenticeship { TrainingCode =  "2", TrainingName = "Software tester", TrainingType = TrainingType.Standard});
            _data.Add(new Apprenticeship { TrainingCode = "2", TrainingName = "Software tester", TrainingType = TrainingType.Standard });
            _data.Add(new Apprenticeship { TrainingCode = "22-166-0", TrainingName = "Fake framework", TrainingType = TrainingType.Framework });

            var result = _sut.BuildFacetes(_data, _userQuery, Originator.Employer);

            result.TrainingCourses.Count.Should().Be(2);
            result.TrainingCourses.Count(m => m.Selected).Should().Be(0);

            result.TrainingCourses[0]?.Data.Id.Should().Be("2");
            result.TrainingCourses[0]?.Data.Name.Should().Be("Software tester");
            result.TrainingCourses[0]?.Data.TrainingType.Should().Be(TrainingType.Standard);
            result.TrainingCourses[1]?.Data.Id.Should().Be("22-166-0");
            result.TrainingCourses[1]?.Data.Name.Should().Be("Fake framework");
            result.TrainingCourses[1]?.Data.TrainingType.Should().Be(TrainingType.Framework);
        }

        [Test]
        public void ShouldHave2UniqueCoursesAndOneSelected()
        {
            _data.Add(new Apprenticeship { TrainingCode = "2", TrainingName = "Software tester", TrainingType = TrainingType.Standard });
            _data.Add(new Apprenticeship { TrainingCode = "2", TrainingName = "Software tester", TrainingType = TrainingType.Standard });
            _data.Add(new Apprenticeship { TrainingCode = "22-166-0", TrainingName = "Fake framework", TrainingType = TrainingType.Framework });

            _userQuery.TrainingCourses = new List<string> { "33-33-3", "22-166-0", "3" };

            var result = _sut.BuildFacetes(_data, _userQuery, Originator.Employer);

            result.TrainingCourses.Count.Should().Be(2);
            result.TrainingCourses.Count(m => m.Selected).Should().Be(1);

            result.TrainingCourses[0]?.Data.Name.Should().Be("Software tester");
            result.TrainingCourses[1]?.Data.Name.Should().Be("Fake framework");

            result.TrainingCourses[0]?.Selected.Should().BeFalse();
            result.TrainingCourses[1]?.Selected.Should().BeTrue();
        }
    }
}
