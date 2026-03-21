using nodiApp.ViewModels;

namespace nodiApp.Views;

public partial class NoteDetailPage : ContentPage
{
    private readonly NoteDetailViewModel _vm;

    public NoteDetailPage(NoteDetailViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.InitializeAsync();
    }
}
