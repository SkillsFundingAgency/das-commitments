
namespace SFA.DAS.CommitmentsV2.Domain.Interfaces;

public interface IEmailOptionalService
{
    bool ApprenticeEmailIsRequiredFor(long employerId, long providerId);
    bool ApprenticeEmailIsRequiredForProvider(long providerId);
    bool ApprenticeEmailIsRequiredForEmployer(long employerId);
    bool ApprenticeEmailIsOptionalFor(long employerId, long providerId);
    bool ApprenticeEmailIsOptionalForProvider(long providerId);
    bool ApprenticeEmailIsOptionalForEmployer(long employerId);
}