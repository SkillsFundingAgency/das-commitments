namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships.Search.Parameters
{
    public class OrderedApprenticeshipSearchParameters : ApprenticeshipSearchParameters
    {
        public string FieldName { get; set; }
    }

    public class ReverseOrderedApprenticeshipSearchParameters : OrderedApprenticeshipSearchParameters
    {   }
}
