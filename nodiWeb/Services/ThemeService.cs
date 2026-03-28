namespace nodiWeb.Services;

/// <summary>
/// Holds the current user's theme preference and notifies subscribers when it changes.
/// Scoped per Blazor circuit so each user has independent state.
/// </summary>
public class ThemeService
{
    public bool IsDarkMode { get; private set; } = true;

    public event Action? OnThemeChanged;

    public void Apply(bool isDark)
    {
        IsDarkMode = isDark;
        OnThemeChanged?.Invoke();
    }

    public void Toggle() => Apply(!IsDarkMode);
}
