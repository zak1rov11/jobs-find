namespace JobsApi;

public class Sphere
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public List<Job> Jobs { get; set; } = [];
}

public class AppUser
{
    public int Id { get; set; }
    public string UserName { get; set; } = "";
    public string PasswordHash { get; set; } = "";
}

public class Job
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Company { get; set; } = "";
    public int SalaryFrom { get; set; }
    public int SalaryTo { get; set; }
    public bool IsRemote { get; set; }
    public string? LogoFileName { get; set; }
    public DateTime CreatedAt { get; set; }
    public int SphereId { get; set; }
    public Sphere? Sphere { get; set; }
    public int OwnerId { get; set; }
    public AppUser? Owner { get; set; }
}
