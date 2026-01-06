using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls.Primitives;

namespace GitHub_Store
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<AppInfo> _allApps = new();
        private HashSet<string> _selectedPlatforms = new();
        private HashSet<string> _selectedGenres = new();
        private HashSet<string> _selectedTypes = new();

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            SearchBox.TextChanged += SearchBox_TextChanged;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await ReloadDataAsync();
            UpdateSearchPlaceholder();
        }

        private async Task<List<AppInfo>> LoadAppsAsync()
        {
            string url = "https://raw.githubusercontent.com/Koriebonx98/AppStore-/main/Default.json";
            // Add cache-busting query string
            url += (url.Contains("?") ? "&" : "?") + "t=" + DateTime.UtcNow.Ticks;
            using var client = new HttpClient();
            var json = await client.GetStringAsync(url);
            // Sanitize JSON: replace unescaped newlines and carriage returns inside string values
            // This is a simple workaround for invalid JSON from the source
            json = json.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ");
            var apps = JsonSerializer.Deserialize<List<AppInfo>>(json);
            foreach (var app in apps)
            {
                if (app.Image != null && app.Image.Contains("github.com") && app.Image.Contains("/blob/"))
                {
                    app.Image = app.Image.Replace("github.com", "raw.githubusercontent.com")
                                         .Replace("/blob/", "/");
                }
                // Convert Desc URL to raw if needed
                if (!string.IsNullOrWhiteSpace(app.Desc) && app.Desc.Contains("github.com") && app.Desc.Contains("/blob/"))
                {
                    app.Desc = app.Desc.Replace("github.com", "raw.githubusercontent.com").Replace("/blob/", "/");
                }
                // No fetching of description here; handled in AppInfoDialog
            }
            return apps;
        }

        private async Task ReloadDataAsync()
        {
            var apps = await LoadAppsAsync();
            _allApps = apps.OrderBy(a => a.Name).ToList();
            GenerateFilterButtons();
            FilterAndDisplayApps();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            _ = ReloadDataAsync();
        }

        private void GenerateFilterButtons()
        {
            PlatformFilterPanel.Children.Clear();
            GenreFilterPanel.Children.Clear();
            TypeFilterPanel.Children.Clear();

            var platforms = _allApps.Select(a => a.Platform ?? "").Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().OrderBy(s => s);
            var genres = _allApps.Select(a => a.Genre ?? "").Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().OrderBy(s => s);
            var types = _allApps.Select(a => a.Type ?? "").Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().OrderBy(s => s);

            foreach (var platform in platforms)
            {
                var btn = CreateFilterToggleButton(platform, _selectedPlatforms, FilterAndDisplayApps);
                PlatformFilterPanel.Children.Add(btn);
            }
            foreach (var genre in genres)
            {
                var btn = CreateFilterToggleButton(genre, _selectedGenres, FilterAndDisplayApps);
                GenreFilterPanel.Children.Add(btn);
            }
            foreach (var type in types)
            {
                var btn = CreateFilterToggleButton(type, _selectedTypes, FilterAndDisplayApps);
                TypeFilterPanel.Children.Add(btn);
            }
        }

        private ToggleButton CreateFilterToggleButton(string value, HashSet<string> selectedSet, RoutedEventHandler onClick)
        {
            var btn = new ToggleButton
            {
                Content = value,
                Margin = new Thickness(0, 0, 6, 4),
                MinWidth = 80,
                Padding = new Thickness(10, 4, 10, 4),
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Background = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(0, 120, 215)),
                BorderThickness = new Thickness(1),
                Foreground = Brushes.Black,
                Cursor = Cursors.Hand,
                IsChecked = selectedSet.Contains(value),
            };
            btn.Checked += (s, e) => { selectedSet.Add(value); onClick(s, e); };
            btn.Unchecked += (s, e) => { selectedSet.Remove(value); onClick(s, e); };
            return btn;
        }

        private void FilterAndDisplayApps(object sender = null, RoutedEventArgs e = null)
        {
            string query = SearchBox.Text.Trim().ToLower();
            var filtered = _allApps.Where(app =>
                (string.IsNullOrWhiteSpace(query) || (app.Name != null && app.Name.ToLower().Contains(query))) &&
                (_selectedPlatforms.Count == 0 || (_selectedPlatforms.Contains(app.Platform))) &&
                (_selectedGenres.Count == 0 || (_selectedGenres.Contains(app.Genre))) &&
                (_selectedTypes.Count == 0 || (_selectedTypes.Contains(app.Type)))
            ).ToList();
            AppsGrid.ItemsSource = filtered;
            UpdateSearchPlaceholder();
        }

        private void AppBox_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is AppInfo app && app != null)
            {
                // Show the app info dialog as an overlay
                var dialog = new AppInfoDialog(app)
                {
                    Owner = this,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                dialog.ShowDialog();
            }
            else
            {
                MessageBox.Show("App details could not be loaded.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterAndDisplayApps();
        }

        private void UpdateSearchPlaceholder()
        {
            if (SearchPlaceholder != null && SearchBox != null)
            {
                SearchPlaceholder.Visibility = string.IsNullOrEmpty(SearchBox.Text) ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }
}