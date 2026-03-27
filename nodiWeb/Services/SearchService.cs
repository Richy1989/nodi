namespace nodiWeb.Services;

/// <summary>
/// Mediates the search term between the top-bar (MainLayout) and whatever
/// page is currently handling note display (Home). Registered as Scoped so
/// the same instance is shared within a single Blazor circuit.
/// </summary>
public class SearchService
{
    public event Action<string?>? OnSearch;

    public void Search(string? term) => OnSearch?.Invoke(term);
}
