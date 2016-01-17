using ActivityTrackerUWP.Services.SettingsServices;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web;

namespace ActivityTrackerUWP.Helpers
{
    static public class ADALHelper
    {
        // RedirectUri for Azure AD
        static private string redirectUri = WebAuthenticationBroker.GetCurrentApplicationCallbackUri().ToString();
        // ClientId which you need to obtain from Azure Portal.
        // See http://msdn.microsoft.com/en-us/library/dn531010.aspx for more detail
        static private string ClientId = "a8c735c8-13bd-4736-beeb-8c715a754bb4";
        static private AuthenticationContext authContext = null;

        static private ISettingsService _settings;

        static ADALHelper()
        {
            // Get Setting SettingsHelper
            _settings = SettingsService.Instance;
        }
        /// <summary>
        /// This method try to obtain AccessToken by using OAuth2 authentication to Azure AD/AD FS.
        /// </summary>
        static public async Task<string> GetTokenSilent(bool signOut = false)
        {
            // Check mandatory parameters
            if (string.IsNullOrEmpty(_settings.OAuthUrl) || string.IsNullOrEmpty(_settings.ServerUrl))
                throw new Exception("OAuthUrl or ServerUrl is null");
            
            // If user signed, clear all token cache
            if (signOut)
            {
                authContext = new AuthenticationContext(_settings.OAuthUrl);
                authContext.TokenCache.Clear();                
            }
            else
            {
                // If authContext is null, then create AuthenticationContext by using OAuthUrl.
                if (authContext == null)
                    authContext = new AuthenticationContext(_settings.OAuthUrl);
            }

            // Retrieve AccessToken
            AuthenticationResult result = await authContext.AcquireTokenAsync(_settings.ResourceName, ClientId, new Uri(redirectUri));

            // Check the result.
            if (result != null && result.Status == AuthenticationStatus.Success)
                return result.AccessToken;
            else
            {
                string errorMessage = "";
                switch (result.Error)
                {
                    case "authentication_canceled":
                        // User cancelled, so no need to display a message.
                        break;
                    case "temporarily_unavailable":
                    case "server_error":
                        errorMessage = "Please retry the operation. If the error continues, please contact your administrator.";
                        break;
                    default:
                        // An error occurred when acquiring a token. Show the error description in a MessageDialog. 
                        errorMessage = string.Format("If the error continues, please contact your administrator.\n\nError: {0}\n\nError Description:\n\n{1}", result.Error, result.ErrorDescription);
                        break;
                }
                throw new Exception(errorMessage);
            }
        }      
    }
}
