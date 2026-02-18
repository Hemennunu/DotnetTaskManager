using TaskFlow.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using TaskStatus = TaskFlow.Domain.Enums.TaskStatus;

namespace TaskFlow.Application.DTOs
{
    public class UpdateTaskDto
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        public TaskStatus Status { get; set; }
        public TaskPriority Priority { get; set; }
    }
}
