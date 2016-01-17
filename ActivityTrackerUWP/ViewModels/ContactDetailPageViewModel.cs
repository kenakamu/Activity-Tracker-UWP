using ActivityTrackerUWP.Models;
using ActivityTrackerUWP.Views;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Template10.Services.NavigationService;
using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace ActivityTrackerUWP.ViewModels
{
    public class ContactDetailPageViewModel : Mvvm.ViewModelBase
    {
        #region Members
        
        // Record Id
        private Guid id;
        public Guid Id
        {
            get { return id; }
            set { Set(ref id, value); GetContact(value); }           
        }
               
        // Return completed Activities
        public List<Activity> CompletedActivities
        {
            get { return _helper.CompletedActivities; }
            set { _helper.CompletedActivities = value; RaisePropertyChanged(); }
        }

        // Currently selected Contact
        public Contact CurrentContact
        {
            get { return _helper.CurrentContact; }
            set { _helper.CurrentContact = value; RaisePropertyChanged(); }
        }

        // Currently selected Activity
        private Activity currentActivity;
        public Activity CurrentActivity
        {
            get { return currentActivity; }
            set { Set(ref currentActivity, value); RaisePropertyChanged(); }
        }

        private SymbolIcon pin;
        public SymbolIcon Pin
        {
            get { return pin; }
            set { Set(ref pin, value); PinLabel = (pin.Symbol == Symbol.Pin) ? "Pin" : "UnPin"; }            
        }

        private string pinLabel;
        public string PinLabel
        {
            get { return pinLabel; }
            set { Set(ref pinLabel, value); }
        }

        private bool isCheckInVisible;
        public bool IsCheckInVisible
        {
            get { return isCheckInVisible; }
            set { Set(ref isCheckInVisible, value); }
        }

        #endregion

        #region Navigation

        public override async void OnNavigatedTo(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            // If this is refresh, then try to restore data from cache.
            if (mode == NavigationMode.Refresh)
            {
                if (_settings.CachedCurrentContact != null)
                {
                    var cachedCurrentContact = _settings.CachedCurrentContact;
                    CurrentContact = cachedCurrentContact;
                    _settings.CachedCurrentContact = null;
                }
                if (_settings.CachedCurrentActivity != null)
                {
                    var cachedCurrentActivity = _settings.CachedCurrentActivity;
                    CurrentActivity = cachedCurrentActivity;
                    _settings.CachedCurrentActivity = null;
                }
                if (_settings.CachedCompletedActivities != null)
                {
                    var cachedCompletedActivities = _settings.CachedCompletedActivities;
                    CompletedActivities = cachedCompletedActivities;
                    _settings.CachedCompletedActivities = null;
                }
            }

            if (parameter is System.Guid && (Guid)parameter != Guid.Empty)
                Id = (Guid)parameter;
            else
                throw new Exception("Need to pass Id to ContactDetailPage");

        }

        public override async Task OnNavigatedFromAsync(IDictionary<string, object> state, bool suspending)
        {
            if (suspending)
            {
                // When suspending, cache it for quick resume in the near future.
                // As we cache non-premitive types, utilizing SettingsServices.
                _settings.CachedCurrentContact = CurrentContact;
                _settings.CachedCurrentActivity = CurrentActivity;
                _settings.CachedCompletedActivities = CompletedActivities;
            }
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
        public ContactDetailPageViewModel()
        {
        }

        /// <summary>
        /// Retrieve selected Contact
        /// </summary>
        /// <param name="contact"></param>
        private async void GetContact(Guid id)
        {
            Views.Shell.SetBusy(true, "Getting Contact...");

            // Try to re-use cached contact record when appropriate
            if (CurrentContact == null || id != CurrentContact.Id)
            {
                // Retrieve Contact if cache cannot be used
                CurrentContact = await crmservice.RetrieveContact(id);
                // As contact record has been changed, clear the activity
                CompletedActivities.Clear();
                CurrentActivity = null;
            }

            // If there is no completed activities for the contact, this runs all the time.
            if (CompletedActivities.Count == 0)
                CompletedActivities = await crmservice.RetrieveActivitiesOfContact(id);

            // Update Pin status
            Pin = _helper.ToggleAppBarButton();

            Views.Shell.SetBusy(false);

            CheckVoiceCommand();
        }

        /// <summary>
        /// Check Voice Command
        /// </summary>
        private void CheckVoiceCommand()
        {
            // If voiceCommand available
            if (!String.IsNullOrEmpty(_helper.VoiceCommandName))
            {
                switch (_helper.VoiceCommandName)
                {
                    // If this is Checkin command, then simply do CheckInPage.
                    case "Checkin":
                        CurrentActivity = null;
                        CheckIn();
                        break;
                }
                return;
            }
        }

        /// <summary>
        /// CheckIn contact
        /// </summary>
        public void CheckIn()
        {
            CurrentActivity = null;
            if (IsCheckInVisible)
            {
                CurrentActivity = new Activity()
                {
                    Subject = "Check in with " + CurrentContact.FullName + " on " + DateTime.Now.Date.ToString("d"),
                    ActualEnd = DateTime.Now.Date
                };
            }
            else
                this.NavigationService.Navigate(typeof(CheckInPage), Guid.Empty);
        }

        /// <summary>
        /// Task Clicked Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">ItemClickEventArgs contains Activity</param>
        public async void TaskClicked(object sender, ItemClickEventArgs e)
        {
            Activity activity = e.ClickedItem as Activity;
            // Only task is handled
            if (activity.ActivityTypeCode != "task")
                return;

            if (IsCheckInVisible)
            {
                Views.Shell.SetBusy(true, "Loading Task...");

                // Retrieve a Task            
                CurrentActivity = await crmservice.RetrieveTask(activity.Id);

                Views.Shell.SetBusy(false);
            }
            else
            {
                NavigationService.Navigate(typeof(CheckInPage), activity.Id);
            }
        }

        /// <summary>
        /// Pin or Unpin Contact
        /// </summary>
        public async void PinContact()
        {
            await _helper.PinUnpin();
            Pin = _helper.ToggleAppBarButton();
        }

        /// <summary>
        /// Open Map apprication
        /// </summary>
        /// <returns></returns>
        public async Task AddressTo()
        {
            await Launcher.LaunchUriAsync(new Uri("bingmaps:?where=" + CurrentContact.Address1));
        }

        /// <summary>
        /// Open Mail application
        /// </summary>
        /// <returns></returns>
        public async Task MailTo()
        {
            await Launcher.LaunchUriAsync(new Uri(String.Format("mailto:{0}", CurrentContact.Email1)));
        }

        /// <summary>
        /// Open Phone Call application
        /// </summary>
        /// <returns></returns>
        public async Task CallTo()
        {
            await Launcher.LaunchUriAsync(new Uri(String.Format("CallTo:{0}", CurrentContact.Telephone1)));
        }

        /// <summary>
        /// Save Task
        /// </summary>
        public async Task SaveTask()
        {
            Views.Shell.SetBusy(true, "Upserting...");

            // Call helper upsert method, which create/update a task, then complete.
            await crmservice.UpsertTask(CurrentActivity, CurrentContact);

            Views.Shell.SetBusy(false);

            // Get all completed activities again.
            CompletedActivities = await crmservice.RetrieveActivitiesOfContact(CurrentContact.Id);
        }

        /// <summary>
        /// Delete Task
        /// </summary>
        public async Task DeleteTask()
        {
            Views.Shell.SetBusy(true, "Deleting...");

            // Call helper delete method
            await crmservice.DeleteTask(CurrentActivity);
            CurrentActivity = null;
            Views.Shell.SetBusy(false);

            // Get all completed activities again.
            CompletedActivities = await crmservice.RetrieveActivitiesOfContact(CurrentContact.Id);
        }

        /// <summary>
        /// Listen for voice and set the result to note.
        /// </summary>
        /// <returns></returns>
        public async Task GetVoice()
        {
            // Set note text what user spoke.
            CurrentActivity.Notes += ' ' + await VoiceToText();
            this.RaisePropertyChanged("CurrentActivity");
        }

        #endregion
    }
}

