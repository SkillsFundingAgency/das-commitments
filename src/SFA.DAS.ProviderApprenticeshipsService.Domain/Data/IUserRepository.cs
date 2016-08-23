using System.Threading.Tasks;

namespace SFA.DAS.ProviderApprenticeshipsService.Domain.Data
{
    public interface IUserRepository
    {
        Task<User> GetById(string id);
        Task<User> GetByEmailAddress(string emailAddress);
        Task Create(User registerUser);
        Task Update(User user);
        Task<Users> GetAllUsers();
    }
}