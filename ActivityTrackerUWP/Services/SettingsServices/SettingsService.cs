using ActivityTrackerUWP.Models;
using ActivityTrackerUWP.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;

namespace ActivityTrackerUWP.Services.SettingsServices
{
    // DOCS: https://github.com/Windows-XAML/Template10/wiki/Docs-%7C-SettingsService
    public partial class SettingsService : ISettingsService
    {
        public static SettingsService Instance { get; }
        static SettingsService()
        {
            // implement singleton pattern
            Instance = Instance ?? new SettingsService();
        }

        Template10.Services.SettingsService.ISettingsHelper _helper;

        private SettingsService()
        {
            _helper = new Template10.Services.SettingsService.SettingsHelper();
        }

        public bool UseShellBackButton
        {
            get { return _helper.Read<bool>(nameof(UseShellBackButton), true); }
            set
            {
                _helper.Write(nameof(UseShellBackButton), value);
                ApplyUseShellBackButton(value);
            }
        }

        public ApplicationTheme AppTheme
        {
            get
            {
                var theme = ApplicationTheme.Dark;
                var value = _helper.Read<string>(nameof(AppTheme), theme.ToString(), Template10.Services.SettingsService.SettingsStrategies.Roam);
                return Enum.TryParse<ApplicationTheme>(value, out theme) ? theme : ApplicationTheme.Dark;
            }
            set
            {
                _helper.Write(nameof(AppTheme), value.ToString(), Template10.Services.SettingsService.SettingsStrategies.Roam);
                //ApplyAppTheme(value);
            }
        }

        public TimeSpan CacheMaxDuration
        {
            get { return _helper.Read<TimeSpan>(nameof(CacheMaxDuration), TimeSpan.FromDays(2)); }
            set
            {
                _helper.Write(nameof(CacheMaxDuration), value);
                ApplyCacheMaxDuration(value);
            }
        }
        
        // Activity Tracker related objects
        public string OAuthUrl
        {
            get { return _helper.Read<string>(nameof(OAuthUrl), "", Template10.Services.SettingsService.SettingsStrategies.Roam); }
            set { _helper.Write(nameof(OAuthUrl), value, Template10.Services.SettingsService.SettingsStrategies.Roam); }
        }

        public string ServerUrl
        {
            get { return _helper.Read<string>(nameof(ServerUrl), "", Template10.Services.SettingsService.SettingsStrategies.Roam); }
            set { _helper.Write(nameof(ServerUrl), value, Template10.Services.SettingsService.SettingsStrategies.Roam); }
        }

        public string ResourceName
        {
            get { return _helper.Read<string>(nameof(ResourceName), "", Template10.Services.SettingsService.SettingsStrategies.Roam); }
            set { _helper.Write(nameof(ResourceName), value, Template10.Services.SettingsService.SettingsStrategies.Roam); }
        }

        public Version CrmVersion
        {
            get { return _helper.Read<Version>(nameof(CrmVersion), null, Template10.Services.SettingsService.SettingsStrategies.Roam); }
            set { _helper.Write(nameof(CrmVersion), value, Template10.Services.SettingsService.SettingsStrategies.Roam); }
        }

        public List<Contact> RecentContacts
        {
            get { return _helper.Read<List<Contact>>(nameof(RecentContacts), new List<Contact>()); }
            set { _helper.Write(nameof(RecentContacts), value); }
        }

        public Contact CachedCurrentContact
        {
            get { return _helper.Read<Contact>(nameof(CachedCurrentContact), new Contact()); }
            set { _helper.Write(nameof(CachedCurrentContact), value); }
        }

        public Activity CachedCurrentActivity
        {
            get { return _helper.Read<Activity>(nameof(CachedCurrentActivity), new Activity()); }
            set { _helper.Write(nameof(CachedCurrentActivity), value); }
        }

        public List<Activity> CachedCompletedActivities
        {
            get { return _helper.Read<List<Activity>>(nameof(CachedCompletedActivities), new List<Activity>()); }
            set { _helper.Write(nameof(CachedCompletedActivities), value); }
        }
        
    }
}

