# Game Launcher

This project is a game launcher developed in C# using WPF (Windows Presentation Foundation). The launcher is designed to check for updates and download the latest version of the game before launching it.

## Features

- Checks for updates by comparing the current version with the online version.
- Downloads the game update if available.
- Displays the download progress with an estimated time remaining.
- Extracts the downloaded update and replaces the existing game files.
- Launches the game after the update is complete.

## Getting Started

To use the game launcher, follow these steps:

1. Clone the repository or download the source code.
2. Open the solution in Visual Studio or your preferred C# development environment.
3. Build the solution to restore NuGet packages and compile the project.
4. Run the launcher application.

## Usage

When you run the launcher application, it performs the following tasks:

1. Checks for updates by comparing the current version with the online version specified in the configuration.
2. If an update is available and not forced, asks the user if they want to update the game.
3. Downloads the update and displays the download progress.
4. Extracts the downloaded update and replaces the existing game files.
5. Launches the game application.

## Configuration

The launcher can be configured using a `config.txt` file. You have two options:

### Option 1: Embed the configuration into the application

If you want to embed the configuration into the launcher executable, set the `embedConfig` variable to `true` in the `MainWindow.xaml.cs` file. Then, define the configuration values in the dictionary inside the `RedlabsConfigInfo` constructor.

### Option 2: Use an external configuration file

If you prefer to use an external configuration file, create a `config.txt` file in the root directory of the launcher application. The file should contain the following configuration values:

- `APPLICATION_NAME`: The name of the game application.
- `DOWNLOAD_URL`: The URL for downloading the game update.
- `VERSION_URL`: The URL for checking the online version of the game.
- `DOWNLOAD_VERSION_FILE`: Set to `true` if the version URL leads to a download link instead of a direct file.
- `FILE_TO_RUN`: The name of the game executable file.
- `FORCE_UPDATE`: Set to `true` if you want to force the user to update instead of asking.

## Dependencies

This project uses the following external libraries:

- Xceed.Wpf.AvalonDock: A docking library for WPF.

## License

This project is licensed under the [MIT License](LICENSE).

