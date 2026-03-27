using System.Net.Http.Headers;
using System.Net.Http.Json;
using nodeCommon;

namespace nodiWeb.Services;

/// <summary>
/// Thin wrapper around the nodiCore REST API. All methods return value tuples or
/// nullable results so callers never have to catch exceptions themselves.
/// Authenticated endpoints attach the current user's JWT via <see cref="CreateClient"/>.
/// </summary>
public class ApiService(IHttpClientFactory httpClientFactory, AuthStateProvider authState)
{
    /// <summary>
    /// Creates an <see cref="HttpClient"/> pre-configured with the nodiCore base URL.
    /// If a user is currently signed in, the Bearer token is added to every request
    /// so the caller doesn't have to think about auth headers.
    /// </summary>
    private HttpClient CreateClient()
    {
        var client = httpClientFactory.CreateClient("nodiApi");
        if (authState.CurrentUser is not null)
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", authState.CurrentUser.Token);
        return client;
    }

    // -------------------------------------------------------------------------
    // Auth
    // -------------------------------------------------------------------------

    /// <summary>
    /// Authenticates a user and returns the JWT response on success.
    /// Uses a raw (unauthenticated) client — the token doesn't exist yet.
    /// Returns <c>(null, errorMessage)</c> on failure so the UI can surface the reason.
    /// </summary>
    public async Task<(AuthResponse? Result, string? Error)> LoginAsync(string username, string password)
    {
        try
        {
            var client = httpClientFactory.CreateClient("nodiApi");
            var response = await client.PostAsJsonAsync("api/auth/login", new { username, password });
            if (response.IsSuccessStatusCode)
                return (await response.Content.ReadFromJsonAsync<AuthResponse>(), null);



            var err = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            return (null, err?.Message ?? "Login failed.");
        }
        catch (Exception ex) { return (null, ex.Message); }
    }

    /// <summary>
    /// Registers a new user account. Registration may be closed by the admin
    /// (controlled via the AppSettings table in nodiCore), in which case the API
    /// returns a non-success status and the error message is surfaced to the caller.
    /// </summary>
    public async Task<(AuthResponse? Result, string? Error)> RegisterAsync(string username, string email, string password)
    {
        try
        {
            var client = httpClientFactory.CreateClient("nodiApi");
            var response = await client.PostAsJsonAsync("api/auth/register", new { username, email, password });
            if (response.IsSuccessStatusCode)
                return (await response.Content.ReadFromJsonAsync<AuthResponse>(), null);
            var err = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            return (null, err?.Message ?? "Registration failed.");
        }
        catch (Exception ex) { return (null, ex.Message); }
    }

    // -------------------------------------------------------------------------
    // Notes
    // -------------------------------------------------------------------------

    /// <summary>
    /// Fetches the current user's notes. Defaults to active (non-archived, non-deleted) notes.
    /// Pass <paramref name="archived"/> or <paramref name="deleted"/> to switch views,
    /// <paramref name="tagId"/> to filter by tag, or <paramref name="search"/> for full-text search.
    /// </summary>
    public async Task<List<NoteDto>> GetNotesAsync(bool archived = false, bool deleted = false, int? tagId = null, string? search = null)
    {
        var query = $"api/notes?archived={archived}&deleted={deleted}";
        if (tagId.HasValue) query += $"&tagId={tagId}";
        if (!string.IsNullOrWhiteSpace(search)) query += $"&search={Uri.EscapeDataString(search)}";
        var response = await CreateClient().GetAsync(query);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<List<NoteDto>>() ?? []
            : [];
    }

    /// <summary>Returns a single note by ID, or <c>null</c> if not found.</summary>
    public async Task<NoteDto?> GetNoteAsync(int id)
    {
        var response = await CreateClient().GetAsync($"api/notes/{id}");
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<NoteDto>()
            : null;
    }

    /// <summary>
    /// Creates a new note. The <paramref name="request"/> anonymous object is serialised
    /// directly so callers can pass only the fields they want to set.
    /// Returns <c>null</c> if the API rejects the request.
    /// </summary>
    public async Task<NoteDto?> CreateNoteAsync(object request)
    {
        var response = await CreateClient().PostAsJsonAsync("api/notes", request);
        return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<NoteDto>() : null;
    }

