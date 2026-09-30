using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Queries.ValidateSelectMultipleLearnersRequest;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Mapping.BulkUpload;

public class ValidateSelectMultipleLearnersRequestToValidateSelectMultipleLearnersCommand : IMapper<ValidateSelectMultipleLearnersRequest, ValidateSelectMultipleLearnersQuery>
{
    public Task<ValidateSelectMultipleLearnersQuery> Map(ValidateSelectMultipleLearnersRequest source)
    {
        return Task.FromResult(new ValidateSelectMultipleLearnersQuery
        {
            CsvRecords = source.CsvRecords,
            ProviderId = source.ProviderId,
            ReservationValidationResults = source.BulkReservationValidationResults,
            ProviderStandardResults = source.ProviderStandardsData,
            OtjTrainingHours = source.OtjTrainingHours
        });
    }
}