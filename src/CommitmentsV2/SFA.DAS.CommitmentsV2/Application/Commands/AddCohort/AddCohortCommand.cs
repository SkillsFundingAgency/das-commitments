using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.AddCohort;

public class AddCohortCommand : IRequest<AddCohortResult>
{
    public Party? RequestingParty { get; set; }
    public long AccountId { get; }
    public long AccountLegalEntityId { get; }
    public long ProviderId { get; }
    public string CourseCode { get; }
    public DeliveryModel? DeliveryModel { get; }
    public int? Cost { get; }
    public int? TrainingPrice { get; }
    public int? EndPointAssessmentPrice { get; }
    public DateTime? StartDate { get; }
    public DateTime? ActualStartDate { get; }
    public DateTime? EndDate { get; }
    public string OriginatorReference { get; }
    public Guid? ReservationId { get; }
    public string FirstName { get; }
    public string LastName { get; }
    public string Email { get; }
    public DateTime? DateOfBirth { get; }
    public string Uln { get; }
    public long? TransferSenderId { get; }
    public int? PledgeApplicationId { get; }
    public int? EmploymentPrice { get; set; }
    public DateTime? EmploymentEndDate { get; set; }
    public UserInfo UserInfo { get; }
    public bool IgnoreStartDateOverlap { get; set; }
    public bool? IsOnFlexiPaymentPilot { get; set; }
    public long? LearnerDataId { get; set; }


    public AddCohortCommand(Party? requestingParty, long accountId, long accountLegalEntityId, long providerId, string courseCode,
        DeliveryModel? deliveryModel, int? cost, DateTime? startDate, DateTime? actualStartDate, DateTime? endDate,
        string originatorReference, Guid? reservationId, string firstName,
        string lastName, string email, DateTime? dateOfBirth, string uln, long? transferSenderId,
        int? pledgeApplicationId,
        int? employmentPrice, DateTime? employmentEndDate, UserInfo userInfo, bool ignoreStartDateOverlap,
        bool? isOnFlexiPaymentPilot, int? trainingPrice, int? endPointAssessmentPrice, long? learnerDataId)
    {
        RequestingParty = requestingParty;
        AccountId = accountId;
        AccountLegalEntityId = accountLegalEntityId;
        ProviderId = providerId;
        CourseCode = courseCode;
        DeliveryModel = deliveryModel;
        Cost = cost;
        StartDate = startDate;
        ActualStartDate = actualStartDate;
        EndDate = endDate;
        OriginatorReference = originatorReference;
        ReservationId = reservationId;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        DateOfBirth = dateOfBirth;
        Uln = uln;
        TransferSenderId = transferSenderId;
        PledgeApplicationId = pledgeApplicationId;
        EmploymentPrice = employmentPrice;
        EmploymentEndDate = employmentEndDate;
        IsOnFlexiPaymentPilot = isOnFlexiPaymentPilot;
        TrainingPrice = trainingPrice;
        EndPointAssessmentPrice = endPointAssessmentPrice;

        UserInfo = userInfo ?? throw new ArgumentNullException(nameof(userInfo));
        IgnoreStartDateOverlap = ignoreStartDateOverlap;
        LearnerDataId = learnerDataId;
    }
}