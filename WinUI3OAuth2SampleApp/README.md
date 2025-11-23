# WinUI3 OAuth2 Sample Application

A complete WinUI 3 Desktop packaged application demonstrating OAuth2 authentication using the WindowsAppSDK v1.7.0-Preview1 OAuth2Manager API.

## Project Structure

```
WinUI3OAuth2SampleApp/
├── WinUI3OAuth2SampleApp/          # Main application project
│   ├── App.xaml                     # Application XAML definition
│   ├── App.xaml.cs                  # Application code-behind with protocol activation handling
│   ├── MainWindow.xaml              # Main window UI definition
│   ├── MainWindow.xaml.cs           # Main window code-behind
│   ├── Helpers/
│   │   └── OAuth2Helper.cs          # OAuth2 authentication implementation
│   ├── app.manifest                 # Application manifest for Windows settings
│   └── WinUI3OAuth2SampleApp.csproj # Project file
├── WinUI3OAuth2SampleApp.Package/   # Packaging project
│   ├── Package.appxmanifest         # Package manifest with protocol activation
│   ├── Images/                      # App icons and images
│   └── WinUI3OAuth2SampleApp.Package.wapproj
└── WinUI3OAuth2SampleApp.sln        # Solution file
```

## Features

This application demonstrates:

1. **OAuth2 Authorization Code Flow**: Complete implementation of the OAuth2 authorization code grant type
2. **Protocol Activation**: Handling of custom URI schemes for OAuth2 callbacks (`my-app://oauth-callback/`)
3. **Token Exchange**: Exchange of authorization code for access and refresh tokens
4. **Token Management**: Display and handling of access tokens, token types, and refresh tokens
5. **Modern WinUI 3 UI**: Clean, responsive user interface using WinUI 3 controls

## Prerequisites

- Windows 10 version 19041 (May 2020 Update) or later
- Windows 11 recommended for best experience
- Visual Studio 2022 (17.8 or later) with:
  - .NET Desktop Development workload
  - Windows App SDK C# Templates
  - Universal Windows Platform development workload
- .NET 9 SDK

## Building the Application

### Using Visual Studio

1. Open `WinUI3OAuth2SampleApp.sln` in Visual Studio 2022
2. Select the `WinUI3OAuth2SampleApp.Package` project as the startup project
3. Choose your platform (x64 recommended)
4. Press F5 to build and run

### Using Command Line

```bash
# Restore dependencies
dotnet restore WinUI3OAuth2SampleApp.sln

# Build the application (x64 platform)
dotnet build WinUI3OAuth2SampleApp.sln -c Release /p:Platform=x64

# Note: For packaged deployment, use Visual Studio or MSBuild with the .wapproj project
```

## Configuration

Before using the application with a real OAuth2 provider, you need to configure the OAuth2 endpoints and credentials:

1. Open `WinUI3OAuth2SampleApp/Helpers/OAuth2Helper.cs`
2. Update the following constants:

```csharp
private const string ClientId = "your_actual_client_id";
private const string ClientSecret = "your_actual_client_secret";
private const string RedirectUri = "your-app:/oauth-callback/";
private const string Scope = "your required scopes";
private const string AuthorizationEndpoint = "https://your.oauth.provider/authorize";
private const string TokenEndpoint = "https://your.oauth.provider/token";
```

3. Update `WinUI3OAuth2SampleApp.Package/Package.appxmanifest` to match your redirect URI:

```xml
<uap:Extension Category="windows.protocol">
  <uap:Protocol Name="your-app">
    <uap:DisplayName>Your App Name</uap:DisplayName>
  </uap:Protocol>
</uap:Extension>
```

## How It Works

### 1. Authorization Request

When you click "Sign In with OAuth2":
- The app creates an `AuthRequestParams` object with your client ID and redirect URI
- Calls `OAuth2Manager.RequestAuthWithParamsAsync()` which opens a browser
- User authenticates with the OAuth2 provider
- Provider redirects back to `my-app:/oauth-callback/` with an authorization code

### 2. Protocol Activation

When the callback URL is invoked:
- Windows activates the app via the protocol handler
- `App.OnActivated()` method receives the callback URI
- `OAuth2Manager.CompleteAuthRequest()` completes the authorization flow
- The authorization response is delivered to the waiting request

### 3. Token Exchange

After receiving the authorization code:
- Creates a `TokenRequestParams` from the auth response
- Creates `ClientAuthentication` with your client credentials
- Calls `OAuth2Manager.RequestTokenAsync()` to exchange the code for tokens
- Receives access token, token type, and optionally a refresh token

### 4. Using the Token

Once tokens are obtained:
- Access token is displayed in the UI
- In a real app, you would use the access token to make authenticated API requests
- Refresh token can be used to obtain new access tokens when the current one expires

