using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FluentValidation.Results;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace JobsApi;

public static class Helpers
{
    public static int? Uid(ClaimsPrincipal u) =>
        int.TryParse(u.FindFirstValue("uid"), out var id) ? id : null;

    static string Camel(string s) => string.IsNullOrEmpty(s) ? s : char.ToLowerInvariant(s[0]) + s[1..];

    public static Dictionary<string, string[]> ToErrors(ValidationResult r) =>
        r.Errors.GroupBy(e => e.PropertyName)
                .ToDictionary(g => Camel(g.Key), g => g.Select(e => e.ErrorMessage).ToArray());

    public static IResult Error(string field, string message) =>
        Results.ValidationProblem(new Dictionary<string, string[]> { [field] = [message] });

    public static IResult Forbidden(string message) =>
        Results.Problem(statusCode: 403, title: "Доступ запрещён", detail: message);

    public static string CreateToken(AppUser u, IConfiguration cfg)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(cfg["Jwt:Key"]!));
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([new Claim("uid", u.Id.ToString()), new Claim("name", u.UserName)]),
            Expires = DateTime.UtcNow.AddHours(8),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        };
        return new JsonWebTokenHandler().CreateToken(descriptor);
    }
}

public static class PasswordService
{
    const int Iter = 100_000;

    public static string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var h = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iter, HashAlgorithmName.SHA256, 32);
        return $"{Iter}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(h)}";
    }

    public static bool Verify(string password, string stored)
    {
        var p = stored.Split('.');
        if (p.Length != 3) return false;
        var salt = Convert.FromBase64String(p[1]);
        var expected = Convert.FromBase64String(p[2]);
        var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, int.Parse(p[0]), HashAlgorithmName.SHA256, expected.Length);
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
