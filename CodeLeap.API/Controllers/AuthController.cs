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
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _logger.LogInformation("Register attempt for email: {Email}", request.Email);

            var result = await _userService.Register(request);

            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _logger.LogInformation("Login attempt for email: {Email}", request.Email);

            var result = await _userService.Login(request);

            return Ok(result);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _userService.RefreshToken(request.RefreshToken);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Refresh token failed: {Message}", ex.Message);
                return Unauthorized(ex.Message);
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
                return BadRequest("Refresh token is required");

            try
            {
                await _userService.Logout(request.RefreshToken);

                _logger.LogInformation("User logged out successfully");

                return Ok("Logged out");
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Logout failed: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
        }
    }
}
