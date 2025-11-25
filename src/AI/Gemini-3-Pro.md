# Google Gemini 3 Pro
- **Link to Conversation:** https://gemini.google.com/share/b64e06791fdc

## Code Snippets from Gemini Conversation

### `Program.cs`

```cs
// Program.cs
using Microsoft.Windows.AppLifecycle;
using Microsoft.Windows.AppNotifications;
using System.Diagnostics;
using System.Web; // For HttpUtility

namespace MyApp;

internal class Program
{
    [STAThread]
    static async Task Main(string[] args)
    {
        WinRT.ComWrappersSupport.InitializeComWrappers();

        // 1. Generate a unique key for THIS process instance.
        // typically you might only do this if launching normally, 
        // but for simplicity, let's assign a Guid to every running instance.
        string myInstanceKey = Guid.NewGuid().ToString();
        
        // Register this instance so it can be found by others.
        var mainInstance = AppInstance.FindOrRegisterForKey(myInstanceKey);

        // 2. Check what triggered this launch.
        var activatedArgs = AppInstance.GetCurrent().GetActivatedEventArgs();
        var kind = activatedArgs.Kind;

        if (kind == ExtendedActivationKind.Protocol)
        {
            var protocolArgs = activatedArgs.Data as Windows.ApplicationModel.Activation.IProtocolActivatedEventArgs;
            if (protocolArgs != null)
            {
                // Parse the URI
                Uri uri = protocolArgs.Uri;
                var query = HttpUtility.ParseQueryString(uri.Query);
                
                // 3. Extract the STATE parameter
                string? targetInstanceKey = query["state"];

                // 4. If a state exists and it DOES NOT match my key, 
                // we are the "Dispatcher" instance. We need to find the real target.
                if (!string.IsNullOrEmpty(targetInstanceKey) && targetInstanceKey != myInstanceKey)
                {
                    try
                    {
                        // Find the instance that owns this key
                        var specificInstance = AppInstance.GetInstances()
                            .FirstOrDefault(i => i.Key == targetInstanceKey);

                        if (specificInstance != null)
                        {
                            // Redirect the arguments to that specific window
                            await specificInstance.RedirectActivationToAsync(activatedArgs);
                            
                            // Kill this "Dispatcher" process immediately
                            return; 
                        }
                    }
                    catch (Exception ex)
                    {
                        // Handle scenarios where the original instance might have closed 
                        // while the user was in the browser.
                        Debug.WriteLine($"Failed to redirect: {ex.Message}");
                    }
                }
            }
        }

        // 5. Normal Launch (or Protocol launch meant for THIS instance, or fallback)
        // Store the Key somewhere accessible (e.g. a static global or DI container) 
        // so the ViewModel can use it when building the OAuth Request URL.
        // MyGlobalConfig.CurrentInstanceKey = myInstanceKey; 

        Application.Start((p) =>
        {
            var context = new Microsoft.UI.Dispatching.DispatcherQueueSynchronizationContext(
                Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread());
            SynchronizationContext.SetSynchronizationContext(context);
            new App();
        });
    }
}
```


```cs
// 1. Generate a unique key for THIS process instance.
// Typically you might only do this if launching normally but, for simplicity, let's assign a Guid to every running instance.
string myInstanceKey = Guid.NewGuid().ToString();

// Register this instance so it can be found by others.
var mainInstance = AppInstance.FindOrRegisterForKey(myInstanceKey);

// 2. Check what triggered this launch.
var activatedArgs = AppInstance.GetCurrent().GetActivatedEventArgs();
var kind = activatedArgs.Kind;

if (kind == ExtendedActivationKind.Protocol)
{
    var protocolArgs = activatedArgs.Data as Windows.ApplicationModel.Activation.IProtocolActivatedEventArgs;
    if (protocolArgs != null)
    {
        // Parse the URI
        Uri uri = protocolArgs.Uri;
        var query = HttpUtility.ParseQueryString(uri.Query);
                
        // 3. Extract the STATE parameter
        string? targetInstanceKey = query["state"];

        // 4. If a state exists and it DOES NOT match my key, 
        // we are the "Dispatcher" instance. We need to find the real target.
        if (!string.IsNullOrEmpty(targetInstanceKey) && targetInstanceKey != myInstanceKey)
        {
            try
            {
                // Find the instance that owns this key
                var specificInstance = AppInstance.GetInstances()
                    .FirstOrDefault(i => i.Key == targetInstanceKey);

                if (specificInstance != null)
                {
                    // Redirect the arguments to that specific window
                    await specificInstance.RedirectActivationToAsync(activatedArgs);
                            
                    // Kill this "Dispatcher" process immediately
                    return; 
                }
            }
            catch (Exception ex)
            {
                // Handle scenarios where the original instance might have closed 
                // while the user was in the browser.
                Debug.WriteLine($"Failed to redirect: {ex.Message}");
            }
        }
    }
}

// 5. Normal Launch (or Protocol launch meant for THIS instance, or fallback)
// Store the Key somewhere accessible (e.g. a static global or DI container) 
// so the ViewModel can use it when building the OAuth Request URL.
// MyGlobalConfig.CurrentInstanceKey = myInstanceKey; 

Application.Start((p) =>
{
    var context = new Microsoft.UI.Dispatching.DispatcherQueueSynchronizationContext(
        Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread());
    SynchronizationContext.SetSynchronizationContext(context);
    new App();
});
```