using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace BlazorCognito;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);

        builder.Logging.SetMinimumLevel(LogLevel.Debug);

        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

        builder.Services.AddOidcAuthentication(options => {
            var cognitoDomain = "YOUR-COGNITO-POOL-NAME.auth.us-east-2.amazoncognito.com";
            var cognitoUserPoolId = "us-east-2_YOUR-COGNITO-POOL-ID";
            var cognitoAppClientId = "YOUR-COGNITO-APP-CLIENT-ID";

            // Basic configuration
            options.ProviderOptions.Authority = $"https://cognito-idp.us-east-2.amazonaws.com/{cognitoUserPoolId}";
            options.ProviderOptions.MetadataUrl = $"https://cognito-idp.us-east-2.amazonaws.com/{cognitoUserPoolId}/.well-known/openid-configuration";
            options.ProviderOptions.ClientId = cognitoAppClientId;
            options.ProviderOptions.ResponseType = "code";

            // Configure scopes
            options.ProviderOptions.DefaultScopes.Clear();
            options.ProviderOptions.DefaultScopes.Add("openid");
            options.ProviderOptions.DefaultScopes.Add("email");
            // options.ProviderOptions.DefaultScopes.Add("profile");  // FYI: Adding the profile scope leads to failure for reasons unknown

            // Configure callbacks
            options.ProviderOptions.RedirectUri = "https://localhost:7115/authentication/login-callback";

            // Map Cognito claims to standard claims
            options.UserOptions.NameClaim = "email";  // Use email as the Name claim
        });

        await builder.Build().RunAsync();
    }
}

/* NOTES:
1. Key findings from multiple trys and fails re logout problems:
   - Use the correct hosted UI path `/logout` (not `/oauth2/logout`)
   - Use `logout_uri` (not `redirect_uri`)
   - Use the `<LogOut>` template in Authentication.razor (instead of `<LogOutSucceeded>`)
   - Use the Cognito domain URL format `https://{cognitoDomain}/logout`
   - Remove OIDC middleware's interference with logout by not setting certain options

2. The working final solution used:
   - Cognito hosted UI domain for logout
   - Simple parameter structure with just `client_id` and `logout_uri`
   - Direct navigation rather than relying on the OIDC middleware's logout flow
 */