using System.Collections.Generic;

using SFA.DAS.Commitments.Application.Queries;
using SFA.DAS.Commitments.Domain.Entities;

using Apprenticeship = SFA.DAS.Commitments.Api.Types.Apprenticeship.Apprenticeship;

namespace SFA.DAS.Commitments.Api.Models
{
    public sealed class GetFilteredApprenticeshipsResponse : QueryResponse<IList<Apprenticeship>>
    {
        // ToDo: Use API type.
        public Facets Facets { get; set; }
    }

    public sealed class Facets
    {
        public List<FacetItem<ApprenticeshipStatus>> ApprenticeshipStatuses { get; set; }

        public List<FacetItem<RecordStatus>> RecordStatuses { get; set; }

        public List<FacetItem<string>> TrainingProviders { get; set; }

        public List<FacetItem<TrainingCourse>> TrainingCourses { get; set; }
    }

    public sealed class FacetItem<T>
    {
        public T Data { get; set; }

        public bool Selected { get; set; }
    }

    public class TrainingCourse
    {
        public string Name { get; set; }

        public string Id { get; set; }

        public TrainingType TrainingType { get; set; }
    }

    public enum RecordStatus
    {
        NoActionNeeded = 0,
        ChangesPending = 1,
        ChangesForReview = 2,
        ChangeRequested = 3
    }

    public enum ApprenticeshipStatus
    {
        None = 0,
        WaitingToStart = 1,
        Live = 2,
        Paused = 3,
        Stopped = 4,
        Finished = 5
    }

    public sealed class ApprenticeshipQuery
    {
        public List<ApprenticeshipStatus> ApprenticeshipStatuses { get; set; }

        public List<RecordStatus> RecordStatuses { get; set; }

        public List<string> TrainingProviders { get; set; }

        public List<TrainingCourse> TrainingCourses { get; set; }
    }
}