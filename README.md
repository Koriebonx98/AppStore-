# AppStore
AppStore for many of my projects 

## Web Interface

A functional HTML page is available at `index.html` that provides a user-friendly web interface to browse all applications.

### Features

- **Dynamic Loading**: Automatically loads app data from `data.json`
- **Alphabetical Organization**: Apps are sorted and grouped by first letter (A-Z)
- **Real-time Search**: Case-insensitive search to filter apps by name
- **Auto-Refresh**: Automatically reloads data every 30 seconds to reflect changes
- **Responsive Design**: Mobile-friendly layout with beautiful gradients and card-based UI
- **Security**: Built-in XSS protection with HTML escaping and URL sanitization

### Usage

1. Open `index.html` in a web browser
2. Use the search bar to filter apps by name
3. Click on "View Repository →" to visit the app's GitHub repository
4. To add new apps, edit `data.json` and reload the page

### Hosting

To host this page:
- Simply place `index.html` and `data.json` on any web server
- Or use GitHub Pages by enabling it in repository settings
- No build process or dependencies required!
