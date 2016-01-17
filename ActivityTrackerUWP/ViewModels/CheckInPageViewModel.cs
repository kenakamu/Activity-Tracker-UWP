using ActivityTrackerUWP.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Template10.Services.NavigationService;
using Windows.UI.Xaml.Navigation;

namespace ActivityTrackerUWP.ViewModels
{
    public class CheckInPageViewModel : Mvvm.ViewModelBase
    {
        #region Members

        // Record Id
        private Guid id;
        public Guid Id
        {
            get { return id; }
            set { Set(ref id, value);  GetTask(value); }
        }

        // Currently selected Contact
        public Contact CurrentContact
        {
            get { return _helper.CurrentContact; }
        }

        // Currently selected Activity
        private Activity currentActivity;
        public Activity CurrentActivity
        {
            get { return currentActivity; }
            set { Set(ref currentActivity, value);  }
        }

        #endregion

        #region Navigation

        public override void OnNavigatedTo(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if (state.ContainsKey(nameof(Id)))
            {
                Id = (Guid)state[nameof(Id)];
                state.Clear();
            }
            else
            {
                if(parameter != null)
                    Id = (Guid)parameter;
            }
        }

        public override async Task OnNavigatedFromAsync(IDictionary<string, object> state, bool suspending)
        {
            if (suspending)
                state[nameof(Id)] = Id;
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
        public CheckInPageViewModel()
        {
        }

        /// <summary>
        /// Retreieve selected Task 
        /// </summary>
        /// <param name="activity"></param>
        private async void GetTask(Guid id)
        {
            Views.Shell.SetBusy(true, "Getting Task...");

            if (id == null || id == Guid.Empty)
            {
                CurrentActivity = new Activity()
                {
                    Subject = "Check in with " + CurrentContact.FullName + " on " + DateTime.Now.Date.ToString("d"),
                    ActualEnd = DateTime.Now.Date
                };
            }
            else
            {
                // Retrieve a Task            
                CurrentActivity = await crmservice.RetrieveTask(id);
            }

            Views.Shell.SetBusy(false);
        }

        /// <summary>
        /// Save Task
        /// </summary>
        public async Task SaveTask()
        {
            Views.Shell.SetBusy(true, "Upserting...");

            // Call helper upsert method, which create/update a task, then complete.
            await crmservice.UpsertTask(CurrentActivity, CurrentContact);
            _helper.CompletedActivities.Clear();

            Views.Shell.SetBusy(false);

            // Go back to Contact Page
            NavigationService.GoBack();
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

        /// <summary>
        /// Delete Task
        /// </summary>
        public async Task DeleteTask()
        {
            Views.Shell.SetBusy(true, "Deleting....");

            // Call helper delete method
            await crmservice.DeleteTask(CurrentActivity);
            _helper.CompletedActivities.Clear();

            Views.Shell.SetBusy(false);

            // Go back to Contact Page
            NavigationService.GoBack();
        }

        #endregion        
    }
}
