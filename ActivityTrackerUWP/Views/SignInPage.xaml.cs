using ActivityTrackerUWP.ViewModels;
using Windows.UI.Xaml.Controls;

namespace ActivityTrackerUWP.Views
{
    public sealed partial class SignInPage : Page
    {
        public SignInPage()
        {
            InitializeComponent();
            NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Disabled;
        }

        // strongly-typed view models enable x:bind
        public SignInPageViewModel ViewModel => this.DataContext as SignInPageViewModel;


    }
}
