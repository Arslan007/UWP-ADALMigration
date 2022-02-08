using Microsoft.Identity.Client;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
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
        private const string redirectUI = "urn:ietf:wg:oauth:2.0:oob";
        private string[] scopes = new string[] { "user.read" };
        private IPublicClientApplication PublicClientApp;
        private AuthenticationResult authResult;

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await Login();
        }

        public MainPage()
        {
            this.InitializeComponent();
        }

        private async Task Login()
        {
            // Initialize the MSAL library by building a public client application
            PublicClientApp = PublicClientApplicationBuilder.Create(clientId)
                .WithAuthority(authority)
                .WithUseCorporateNetwork(false)
                .WithRedirectUri(redirectUI)
                 .WithLogging((level, message, containsPii) =>
                 {
                     Debug.WriteLine($"MSAL: {level} {message} ");
                 }, LogLevel.Warning, enablePiiLogging: false, enableDefaultPlatformLogging: true)
                .Build();

            var accounts = await PublicClientApp.GetAccountsAsync();
            var account = accounts.FirstOrDefault();

            try
            {
                authResult = await PublicClientApp.AcquireTokenSilent(scopes, account)
                   .ExecuteAsync();

                if (authResult != null)
                {
                    account = authResult.Account;
                }
            }
            catch (MsalUiRequiredException ex)
            {
                // A MsalUiRequiredException happened on AcquireTokenSilentAsync. This indicates you need to call AcquireTokenAsync to acquire a token
                Debug.WriteLine($"MsalUiRequiredException: {ex.Message}");

                authResult = await PublicClientApp.AcquireTokenInteractive(scopes)
                    .WithAccount(account)
                    .WithLoginHint("arslan.pervaiz@irisnde.com")
                    .WithPrompt(Prompt.NoPrompt)
                    .ExecuteAsync();
            }

            tblName.Text = $"Name: {authResult.ClaimsPrincipal.Claims.FirstOrDefault(i => i.Type == "name")?.Value}";
            tblEmail.Text = $"Email: {account.Username}";
        }

        private async void btnSignout_Click(object sender, RoutedEventArgs e)
        {
            var accounts = await PublicClientApp.GetAccountsAsync();
            var account = accounts.FirstOrDefault();
            await PublicClientApp.RemoveAsync(account);
            tblName.Text = $"Name:";
            tblEmail.Text = $"Email:";
        }
    }
}