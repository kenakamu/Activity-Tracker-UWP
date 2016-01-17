using ActivityTrackerUWP.Models;
using ActivityTrackerUWP.Services.SettingsServices;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.StartScreen;
using Windows.UI.Xaml.Controls;

namespace ActivityTrackerUWP.Helpers
{
    /// <summary>
    /// Helper class provides temporary cache and common methods
    /// </summary>
    public class ActivityTrackerHelper
    {
        public static ActivityTrackerHelper Instance { get; }
        static ActivityTrackerHelper()
        {
            // implement singleton pattern
            Instance = Instance ?? new ActivityTrackerHelper();
        }

        #region Class Member
              
        // Temporary cache. Use Settings service for persist cache.
        public string SearchCriteria { get; set; }
        public List<Contact> SearchResults { get; set; }
        public Contact CurrentContact { get; set; }
        public Activity CurrentActivity { get; set; }
        public List<Activity> CompletedActivities { get; set; }

        // Store VoiceCommandName for Cortana Integration
        public string VoiceCommandName { get; set; }

        private ISettingsService _settings;

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        private ActivityTrackerHelper()
        {
            // Get Setting SettingsHelper
            _settings = SettingsService.Instance;

            // Instantiate Lists
            SearchResults = new List<Contact>();
            CompletedActivities = new List<Activity>();            
        }

        /// <summary>
        /// Clear local member data
        /// </summary>
        public void ClearCache()
        {
            SearchCriteria = "";
            SearchResults = new List<Contact>();
            CurrentContact = null;
            CurrentActivity = null;
            CompletedActivities = new List<Activity>();
        }

        /// <summary>
        /// Retrieve OAuth and Resource. See https://msdn.microsoft.com/en-us/library/gg327838.aspx for detail
        /// </summary>
        /// <returns>Result</returns>
        public async Task<bool> DiscoveryAuthority()
        {
            // If server url is null, then return false.
            if (String.IsNullOrEmpty(_settings.ServerUrl))
                return false;

            try
            {
                AuthenticationParameters ap = 
                    await AuthenticationParameters.CreateFromResourceUrlAsync(new Uri(_settings.ServerUrl + "/api/data/"));
                _settings.OAuthUrl = ap.Authority;
                _settings.ResourceName = ap.Resource;

                return true;
            }
            catch (Exception ex)
            {
                MessageDialog dialog = new MessageDialog("OAuth Url retrieve failed. Please check Service URL again.");
                await dialog.ShowAsync();
                return false;
            }
        }

        /// <summary>
        /// Get Dynamics CRM Organization version 
        /// </summary>
        /// <returns>Version Number</returns>
        public async Task<Version> CheckVersion()
        {
            string serverUrl = _settings.ServerUrl;

            try
            {                
                using (HttpClient httpClient = new HttpClient())
                {
                    // Get Organization Name
                    var orgName = serverUrl.Substring(8, serverUrl.IndexOf(".") - 8);
                    // Get version by calling discovery
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await ADALHelper.GetTokenSilent());
                    HttpResponseMessage httpResponse = await httpClient.GetAsync(serverUrl.Replace(orgName, "disco") + "/api/discovery/v8.0/Instances");
                    
                    // Get results, check Organization Name
                    var results = Newtonsoft.Json.Linq.JObject.Parse(httpResponse.Content.ReadAsStringAsync().Result)["value"];
                    var org = results.Where(x => x["UrlName"].ToString() == orgName).FirstOrDefault();

                    if (org != null)
                        return new Version(org["Version"].ToString());
                    else
                        // Currently, no NA region doesn't return any version, so put it as 7.0 for now
                        return new Version("7.0.0.0");
                }
            }
            catch (Exception ex)
            {
                MessageDialog dialog = new MessageDialog("Failed to check version.");
                await dialog.ShowAsync();
                return null;                
            }
        }

        #region Application methods

