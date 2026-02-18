using TaskFlow.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Application.DTOs
{
    public class CreateTaskDto
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        public TaskPriority Priority { get; set; } = TaskPriority.Medium;

        [Required]
        public string AssignedUserId { get; set; } = string.Empty;
    }
}
