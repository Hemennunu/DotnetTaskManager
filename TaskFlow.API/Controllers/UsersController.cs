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
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(IUserRepository userRepository, UserManager<ApplicationUser> userManager)
        {
            _userRepository = userRepository;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            // Only admin can view all users
            if (!await _userManager.IsInRoleAsync(user, Roles.Admin))
                return Forbid();

            var users = await _userRepository.GetAllUsersAsync();
            var userDtos = new List<UserDto>();

            foreach (var appUser in users)
            {
                var roles = await _userManager.GetRolesAsync(appUser);
                userDtos.Add(new UserDto
                {
                    Id = appUser.Id,
                    UserName = appUser.UserName ?? string.Empty,
                    Email = appUser.Email ?? string.Empty,
                    FirstName = appUser.FirstName,
                    LastName = appUser.LastName,
                    CreatedAt = appUser.CreatedAt,
                    Roles = roles.ToArray()
                });
            }

            return Ok(userDtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(string id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            // Only admin can view other users, users can view themselves
            if (!await _userManager.IsInRoleAsync(user, Roles.Admin) && user.Id != id)
                return Forbid();

            var appUser = await _userRepository.GetUserByIdAsync(id);
            if (appUser == null)
                return NotFound();

            var roles = await _userManager.GetRolesAsync(appUser);
            var userDto = new UserDto
            {
                Id = appUser.Id,
                UserName = appUser.UserName ?? string.Empty,
                Email = appUser.Email ?? string.Empty,
                FirstName = appUser.FirstName,
                LastName = appUser.LastName,
                CreatedAt = appUser.CreatedAt,
                Roles = roles.ToArray()
            };

            return Ok(userDto);
        }

        [HttpPost]
        public async Task<ActionResult<UserDto>> CreateUser(CreateUserDto createUserDto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            // Only admin can create users
            if (!await _userManager.IsInRoleAsync(user, Roles.Admin))
                return Forbid();

            var appUser = new ApplicationUser
            {
                UserName = createUserDto.UserName,
                Email = createUserDto.Email,
                FirstName = createUserDto.FirstName,
                LastName = createUserDto.LastName
            };

            try
            {
                var createdUser = await _userRepository.CreateUserAsync(appUser, createUserDto.Password);
                
                // Add role
                await _userManager.AddToRoleAsync(createdUser, createUserDto.Role);

                var roles = await _userManager.GetRolesAsync(createdUser);
                var userDto = new UserDto
                {
                    Id = createdUser.Id,
                    UserName = createdUser.UserName ?? string.Empty,
                    Email = createdUser.Email ?? string.Empty,
                    FirstName = createdUser.FirstName,
                    LastName = createdUser.LastName,
                    CreatedAt = createdUser.CreatedAt,
                    Roles = roles.ToArray()
                };

                return CreatedAtAction(nameof(GetUser), new { id = userDto.Id }, userDto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            // Only admin can delete users and cannot delete themselves
            if (!await _userManager.IsInRoleAsync(user, Roles.Admin) || user.Id == id)
                return Forbid();

            var result = await _userRepository.DeleteUserAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }
    }
}
