using ActivityTrackerUWP.Models;
using ActivityTrackerUWP.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Template10.Services.NavigationService;
using Windows.UI.Popups;
using Windows.UI.StartScreen;
using Windows.UI.Xaml.Navigation;

namespace ActivityTrackerUWP.ViewModels
{
    public class SignInPageViewModel : Mvvm.ViewModelBase  
    {        
        #region Members
             
        public string ServerUrl
        {
            get
            {
                if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
                   return "https://contoso.crm.dynamics.com";
                else
                   return _settings.ServerUrl;
            }
            set { _settings.ServerUrl = value; }
        }

        string originalServerUrl;

        #endregion

        #region Navigation

        public override void OnNavigatedTo(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            originalServerUrl = _settings.ServerUrl;
        }

        public override async Task OnNavigatedFromAsync(IDictionary<string, object> state, bool suspending)
        {            
            await Task.Yield();
        }

        public override void OnNavigatingFrom(NavigatingEventArgs args)
        {
            args.Cancel = false;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Constructor
        /// </summary>
        public SignInPageViewModel()
        {
        }

        /// <summary>
        /// Signin to CRM.
        /// </summary>
        public async void SignIn()
        {
            if(originalServerUrl == _settings.ServerUrl)
                return;

            Views.Shell.SetBusy(true, "SignIn....");
            
            // Get OAuth Address
            bool login = await _helper.DiscoveryAuthority();

            if(!login)
            {
                Views.Shell.SetBusy(false);
                return;
            }

            try
            {
                // Get AccessToken
                var accessToken = await ADALHelper.GetTokenSilent(true);
                // Get CRM Version
                _settings.CrmVersion = await _helper.CheckVersion();
                
                // Store original address
                originalServerUrl = _settings.ServerUrl;

                // Once login to new org, then reset cached data and secondary tiles
                // Reset Recent Contacts Cache
                _settings.RecentContacts = new List<Contact>();
                _helper.ClearCache();
                foreach(var tile in await SecondaryTile.FindAllAsync())
                {
                    await tile.RequestDeleteAsync();
                }
            }
            catch(Exception ex)
            {                
                MessageDialog dialog = new MessageDialog(ex.Message);
                await dialog.ShowAsync();
                Views.Shell.SetBusy(false);
                return;
            }

            Views.Shell.SetBusy(false);

            NavigationService.Navigate(typeof(Views.MainPage));
        }

        #endregion
    }
}