    /// <summary>
    /// Updates an existing note. Only the fields present in <paramref name="request"/> need
    /// to be supplied — the API performs a full replace, so callers should include all fields.
    /// Returns <c>null</c> on failure.
    /// </summary>
    public async Task<NoteDto?> UpdateNoteAsync(int id, object request)
    {
        var response = await CreateClient().PutAsJsonAsync($"api/notes/{id}", request);
        return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<NoteDto>() : null;
    }

    /// <summary>Soft-deletes a note (moves it to Trash). Recoverable via <see cref="RestoreNoteAsync"/>.</summary>
    public async Task<bool> DeleteNoteAsync(int id) =>
        (await CreateClient().DeleteAsync($"api/notes/{id}")).IsSuccessStatusCode;

    /// <summary>Permanently deletes a note. This cannot be undone.</summary>
    public async Task<bool> PermanentDeleteNoteAsync(int id) =>
        (await CreateClient().DeleteAsync($"api/notes/{id}/permanent")).IsSuccessStatusCode;

    /// <summary>Restores a soft-deleted note from Trash back to the active notes list.</summary>
    public async Task<bool> RestoreNoteAsync(int id) =>
        (await CreateClient().PutAsync($"api/notes/{id}/restore", null)).IsSuccessStatusCode;

    // -------------------------------------------------------------------------
    // Tags
    // -------------------------------------------------------------------------

    /// <summary>Returns all tags belonging to the current user.</summary>
    public async Task<List<TagDto>> GetTagsAsync()
    {
        var response = await CreateClient().GetAsync("api/tags");
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<List<TagDto>>() ?? []
            : [];
    }

    /// <summary>Creates a new tag with the given name. Returns <c>null</c> on failure (e.g. duplicate).</summary>
    public async Task<TagDto?> CreateTagAsync(string name)
    {
        var response = await CreateClient().PostAsJsonAsync("api/tags", new { name });
        return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<TagDto>() : null;
    }

    /// <summary>Deletes a tag. Associated notes are not deleted — they just lose the tag.</summary>
    public async Task<bool> DeleteTagAsync(int id) =>
        (await CreateClient().DeleteAsync($"api/tags/{id}")).IsSuccessStatusCode;

    // -------------------------------------------------------------------------
    // Admin  (requires Admin role)
    // -------------------------------------------------------------------------

    /// <summary>Returns all registered users. Admin only.</summary>
    public async Task<List<UserDto>> GetUsersAsync()
    {
        var response = await CreateClient().GetAsync("api/admin/users");
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<List<UserDto>>() ?? []
            : [];
    }

    /// <summary>Updates a user's details (e.g. role, email). Admin only.</summary>
    public async Task<bool> UpdateUserAsync(int id, object request) =>
        (await CreateClient().PutAsJsonAsync($"api/admin/users/{id}", request)).IsSuccessStatusCode;

    /// <summary>Deletes a user account and all their data. Admin only.</summary>
    public async Task<bool> DeleteUserAsync(int id) =>
        (await CreateClient().DeleteAsync($"api/admin/users/{id}")).IsSuccessStatusCode;

    /// <summary>
    /// Returns all app-wide settings (e.g. <c>RegistrationOpen</c>).
    /// These are stored in the AppSettings table in nodiCore. Admin only.
    /// </summary>
    public async Task<List<AppSettingDto>> GetSettingsAsync()
    {
        var response = await CreateClient().GetAsync("api/admin/settings");
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<List<AppSettingDto>>() ?? []
            : [];
    }

    /// <summary>Persists a batch of app settings in one request. Admin only.</summary>
    public async Task<bool> UpdateSettingsAsync(List<AppSettingDto> settings) =>
        (await CreateClient().PutAsJsonAsync("api/admin/settings", settings)).IsSuccessStatusCode;

    /// <summary>Deserialises the <c>{ "message": "..." }</c> error body returned by nodiCore on failures.</summary>
    private record ErrorResponse(string? Message);
}
