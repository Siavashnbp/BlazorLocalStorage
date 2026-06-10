using System.Text.Json;

namespace BlazorLocalStorageManager.Models
{
    public sealed class BrowserStorageOptions
    {
        public string KeyPrefix { get; set; } = "";
        public JsonSerializerOptions JsonSerializerOptions { get; set; } = new(JsonSerializerOptions.Web);
    }
}
