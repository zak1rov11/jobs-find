namespace JobsApi;

public record SphereDto(int Id, string Name);

public record JobDto(
    int Id, string Title, string Company, int SalaryFrom, int SalaryTo, bool IsRemote,
    string? LogoUrl, int SphereId, string SphereName, int OwnerId, string OwnerName, DateTime CreatedAt);

public record JobRequest(
    string Title, string Company, int SalaryFrom, int SalaryTo, bool IsRemote, int SphereId);

public record PagedResult<T>(List<T> Items, int Page, int PageSize, int Total);

public record RegisterRequest(string UserName, string Password);
public record LoginRequest(string UserName, string Password);
public record AuthResponse(string Token, int UserId, string UserName);
