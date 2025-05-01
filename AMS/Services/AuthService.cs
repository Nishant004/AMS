using System.Text.Json;
using AMS.Models.ServiceModels;

public class AuthService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Task LoginAsync(AuthSession session)
    {
        var sessionJson = JsonSerializer.Serialize(session);
        _httpContextAccessor.HttpContext.Session.SetString("User", sessionJson);
        return Task.CompletedTask;
    }

    public Task LogoutAsync()
    {
        _httpContextAccessor.HttpContext.Session.Clear();
        return Task.CompletedTask;
    }

    public AuthSession? GetCurrentUser()
    {
        var json = _httpContextAccessor.HttpContext.Session.GetString("User");
        return string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize<AuthSession>(json);
    }
}





