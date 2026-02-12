using CodeLeap.Application.DTOs.User;
using CodeLeap.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CodeLeap.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IUserService userService,
            ILogger<AuthController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            _logger.LogInformation("Register attempt for email: {Email}", request.Email);

            var result = await _userService.Register(request);

            _logger.LogInformation("User registered successfully: {Email}", request.Email);

            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            _logger.LogInformation("Login attempt for email: {Email}", request.Email);

            var result = await _userService.Login(request);

            if (result == null)
            {
                _logger.LogWarning("Login failed for email: {Email}", request.Email);
                return Unauthorized("Invalid email or password");
            }

            _logger.LogInformation("User logged in successfully: {Email}", request.Email);

            return Ok(result);
        }
    }
}