using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MedConnect.Domain;
using Microsoft.IdentityModel.Tokens;

namespace MedConnect.Features.Identity;

public class JwtTokenService(IConfiguration configuration)
{
    private const string TwoFactorPending = "pending";
    private static readonly TimeSpan TwoFactorTicketLifetime = TimeSpan.FromMinutes(5);

    public (string Token, DateTime ExpiresAtUtc) GenerateToken(ApplicationUser user, IList<string> roles)
        => CreateToken(user, roles, twoFactorState: null);

    /// Short-lived token used to complete the second factor. Only usable against
    /// the 2FA verification endpoint.
    public (string Token, DateTime ExpiresAtUtc) GenerateTwoFactorTicket(ApplicationUser user)
        => CreateToken(user, [], TwoFactorPending);

    private (string Token, DateTime ExpiresAtUtc) CreateToken(ApplicationUser user, IList<string> roles, string? twoFactorState)
    {
        var jwtSection = configuration.GetSection("Jwt");
        var minutes = jwtSection.GetValue("AccessTokenMinutes", 30);
        var expires = twoFactorState is not null
            ? DateTime.UtcNow.Add(TwoFactorTicketLifetime)
            : DateTime.UtcNow.AddMinutes(minutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new("FullName", user.FullName)
        };

        if (twoFactorState is not null)
        {
            claims.Add(new Claim("2fa", twoFactorState));
        }

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        if (user.FacilityId.HasValue)
        {
            claims.Add(new Claim("FacilityId", user.FacilityId.Value.ToString()));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtSection["Issuer"],
            audience: jwtSection["Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}