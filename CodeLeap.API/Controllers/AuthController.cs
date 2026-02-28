using CodeLeap.Application.DTOs.User;
using CodeLeap.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CodeLeap.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ITokenBlacklistService _tokenBlacklistService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IUserService userService,
            ITokenBlacklistService tokenBlacklistService,
            ILogger<AuthController> logger)
        {
            _userService = userService;
            _tokenBlacklistService = tokenBlacklistService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _logger.LogInformation("Register attempt for email: {Email}", request.Email);

            var result = await _userService.Register(request);

            return CreatedAtAction(
                actionName: nameof(UsersController.GetMe),
                controllerName: "Users",
                routeValues: null,
                value: result
            );
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _logger.LogInformation("Login attempt for email: {Email}", request.Email);

            var result = await _userService.Login(request);

            return Ok(result);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userService.RefreshToken(request.RefreshToken);
            return Ok(result);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var jti = User.FindFirstValue(JwtRegisteredClaimNames.Jti);
            var exp = User.FindFirstValue(JwtRegisteredClaimNames.Exp);

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(jti) || string.IsNullOrEmpty(exp))
                return Unauthorized();

            var expiry = DateTimeOffset
                .FromUnixTimeSeconds(long.Parse(exp))
                .UtcDateTime;

            // Blacklist current access token
            await _tokenBlacklistService.BlacklistAsync(jti, expiry);

            // Revoke ALL refresh tokens of user
            await _userService.Logout(email);

            _logger.LogInformation("User {Email} logged out from all sessions", email);

            return NoContent(); // 204
        }
    }
}