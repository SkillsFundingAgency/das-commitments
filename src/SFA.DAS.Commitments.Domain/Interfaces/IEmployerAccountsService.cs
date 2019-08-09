using System.Threading.Tasks;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Domain.Interfaces
{
    public interface IEmployerAccountsService
    {
        Task<Account> GetAccount(long accountId);
    }
}