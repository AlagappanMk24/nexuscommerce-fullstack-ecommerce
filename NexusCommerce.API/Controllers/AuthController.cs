using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NexusCommerce.Application.Contracts.Services;
using NexusCommerce.Application.DTOs.Auth;
using NexusCommerce.Domain.Entities.Identity;

namespace NexusCommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(UserManager<ApplicationUser> userManager, IAuthManager authManager) : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly IAuthManager _authManager = authManager;

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var errors = await _authManager.RegisterAsync(dto);
            if (errors != null)
                return BadRequest(errors);

            return Ok("Registration successful.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var token = await _authManager.LoginAsync(dto);
            if (token == null)
                return Unauthorized("Invalid email or password.");

            return Ok(new { token });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] string email)
        {
            //var resetBaseUrl = $"{Request.Scheme}://{Request.Host}/reset-password";
            var resetBaseUrl = "http://localhost:4200/auth/reset-password";
            var result = await _authManager.ForgotPasswordAsync(email, resetBaseUrl);
            // to avoid exposing whether email exists
            return Ok("If this email is registered, a reset link has been sent.");
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _authManager.ResetPasswordAsync(dto);
            if (!result) return BadRequest("Invalid or expired token.");

            return Ok("Password reset successful.");
        }
    }
}