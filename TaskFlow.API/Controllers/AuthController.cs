using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Constants;

namespace TaskFlow.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AuthController(IAuthService authService, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _authService = authService;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var isValid = await _authService.ValidateUserAsync(request.UserName, request.Password);
            if (!isValid)
                return Unauthorized("Invalid credentials");

            var token = await _authService.GenerateTokenAsync(request.UserName);
            if (string.IsNullOrEmpty(token))
                return Unauthorized("Failed to generate token");

            return Ok(new { Token = token });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = new ApplicationUser
            {
                UserName = request.UserName,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            // Assign role
            if (!await _roleManager.RoleExistsAsync(Roles.User))
                await _roleManager.CreateAsync(new IdentityRole(Roles.User));

            await _userManager.AddToRoleAsync(user, Roles.User);

            var token = await _authService.GenerateTokenAsync(request.UserName);
            return Ok(new { Token = token });
        }

        [HttpPost("seed-admin")]
        public async Task<IActionResult> SeedAdmin()
        {
            // Check if admin user already exists
            var adminUser = await _userManager.FindByNameAsync("admin");
            if (adminUser != null)
                return Ok("Admin user already exists");

            // Create admin role if it doesn't exist
            if (!await _roleManager.RoleExistsAsync(Roles.Admin))
                await _roleManager.CreateAsync(new IdentityRole(Roles.Admin));

            // Create admin user
            adminUser = new ApplicationUser
            {
                UserName = "admin",
                Email = "admin@taskflow.com",
                FirstName = "System",
                LastName = "Administrator"
            };

            var result = await _userManager.CreateAsync(adminUser, "Admin123!");
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            await _userManager.AddToRoleAsync(adminUser, Roles.Admin);

            return Ok("Admin user created successfully");
        }
    }

    public class LoginRequest
    {
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterRequest
    {
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}
