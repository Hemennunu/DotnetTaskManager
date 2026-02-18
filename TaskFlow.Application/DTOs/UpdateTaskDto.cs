using TaskFlow.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using TaskStatus = TaskFlow.Domain.Enums.TaskStatus;

namespace TaskFlow.Application.DTOs
{
    public class UpdateTaskDto
    {
        [Required]
        public TaskStatus Status { get; set; }
    }
}
