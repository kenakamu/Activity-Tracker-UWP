using ActivityTrackerUWP.Helpers;
using ActivityTrackerUWP.Services.SettingsServices;
using ActivityTrackerUWP.Views;
using System;
using System.Threading.Tasks;
using Template10.Services.NavigationService;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.VoiceCommands;
using Windows.Media.SpeechRecognition;
using Windows.Storage;
using Windows.UI.Xaml;

namespace ActivityTrackerUWP
{
    /// Documentation on APIs used in this page:
    /// https://github.com/Windows-XAML/Template10/wiki

    sealed partial class App : Template10.Common.BootStrapper
    {
        ISettingsService _settings;
        ActivityTrackerHelper _helper;
       
        public App()
        {            
            InitializeComponent();
            SplashFactory = (e) => new Views.Splash(e);

            #region App settings

            _settings = SettingsService.Instance;
            _helper = ActivityTrackerHelper.Instance;    
            RequestedTheme = _settings.AppTheme;
            CacheMaxDuration = _settings.CacheMaxDuration;
            ShowShellBackButton = _settings.UseShellBackButton;

            #endregion
        }

        // runs even if restored from state
        public override async Task OnInitializeAsync(IActivatedEventArgs args)
        {            
            // setup hamburger shell
            var nav = NavigationServiceFactory(BackButton.Attach, ExistingContent.Include);
            // Disable Frame ConetntTransition to avoid Header animation when navigating.
            nav.Frame.ContentTransitions.Clear();           
            
            Window.Current.Content = new Views.Shell((NavigationService)nav);
            await Task.Yield();            
        }

        // runs only when not restored from state
        public override async Task OnStartAsync(StartKind startKind, IActivatedEventArgs args)
        {
            // Register Cortana Integaration
            if (VoiceCommandDefinitionManager.InstalledCommandDefinitions.Count == 0)
            {
                var VCDFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Cortana/ActivityTrackerVCD.xml"));
                await VoiceCommandDefinitionManager.InstallCommandDefinitionsFromStorageFileAsync(VCDFile);
            }

            #region OnLaunched

            // Handle OnLaunched Event
            if (startKind == StartKind.Launch)
            {
                // Cast args.
                var e = args as ILaunchActivatedEventArgs;

                // Check if this is launched via SecondaryTile.
                if (e.Arguments != "")
                {
                    // If parameter is Guid, then this is launched via SecondaryTile
                    Guid contactId;

                    // If tile had Guid, then 
                    if (Guid.TryParse(e.Arguments.ToString(), out contactId))
                    {
                        // Navigate to ContactDetail by passing Id.
                        NavigationService.Navigate(typeof(ContactDetailPage), contactId);
                    }
                }
            }

            #endregion

            #region OnActivated

            // Handle OnActivated Event
            else if (startKind == StartKind.Activate)
            {
                var e = args as IActivatedEventArgs;

                // Check if this is Voice activation.
                if (e.Kind == ActivationKind.VoiceCommand)
                {
                    if (e.GetType().Equals(typeof(VoiceCommandActivatedEventArgs)))
                    {
                        var commandArgs = e as VoiceCommandActivatedEventArgs;
                        SpeechRecognitionResult speechRecognitionResult = commandArgs.Result;

                        // If so, get the name of the voice command, the actual text spoken, and the value of Command/Navigate@Target.
                        string voiceCommandName = speechRecognitionResult.RulePath[0];
                        //string textSpoken = speechRecognitionResult.Text;
                        string navigationTarget = speechRecognitionResult.SemanticInterpretation.Properties["NavigationTarget"][0];

                        switch (voiceCommandName)
                        {
                            case "Find":
                            case "Checkin":
                                string searchCriteria = speechRecognitionResult.SemanticInterpretation.Properties["searchCriteria"][0];
                                _helper.VoiceCommandName = voiceCommandName;
                                NavigationService.Navigate(typeof(MainPage), searchCriteria);
                                break;
                            default:
                                break;
                        }

                    }
                }
            }

            #endregion
            
            if (NavigationService.CurrentPageType == null)
            {                
                if (string.IsNullOrEmpty(_settings.OAuthUrl))
                    // navigate to first page
                    NavigationService.Navigate(typeof(SignInPage));
                else
                    // navigate to first page
                    NavigationService.Navigate(typeof(MainPage));
            }            
        }
    }
}

