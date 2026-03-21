using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using nodiApp.Models;

namespace nodiApp.Services;

public record AuthResponse(string Token, int UserId, string Username, string Email, string Role);
public record ChecklistItemDto(int Id, string Text, bool IsChecked, int Order);
public record NoteDto(int Id, string Title, string? Content, string Color, string Type,
    bool IsPinned, bool IsArchived, bool IsDeleted, DateTime CreatedAt, DateTime UpdatedAt,
    List<ChecklistItemDto> ChecklistItems, List<TagDto> Tags);
public record TagDto(int Id, string Name, int NoteCount);

public class ApiService
{
    private readonly HttpClient _client;
    private string? _token;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public ApiService()
    {
        _client = new HttpClient();
        var baseUrl = Preferences.Get("ApiBaseUrl", "http://10.0.2.2:5100");
        _client.BaseAddress = new Uri(baseUrl);

        _token = Preferences.Get("AuthToken", null as string);
        if (_token is not null)
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
    }

    public bool IsAuthenticated => _token is not null;

    public void SetToken(string? token)
    {
        _token = token;
        if (token is not null)
        {
            Preferences.Set("AuthToken", token);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        else
        {
            Preferences.Remove("AuthToken");
            _client.DefaultRequestHeaders.Authorization = null;
        }
    }

    public async Task<(AuthResponse? Result, string? Error)> LoginAsync(string username, string password)
    {
        try
        {
            var resp = await _client.PostAsJsonAsync("api/auth/login", new { username, password });
            if (resp.IsSuccessStatusCode)
            {
                var auth = await resp.Content.ReadFromJsonAsync<AuthResponse>(JsonOpts);
                return (auth, null);
            }
            return (null, "Invalid username or password.");
        }
        catch (Exception ex) { return (null, $"Connection error: {ex.Message}"); }
    }

    public async Task<List<NoteDto>> GetNotesAsync()
    {
        try
        {
            return await _client.GetFromJsonAsync<List<NoteDto>>("api/notes", JsonOpts) ?? [];
        }
        catch { return []; }
    }

    public async Task<NoteDto?> CreateNoteAsync(object request)
    {
        try
        {
            var resp = await _client.PostAsJsonAsync("api/notes", request, JsonOpts);
            return resp.IsSuccessStatusCode ? await resp.Content.ReadFromJsonAsync<NoteDto>(JsonOpts) : null;
        }
        catch { return null; }
    }

    public async Task<NoteDto?> UpdateNoteAsync(int serverId, object request)
    {
        try
        {
            var resp = await _client.PutAsJsonAsync($"api/notes/{serverId}", request, JsonOpts);
            return resp.IsSuccessStatusCode ? await resp.Content.ReadFromJsonAsync<NoteDto>(JsonOpts) : null;
        }
        catch { return null; }
    }

    public async Task<bool> DeleteNoteAsync(int serverId)
    {
        try { return (await _client.DeleteAsync($"api/notes/{serverId}")).IsSuccessStatusCode; }
        catch { return false; }
    }

    public async Task<bool> PermanentDeleteNoteAsync(int serverId)
    {
        try { return (await _client.DeleteAsync($"api/notes/{serverId}/permanent")).IsSuccessStatusCode; }
        catch { return false; }
    }
}
