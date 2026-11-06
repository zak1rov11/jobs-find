using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace JobsApi;

public static class AuthEndpoints
{
    public static void MapAuth(this WebApplication app)
    {
        var g = app.MapGroup("/api/auth").WithTags("Auth");

        g.MapPost("/register", async (RegisterRequest r, IValidator<RegisterRequest> v, AppDb db, IConfiguration cfg) =>
        {
            var res = await v.ValidateAsync(r);
            if (!res.IsValid) return Results.ValidationProblem(Helpers.ToErrors(res));
            if (await db.Users.AnyAsync(u => u.UserName == r.UserName))
                return Helpers.Error("userName", "Такой логин уже занят");

            var user = new AppUser { UserName = r.UserName, PasswordHash = PasswordService.Hash(r.Password) };
            db.Users.Add(user);
            await db.SaveChangesAsync();
            return Results.Ok(new AuthResponse(Helpers.CreateToken(user, cfg), user.Id, user.UserName));
        });

        g.MapPost("/login", async (LoginRequest r, AppDb db, IConfiguration cfg) =>
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.UserName == r.UserName);
            if (user is null || !PasswordService.Verify(r.Password ?? "", user.PasswordHash))
                return Results.Problem(statusCode: 401, title: "Неверный логин или пароль");
            return Results.Ok(new AuthResponse(Helpers.CreateToken(user, cfg), user.Id, user.UserName));
        });
    }
}