        /// <summary>
        /// This method maintain Recently Used Contacts
        /// </summary>
        /// <param name="contact">Contact record</param>
        public void UpdateRecentContacts(Contact contact)
        {
            var recentContact = _settings.RecentContacts;

            // Update Recently Used Contacts
            if (recentContact.Where(x => x.Id == contact.Id).FirstOrDefault() != null)
            {
                // If the record is already on top of the list, then do nothing.
                if (recentContact.IndexOf(recentContact.Where(x => x.Id == contact.Id).First()) == 0)
                    return;

                // otherwise remove from the list first.
                recentContact.Remove(recentContact.Where(x => x.Id == contact.Id).First());
            }

            // If Recently Used Contacts is already 10, then removed the oldest one.
            if (recentContact.Count == 10)
                recentContact.RemoveAt(9);

            // Add currently selected record on top of the list.
            recentContact.Insert(0, contact);

            _settings.RecentContacts = recentContact;
        }
       
        // titleId for secondary tile
        public const string appbarTileId = "ActivityTrackerContact";

        /// <summary>
        /// Create/Remove Secondary Tile 
        /// </summary>
        public async Task PinUnpin()
        {
            // If SecondaryTile already exists, then remove it.
            if (SecondaryTile.Exists(appbarTileId + CurrentContact.Id))
            {
                // Unpin
                SecondaryTile secondaryTile = new SecondaryTile(appbarTileId + CurrentContact.Id);

                bool isUnpinned = await secondaryTile.RequestDeleteAsync();

                // Delete data from local folder if exists.
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                try
                {
                    StorageFile File = await localFolder.GetFileAsync(CurrentContact.Id + ".png");
                    await File.DeleteAsync();
                }
                catch
                {
                    // If no file, do nothing
                }
            }
            else
            {
                Uri square150x150Logo;

                // If Contact has its own picture, then ues it for SecondayTile.
                if (CurrentContact.EntityImage != null)
                {
                    // Store the data to local Folder.
                    StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                    // Create file
                    StorageFile File = await localFolder.CreateFileAsync(CurrentContact.Id + ".png", CreationCollisionOption.ReplaceExisting);

                    // Write data to file
                    using (Stream stream = await File.OpenStreamForWriteAsync())
                    {
                        var fs = new BinaryWriter(stream);
                        fs.Write(CurrentContact.EntityImage);
                    }

                    // Specify the saved file as SecondaryTile Icon.
                    square150x150Logo = new Uri("ms-appdata:///local/" + CurrentContact.Id + ".png");
                }
                else
                    // If Contact doesn't have picture, then use existing icon.
                    square150x150Logo = new Uri("ms-appx:///Assets/icon-contact.png");

                // Set record Id as Activation Argument.
                string tileActivationArguments = CurrentContact.Id.ToString();
                // Set Contact FullName as display
                string displayName = CurrentContact.FullName;

                TileSize newTileDesiredSize = TileSize.Square150x150;

                // Instantiate Secondary tile by specifying above information.
                SecondaryTile secondaryTile = new SecondaryTile(appbarTileId + CurrentContact.Id,
                                                                displayName,
                                                                tileActivationArguments,
                                                                square150x150Logo,
                                                                newTileDesiredSize);
                // Display FullName to the tile.
                secondaryTile.VisualElements.ShowNameOnSquare150x150Logo = true;

                // Create SecondaryTile
                bool isPinned = await secondaryTile.RequestCreateAsync();
            }
        }

        /// <summary>
        /// Get Pin/Unpin SymbolIcon
        /// </summary>
        /// <returns>Pin or Unpin SymbolIcon</returns>
        public SymbolIcon ToggleAppBarButton()
        {
            if (CurrentContact != null && SecondaryTile.Exists(appbarTileId + CurrentContact.Id))
                // Change icon. Do not set any label
                return new SymbolIcon(Symbol.UnPin);
            else
                // Change icon. Do not set any label
                return new SymbolIcon(Symbol.Pin);
        }

        #endregion
    }
}
