# OAuth2Manager Implementation
> _**NOTE:** I'm temporarily putting this code here from my phone._

# Resources and References

- [Albert Akhmetov | Single Instance Apps in WinUI 3 with C#](https://albertakhmetov.com/posts/2025/single-instance-apps-in-winui-3-with-c%23/)
- 

---

## Authorization Code Request

```cs
using Microsoft.UI;
using Microsoft.Security.Authentication.OAuth;
using System;
using System.Threading.Tasks;

public class OAuth2Example
{
    public async Task PerformAuthorizationRequestAsync()
    {
        // Get the WindowId for the application window
        WindowId parentWindowId = this.AppWindow().Id();

        var authRequestParams = AuthRequestParams.CreateForAuthorizationCodeRequest("my_client_id",
            new Uri("my-app:/oauth-callback/"));
        authRequestParams.Scope = "user:email user:birthday";

        var authRequestResult = await OAuth2Manager.RequestAuthWithParamsAsync(parentWindowId, 
            new Uri("https://my.server.com/oauth/authorize"), authRequestParams);

        if (authRequestResult.Response != null)
        {
            // Obtain the authorization code
            // var authorizationCode = authRequestResult.Response.Code;

            // Obtain the access token
            await DoTokenExchangeAsync(authRequestResult.Response);
        }
        else
        {
            var authFailure = authRequestResult.Failure;
            NotifyFailure(authFailure.Error, authFailure.ErrorDescription);
        }
    }

    private async Task DoTokenExchangeAsync(AuthResponse authResponse)
    {
        var tokenRequestParams = TokenRequestParams.CreateForAuthorizationCodeRequest(authResponse);
        var clientAuth = ClientAuthentication.CreateForBasicAuthorization("my_client_id", "my_client_secret");

        var tokenRequestResult = await OAuth2Manager.RequestTokenAsync(new Uri("https://my.server.com/oauth/token"), tokenRequestParams, clientAuth);

        if (tokenRequestResult.Response != null)
        {
            var accessToken = tokenRequestResult.Response.AccessToken;
            var tokenType = tokenRequestResult.Response.TokenType;

            // RefreshToken string null/empty when not present
            var refreshToken = tokenRequestResult.Response.RefreshToken;
            if (!string.IsNullOrEmpty(refreshToken))
            {
                var expiresIn = tokenRequestResult.Response.ExpiresIn;
                var expires = DateTime.UtcNow.AddSeconds(expiresIn != 0 ? expiresIn : 3600);

                // Schedule a refresh of the access token
                ScheduleRefreshAt(expires, refreshToken);
            }

            // Use the access token for resources
            DoRequestWithToken(accessToken, tokenType);
        }
        else
        {
            var tokenFailure = tokenRequestResult.Failure;
            NotifyFailure(tokenFailure.Error, tokenFailure.ErrorDescription);
        }
    }

    private void ScheduleRefreshAt(DateTime expires, string refreshToken)
    {
        // Implement token refresh logic here
    }

    private void DoRequestWithToken(string accessToken, string tokenType)
    {
        // Implement resource request logic here
    }

    private void NotifyFailure(string error, string errorDescription)
    {
        // Implement failure notification logic here
    }
}
```

## Completing an Authorization Request

```cs
using Microsoft.UI.Xaml;
using Microsoft.Security.Authentication.OAuth;

public partial class App : Application
{
    protected override void OnActivated(IActivatedEventArgs args)
    {
        base.OnActivated(args);

        if (args.Kind == ActivationKind.Protocol)
        {
            var protocolArgs = args as ProtocolActivatedEventArgs;
            if (OAuth2Manager.CompleteAuthRequest(protocolArgs.Uri))
            {
                TerminateCurrentProcess();
            }

            DisplayUnhandledMessageToUser();
        }
    }

    private void TerminateCurrentProcess()
    {
        // Implement process termination logic here
    }

    private void DisplayUnhandledMessageToUser()
    {
        // Implement unhandled message display logic here
    }
}
```

This example demonstrates how to perform an authorization code request, exchange the authorization code for an access token, refresh the access token, and handle protocol activation in a WinUI 3 Desktop application (I'm using .NET 9 / C# 13.0). Note that some methods like `AppWindow().Id()` and `TerminateCurrentProcess()` need to be implemented according to your application's specifics.
