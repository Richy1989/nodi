using nodiApp.Views;

namespace nodiApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("notedetail", typeof(NoteDetailPage));
        }
    }
}
