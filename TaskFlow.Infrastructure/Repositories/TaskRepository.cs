using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Repositories
{
    public class TaskRepository : ITaskRepository
    {
        private readonly ApplicationDbContext _context;

        public TaskRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Task>> GetAllTasksAsync()
        {
            return await _context.Tasks
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedUser)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<Task?> GetTaskByIdAsync(int id)
        {
            return await _context.Tasks
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedUser)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<Task>> GetTasksByUserIdAsync(string userId)
        {
            return await _context.Tasks
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedUser)
                .Where(t => t.AssignedUserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Task>> GetTasksCreatedByUserAsync(string userId)
        {
            return await _context.Tasks
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedUser)
                .Where(t => t.CreatedByUserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<Task> CreateTaskAsync(Task task)
        {
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            return task;
        }

        public async Task<Task> UpdateTaskAsync(Task task)
        {
            task.UpdatedAt = DateTime.UtcNow;
            _context.Tasks.Update(task);
            await _context.SaveChangesAsync();
            return task;
        }

        public async Task<bool> DeleteTaskAsync(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
                return false;

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> TaskExistsAsync(int id)
        {
            return await _context.Tasks.AnyAsync(t => t.Id == id);
        }
    }
}
