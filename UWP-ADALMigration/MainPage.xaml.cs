using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace UWP_ADALMigration
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        //
        // The Client ID is used by the application to uniquely identify itself to Azure AD.
        // The Tenant is the name of the Azure AD tenant in which this application is registered.
        // The AAD Instance is the instance of Azure, for example public Azure or Azure China.
        // The Authority is the sign-in URL of the tenant.
        //
        private const string tenant = "";
        private const string clientId = "";
        private const string aadInstance = "https://login.microsoftonline.com/{0}";
        private static string authority = string.Format(CultureInfo.InvariantCulture, aadInstance, tenant);
        private AuthenticationContext authContext = null;
        private const string resourceId = "";

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await Login();
        }

        public MainPage()
        {
            this.InitializeComponent();
            authContext = new AuthenticationContext(authority);
        }

        private async Task Login()
        {
            //
            // Use ADAL to get an access token to call the To Do list service.
            //
            AuthenticationResult result = null;
            LoggerCallbackHandler.LogCallback = Log;
            LoggerCallbackHandler.PiiLoggingEnabled = true;
            LoggerCallbackHandler.UseDefaultLogging = true;

            try
            {
                var userCredential = new UserIdentifier("arslan.pervaiz@irisndt.com", UserIdentifierType.OptionalDisplayableId);

                result = await authContext.AcquireTokenAsync(resourceId, clientId, new Uri("urn:ietf:wg:oauth:2.0:oob"), new PlatformParameters(PromptBehavior.Auto, false), userCredential);
                var cachedInfo = authContext.TokenCache.ReadItems().FirstOrDefault(x => x.IdToken == result.IdToken);

                tblName.Text = $"Name: {cachedInfo.GivenName} {cachedInfo.FamilyName}";
                tblEmail.Text = $"Email: {cachedInfo.DisplayableId}";
            }
            catch (AdalException ex)
            {
                if (ex.ErrorCode == "authentication_canceled")
                {
                    // The user cancelled the sign-in, no need to display a message.
                }
                else
                {
                    MessageDialog dialog = new MessageDialog(string.Format("If the error continues, please contact your administrator.\n\nError Description:\n\n{0}", ex.Message), "Sorry, an error occurred while signing you in.");
                    await dialog.ShowAsync();
                }
            }
        }

        private static void Log(LogLevel level, string message, bool containsPii)
        {
            Debug.WriteLine($"ADAL Logs {level} {message}");
        }

        private void btnSignout_Click(object sender, RoutedEventArgs e)
        {
            // Clear session state from the token cache.
            authContext.TokenCache.Clear();
            tblName.Text = $"Name:";
            tblEmail.Text = $"Email:";
        }
    }
}