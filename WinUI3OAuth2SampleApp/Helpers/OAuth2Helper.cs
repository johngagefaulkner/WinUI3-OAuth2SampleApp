using Microsoft.Security.Authentication.OAuth;
using Microsoft.UI;
using System;
using System.Threading.Tasks;

namespace WinUI3OAuth2SampleApp.Helpers;

/// <summary>
/// Helper class that implements OAuth2 authentication flow using the WindowsAppSDK OAuth2Manager
/// Based on the sample code from src/README.md
/// 
/// IMPORTANT: Namespace Compatibility Note
/// ========================================
/// The namespace 'Microsoft.Security.Authentication.OAuth' is from WindowsAppSDK v1.7.0-Preview1.
/// 
/// If the namespace is not available in your SDK version:
/// 1. Verify you have WindowsAppSDK 1.7.0-preview1 or later installed
/// 2. Check NuGet package version in the .csproj file
/// 3. Consult the WindowsAppSDK release notes for namespace changes
/// 4. If the OAuth2Manager API is not available, consider alternatives:
///    - Microsoft.Identity.Client (MSAL) for Microsoft identity
///    - IdentityModel.OidcClient for general OAuth2/OIDC
///    - Custom implementation using HttpClient and browser integration
/// 
/// For stable production apps, consider waiting for OAuth2Manager in a stable release
/// or use the established alternatives mentioned above.
/// </summary>
public class OAuth2Helper
{
    private readonly MainWindow _mainWindow;

    // OAuth2 Configuration
    // ⚠️ SECURITY WARNING: Do not hardcode client secrets in production!
    // 
    // For production deployments, use secure configuration management:
    // - Windows Credential Manager (PasswordVault API)
    // - Azure Key Vault
    // - Secure configuration files with encryption
    // - Environment variables (for server-side scenarios)
    // - User secrets during development (dotnet user-secrets)
    //
    // TODO: Replace these with your actual OAuth2 provider's values
    private const string ClientId = "my_client_id";
    private const string ClientSecret = "my_client_secret";  // ⚠️ Use secure storage in production!
    private const string RedirectUri = "my-app:/oauth-callback/";
    private const string Scope = "user:email user:birthday";
    private const string AuthorizationEndpoint = "https://my.server.com/oauth/authorize";
    private const string TokenEndpoint = "https://my.server.com/oauth/token";

    public OAuth2Helper(MainWindow mainWindow)
    {
        _mainWindow = mainWindow;
    }

    /// <summary>
    /// Performs the OAuth2 authorization request flow
    /// </summary>
    public async Task PerformAuthorizationRequestAsync()
    {
        try
        {
            // Get the WindowId for the application window
            WindowId parentWindowId = _mainWindow.AppWindow.Id;

            // Create authorization request parameters
            var authRequestParams = AuthRequestParams.CreateForAuthorizationCodeRequest(
                ClientId,
                new Uri(RedirectUri));
            authRequestParams.Scope = Scope;

            _mainWindow.UpdateStatus("Opening browser for authentication...");

            // Request authorization from the OAuth2 provider
            var authRequestResult = await OAuth2Manager.RequestAuthWithParamsAsync(
                parentWindowId,
                new Uri(AuthorizationEndpoint),
                authRequestParams);

            if (authRequestResult.Response != null)
            {
                _mainWindow.UpdateStatus("Authorization successful. Exchanging code for token...");

                // Obtain the authorization code and exchange it for tokens
                await DoTokenExchangeAsync(authRequestResult.Response);
            }
            else if (authRequestResult.Failure != null)
            {
                var authFailure = authRequestResult.Failure;
                NotifyFailure(authFailure.Error, authFailure.ErrorDescription);
            }
            else
            {
                NotifyFailure("Unknown Error", "Authorization request returned no response or failure information.");
            }
        }
        catch (Exception ex)
        {
            NotifyFailure("Exception", $"An error occurred during authorization: {ex.Message}");
        }
    }

    /// <summary>
    /// Exchanges the authorization code for access and refresh tokens
    /// </summary>
    private async Task DoTokenExchangeAsync(AuthResponse authResponse)
    {
        try
        {
            // Create token request parameters from the auth response
            var tokenRequestParams = TokenRequestParams.CreateForAuthorizationCodeRequest(authResponse);
            
            // Create client authentication credentials
            var clientAuth = ClientAuthentication.CreateForBasicAuthorization(ClientId, ClientSecret);

            _mainWindow.UpdateStatus("Requesting access token...");

            // Request token from the OAuth2 provider
            var tokenRequestResult = await OAuth2Manager.RequestTokenAsync(
                new Uri(TokenEndpoint),
                tokenRequestParams,
                clientAuth);

            if (tokenRequestResult.Response != null)
            {
                var response = tokenRequestResult.Response;
                var accessToken = response.AccessToken;
                var tokenType = response.TokenType;
                var refreshToken = response.RefreshToken;

                _mainWindow.UpdateStatus("Authentication successful! Tokens received.");
                _mainWindow.UpdateTokenDisplay(accessToken, tokenType, refreshToken);

                // Handle refresh token if present
                if (!string.IsNullOrEmpty(refreshToken))
                {
                    var expiresIn = response.ExpiresIn;
                    var expires = DateTime.UtcNow.AddSeconds(expiresIn != 0 ? expiresIn : 3600);

                    // Schedule a refresh of the access token
                    ScheduleRefreshAt(expires, refreshToken);
                }

                // In a real application, you would use the access token to make API requests
                DoRequestWithToken(accessToken, tokenType);
            }
            else if (tokenRequestResult.Failure != null)
            {
                var tokenFailure = tokenRequestResult.Failure;
                NotifyFailure(tokenFailure.Error, tokenFailure.ErrorDescription);
            }
            else
            {
                NotifyFailure("Unknown Error", "Token request returned no response or failure information.");
            }
        }
        catch (Exception ex)
        {
            NotifyFailure("Exception", $"An error occurred during token exchange: {ex.Message}");
        }
    }

    /// <summary>
    /// Schedules a token refresh at the specified expiration time
    /// </summary>
    private void ScheduleRefreshAt(DateTime expires, string refreshToken)
    {
        // In a real application, you would implement token refresh logic here
        // This could involve:
        // 1. Setting up a timer to refresh before expiration
        // 2. Storing the refresh token securely
        // 3. Implementing the refresh token flow

        var timeUntilExpiration = expires - DateTime.UtcNow;
        _mainWindow.UpdateStatus($"Token will expire in {timeUntilExpiration.TotalMinutes:F1} minutes. Refresh token available.");
    }

    /// <summary>
    /// Makes a request using the obtained access token
    /// </summary>
    private void DoRequestWithToken(string accessToken, string tokenType)
    {
        // In a real application, you would use the access token to make authenticated API requests
        // Example:
        // using var client = new HttpClient();
        // client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(tokenType, accessToken);
        // var response = await client.GetAsync("https://api.example.com/user");

        // For this demo, we just log that we have the token
        System.Diagnostics.Debug.WriteLine($"Access token received: {tokenType} {accessToken}");
    }

    /// <summary>
    /// Notifies the user of an authentication failure
    /// </summary>
    private void NotifyFailure(string error, string? errorDescription)
    {
        var message = $"OAuth2 Error: {error}";
        if (!string.IsNullOrEmpty(errorDescription))
        {
            message += $"\nDescription: {errorDescription}";
        }

        _mainWindow.UpdateStatus(message);
        System.Diagnostics.Debug.WriteLine(message);
    }
}
