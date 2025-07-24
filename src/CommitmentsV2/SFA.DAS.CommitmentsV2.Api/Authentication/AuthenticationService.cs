using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Types;
using IAuthenticationService = SFA.DAS.CommitmentsV2.Authentication.IAuthenticationService;

namespace SFA.DAS.CommitmentsV2.Api.Authentication;

public class AuthenticationService : IAuthenticationService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationServiceType AuthenticationServiceType => AuthenticationServiceType.HttpContext;

    public AuthenticationService(IHttpContextAccessor httpContextAccessor, ILogger<AuthenticationService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public IEnumerable<string> GetAllUserRoles()
    {
        return _httpContextAccessor.HttpContext.User.Identities.SelectMany(i => i.FindAll(i.RoleClaimType).Select(c => c.Value));
    }

    public bool IsUserInRole(string role)
    {
        return _httpContextAccessor.HttpContext.User.IsInRole(role);
    }

    public Party GetUserParty()
    {
        _logger.LogInformation("=== AUTHENTICATION SERVICE: GetUserParty called ===");
        
        if (_httpContextAccessor.HttpContext?.User == null)
        {
            _logger.LogError("HttpContext or User is null!");
            throw new ArgumentException("HttpContext or User is null");
        }

        _logger.LogInformation("User.Identity.IsAuthenticated: {IsAuthenticated}", _httpContextAccessor.HttpContext.User.Identity?.IsAuthenticated);
        _logger.LogInformation("User.Identity.Name: {UserName}", _httpContextAccessor.HttpContext.User.Identity?.Name);

        // Log all user claims
        _logger.LogInformation("=== USER CLAIMS ===");
        foreach (var claim in _httpContextAccessor.HttpContext.User.Claims)
        {
            _logger.LogInformation("Claim: {ClaimType} = {ClaimValue}", claim.Type, claim.Value);
        }

        var isEmployer = IsUserInRole(Role.Employer);
        var isProvider = IsUserInRole(Role.Provider);
        
        _logger.LogInformation("IsUserInRole(Role.Employer): {IsEmployer}", isEmployer);
        _logger.LogInformation("IsUserInRole(Role.Provider): {IsProvider}", isProvider);
            
        // The client app should be one _or_ the other (not both, not neither).
        if (isEmployer ^ isProvider)
        {
            var party = isEmployer ? Party.Employer : Party.Provider;
            _logger.LogInformation("Successfully determined party: {Party}", party);
            return party;
        }

        _logger.LogError("Unable to determine party - IsEmployer: {IsEmployer}, IsProvider: {IsProvider}", isEmployer, isProvider);

        //This method may need revisiting in future, as it does not support TransferSenders, who are in the Employer role. Specific endpoints will be
        //made available for TransferSender functionality, so perhaps it doesn't matter - in this case, we would just need to assert that the user is
        //in the Employer role and thereby infer that they must be the TransferSender in that context. Alternatively, could another implementation of this
        //be created for use within the TransferSender functionality? This would assert that the user is in the Employer role, and return TransferSender
        //as the Party, or otherwise throw an exception.

        throw new ArgumentException($"Unable to map User Role (IsEmployer:{isEmployer}, IsProvider:{isProvider}) to Party");
    }
}