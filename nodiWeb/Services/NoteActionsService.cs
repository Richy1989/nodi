namespace nodiWeb.Services;

public class NoteActionsService
{
    public event Action? OnExpandEditor;

    public void ExpandEditor() => OnExpandEditor?.Invoke();
}
