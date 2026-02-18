using System.Collections.Generic;
using System.Threading.Tasks;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Interfaces
{
    public interface ITaskRepository
    {
        Task<IEnumerable<Domain.Entities.Task>> GetAllTasksAsync();
        Task<Domain.Entities.Task?> GetTaskByIdAsync(int id);
        Task<IEnumerable<Domain.Entities.Task>> GetTasksByUserIdAsync(string userId);
        Task<IEnumerable<Domain.Entities.Task>> GetTasksCreatedByUserAsync(string userId);
        Task<Domain.Entities.Task> CreateTaskAsync(Domain.Entities.Task task);
        Task<Domain.Entities.Task> UpdateTaskAsync(Domain.Entities.Task task);
        Task<bool> DeleteTaskAsync(int id);
        Task<bool> TaskExistsAsync(int id);
    }
}
