using ActivityTrackerUWP.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace ActivityTrackerUWP.Views
{  
    public sealed partial class ContactDetailPage : Page
    {
        public ContactDetailPage()
        {
            this.InitializeComponent();
        }

        // strongly-typed view models enable x:bind
        public ContactDetailPageViewModel ViewModel => this.DataContext as ContactDetailPageViewModel;

        /// <summary>
        /// Check Current Visual State and pass the result to ViewModel
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var currentState = VisualStateManager.GetVisualStateGroups(grid)[0].CurrentState.Name;
            
            if (currentState == "VisualStateNarrow")
                ViewModel.IsCheckInVisible = false;
            else
                ViewModel.IsCheckInVisible = true;
        }
    }
}
