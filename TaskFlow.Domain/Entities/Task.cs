using TaskStatus = TaskFlow.Domain.Enums.TaskStatus;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Domain.Entities
{
    public class Task
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public TaskStatus Status { get; set; } = TaskStatus.Pending;
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        // Foreign key for assigned user
        public string? AssignedUserId { get; set; }
        public virtual ApplicationUser? AssignedUser { get; set; }
        
        // Foreign key for admin who created the task
        public required string CreatedByUserId { get; set; }
        public virtual ApplicationUser CreatedByUser { get; set; } = null!;
    }
}
