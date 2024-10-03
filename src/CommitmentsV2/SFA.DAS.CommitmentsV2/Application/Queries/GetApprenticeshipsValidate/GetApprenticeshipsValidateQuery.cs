namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipsValidate;

public class GetApprenticeshipsValidateQuery : IRequest<GetApprenticeshipsValidateQueryResult>
{
    public string FirstName { get; set; }
    public string LastName { get; }
    public DateTime DateOfBirth { get; }

    public GetApprenticeshipsValidateQuery(string firstName, string lastName, DateTime dateOfBirth)
    {
        FirstName = firstName;
        LastName = lastName;
        DateOfBirth = dateOfBirth;
    }
}