using CP_Task.Data;
using CP_Task.Model;

namespace CP_Task.Infastructure.IServices
{
    public interface IApplicationsService
    {
        Task<string> AddApplicationAsync(ApplicationDto applicationDto);
        Task DeleteApplicationAsync(string id);
        Task<Application> GetApplicationAsync(string id);
        Task<IEnumerable<Application>> GetApplicationsAsync();
        Task<string> UpdateApplicationAsync(string id, ApplicationDto applicationDto);
    }
}
