using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Xceed.Wpf.AvalonDock.Layout;
using MessageBox = System.Windows.MessageBox;



namespace RedlabsUpdateUtility
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		string rootPath;
		string configPath;
		string dataPath;
		string versionPath;
		string updateVersionPath;
		string zipPath;
		string applicationFolderPath;
		bool finishedPatch = false;

		Stopwatch downloadStopwatch;

		RedlabsConfigInfo configInfo;

		public MainWindow()
		{
			InitializeComponent();
		}

		private void Window_ContentRendered(object sender, EventArgs e)
		{
			rootPath = Directory.GetCurrentDirectory();
			dataPath = Path.Combine(rootPath, "Data");
			configPath = Path.Combine(rootPath, "config.txt");
			versionPath = Path.Combine(dataPath, "version.txt");
			updateVersionPath = Path.Combine(dataPath, "update_version.txt");
			zipPath = Path.Combine(dataPath, "Application.zip");
			applicationFolderPath = Path.Combine(dataPath, "Application");

			/* If you want to embed the config into the exe,
				* set "embedConfig" to true, and enter the config
				* values into the dictionary defined below */
			bool embedConfig = true;
			if(embedConfig)
			{
				configInfo = new RedlabsConfigInfo(new Dictionary<string, string>()
				{
					{ "APPLICATION_NAME", "DBOPS" }, // The name of the application
					{ "DOWNLOAD_URL", "https://drive.google.com/uc?export=download&id=1pY7gDHTtjerKSPz6I0xrHlRXiXSveJwW" }, // The online download URL for the application
					{ "VERSION_URL", "https://drive.google.com/uc?export=download&id=1k-vbFH8agxy1x-TV9gbKG71eeBJAr4vK" }, // The online version URL for the application
					{ "DOWNLOAD_VERSION_FILE", "true" }, // True if the version URL leads to a download link instead of a direct file
					{ "FILE_TO_RUN", "Client.exe" }, // The file to run when starting the application
					{ "FORCE_UPDATE", "false" } // True if you want to force the user to update instead of asking
				});
			}
			else
			{
				if(File.Exists(configPath))
				{
					configInfo = RedlabsConfig.Parse(File.ReadAllText(configPath));
				}
				else
				{
					OnError($"Missing config file at expected path \"{configPath}\" Unable to launch application.");
					Close();
					return;
				}
			}

			Title = $"{configInfo.values["APPLICATION_NAME"]} Launcher";
			TitleBlock.Text = "Checking for updates...";

			Directory.CreateDirectory(dataPath);

			string updateVersion = "";
			try
			{
				WebClient versionWebClient = new WebClient();
				if(bool.Parse(configInfo.values["DOWNLOAD_VERSION_FILE"]))
				{
					versionWebClient.DownloadFile(new Uri(configInfo.values["VERSION_URL"]), updateVersionPath);
					updateVersion = File.ReadAllText(updateVersionPath);
					File.Delete(updateVersionPath);
				}
				else
				{
					updateVersion = versionWebClient.DownloadString(configInfo.values["VERSION_URL"]);
				}
			}
			catch(WebException ex)
			{
				OnError($"Error when checking for update: {ex}");
				finishedPatch = true;
				Start.Opacity = 100;
				return;
			}

			string currentVersion = "";
			if(File.Exists(versionPath))
			{
				currentVersion = File.ReadAllText(versionPath);
			}

			if(currentVersion == updateVersion && !string.IsNullOrEmpty(currentVersion))
			{
				TitleBlock.Text = "No updates found.";
				finishedPatch = true;
				Start.Opacity = 100;
			}
			else
			{
				bool shouldUpdate = true;

				if(Directory.Exists(applicationFolderPath) && !bool.Parse(configInfo.values["FORCE_UPDATE"]))
				{
					MessageBoxButton buttons = MessageBoxButton.YesNo;
					MessageBoxResult result;
					result = MessageBox.Show($"An update was found. Update now?", "Update Found", buttons, MessageBoxImage.Question);

					shouldUpdate = result.Equals(MessageBoxResult.Yes);
				}

				if(shouldUpdate)
				{
					if(Directory.Exists(applicationFolderPath))
					{
						TitleBlock.Text = $"Downloading update v{updateVersion}...";
					}
					else
					{
						TitleBlock.Text = $"Downloading {configInfo.values["APPLICATION_NAME"]} v{updateVersion}...";
					}

					downloadStopwatch = new Stopwatch();
					downloadStopwatch.Start();

					FileDownloader fileDownloader = new FileDownloader();
					fileDownloader.DownloadProgressChanged += OnDownloadProgressChanged;
					fileDownloader.DownloadFileCompleted += OnDownloadCompleted;
					fileDownloader.DownloadFileAsync(configInfo.values["DOWNLOAD_URL"], zipPath, updateVersion);
					WindowIconTools.SetProgressState(TaskbarProgressBarState.Normal);
				}
				else
				{
					finishedPatch = true;
					Start.Opacity = 100;
				}
			}
		}

		void OnDownloadProgressChanged(object sender, FileDownloader.DownloadProgress e)
		{

			if(downloadStopwatch.Elapsed.TotalSeconds > 0.5)
			{
				if(e.BytesReceived >= e.TotalBytesToReceive && e.TotalBytesToReceive != 0)
				{
					TitleBlock.Text = "Game ready to start.";
				}
				else
				{
					double downloadedMB = e.BytesReceived / 1000000;
					double elapsedTime = downloadStopwatch.Elapsed.TotalSeconds;

					double downloadPerSecond = downloadedMB / elapsedTime;
					double downloadLeft = e.TotalBytesToReceive / 1000000 - downloadedMB;

					double secondsLeft = Math.Round(downloadLeft / downloadPerSecond);

					string downloadTimeLeftFormatted;

					if(secondsLeft >= 60)
					{
						double minutesLeft = Math.Round(secondsLeft / 60, MidpointRounding.AwayFromZero);
						if(minutesLeft >= 60)
						{
							double hoursLeft = Math.Round(minutesLeft / 60, MidpointRounding.AwayFromZero);
							downloadTimeLeftFormatted = string.Format("{0} hour{1} left", hoursLeft, hoursLeft == 1 ? "" : "s");
						}
						else
						{
							downloadTimeLeftFormatted = string.Format("{0} minute{1} left", minutesLeft, minutesLeft == 1 ? "" : "s");
						}
					}
					else
					{
						downloadTimeLeftFormatted = string.Format("{0} second{1} left", secondsLeft, secondsLeft == 1 ? "" : "s");
					}

					TitleBlock.Text = string.Format("Downloaded {0}/{1}mb... {2}", downloadedMB.ToString("0.0"), (e.TotalBytesToReceive / 1000000).ToString("0.0"), downloadTimeLeftFormatted);
					ProgressBar.Value = e.ProgressPercentage;
					WindowIconTools.SetProgressValue((ulong)e.ProgressPercentage, 100);
				}
			}
		}

		void OnDownloadCompleted(object sender, AsyncCompletedEventArgs e)
		{
			if(Directory.Exists(applicationFolderPath))
			{
				Directory.Delete(applicationFolderPath, true);
			}
			ZipFile.ExtractToDirectory(zipPath, applicationFolderPath);
			File.Delete(zipPath);
			File.WriteAllText(versionPath, e.UserState.ToString()); // Write the version file with the downloaded onlineVersion

			WindowIconTools.SetProgressState(TaskbarProgressBarState.NoProgress);

			finishedPatch = true;
			Start.Opacity = 100;
		}

		void StartApplication()
		{
			ProcessStartInfo startInfo = new ProcessStartInfo(Path.Combine(applicationFolderPath, configInfo.values["FILE_TO_RUN"]));
			startInfo.WorkingDirectory = applicationFolderPath;
			try
			{
				Process.Start(startInfo);
			}
			catch(Exception ex)
			{
				OnError($"Error starting application: {ex}");
			}
			Close();
		}

		void OnError(string message)
		{
			MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
		}

        private void Start_Click(object sender, RoutedEventArgs e)
        {
			if (finishedPatch)
			{
				StartApplication();
			}
			else
			{
				
			}

        }
    }
}
