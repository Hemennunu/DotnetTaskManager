using TaskFlow.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using TaskStatus = TaskFlow.Domain.Enums.TaskStatus;

namespace TaskFlow.Application.DTOs
{
    public class UpdateTaskFullDto
    {
        [Required]
        [StringLength(200)]
        public string? Title { get; set; }
        
        public string? Description { get; set; }
        
        public TaskPriority? Priority { get; set; }
        
        public TaskStatus? Status { get; set; }
        
        public string? AssignedUserId { get; set; }
    }
}
