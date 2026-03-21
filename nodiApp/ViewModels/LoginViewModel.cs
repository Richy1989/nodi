using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using nodiApp.Services;

namespace nodiApp.ViewModels;

public partial class LoginViewModel(ApiService api, SyncService sync) : ObservableObject
{
    [ObservableProperty] private string username = string.Empty;
    [ObservableProperty] private string password = string.Empty;
    [ObservableProperty] private string? errorMessage;
    [ObservableProperty] private bool isLoading;

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Please enter username and password.";
            return;
        }

        IsLoading = true;
        ErrorMessage = null;

        var (result, error) = await api.LoginAsync(Username, Password);

        if (result is not null)
        {
            api.SetToken(result.Token);
            Preferences.Set("Username", result.Username);
            await sync.SyncAsync();
            await Shell.Current.GoToAsync("//notes");
        }
        else
        {
            ErrorMessage = error ?? "Login failed.";
        }

        IsLoading = false;
    }
}
