using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinUI3OAuth2SampleApp.Helpers;

namespace WinUI3OAuth2SampleApp;

/// <summary>
/// Main application window that demonstrates OAuth2 authentication flow
/// </summary>
public sealed partial class MainWindow : Window
{
    private readonly OAuth2Helper _oauthHelper;

    public MainWindow()
    {
        this.InitializeComponent();
        _oauthHelper = new OAuth2Helper(this);
        
        Title = "WinUI3 OAuth2 Sample App";
    }

    /// <summary>
    /// Handles the sign-in button click to initiate OAuth2 flow
    /// </summary>
    private async void SignInButton_Click(object sender, RoutedEventArgs e)
    {
        SignInButton.IsEnabled = false;
        StatusText.Text = "Initiating OAuth2 authentication...";

        try
        {
            await _oauthHelper.PerformAuthorizationRequestAsync();
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Error: {ex.Message}";
        }
        finally
        {
            SignInButton.IsEnabled = true;
        }
    }

    /// <summary>
    /// Updates the status text in the UI
    /// </summary>
    public void UpdateStatus(string message)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            StatusText.Text = message;
        });
    }

    /// <summary>
    /// Updates the token display in the UI
    /// </summary>
    public void UpdateTokenDisplay(string accessToken, string tokenType, string? refreshToken)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            AccessTokenText.Text = accessToken;
            TokenTypeText.Text = tokenType;
            RefreshTokenText.Text = refreshToken ?? "N/A";
        });
    }
}
