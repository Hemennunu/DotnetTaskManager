using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Constants;

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

            // Admin can see all tasks, users can only see their assigned tasks
            IEnumerable<Task> tasks;
            if (await _userManager.IsInRoleAsync(user, Roles.Admin))
            {
                tasks = await _taskRepository.GetAllTasksAsync();
            }
            else
            {
                tasks = await _taskRepository.GetTasksByUserIdAsync(user.Id);
            }

            var taskDtos = tasks.Select(MapToTaskDto);
            return Ok(taskDtos);
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
        public async Task<ActionResult<TaskDto>> CreateTask(CreateTaskDto createTaskDto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            // Only admin can create tasks
            if (!await _userManager.IsInRoleAsync(user, Roles.Admin))
                return Forbid();

            var task = new Task
            {
                Title = createTaskDto.Title,
                Description = createTaskDto.Description,
                Priority = createTaskDto.Priority,
                Status = Domain.Enums.TaskStatus.Pending,
                AssignedUserId = createTaskDto.AssignedUserId,
                CreatedByUserId = user.Id
            };

            var createdTask = await _taskRepository.CreateTaskAsync(task);
            var taskDto = MapToTaskDto(createdTask);

            return CreatedAtAction(nameof(GetTask), new { id = taskDto.Id }, taskDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, UpdateTaskDto updateTaskDto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var task = await _taskRepository.GetTaskByIdAsync(id);
            if (task == null)
                return NotFound();

            // Check if user has permission to update this task
            if (!await CanAccessTask(user, task))
                return Forbid();

            // Users can only update status, admins can update everything
            if (!await _userManager.IsInRoleAsync(user, Roles.Admin))
            {
                task.Status = updateTaskDto.Status;
            }
            else
            {
                task.Title = updateTaskDto.Title;
                task.Description = updateTaskDto.Description;
                task.Status = updateTaskDto.Status;
                task.Priority = updateTaskDto.Priority;
            }

            await _taskRepository.UpdateTaskAsync(task);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var task = await _taskRepository.GetTaskByIdAsync(id);
            if (task == null)
                return NotFound();

            // Only admin can delete tasks
            if (!await _userManager.IsInRoleAsync(user, Roles.Admin))
                return Forbid();

            await _taskRepository.DeleteTaskAsync(id);
            return NoContent();
        }

        private async Task<bool> CanAccessTask(ApplicationUser user, Task task)
        {
            // Admin can access all tasks
            if (await _userManager.IsInRoleAsync(user, Roles.Admin))
                return true;

            // Users can access tasks assigned to them
            return task.AssignedUserId == user.Id;
        }

        private static TaskDto MapToTaskDto(Task task)
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
                AssignedUserName = task.AssignedUser?.UserName,
                CreatedByUserId = task.CreatedByUserId,
                CreatedByUserName = task.CreatedByUser?.UserName
            };
        }
    }
}
