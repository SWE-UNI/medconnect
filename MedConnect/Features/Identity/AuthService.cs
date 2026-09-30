using MedConnect.Common;
using MedConnect.Common.Constants;
using MedConnect.Data;
using MedConnect.Domain;
using MedConnect.Features.Identity.Dtos;
using MedConnect.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MedConnect.Features.Identity;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    JwtTokenService tokenService,
    TotpService totpService,
    ApplicationDbContext db,
    AuditLogger audit)
{
    /// Self-service registration. Only patients may create their own account;
    /// staff accounts go through <see cref="RegisterStaffAsync"/>, which is
    /// Admin-only.
    public async Task<(bool Succeeded, string? Error, AuthResponseDto? Response)> RegisterAsync(RegisterDto dto)
    {
        if (dto.NHISNumber is null || dto.ContactInfo is null || dto.DateOfBirth is null)
        {
            return (false, "NHISNumber, DateOfBirth, and ContactInfo are required to register a patient.", null);
        }

        var nhis = dto.NHISNumber.Trim();
        if (await db.Patients.AnyAsync(p => p.NHISNumber == nhis))
        {
            return (false, "That NHIS number is already registered to another patient.", null);
        }

        var contact = dto.ContactInfo.Trim();
        if (!GhanaPhone.TryNormalize(contact, out var normalizedContact))
        {
            return (false, "ContactInfo must be a valid Ghana phone number (e.g. 0241234567).", null);
        }

        contact = normalizedContact!;

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FullName = dto.FullName
        };

        var createResult = await userManager.CreateAsync(user, dto.Password);
        if (!createResult.Succeeded)
        {
            return (false, string.Join(" ", createResult.Errors.Select(e => e.Description)), null);
        }

        await userManager.AddToRoleAsync(user, Roles.Patient);

        db.Patients.Add(new Patient
        {
            NHISNumber = nhis,
            FullName = dto.FullName,
            DateOfBirth = dto.DateOfBirth.Value,
            ContactInfo = contact,
            UserId = user.Id
        });
        await db.SaveChangesAsync();

        var (token, expires) = tokenService.GenerateToken(user, [Roles.Patient]);
        await audit.LogAsync(user.Id, "PatientRegistered", "Patient", user.Id, $"NHIS {nhis}");
        return (true, null, new AuthResponseDto(token, expires, Roles.Patient, user.FullName));
    }

    /// Admin-only staff account creation (caller must hold the Admin role).
    public async Task<(bool Succeeded, string? Error, AuthResponseDto? Response)> RegisterStaffAsync(RegisterStaffDto dto, string? actorUserId = null)
    {
        if (!Roles.Staff.Contains(dto.Role))
        {
            return (false, $"Role must be one of: {string.Join(", ", Roles.Staff)}", null);
        }

        if (Roles.FacilityScoped.Contains(dto.Role) && dto.FacilityId is null)
        {
            return (false, $"FacilityId is required for the {dto.Role} role.", null);
        }

        if (dto.Role == Roles.Doctor && dto.DivisionId is null)
        {
            return (false, "DivisionId is required for the Doctor role.", null);
        }

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FullName = dto.FullName,
            FacilityId = dto.FacilityId,
            DivisionId = dto.Role == Roles.Doctor ? dto.DivisionId : null
        };

        var createResult = await userManager.CreateAsync(user, dto.Password);
        if (!createResult.Succeeded)
        {
            return (false, string.Join(" ", createResult.Errors.Select(e => e.Description)), null);
        }

        await userManager.AddToRoleAsync(user, dto.Role);

        var (token, expires) = tokenService.GenerateToken(user, [dto.Role]);
        await audit.LogAsync(actorUserId, "StaffCreated", "User", user.Id, $"Role {dto.Role} as {dto.Email}");
        return (true, null, new AuthResponseDto(token, expires, dto.Role, user.FullName));
    }

    /// Patients sign in with their NHIS number; staff sign in with email.
    /// Accounts with two-factor enabled must finish the second factor: either a
    /// code sent with this call, or a follow-up to /auth/2fa/verify using the
    /// short-lived ticket returned here.
    public async Task<(bool Succeeded, string? Error, AuthResponseDto? Response)> LoginAsync(LoginDto dto)
    {
        var user = await ResolveUserAsync(dto);
        if (user is null)
        {
            return (false, "Invalid email, NHIS number, or password.", null);
        }

        if (await userManager.IsLockedOutAsync(user))
        {
            var minutes = Math.Max(1, Math.Ceiling((user.LockoutEnd!.Value - DateTimeOffset.UtcNow).TotalMinutes));
            await audit.LogAsync(user.Id, "LoginBlocked", "User", user.Id, "Account locked");
            return (false, $"Account temporarily locked after too many failed attempts. Try again in {minutes} minute(s).", null);
        }

        if (!await userManager.CheckPasswordAsync(user, dto.Password))
        {
            await userManager.AccessFailedAsync(user);
            await audit.LogAsync(user.Id, "LoginFailed", "User", user.Id, "Invalid password");
            return (false, "Invalid email, NHIS number, or password.", null);
        }

        await userManager.ResetAccessFailedCountAsync(user);

        var roles = await userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? string.Empty;

        if (user.TwoFactorEnabled)
        {
            if (!string.IsNullOrWhiteSpace(dto.Code))
            {
                if (!VerifyTotp(user, dto.Code))
                {
                    return (false, "The authenticator code is invalid or expired.", null);
                }

                var (token, expires) = tokenService.GenerateToken(user, roles);
                await audit.LogAsync(user.Id, "Login", "User", user.Id, "Two-factor");
                return (true, null, new AuthResponseDto(token, expires, role, user.FullName));
            }

            var (ticket, ticketExpires) = tokenService.GenerateTwoFactorTicket(user);
            return (true, null, new AuthResponseDto(ticket, ticketExpires, role, user.FullName, TwoFactorRequired: true));
        }

        var (finalToken, finalExpires) = tokenService.GenerateToken(user, roles);
        await audit.LogAsync(user.Id, "Login", "User", user.Id);
        return (true, null, new AuthResponseDto(finalToken, finalExpires, role, user.FullName));
    }

    public Task<bool?> IsTwoFactorEnabledAsync(string userId) =>
        db.Users.Where(u => u.Id == userId)
            .Select(u => (bool?)u.TwoFactorEnabled)
            .FirstOrDefaultAsync();

    public async Task<(bool Succeeded, string? Error, TwoFactorSetupDto? Response)> SetupTwoFactorAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return (false, "User not found.", null);
        }

        var secret = totpService.GenerateSecret();
        user.AuthenticatorKey = secret;
        await userManager.UpdateAsync(user);

        var uri = totpService.GetOtpauthUri(secret, user.Email ?? user.FullName);
        return (true, null, new TwoFactorSetupDto(secret, uri, user.TwoFactorEnabled));
    }

    public async Task<(bool Succeeded, string? Error)> EnableTwoFactorAsync(string userId, string code)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return (false, "User not found.");
        }

        if (string.IsNullOrEmpty(user.AuthenticatorKey))
        {
            return (false, "Set up an authenticator key first via /auth/2fa/setup.");
        }

        if (!VerifyTotp(user, code))
        {
            return (false, "The authenticator code is invalid or expired.");
        }

        user.TwoFactorEnabled = true;
        await userManager.UpdateAsync(user);
        await audit.LogAsync(user.Id, "TwoFactorEnabled", "User", user.Id);
        return (true, null);
    }

    public async Task<(bool Succeeded, string? Error, AuthResponseDto? Response)> VerifyTwoFactorAsync(string userId, string code)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null || string.IsNullOrEmpty(user.AuthenticatorKey))
        {
            return (false, "Two-factor authentication is not set up for this account.", null);
        }

        if (!VerifyTotp(user, code))
        {
            return (false, "The authenticator code is invalid or expired.", null);
        }

        var roles = await userManager.GetRolesAsync(user);
        var (token, expires) = tokenService.GenerateToken(user, roles);
        await audit.LogAsync(user.Id, "Login", "User", user.Id, "Two-factor");
        return (true, null, new AuthResponseDto(token, expires, roles.FirstOrDefault() ?? string.Empty, user.FullName));
    }

    public async Task<(bool Succeeded, string? Error)> DisableTwoFactorAsync(string userId, string code)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return (false, "User not found.");
        }

        if (string.IsNullOrEmpty(user.AuthenticatorKey))
        {
            return (false, "Two-factor authentication is not set up for this account.");
        }

        if (!VerifyTotp(user, code))
        {
            return (false, "The authenticator code is invalid or expired.");
        }

        user.TwoFactorEnabled = false;
        user.AuthenticatorKey = null;
        await userManager.UpdateAsync(user);
        await audit.LogAsync(user.Id, "TwoFactorDisabled", "User", user.Id);
        return (true, null);
    }

    public async Task<(bool Succeeded, string Message, string? ResetToken)> ForgotPasswordAsync(ForgotPasswordDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Identifier))
        {
            return (false, "Please provide your email address or NHIS number.", null);
        }

        var identifier = dto.Identifier.Trim();
        ApplicationUser? user = null;

        var patientUserId = await db.Patients
            .Where(p => p.NHISNumber == identifier)
            .Select(p => p.UserId)
            .FirstOrDefaultAsync();

        if (patientUserId != null)
        {
            user = await userManager.FindByIdAsync(patientUserId);
        }

        user ??= await userManager.FindByEmailAsync(identifier);

        if (user == null)
        {
            // Security best practice: don't reveal non-existent accounts
            return (true, "If an account matches that email or NHIS number, password reset instructions have been generated.", null);
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        await audit.LogAsync(user.Id, "PasswordResetRequested", "User", user.Id);

        return (true, "Password reset instructions have been generated. Enter your reset token and new password to complete.", token);
    }

    public async Task<(bool Succeeded, string? Error)> ResetPasswordAsync(ResetPasswordDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Identifier) || string.IsNullOrWhiteSpace(dto.Token) || string.IsNullOrWhiteSpace(dto.NewPassword))
        {
            return (false, "Identifier, reset token, and new password are all required.");
        }

        var identifier = dto.Identifier.Trim();
        ApplicationUser? user = null;

        var patientUserId = await db.Patients
            .Where(p => p.NHISNumber == identifier)
            .Select(p => p.UserId)
            .FirstOrDefaultAsync();

        if (patientUserId != null)
        {
            user = await userManager.FindByIdAsync(patientUserId);
        }

        user ??= await userManager.FindByEmailAsync(identifier);

        if (user == null)
        {
            return (false, "Invalid account identifier or reset token.");
        }

        var result = await userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
        if (!result.Succeeded)
        {
            return (false, string.Join(" ", result.Errors.Select(e => e.Description)));
        }

        await audit.LogAsync(user.Id, "PasswordResetCompleted", "User", user.Id);
        return (true, null);
    }

    private bool VerifyTotp(ApplicationUser user, string code) =>
        totpService.ValidateCode(user.AuthenticatorKey ?? string.Empty, code);

    private async Task<ApplicationUser?> ResolveUserAsync(LoginDto dto)
    {
        if (!string.IsNullOrWhiteSpace(dto.NHISNumber))
        {
            var userId = await db.Patients
                .Where(p => p.NHISNumber == dto.NHISNumber.Trim())
                .Select(p => p.UserId)
                .FirstOrDefaultAsync();

            return userId is null ? null : await userManager.FindByIdAsync(userId);
        }

        return await userManager.FindByEmailAsync(dto.Email?.Trim() ?? string.Empty);
    }
}