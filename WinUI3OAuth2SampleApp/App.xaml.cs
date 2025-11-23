using Microsoft.UI.Xaml;
using Microsoft.Security.Authentication.OAuth;

namespace WinUI3OAuth2SampleApp;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// 
/// NOTE: The Microsoft.Security.Authentication.OAuth namespace is from WindowsAppSDK v1.7.0-Preview1.
/// Verify this namespace is available in your installed SDK version.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Initializes the singleton application object. This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        this.InitializeComponent();
    }

    /// <summary>
    /// Invoked when the application is launched normally by the end user.
    /// </summary>
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        m_window = new MainWindow();
        m_window.Activate();
    }

    /// <summary>
    /// Invoked when the application is activated through a protocol (custom URI scheme).
    /// This handles OAuth2 callback redirects.
    /// </summary>
    protected override void OnActivated(IActivatedEventArgs args)
    {
        base.OnActivated(args);

        if (args.Kind == ActivationKind.Protocol)
        {
            var protocolArgs = args as ProtocolActivatedEventArgs;
            if (protocolArgs != null && OAuth2Manager.CompleteAuthRequest(protocolArgs.Uri))
            {
                // OAuth request completed successfully
                // The window that initiated the request will receive the response
                TerminateCurrentProcess();
            }
            else
            {
                // Unhandled protocol activation
                DisplayUnhandledMessageToUser();
            }
        }
    }

    /// <summary>
    /// Terminates the current process after OAuth callback is processed
    /// </summary>
    private void TerminateCurrentProcess()
    {
        // This instance was launched just to handle the callback
        // Close it gracefully
        System.Environment.Exit(0);
    }

    /// <summary>
    /// Displays a message when an unhandled protocol activation occurs
    /// </summary>
    private void DisplayUnhandledMessageToUser()
    {
        // Create a window to show the error if needed
        if (m_window == null)
        {
            m_window = new MainWindow();
            m_window.Activate();
        }
        // In a real app, you might want to show a dialog or notification
    }

    private Window? m_window;
}
