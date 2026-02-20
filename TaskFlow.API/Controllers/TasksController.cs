using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Entities;
using TaskStatus = TaskFlow.Domain.Enums.TaskStatus;

namespace TaskFlow.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly ITaskRepository _taskRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public TasksController(ITaskRepository taskRepository, UserManager<ApplicationUser> userManager)
        {
            _taskRepository = taskRepository;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskDto>>> GetTasks()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            IEnumerable<Domain.Entities.Task> tasks;
            if (await _userManager.IsInRoleAsync(user, Roles.Admin))
            {
                tasks = await _taskRepository.GetAllTasksAsync();
            }
            else
            {
                tasks = await _taskRepository.GetTasksByUserIdAsync(user.Id);
            }

            return Ok(tasks.Select(t => MapToTaskDto(t)));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TaskDto>> GetTask(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var task = await _taskRepository.GetTaskByIdAsync(id);
            if (task == null)
                return NotFound();

            // Check if user has permission to view this task
            if (!await CanAccessTask(user, task))
                return Forbid();

            return Ok(MapToTaskDto(task));
        }

        [HttpPost]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult<TaskDto>> CreateTask(CreateTaskDto createTaskDto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            // Check if assigned user exists
            var assignedUser = await _userManager.FindByIdAsync(createTaskDto.AssignedUserId);
            if (assignedUser == null)
                return BadRequest("Assigned user not found");

            var task = new Domain.Entities.Task
            {
                Title = createTaskDto.Title,
                Description = createTaskDto.Description,
                Priority = createTaskDto.Priority,
                Status = TaskStatus.Pending,
                AssignedUserId = createTaskDto.AssignedUserId,
                CreatedByUserId = user.Id
            };

            var createdTask = await _taskRepository.CreateTaskAsync(task);
            var taskDto = MapToTaskDto(createdTask);

            return CreatedAtAction(nameof(GetTask), new { id = taskDto.Id }, taskDto);
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateTaskStatus(int id, UpdateTaskDto updateTaskDto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var task = await _taskRepository.GetTaskByIdAsync(id);
            if (task == null)
                return NotFound();

            // Check if user has permission to update this task
            // Admin can update any task, users can only update their assigned tasks
            if (!await _userManager.IsInRoleAsync(user, Roles.Admin) && task.AssignedUserId != user.Id)
                return Forbid();

            // Only update status
            task.Status = updateTaskDto.Status;
            task.UpdatedAt = DateTime.UtcNow;

            await _taskRepository.UpdateTaskAsync(task);
            return NoContent();
        }

        [HttpPut("{id}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult<TaskDto>> UpdateTask(int id, UpdateTaskFullDto updateTaskDto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var task = await _taskRepository.GetTaskByIdAsync(id);
            if (task == null)
                return NotFound();

            // Check if assigned user exists (if being changed)
            if (updateTaskDto.AssignedUserId != null)
            {
                var assignedUser = await _userManager.FindByIdAsync(updateTaskDto.AssignedUserId);
                if (assignedUser == null)
                    return BadRequest("Assigned user not found");
                task.AssignedUserId = updateTaskDto.AssignedUserId;
            }

            // Update fields
            if (!string.IsNullOrEmpty(updateTaskDto.Title))
                task.Title = updateTaskDto.Title;
            if (!string.IsNullOrEmpty(updateTaskDto.Description))
                task.Description = updateTaskDto.Description;
            if (updateTaskDto.Priority.HasValue)
                task.Priority = updateTaskDto.Priority.Value;
            if (updateTaskDto.Status.HasValue)
                task.Status = updateTaskDto.Status.Value;

            task.UpdatedAt = DateTime.UtcNow;

            await _taskRepository.UpdateTaskAsync(task);
            return Ok(MapToTaskDto(task));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var task = await _taskRepository.GetTaskByIdAsync(id);
            if (task == null)
                return NotFound();

            await _taskRepository.DeleteTaskAsync(id);
            return NoContent();
        }

        private async Task<bool> CanAccessTask(ApplicationUser user, Domain.Entities.Task task)
        {
            // Admin can access all tasks
            if (await _userManager.IsInRoleAsync(user, Roles.Admin))
                return true;

            // Users can access tasks assigned to them
            return task.AssignedUserId == user.Id;
        }

        private static TaskDto MapToTaskDto(Domain.Entities.Task task)
        {
            return new TaskDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                Priority = task.Priority,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt,
                AssignedUserId = task.AssignedUserId,
                AssignedUserName = task.AssignedUser?.UserName ?? string.Empty,
                CreatedByUserId = task.CreatedByUserId,
                CreatedByUserName = task.CreatedByUser?.UserName ?? string.Empty
            };
        }
    }
}
