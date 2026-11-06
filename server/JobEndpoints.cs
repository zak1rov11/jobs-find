using System.Security.Claims;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace JobsApi;

public static class JobEndpoints
{
    static readonly string[] AllowedExt = [".jpg", ".jpeg", ".png", ".webp"];
    const long MaxFile = 5 * 1024 * 1024;

    public static void MapJobs(this WebApplication app)
    {
        app.MapGet("/api/spheres", async (AppDb db) =>
            Results.Ok(await db.Spheres.OrderBy(s => s.Name).Select(s => new SphereDto(s.Id, s.Name)).ToListAsync()))
           .WithTags("Spheres");

        var g = app.MapGroup("/api/jobs").WithTags("Jobs");
        g.MapGet("/", List);
        g.MapGet("/{id:int}", GetById);
        g.MapPost("/", Create).RequireAuthorization();
        g.MapPut("/{id:int}", Update).RequireAuthorization();
        g.MapDelete("/{id:int}", Delete).RequireAuthorization();
        g.MapPost("/{id:int}/logo", UploadLogo).RequireAuthorization().DisableAntiforgery();
    }

    static IQueryable<JobDto> Project(IQueryable<Job> q) => q.Select(j => new JobDto(
        j.Id, j.Title, j.Company, j.SalaryFrom, j.SalaryTo, j.IsRemote,
        j.LogoFileName == null ? null : "/uploads/" + j.LogoFileName,
        j.SphereId, j.Sphere!.Name, j.OwnerId, j.Owner!.UserName, j.CreatedAt));

    static async Task<IResult> List(AppDb db, int page = 1, int pageSize = 6, int? sphereId = null,
        bool? remote = null, int? minSalary = null, string? sort = null)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var q = db.Jobs.AsNoTracking().AsQueryable();
        if (sphereId is not null) q = q.Where(j => j.SphereId == sphereId);
        if (remote is not null) q = q.Where(j => j.IsRemote == remote);
        if (minSalary is not null) q = q.Where(j => j.SalaryFrom >= minSalary);

        q = sort == "salary"
            ? q.OrderByDescending(j => j.SalaryFrom).ThenByDescending(j => j.SalaryTo).ThenBy(j => j.Id)
            : q.OrderByDescending(j => j.CreatedAt).ThenByDescending(j => j.Id);

