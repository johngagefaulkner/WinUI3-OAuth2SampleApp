using Microsoft.Security.Authentication.OAuth;
using Microsoft.UI;
using System;
using System.Threading.Tasks;

namespace WinUI3OAuth2SampleApp.Helpers;

/// <summary>
/// Configuration class for OAuth2 settings
/// In production, load these values from secure storage (Credential Manager, Key Vault, etc.)
/// </summary>
public class OAuth2Configuration
{
    // ⚠️ SECURITY WARNING: These are placeholder values for demonstration only!
    // In production, load from secure storage such as:
    // - Windows.Security.Credentials.PasswordVault
    // - Azure Key Vault
    // - Encrypted app configuration
    // - Environment variables (server-side only)
    
    public string ClientId { get; set; } = "my_client_id";
    public string ClientSecret { get; set; } = "REPLACE_WITH_SECURE_RETRIEVAL"; // ⚠️ Load from secure storage!
    public string RedirectUri { get; set; } = "my-app:/oauth-callback/";
    public string Scope { get; set; } = "user:email user:birthday";
    public string AuthorizationEndpoint { get; set; } = "https://my.server.com/oauth/authorize";
    public string TokenEndpoint { get; set; } = "https://my.server.com/oauth/token";

    /// <summary>
    /// Example: Load configuration from Windows Credential Manager
    /// </summary>
    public static OAuth2Configuration LoadFromSecureStorage()
    {
        // Example implementation (uncomment in production):
        /*
        var vault = new Windows.Security.Credentials.PasswordVault();
        var config = new OAuth2Configuration();
        
        try
        {
            var credential = vault.Retrieve("MyApp_OAuth2", "ClientSecret");
            config.ClientSecret = credential.Password;
        }
        catch (Exception)
        {
            // Handle missing credentials
            throw new InvalidOperationException("OAuth2 credentials not configured. Please run setup.");
        }
        
        return config;
        */
        
        // For this demo, return default (insecure) configuration
        return new OAuth2Configuration();
    }
}

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
    private readonly OAuth2Configuration _config;

    public OAuth2Helper(MainWindow mainWindow)
    {
        _mainWindow = mainWindow;
        
        // In production: _config = OAuth2Configuration.LoadFromSecureStorage();
        _config = new OAuth2Configuration();
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
                _config.ClientId,
                new Uri(_config.RedirectUri));
            authRequestParams.Scope = _config.Scope;

            _mainWindow.UpdateStatus("Opening browser for authentication...");

            // Request authorization from the OAuth2 provider
            var authRequestResult = await OAuth2Manager.RequestAuthWithParamsAsync(
                parentWindowId,
                new Uri(_config.AuthorizationEndpoint),
                authRequestParams);

            await HandleOAuth2ResultAsync(
                authRequestResult.Response,
                authRequestResult.Failure,
                async response =>
                {
                    _mainWindow.UpdateStatus("Authorization successful. Exchanging code for token...");
                    await DoTokenExchangeAsync(response);
                },
                "Authorization");
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
            var clientAuth = ClientAuthentication.CreateForBasicAuthorization(_config.ClientId, _config.ClientSecret);

            _mainWindow.UpdateStatus("Requesting access token...");

            // Request token from the OAuth2 provider
            var tokenRequestResult = await OAuth2Manager.RequestTokenAsync(
                new Uri(_config.TokenEndpoint),
                tokenRequestParams,
                clientAuth);

            HandleOAuth2Result(
                tokenRequestResult.Response,
                tokenRequestResult.Failure,
                response => HandleTokenResponse(response),
                "Token");
        }
        catch (Exception ex)
        {
            NotifyFailure("Exception", $"An error occurred during token exchange: {ex.Message}");
        }
    }

    /// <summary>
    /// Processes a successful token response
    /// </summary>
    private void HandleTokenResponse(TokenResponse response)
    {
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

    /// <summary>
    /// Extracts error information from a failure object using its Error and ErrorDescription properties.
    /// </summary>
    private static (string Error, string? ErrorDescription) ExtractErrorInfo<TFailure>(TFailure failure)
        where TFailure : class
    {
        // The OAuth2 SDK failure types have Error and ErrorDescription properties
        var errorProp = typeof(TFailure).GetProperty("Error");
        var errorDescProp = typeof(TFailure).GetProperty("ErrorDescription");
        
        var error = errorProp?.GetValue(failure)?.ToString() ?? "Unknown";
        var errorDescription = errorDescProp?.GetValue(failure)?.ToString();
        
        return (error, errorDescription);
    }

    /// <summary>
    /// Handles OAuth2 result by processing success or failure cases with a common pattern.
    /// Reduces code duplication between authorization and token exchange flows.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response object</typeparam>
    /// <typeparam name="TFailure">The type of the failure object</typeparam>
    /// <param name="response">The response from the OAuth2 operation, or null if failed</param>
    /// <param name="failure">The failure details from the OAuth2 operation, or null if succeeded</param>
    /// <param name="onSuccess">Action to execute when response is available</param>
    /// <param name="operationName">Name of the operation for error messages (e.g., "Authorization", "Token")</param>
    private void HandleOAuth2Result<TResponse, TFailure>(
        TResponse? response,
        TFailure? failure,
        Action<TResponse> onSuccess,
        string operationName)
        where TResponse : class
        where TFailure : class
    {
        if (response != null)
        {
            onSuccess(response);
        }
        else if (failure != null)
        {
            var (error, errorDescription) = ExtractErrorInfo(failure);
            NotifyFailure(error, errorDescription);
        }
        else
        {
            NotifyFailure("Unknown Error", $"{operationName} request returned no response or failure information.");
        }
    }

    /// <summary>
    /// Handles OAuth2 result asynchronously by processing success or failure cases with a common pattern.
    /// Reduces code duplication between authorization and token exchange flows.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response object</typeparam>
    /// <typeparam name="TFailure">The type of the failure object</typeparam>
    /// <param name="response">The response from the OAuth2 operation, or null if failed</param>
    /// <param name="failure">The failure details from the OAuth2 operation, or null if succeeded</param>
    /// <param name="onSuccess">Async function to execute when response is available</param>
    /// <param name="operationName">Name of the operation for error messages (e.g., "Authorization", "Token")</param>
    private async Task HandleOAuth2ResultAsync<TResponse, TFailure>(
        TResponse? response,
        TFailure? failure,
        Func<TResponse, Task> onSuccess,
        string operationName)
        where TResponse : class
        where TFailure : class
    {
        if (response != null)
        {
            await onSuccess(response);
        }
        else if (failure != null)
        {
            var (error, errorDescription) = ExtractErrorInfo(failure);
            NotifyFailure(error, errorDescription);
        }
        else
        {
            NotifyFailure("Unknown Error", $"{operationName} request returned no response or failure information.");
        }
    }
}
