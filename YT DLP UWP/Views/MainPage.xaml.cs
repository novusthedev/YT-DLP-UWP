using System;
using System.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using YT_DLP_UWP.ViewModels;

namespace YT_DLP_UWP.Views;

public sealed partial class MainPage : Page
{
    private static readonly string ytdlp = AppDomain.CurrentDomain.BaseDirectory + "\\yt-dlp.exe";

    public MainViewModel ViewModel
    {
        get;
    }

    public MainPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        InitializeComponent();
    }

    private async void ToggleButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {

        if (VideoURL.Text == string.Empty)
        {
            ContentDialog NoURLError = new ContentDialog()
            {
                XamlRoot = this.XamlRoot,
                Title = "No URL Provided",
                Content = "You must provide a video URL to download.",
                CloseButtonText = "OK"
            };
            await NoURLError.ShowAsync();
        }
        else
        {
            var window = new Microsoft.UI.Xaml.Window();
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            var savePicker = new Windows.Storage.Pickers.FileSavePicker();
            savePicker.SuggestedStartLocation =
            Windows.Storage.Pickers.PickerLocationId.VideosLibrary;
            savePicker.FileTypeChoices.Add("YouTube Video", new List<string>() { ".mp4" });
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);
            savePicker.SuggestedFileName = VideoURL.Text.ToString();

            Windows.Storage.StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                // Prevent updates to the remote version of the file until
                // we finish making changes and call CompleteUpdatesAsync.
                Windows.Storage.CachedFileManager.DeferUpdates(file);
                // write to file

                try
                {
                    Process video = new Process();
                    video.StartInfo.FileName = ytdlp;
                    video.StartInfo.Arguments = "-v --force-overwrites -f bv[ext=mp4]+ba*[ext=m4a] -N 4 " + VideoURL.Text.ToString() + " -o " + (char)34 + file.Path.ToString() + (char)34;
                    video.Start();

                    video.WaitForExit();

                    if (video.ExitCode != 0)
                    {
                        file.DeleteAsync(Windows.Storage.StorageDeleteOption.PermanentDelete);
                        ContentDialog FileError = new ContentDialog()
                        {
                            XamlRoot = this.XamlRoot,
                            Title = "Download error",
                            Content = "The download has failed. Please try updating YT-DLP & making sure that the video URL is correct. (Code " + video.ExitCode.ToString() + ")",
                            CloseButtonText = "OK"
                        };
                        await FileError.ShowAsync();
                    }
                    else
                    {
                        ContentDialog Success = new ContentDialog()
                        {
                            XamlRoot = this.XamlRoot,
                            Title = "Download complete",
                            Content = "The video has been downloaded successfully.",
                            CloseButtonText = "OK"
                        };
                        await Success.ShowAsync();
                    }

                }
                catch (Exception ex)
                {
                    file.DeleteAsync(Windows.Storage.StorageDeleteOption.PermanentDelete);
                    ContentDialog ExceptionError = new ContentDialog()
                    {
                        XamlRoot = this.XamlRoot,
                        Title = "Critical error",
                        Content = "Something went wrong & the download can't continue. If YT-DLP is missing, Please download it & put it in the base app directory.",
                        CloseButtonText = "OK"
                    };
                    await ExceptionError.ShowAsync();
                }
            }
        }
    }

    private async void UpdateButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        try
        {
            Process update = new Process();
            update.StartInfo.FileName = ytdlp;
            update.StartInfo.Arguments = " -U";
            update.Start();
        }
        catch (Exception ex)
        {
            ContentDialog ExceptionError = new ContentDialog()
            {
                XamlRoot = this.XamlRoot,
                Title = "Critical error",
                Content = "Something went wrong & the update can't continue. If YT-DLP is missing, Please download it & put it in the base app directory.",
                CloseButtonText = "OK"
            };
            await ExceptionError.ShowAsync();
        }
    }
}
