using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace GitHub_Store
{
    public partial class AppInfoDialog : Window
    {
        private string _url;
        public AppInfoDialog(AppInfo app)
        {
            InitializeComponent();
            this.PreviewKeyDown += AppInfoDialog_PreviewKeyDown;
            if (app == null)
            {
                MessageBox.Show("App details are missing.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }
            try
            {
                AppImage.Source = !string.IsNullOrWhiteSpace(app.Image) ? new System.Windows.Media.Imaging.BitmapImage(new System.Uri(app.Image)) : null;
            }
            catch
            {
                AppImage.Source = null;
            }
            AppName.Text = app.Name ?? "Unknown";
            AppGenre.Text = $"Genre: {app.Genre ?? "Unknown"}";
            AppType.Text = $"Type: {app.Type ?? "Unknown"}";
            AppPlatform.Text = $"Platform: {app.Platform ?? "Unknown"}";
            AppEmulatorPlatforms.Text = string.IsNullOrWhiteSpace(app.EmulatorPlatforms) ? "" : $"Emulator Platforms: {app.EmulatorPlatforms}";
            AppDescription.Text = string.IsNullOrWhiteSpace(app.Description) ? "No description available." : app.Description;
            _url = app.Url;

            // Fetch description from Desc URL if present and update UI
            if (!string.IsNullOrWhiteSpace(app.Desc))
            {
                var descUrl = app.Desc.Trim();
                if (!string.IsNullOrEmpty(descUrl))
                    _ = LoadAndSetDescriptionAsync(descUrl);
            }
        }

        private async Task LoadAndSetDescriptionAsync(string descUrl)
        {
            descUrl = descUrl.Trim();
            // Convert /blob/ to raw if needed
            if (descUrl.Contains("github.com") && descUrl.Contains("/blob/"))
            {
                descUrl = descUrl.Replace("github.com", "raw.githubusercontent.com").Replace("/blob/", "/");
            }
            // Add cache-busting query string to always fetch latest
            descUrl += (descUrl.Contains("?") ? "&" : "?") + "t=" + DateTime.UtcNow.Ticks;
            try
            {
                using var client = new HttpClient();
                var descJson = await client.GetStringAsync(descUrl);
                try
                {
                    using var doc = JsonDocument.Parse(descJson);
                    if (doc.RootElement.TryGetProperty("description", out var descProp))
                    {
                        string desc = descProp.GetString();
                        if (!string.IsNullOrWhiteSpace(desc))
                        {
                            // Ensure UI update on UI thread
                            await Dispatcher.InvokeAsync(() => AppDescription.Text = desc);
                            return;
                        }
                    }
                    // If "description" property is missing, show the raw content
                    await Dispatcher.InvokeAsync(() => AppDescription.Text = descJson);
                }
                catch
                {
                    // If parsing fails, show the raw content
                    await Dispatcher.InvokeAsync(() => AppDescription.Text = descJson);
                }
            }
            catch
            {
                // If loading fails, do not show an error, just leave the description as is
            }
        }

        private void AppUrlButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(_url))
                Process.Start(new ProcessStartInfo(_url) { UseShellExecute = true });
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void AppInfoDialog_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
                e.Handled = true;
            }
        }
    }
}
