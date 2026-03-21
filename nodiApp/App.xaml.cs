using nodiApp.Services;

namespace nodiApp
{
    public partial class App : Application
    {
        private readonly ApiService _api;

        public App(ApiService api)
        {
            InitializeComponent();
            _api = api;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var shell = new AppShell();

            // Navigate to the right start page after shell is initialized
            shell.Loaded += async (_, _) =>
            {
                if (_api.IsAuthenticated)
                    await Shell.Current.GoToAsync("//notes");
                else
                    await Shell.Current.GoToAsync("//login");
            };

            return new Window(shell);
        }
    }
}
