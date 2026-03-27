namespace nodiWeb.Services;

/// <summary>
/// Lets the AppBar (MainLayout) trigger note-creation actions that are
/// handled by the Home page. Same scoped-service pattern as SearchService.
/// </summary>
public class NoteActionsService
{
    public event Action<string>? OnOpenEditor;

    public void OpenEditor(string type) => OnOpenEditor?.Invoke(type);
}
