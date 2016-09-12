using SFA.DAS.Tasks.Domain.Entities;

namespace SFA.DAS.ProviderApprenticeshipsService.Web.Models
{
    public class TaskViewModel
    {
        public long ProviderId { get; set; }
        public Task Task { get; set; }
    }
}