        var total = await q.CountAsync();
        var items = await Project(q.Skip((page - 1) * pageSize).Take(pageSize)).ToListAsync();
        return Results.Ok(new PagedResult<JobDto>(items, page, pageSize, total));
    }

    static async Task<IResult> GetById(int id, AppDb db)
    {
        var dto = await Project(db.Jobs.AsNoTracking().Where(j => j.Id == id)).FirstOrDefaultAsync();
        return dto is null ? Results.NotFound() : Results.Ok(dto);
    }

    static async Task<Dictionary<string, string[]>?> Check(JobRequest req, IValidator<JobRequest> v, AppDb db)
    {
        var res = await v.ValidateAsync(req);
        var errors = res.IsValid ? [] : Helpers.ToErrors(res);
        if (res.IsValid && !await db.Spheres.AnyAsync(s => s.Id == req.SphereId))
            errors["sphereId"] = ["Такой сферы нет"];
        return errors.Count == 0 ? null : errors;
    }

    static async Task<IResult> Create(JobRequest req, IValidator<JobRequest> v, AppDb db, ClaimsPrincipal user)
    {
        var errors = await Check(req, v, db);
        if (errors is not null) return Results.ValidationProblem(errors);

        var job = new Job
        {
            Title = req.Title.Trim(), Company = req.Company.Trim(),
            SalaryFrom = req.SalaryFrom, SalaryTo = req.SalaryTo, IsRemote = req.IsRemote,
            SphereId = req.SphereId, OwnerId = Helpers.Uid(user)!.Value, CreatedAt = DateTime.UtcNow
        };
        db.Jobs.Add(job);
        await db.SaveChangesAsync();

        var dto = await Project(db.Jobs.AsNoTracking().Where(j => j.Id == job.Id)).FirstAsync();
        return Results.Created($"/api/jobs/{job.Id}", dto);
    }

    static async Task<IResult> Update(int id, JobRequest req, IValidator<JobRequest> v, AppDb db, ClaimsPrincipal user)
    {
        var job = await db.Jobs.FindAsync(id);
        if (job is null) return Results.NotFound();
        if (job.OwnerId != Helpers.Uid(user)) return Helpers.Forbidden("Менять вакансию может только автор");

        var errors = await Check(req, v, db);
        if (errors is not null) return Results.ValidationProblem(errors);

        job.Title = req.Title.Trim(); job.Company = req.Company.Trim();
        job.SalaryFrom = req.SalaryFrom; job.SalaryTo = req.SalaryTo;
        job.IsRemote = req.IsRemote; job.SphereId = req.SphereId;
        await db.SaveChangesAsync();

        return Results.Ok(await Project(db.Jobs.AsNoTracking().Where(j => j.Id == id)).FirstAsync());
    }

    static async Task<IResult> Delete(int id, AppDb db, ClaimsPrincipal user, IWebHostEnvironment env)
    {
        var job = await db.Jobs.FindAsync(id);
        if (job is null) return Results.NotFound();
        if (job.OwnerId != Helpers.Uid(user)) return Helpers.Forbidden("Удалять вакансию может только автор");

        RemoveFile(env, job.LogoFileName);
        db.Jobs.Remove(job);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    static async Task<IResult> UploadLogo(int id, IFormFile file, AppDb db, ClaimsPrincipal user, IWebHostEnvironment env)
    {
        var job = await db.Jobs.FindAsync(id);
        if (job is null) return Results.NotFound();
        if (job.OwnerId != Helpers.Uid(user)) return Helpers.Forbidden("Загружать логотип может только автор");

        if (file.Length == 0) return Helpers.Error("file", "Файл пустой");
        if (file.Length > MaxFile) return Helpers.Error("file", "Файл больше 5 МБ");
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExt.Contains(ext)) return Helpers.Error("file", "Только jpg, png или webp");
        if (!await LooksLikeImage(file)) return Helpers.Error("file", "Файл не похож на изображение");

        var dir = UploadsDir(env);
        Directory.CreateDirectory(dir);
        var name = Guid.NewGuid() + ext;
        await using (var fs = File.Create(Path.Combine(dir, name)))
            await file.CopyToAsync(fs);

        RemoveFile(env, job.LogoFileName);
        job.LogoFileName = name;
        await db.SaveChangesAsync();

        return Results.Ok(await Project(db.Jobs.AsNoTracking().Where(j => j.Id == id)).FirstAsync());
    }

    public static string UploadsDir(IWebHostEnvironment env) => Path.Combine(env.ContentRootPath, "wwwroot", "uploads");

    static void RemoveFile(IWebHostEnvironment env, string? name)
    {
        if (name is null) return;
        var path = Path.Combine(UploadsDir(env), name);
        if (File.Exists(path)) File.Delete(path);
    }

    static async Task<bool> LooksLikeImage(IFormFile f)
    {
        await using var s = f.OpenReadStream();
        var b = new byte[12];
        if (await s.ReadAsync(b) < 12) return false;
        bool jpg = b[0] == 0xFF && b[1] == 0xD8 && b[2] == 0xFF;
        bool png = b[0] == 0x89 && b[1] == 0x50 && b[2] == 0x4E && b[3] == 0x47;
        bool webp = b[0] == 'R' && b[1] == 'I' && b[2] == 'F' && b[3] == 'F' && b[8] == 'W' && b[9] == 'E' && b[10] == 'B' && b[11] == 'P';
        return jpg || png || webp;
    }
}
