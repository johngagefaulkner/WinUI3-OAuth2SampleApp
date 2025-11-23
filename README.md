# WinUI3-OAuth2SampleApp

This is a complete WinUI 3 Desktop packaged application demonstrating OAuth2 authentication using the WindowsAppSDK v1.7.0-Preview1 OAuth2Manager API.

## Overview

This repository contains a fully functional WinUI 3 application that implements the OAuth2 authorization code flow as described in the `src/README.md` file. The application demonstrates:

- **OAuth2 Authorization Code Flow**: Complete implementation with authorization request and token exchange
- **Protocol Activation**: Custom URI scheme handling for OAuth2 callbacks
- **Modern WinUI 3 UI**: Clean, responsive user interface
- **Token Management**: Display and handling of access tokens and refresh tokens

## Project Structure

The solution contains two projects:

1. **WinUI3OAuth2SampleApp**: Main application project with UI and OAuth2 implementation
2. **WinUI3OAuth2SampleApp.Package**: Windows Application Packaging project for MSIX deployment

See [WinUI3OAuth2SampleApp/README.md](WinUI3OAuth2SampleApp/README.md) for detailed documentation, build instructions, and configuration guide.

## Quick Start

### Prerequisites

- Windows 10 version 19041 or later (Windows 11 recommended)
- Visual Studio 2022 (17.8 or later) with:
  - .NET Desktop Development workload
  - Windows App SDK C# Templates
  - Universal Windows Platform development workload
- .NET 9 SDK

### Building

1. Open `WinUI3OAuth2SampleApp.sln` in Visual Studio 2022
2. Set `WinUI3OAuth2SampleApp.Package` as the startup project
3. Select platform (x64 recommended)
4. Press F5 to build and run

### Configuration

Before connecting to a real OAuth2 provider, update the configuration in `WinUI3OAuth2SampleApp/Helpers/OAuth2Helper.cs`:

```csharp
private const string ClientId = "your_actual_client_id";
private const string ClientSecret = "your_actual_client_secret";
private const string RedirectUri = "your-app:/oauth-callback/";
```

Also update the protocol handler in `WinUI3OAuth2SampleApp.Package/Package.appxmanifest`.

## Implementation Details

The application implements the OAuth2 flow as described in `src/README.md`:

### Key Components

- **App.xaml.cs**: Handles protocol activation for OAuth2 callbacks
- **MainWindow.xaml**: UI for initiating authentication and displaying results
- **OAuth2Helper.cs**: Core OAuth2 implementation with:
  - Authorization request initiation
  - Token exchange
  - Error handling
  - Token refresh scheduling

### OAuth2 Flow

1. User clicks "Sign In with OAuth2"
2. App opens browser with authorization URL
3. User authenticates with OAuth2 provider
4. Provider redirects to custom URI scheme (e.g., `my-app:/oauth-callback/`)
5. Windows activates the app via protocol handler
6. App exchanges authorization code for access token
7. Tokens are displayed in the UI

## Documentation

For complete documentation including:
- Detailed build instructions
- Configuration for popular OAuth2 providers (GitHub, Google, Microsoft)
- Security considerations
- Troubleshooting guide

See [WinUI3OAuth2SampleApp/README.md](WinUI3OAuth2SampleApp/README.md)

## Original Sample Code

The OAuth2 implementation is based on the code samples in [src/README.md](src/README.md).

## License

See LICENSE file for details.
