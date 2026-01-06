using System.Text.Json.Serialization;

namespace GitHub_Store
{
    public class AppInfo
    {
        [JsonPropertyName("Name")]
        public string Name { get; set; }
        [JsonPropertyName("Url")]
        public string Url { get; set; }
        [JsonPropertyName("Image")]
        public string Image { get; set; }
        [JsonPropertyName("Genre")]
        public string Genre { get; set; }
        [JsonPropertyName("Type")]
        public string Type { get; set; }
        [JsonPropertyName("Platform")]
        public string Platform { get; set; }
        [JsonPropertyName("Emulator Platforms")]
        public string EmulatorPlatforms { get; set; }
        [JsonPropertyName("Description")]
        public string Description { get; set; }
        [JsonPropertyName("Desc")]
        public string Desc { get; set; } // URL to JSON file with description
    }
}
