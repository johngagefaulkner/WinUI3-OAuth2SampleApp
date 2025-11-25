# Grok 4.1 Beta
- **Link to Conversation:** https://grok.com/share/bGVnYWN5LWNvcHk_90887dfa-0a2d-4e7a-9f13-5c13a2584204

---

## Code Snippet

```cs
using Microsoft.Windows.AppLifecycle;
using Microsoft.Windows.Security.Authentication.Web.Core; // For OAuth2Manager
using Microsoft.UI.Xaml; // For DispatcherQueue if needed
using System.ComponentModel;
using System.Net; // For Uri parsing helpers
using System.Runtime.CompilerServices; // For CallerMemberName if expanding

namespace YourApp.Services;

public sealed partial class OAuthService : INotifyPropertyChanged
{
    private readonly OAuth2Manager _oauthManager = new();
    private readonly Uri _redirectUri = new("my-app:///auth/redirect");
    private string? _currentState; // Track per-flow state
    private AppInstance? _thisInstance;

    public event PropertyChangedEventHandler? PropertyChanged;

    // Call this from your main window's Loaded event or login button
    public async Task<string?> InitiateAuthAsync(string clientId, Uri authUri, string scope)
    {
        // Step 1: Generate unique state & claim AppInstance key
        _currentState = Guid.NewGuid().ToString("N")[..16]; // Compact alphanumeric, ~64-bit unique
        _thisInstance = AppInstance.FindOrRegisterForKey(_currentState);
        if (!_thisInstance.IsCurrent)
        {
            // Collision? Regenerate (rare, but defensive)
            return await InitiateAuthAsync(clientId, authUri, scope);
        }

        // Step 2: Params with custom redirect + state
        var params = new AuthRequestParams
        {
            ClientId = clientId,
            Authority = authUri, // e.g., https://login.microsoftonline.com/{tenant}/v2.0
            RedirectUri = _redirectUri,
            Scope = scope,
            State = _currentState // Ties back to our key
        };

        // Launch browser flow (PKCE auto-handled)
        await _oauthManager.StartAuthRequestAsync(params);

        // Step 3: Wait for completion (or use TaskCompletionSource for async polish)
        return await WaitForAuthCompletionAsync();
    }

    private async Task<string?> WaitForAuthCompletionAsync()
    {
        // Hook global activation listener (do this once per app in App.xaml.cs, pass args here)
        // For simplicity, assume you wire AppInstance.Activated globally and route to this method
        var tcs = new TaskCompletionSource<string?>();
        
        // In your global handler (see below), this will resolve with access token
        // Placeholder: Resolve on success
        return await tcs.Task;
    }

    // Called from global AppInstance.Activated handler
    public async Task HandleProtocolActivationAsync(AppActivationArguments args)
    {
        if (args.Kind != ExtendedActivationKind.Protocol) return;

        var protocolArgs = args.Data as ProtocolActivatedEventArgs;
        var uri = protocolArgs?.Uri ?? throw new InvalidOperationException("No URI in activation");
        
        // Parse query for state/code (C# 14's improved string interpolation helps here)
        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
        var incomingState = query["state"];
        var code = query["code"];

        if (string.IsNullOrEmpty(incomingState) || string.IsNullOrEmpty(code)) return;

        // Route to owning instance via key lookup
        var owner = AppInstance.FindOrRegisterForKey(incomingState);
        if (!owner.IsCurrent)
        {
            // Redirect non-terminally—source keeps running
            await owner.RedirectActivationToAsync(args);
            return;
        }

        // This is the owner: Complete the flow
        var authResult = await _oauthManager.CompleteAuthRequestAsync(uri);
        if (authResult.Status == AuthRequestCompletionStatus.Success)
        {
            var token = authResult.AccessToken; // Or extract as needed
            // Update UI via DispatcherQueue (best practice for WinUI)
            await Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread().EnqueueAsync(() =>
            {
                // e.g., YourMainWindowViewModel.SetAccessToken(token);
                OnPropertyChanged(nameof(AccessToken)); // If exposing
            });
            UnregisterKey(); // Cleanup
        }
        else
        {
            // Handle error (e.g., user cancel: Status == Aborted)
            HandleAuthError(authResult.Status);
        }
    }

    private void UnregisterKey()
    {
        _thisInstance?.UnregisterKey();
        _currentState = null;
    }

    private void HandleAuthError(AuthRequestCompletionStatus status) { /* Log/toast */ }

    // INotifyPropertyChanged boilerplate (C# 14 partial props)
    partial void OnAccessTokenChanged(string? value); // Implement UI binding if needed
}

// Global wiring in App.xaml.cs (override OnLaunched)
protected override void OnLaunched(LaunchActivatedEventArgs args)
{
    var activationArgs = AppInstance.GetActivatedEventArgs();
    if (activationArgs.Kind == ExtendedActivationKind.Protocol)
    {
        // Inject your service instance (e.g., via DI or singleton)
        _oauthService.HandleProtocolActivationAsync(activationArgs);
    }

    // Rest of your app init...
    m_window = new MainWindow();
    m_window.Activate();
}

// Subscribe once in App ctor:
public App()
{
    AppInstance.GetActivatedEventArgs().AddEventHandler(OnAppActivated); // Or use the event
}

private void OnAppActivated(object? sender, AppActivationEventArgs e)
{
    _oauthService.HandleProtocolActivationAsync(e.ActivationArguments);
}
```