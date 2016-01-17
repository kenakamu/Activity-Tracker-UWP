using ActivityTrackerUWP.ViewModels;
using Windows.UI.Xaml.Controls;

namespace ActivityTrackerUWP.Views
{
    public sealed partial class CheckInPage : Page
    {
        public CheckInPage()
        {
            this.InitializeComponent();
        }

        // strongly-typed view models enable x:bind
        public CheckInPageViewModel ViewModel => this.DataContext as CheckInPageViewModel;
    }
}
