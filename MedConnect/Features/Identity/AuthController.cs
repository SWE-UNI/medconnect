using System.Security.Claims;
using MedConnect.Common.Constants;
using MedConnect.Features.Identity.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedConnect.Features.Identity;

[ApiController]
[Route("api/v1/auth")]
public class AuthController(AuthService authService) : ControllerBase
{
    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    // Public self-service registration. The service always creates a Patient —
    // no caller can pick their own role here.
    [HttpPost("register")]
    [LoginThrottle]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var (succeeded, error, response) = await authService.RegisterAsync(dto);
        return succeeded ? Ok(response) : BadRequest(new { error });
    }

    // Admin-only: creates staff accounts (CHW, Doctor, Nurse, Receptionist,
    // FacilityAdmin, Admin).
    [HttpPost("register-staff")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> RegisterStaff(RegisterStaffDto dto)
    {
        var (succeeded, error, response) = await authService.RegisterStaffAsync(dto, CurrentUserId);
        return succeeded ? Ok(response) : BadRequest(new { error });
    }

    [HttpPost("login")]
    [LoginThrottle]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var (succeeded, error, response) = await authService.LoginAsync(dto);
        return succeeded ? Ok(response) : Unauthorized(new { error });
    }

    [HttpGet("2fa/status")]
    [Authorize]
    public async Task<IActionResult> TwoFactorStatus()
    {
        var enabled = await authService.IsTwoFactorEnabledAsync(CurrentUserId);
        return enabled is null ? NotFound() : Ok(new { enabled = enabled.Value });
    }

    [HttpPost("2fa/setup")]
    [Authorize]
    public async Task<IActionResult> SetupTwoFactor()
    {
        var (succeeded, error, response) = await authService.SetupTwoFactorAsync(CurrentUserId);
        return succeeded ? Ok(response) : BadRequest(new { error });
    }

    [HttpPost("2fa/enable")]
    [Authorize]
    public async Task<IActionResult> EnableTwoFactor(TwoFactorCodeDto dto)
    {
        var (succeeded, error) = await authService.EnableTwoFactorAsync(CurrentUserId, dto.Code);
        return succeeded ? Ok(new { enabled = true }) : BadRequest(new { error });
    }

    [HttpPost("2fa/verify")]
    [Authorize]
    public async Task<IActionResult> VerifyTwoFactor(TwoFactorCodeDto dto)
    {
        // Only accepts the short-lived ticket issued at login for 2FA accounts.
        if (User.FindFirstValue("2fa") != "pending")
        {
            return Unauthorized(new { error = "This endpoint requires a two-factor sign-in ticket." });
        }

        var (succeeded, error, response) = await authService.VerifyTwoFactorAsync(CurrentUserId, dto.Code);
        return succeeded ? Ok(response) : BadRequest(new { error });
    }

    [HttpPost("2fa/disable")]
    [Authorize]
    public async Task<IActionResult> DisableTwoFactor(TwoFactorCodeDto dto)
    {
        var (succeeded, error) = await authService.DisableTwoFactorAsync(CurrentUserId, dto.Code);
        return succeeded ? Ok(new { enabled = false }) : BadRequest(new { error });
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [LoginThrottle]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
    {
        var (succeeded, message, resetToken) = await authService.ForgotPasswordAsync(dto);
        return Ok(new ForgotPasswordResponseDto(succeeded, message, resetToken));
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    [LoginThrottle]
    public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
    {
        var (succeeded, error) = await authService.ResetPasswordAsync(dto);
        return succeeded ? Ok(new { message = "Password has been reset successfully." }) : BadRequest(new { error });
    }
}