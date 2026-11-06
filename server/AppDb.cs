using Microsoft.EntityFrameworkCore;

namespace JobsApi;

public class AppDb(DbContextOptions<AppDb> options) : DbContext(options)
{
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<Sphere> Spheres => Set<Sphere>();
    public DbSet<AppUser> Users => Set<AppUser>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<AppUser>().HasIndex(u => u.UserName).IsUnique();

        b.Entity<Sphere>().HasData(
            new Sphere { Id = 1, Name = "IT" },
            new Sphere { Id = 2, Name = "Продажи" },
            new Sphere { Id = 3, Name = "Маркетинг" },
            new Sphere { Id = 4, Name = "Финансы" },
            new Sphere { Id = 5, Name = "Образование" });

        // Тестовый пользователь: demo / Demo123!
        b.Entity<AppUser>().HasData(new AppUser { Id = 1, UserName = "demo", PasswordHash = "100000.cGV0c2FwaS1zYWx0LTE2Yg==.H00b89fjNi8gz2R6e4k8nUTV3j60z15myD/PZj/LDXk=" });

        b.Entity<Job>(e =>
        {
            e.Property(j => j.Title).HasMaxLength(100);
            e.Property(j => j.Company).HasMaxLength(100);
            e.HasOne(j => j.Sphere).WithMany(s => s.Jobs).HasForeignKey(j => j.SphereId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(j => j.Owner).WithMany().HasForeignKey(j => j.OwnerId).OnDelete(DeleteBehavior.Restrict);

            // ВТОРАЯ МИГРАЦИЯ: сначала создайте Initial с этими строками закомментированными,
            // потом раскомментируйте и создайте AddSortIndexes.
            e.HasIndex(j => j.SalaryFrom);
            e.HasIndex(j => j.CreatedAt);

            e.HasData(
                new Job { Id = 1, Title = "Backend-разработчик", Company = "TechCorp", SalaryFrom = 8000000, SalaryTo = 15000000, IsRemote = true, SphereId = 1, OwnerId = 1, CreatedAt = new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Job { Id = 2, Title = "Менеджер по продажам", Company = "Retail Group", SalaryFrom = 4000000, SalaryTo = 7000000, IsRemote = false, SphereId = 2, OwnerId = 1, CreatedAt = new DateTime(2026, 10, 5, 0, 0, 0, DateTimeKind.Utc) },
                new Job { Id = 3, Title = "SMM-специалист", Company = "Brand Studio", SalaryFrom = 3000000, SalaryTo = 5000000, IsRemote = true, SphereId = 3, OwnerId = 1, CreatedAt = new DateTime(2026, 10, 10, 0, 0, 0, DateTimeKind.Utc) },
                new Job { Id = 4, Title = "Бухгалтер", Company = "FinService", SalaryFrom = 5000000, SalaryTo = 8000000, IsRemote = false, SphereId = 4, OwnerId = 1, CreatedAt = new DateTime(2026, 10, 15, 0, 0, 0, DateTimeKind.Utc) },
                new Job { Id = 5, Title = "Преподаватель английского", Company = "Edu Center", SalaryFrom = 3500000, SalaryTo = 6000000, IsRemote = false, SphereId = 5, OwnerId = 1, CreatedAt = new DateTime(2026, 10, 20, 0, 0, 0, DateTimeKind.Utc) });
        });
    }
}
