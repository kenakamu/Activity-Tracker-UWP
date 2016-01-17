using ActivityTrackerUWP.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace ActivityTrackerUWP.ViewModels
{
    public class MainPageViewModel : Mvvm.ViewModelBase
    {
        #region Members
        
        public string SearchCriteria
        {
            get { return _helper.SearchCriteria; }
            set { if (_helper.SearchCriteria != value) { _helper.SearchCriteria = value; } }
        }

        private string searchTitle;
        public string SearchTitle
        {
            get { return searchTitle; }
            set { Set(ref searchTitle, value); }
        }

        // Return Cached Recetly used Contacts or Contact Search results
        public List<Contact> ContactResults
        {
            get
            {
                if (string.IsNullOrEmpty(SearchCriteria))
                {
                    SearchTitle = "RECENT CONTACTS";
                    return _settings.RecentContacts;
                    
                }
                else
                {                    
                    this.SearchTitle = "SEARCH RESULTS";
                    return _helper.SearchResults;
                }
            }
        }

        #endregion

        #region Navigation

        public override void OnNavigatedTo(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if(parameter != null)
            {
                // If parameter is string, then its search criteria
                if (parameter is string && !string.IsNullOrEmpty(parameter.ToString()))
                {
                    // Get search criteria and do search
                    SearchCriteria = parameter.ToString();
                    SearchContacts();
                }
            }
        }

        public override async Task OnNavigatedFromAsync(IDictionary<string, object> state, bool suspending)
        {            
        }

        #endregion

        #region Methods

        /// <summary>
        /// Constructor
        /// </summary>
        public MainPageViewModel()
        {
        }

        /// <summary>
        /// Search Contacts using SerachCriterial
        /// </summary>
        public async void SearchContacts()
        {
            Views.Shell.SetBusy(true, "Searching...");

            var searchCriteria = SearchCriteria.Replace('*', '%');
            _helper.SearchResults = await crmservice.SearchContacts(searchCriteria);
            base.RaisePropertyChanged("ContactResults");
            
            // If only one record found, then navigate to detail.
            if (_helper.SearchResults.Count == 1)
            {
                _helper.UpdateRecentContacts(_helper.SearchResults[0]);
                NavigationService.Navigate(typeof(Views.ContactDetailPage), _helper.SearchResults[0].Id);
            }

            Views.Shell.SetBusy(false);
        }

        /// <summary>
        /// Check SearchCriteria as changing.
        /// </summary>
        public void SearchTextChanged()
        {
            if (SearchCriteria == "")
                base.RaisePropertyChanged("ContactResults");
        }

        /// <summary>
        /// Naviate to ContactDetail Page when clicking Record
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ContectClicked(object sender, ItemClickEventArgs e)
        {
            _helper.UpdateRecentContacts(e.ClickedItem as Contact);
            NavigationService.Navigate(typeof(Views.ContactDetailPage), (e.ClickedItem as Contact).Id);
        }

        #endregion
    }
}

