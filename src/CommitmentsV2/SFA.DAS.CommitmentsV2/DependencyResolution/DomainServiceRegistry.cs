using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Services;
using StructureMap;

namespace SFA.DAS.CommitmentsV2.DependencyResolution
{
    public class DomainServiceRegistry : Registry
    {
        public DomainServiceRegistry()
        {
            For<ICohortDomainService>().Use<CohortDomainService>();
            For<IChangeOfPartyRequestDomainService>().Use<ChangeOfPartyRequestDomainService>();

            For<IReservationValidationService>().Use<ReservationValidationService>();
            For<IEmployerAgreementService>().Use<EmployerAgreementService>().Singleton();
            For<IUlnUtilisationService>().Use<UlnUtilisationService>();
            For<IOverlapCheckService>().Use<OverlapCheckService>();
            For<IUlnValidator>().Use<UlnValidator>();
            For<IEditApprenticeshipValidationService>().Use<EditApprenitceshipValidationService>();

            //todo: below line doesn't belong here. ideally, push this into a registry in the package itself, or an extension thereof
            For<Learners.Validators.IUlnValidator>().Use<Learners.Validators.UlnValidator>(); 
        }
    }
}