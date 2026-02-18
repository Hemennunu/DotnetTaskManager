using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskStatus = TaskFlow.Domain.Enums.TaskStatus;

namespace TaskFlow.Application.Interfaces
{
    public interface ITaskRepository
    {
        System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<Task>> GetAllTasksAsync();
        System.Threading.Tasks.Task<Task?> GetTaskByIdAsync(int id);
        System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<Task>> GetTasksByUserIdAsync(string userId);
        System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<Task>> GetTasksCreatedByUserAsync(string userId);
        System.Threading.Tasks.Task<Task> CreateTaskAsync(Task task);
        System.Threading.Tasks.Task<Task> UpdateTaskAsync(Task task);
        System.Threading.Tasks.Task<bool> DeleteTaskAsync(int id);
        System.Threading.Tasks.Task<bool> TaskExistsAsync(int id);
    }
}
