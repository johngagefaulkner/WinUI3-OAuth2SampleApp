# Microsoft Copilot
- **Link to conversation:** https://copilot.microsoft.com/chats/jaZNkfZLSsPtjLsYZYnHE

## Code Snippets from Copilot Conversation

### `OAuth2Helper.cs`

```cs
using Microsoft.Windows.AppLifecycle;
using System;

public static class InstanceManager
{
    public static AppInstance RegisterInstance()
    {
        // Generate a unique key for this instance (GUID works well)
        string instanceKey = Guid.NewGuid().ToString();

        // Register this instance with the key
        AppInstance instance = AppInstance.FindOrRegisterInstanceForKey(instanceKey);

        // Store the key for later use (e.g., OAuth2 state parameter)
        CurrentInstanceKey = instanceKey;

        return instance;
    }

    public static string CurrentInstanceKey { get; private set; }
}
```

### `BuildOAuth2RequestUri`

```cs
public static string BuildOAuth2RequestUri(string clientId, string redirectUri)
{
    string state = InstanceManager.CurrentInstanceKey;

    return $"https://authserver.com/authorize?" +
           $"client_id={clientId}&" +
           $"response_type=code&" +
           $"redirect_uri={Uri.EscapeDataString(redirectUri)}&" +
           $"state={state}";
}

// Usage example:
string authUri = BuildOAuth2RequestUri("my-client-id", "my-app:///auth/redirect");
System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
{
    FileName = authUri,
    UseShellExecute = true
});
```

### `App.xaml.cs` Protocol Activation Handling

```cs
protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
{
    var instance = InstanceManager.RegisterInstance();

    // Hook activation events
    instance.Activated += OnActivated;
}

private void OnActivated(object sender, AppActivationArguments e)
{
    if (e.Kind == ExtendedActivationKind.Protocol)
    {
        var protocolArgs = (ProtocolActivatedEventArgs)e.Data;
        Uri uri = protocolArgs.Uri;

        // Example: my-app:///auth/redirect?code=abc123&state=guid
        var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);
        string code = queryParams["code"];
        string state = queryParams["state"];

        // Route to correct instance
        if (state == InstanceManager.CurrentInstanceKey)
        {
            // Handle OAuth2 code for THIS instance
            HandleOAuth2Code(code);
        }
        else
        {
            // Forward to the correct AppInstance
            var targetInstance = AppInstance.FindOrRegisterInstanceForKey(state);
            targetInstance.RedirectActivationToAsync(e);
        }
    }
}

private void HandleOAuth2Code(string code)
{
    // Exchange the code for tokens, etc.
    // This is where you'd call OAuth2Manager API
}
```