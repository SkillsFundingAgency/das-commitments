//using Microsoft.Extensions.Logging;
//using SFA.DAS.CommitmentsV2.Application.Commands.EditApprenticeship;
//using SFA.DAS.CommitmentsV2.Authentication;
//using SFA.DAS.CommitmentsV2.Data;
//using SFA.DAS.CommitmentsV2.Data.Extensions;
//using SFA.DAS.CommitmentsV2.Extensions;
//using SFA.DAS.CommitmentsV2.Models;
//using SFA.DAS.CommitmentsV2.Shared.Interfaces;
//using SFA.DAS.CommitmentsV2.Types;
//using System;
//using System.Threading;
//using System.Threading.Tasks;

//namespace SFA.DAS.CommitmentsV2.Mapping.Apprenticeships.Edit
//{
//    public class EditApprenticeshipCommandToApprenticeshipUpdate : IMapper<EditApprenticeshipCommand, ApprenticeshipUpdate>
//    {
//        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
//        private readonly ICurrentDateTime _currentDateTime;
//        private readonly IAuthenticationService _authnticationService;
//        private readonly ILogger<EditApprenticeshipCommandToApprenticeshipUpdate> _logger;

//        public EditApprenticeshipCommandToApprenticeshipUpdate(IAuthenticationService authenticationService, ICurrentDateTime currentDateTime, ILogger<EditApprenticeshipCommandToApprenticeshipUpdate> logger)
//        {
//            _authnticationService = authenticationService;
//            _currentDateTime = currentDateTime;
//            _logger = logger;
//        }

//        public async Task<ApprenticeshipUpdate> Map(EditApprenticeshipCommand command)
//        {
//            bool apprenticeshipUpdateCreated = false;
//            var apprenticeshipUpdate = new ApprenticeshipUpdate();

//            var apprenticeship = await _dbContext.Value.GetApprenticeshipAggregate(command.ApprenticeshipId, CancellationToken.None);
//            var party = _authnticationService.GetUserParty();

//            if (command.CourseCode != apprenticeship.CourseCode)
//            {
//                apprenticeshipUpdate.TrainingCode = command.CourseCode;
//                apprenticeshipUpdateCreated = true;
//            }

//            if (command.DateOfBirth != apprenticeship.DateOfBirth)
//            {
//                apprenticeshipUpdate.DateOfBirth = command.DateOfBirth;
//                apprenticeshipUpdateCreated = true;
//            }

//            if (command.EndDate != apprenticeship.EndDate)
//            {
//                apprenticeship.EndDate = command.EndDate;
//                apprenticeshipUpdateCreated = true;
//            }

//            if (command.StartDate != apprenticeship.StartDate)
//            {
//                apprenticeshipUpdate.StartDate = command.StartDate;
//                apprenticeshipUpdateCreated = true;
//            }

//            if (command.LastName != apprenticeship.LastName)
//            {
//                apprenticeshipUpdate.LastName = command.LastName;
//                apprenticeshipUpdateCreated = true;
//            }

//            if (command.FirstName != apprenticeship.FirstName)
//            {
//                apprenticeshipUpdate.FirstName = command.FirstName;
//                apprenticeshipUpdateCreated = true;
//            }

//            if (command.Cost != apprenticeship.PriceHistory.GetPrice(_currentDateTime.UtcNow))
//            {
//                apprenticeshipUpdate.FirstName = command.FirstName;
//                apprenticeshipUpdateCreated = true;
//            }

//            if (apprenticeshipUpdateCreated)
//            {
//                apprenticeship.Id = apprenticeship.Id;
//                apprenticeshipUpdate.Originator = party == Party.Employer ? Originator.Employer : Originator.Provider;
//                apprenticeshipUpdate.UpdateOrigin = ApprenticeshipUpdateOrigin.ChangeOfCircumstances;
//                apprenticeshipUpdate.EffectiveFromDate = apprenticeship.StartDate;
//            }

//            return apprenticeshipUpdate;
//        }
//    }
//}
