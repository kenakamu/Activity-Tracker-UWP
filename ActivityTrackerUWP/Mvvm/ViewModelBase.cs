using ActivityTrackerUWP.Helpers;
using ActivityTrackerUWP.Services.CrmServices;
using ActivityTrackerUWP.Services.SettingsServices;
using System;
using System.Threading.Tasks;

namespace ActivityTrackerUWP.Mvvm
{
    // DOCS: https://github.com/Windows-XAML/Template10/wiki/Docs-%7C-MVVM
    public abstract class ViewModelBase : Template10.Mvvm.ViewModelBase
    {       
        // Adding common services and helpers
        public ISettingsService _settings;
        public ActivityTrackerHelper _helper;
        public ICrmService crmservice;

        public ViewModelBase()
        {
            _settings = SettingsService.Instance;
            _helper = ActivityTrackerHelper.Instance;

            if (_settings.CrmVersion != null)
            {
                if (_settings.CrmVersion.Major == 8)
                    crmservice = new CrmWebAPIService();
                else
                    crmservice = new CrmSoapService();
            }
        }

        /// <summary>
        /// Go to About Page 
        /// </summary>
        public void GotoAbout()
        {
            NavigationService.Navigate(typeof(Views.SettingsPage), 1);
        }

        /// <summary>
        /// Generate Text from Voice
        /// </summary>
        /// <returns>Generated text</returns>
        public async Task<string> VoiceToText()
        {
            // Create an instance of SpeechRecognizer.
            var speechRecognizer = new Windows.Media.SpeechRecognition.SpeechRecognizer();

            // Compile the dictation grammar by default.
            await speechRecognizer.CompileConstraintsAsync();

            // Start recognition.
            Windows.Media.SpeechRecognition.SpeechRecognitionResult speechRecognitionResult = await speechRecognizer.RecognizeWithUIAsync();

            return speechRecognitionResult.Text;
        }
    }
}
