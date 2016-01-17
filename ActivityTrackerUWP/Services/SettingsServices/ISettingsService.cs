using ActivityTrackerUWP.Models;
using ActivityTrackerUWP.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;

namespace ActivityTrackerUWP.Services.SettingsServices
{
    public interface ISettingsService
    {
        bool UseShellBackButton { get; set; }
        ApplicationTheme AppTheme { get; set; }
        TimeSpan CacheMaxDuration { get; set; }
        string OAuthUrl { get; set; }
        string ServerUrl { get; set; }
        string ResourceName { get; set; }
        Version CrmVersion { get; set; }
        List<Contact> RecentContacts { get; set; }
        Contact CachedCurrentContact { get; set; }
        Activity CachedCurrentActivity { get; set; }
        List<Activity> CachedCompletedActivities { get; set; }
}
}