## Code Implementation

The implementation is based on the sample code from the `src/README.md` and includes:

### App.xaml.cs
- `OnActivated()`: Handles protocol activation for OAuth2 callbacks
- Integrates with `OAuth2Manager.CompleteAuthRequest()`

### OAuth2Helper.cs
- `PerformAuthorizationRequestAsync()`: Initiates the OAuth2 flow
- `DoTokenExchangeAsync()`: Exchanges authorization code for tokens
- `ScheduleRefreshAt()`: Placeholder for token refresh logic
- `DoRequestWithToken()`: Placeholder for making authenticated API requests
- `NotifyFailure()`: Error handling and user notification

### MainWindow.xaml
- Clean, modern UI showing:
  - OAuth2 configuration details
  - Sign-in button
  - Status messages
  - Token information display
  - Usage instructions

## Testing with Sample Providers

To test the application, you can use popular OAuth2 providers:

### GitHub
```csharp
ClientId = "your_github_client_id"
RedirectUri = "your-app:/oauth-callback/"
Scope = "user:email"
AuthorizationEndpoint = "https://github.com/login/oauth/authorize"
TokenEndpoint = "https://github.com/login/oauth/access_token"
```

### Google
```csharp
ClientId = "your_google_client_id"
RedirectUri = "your-app:/oauth-callback/"
Scope = "openid email profile"
AuthorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth"
TokenEndpoint = "https://oauth2.googleapis.com/token"
```

### Microsoft
```csharp
ClientId = "your_microsoft_client_id"
RedirectUri = "your-app:/oauth-callback/"
Scope = "User.Read"
AuthorizationEndpoint = "https://login.microsoftonline.com/common/oauth2/v2.0/authorize"
TokenEndpoint = "https://login.microsoftonline.com/common/oauth2/v2.0/token"
```

## Security Considerations

⚠️ **Important Security Notes:**

1. **Never commit client secrets**: The client secret in the code is a placeholder. For production:
   - Use Windows Credential Manager (PasswordVault API)
   - Use Azure Key Vault for cloud-based apps
   - Use encrypted configuration files
   - Use environment variables (server-side only)
   - Use `dotnet user-secrets` during development
   
2. **Token Storage**: Implement secure token storage:
   ```csharp
   // Example using Windows Credential Manager
   var vault = new Windows.Security.Credentials.PasswordVault();
   vault.Add(new Windows.Security.Credentials.PasswordCredential(
       "MyApp_OAuth", "AccessToken", accessToken));
   ```

3. **HTTPS Only**: Ensure all OAuth2 endpoints use HTTPS
4. **Validate Responses**: Always validate responses from the OAuth2 provider
5. **Token Expiration**: Implement proper token refresh logic before tokens expire
6. **Scope Minimization**: Request only the scopes your application needs
7. **State Parameter**: Use PKCE or state parameters to prevent CSRF attacks (available in OAuth2Manager)

## Important Notes

### OAuth2Manager Namespace

The OAuth2Manager API uses the `Microsoft.Security.Authentication.OAuth` namespace as documented for WindowsAppSDK v1.7.0-Preview1. However, this is a preview API and the namespace or availability may change between preview builds.

**If you encounter namespace errors:**
1. Verify WindowsAppSDK 1.7.0-preview1 or later is installed
2. Check the official WindowsAppSDK documentation for the correct namespace in your version
3. The OAuth2Manager feature may be in a different preview channel or require specific Windows builds
4. Contact Windows App SDK support or check GitHub issues if the API is not available

This sample is based on the documented API structure from the preview announcement. You may need to adjust imports and API calls based on the actual released version.

## Troubleshooting

### Protocol Activation Not Working
- Ensure the Package.appxmanifest protocol name matches your redirect URI scheme
- Verify the app is properly installed/deployed
- Check Windows Event Viewer for activation errors

### Build Errors
- Ensure you have Windows App SDK 1.7.0-preview1 or later
- Verify .NET 9 SDK is installed
- Check that the Windows SDK 10.0.22621.0 is installed

### OAuth2 Errors
- Verify your client ID and secret are correct
- Check that the redirect URI matches what's registered with the provider
- Ensure the requested scopes are valid for your OAuth2 provider

## References

- [Windows App SDK OAuth2Manager Documentation](https://learn.microsoft.com/windows/windows-app-sdk/)
- [OAuth 2.0 Specification](https://oauth.net/2/)
- [WinUI 3 Documentation](https://learn.microsoft.com/windows/apps/winui/winui3/)

## License

See LICENSE file in the repository root.

## Contributing

This is a sample application. Feel free to use it as a starting point for your own projects.
