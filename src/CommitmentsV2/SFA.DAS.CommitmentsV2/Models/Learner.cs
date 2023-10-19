using System;

namespace SFA.DAS.CommitmentsV2.Models
{
    // This is a pseudo-entity to represent the result of the GetLearnersBatch stored proc, it's not a table in the database.
    public class Learner
    {
        public long ApprenticeshipId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ULN { get; set; }
        public string TrainingCode { get; set; }
        public string TrainingCourseVersion { get; set; }
        public bool TrainingCourseVersionConfirmed { get; set; }
        public string TrainingCourseOption { get; set; }
        public string StandardUId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public DateTime? StopDate { get; set; }
        public DateTime? PauseDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public long UKPRN { get; set; }
        public string LearnRefNumber { get; set; }
        public short PaymentStatus { get; set; }
        public long EmployerAccountId { get; set; }
        public string EmployerName { get; set; }

        public static implicit operator Api.Types.Responses.Learner(Learner dbLearner)
        => new Api.Types.Responses.Learner()
        {
            ApprenticeshipId = dbLearner.ApprenticeshipId,
            FirstName = dbLearner.FirstName,
            LastName = dbLearner.LastName,
            ULN = dbLearner.ULN,
            TrainingCode = dbLearner.TrainingCode,
            TrainingCourseVersion = dbLearner.TrainingCourseVersion,
            TrainingCourseVersionConfirmed = dbLearner.TrainingCourseVersionConfirmed,
            TrainingCourseOption = dbLearner.TrainingCourseOption,
            StandardUId = dbLearner.StandardUId,
            StartDate = dbLearner.StartDate,
            EndDate = dbLearner.EndDate,
            CreatedOn = dbLearner.CreatedOn,
            UpdatedOn = dbLearner.UpdatedOn,
            StopDate = dbLearner.StopDate,
            PauseDate = dbLearner.PauseDate,
            CompletionDate = dbLearner.CompletionDate,
            UKPRN = dbLearner.UKPRN,
            LearnRefNumber = dbLearner.LearnRefNumber,
            PaymentStatus = dbLearner.PaymentStatus,
            EmployerAccountId = dbLearner.EmployerAccountId,
            EmployerName = dbLearner.EmployerName,
        };
    }
}
