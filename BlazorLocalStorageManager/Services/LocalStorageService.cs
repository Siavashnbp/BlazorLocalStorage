using BlazorLocalStorageManager.Interfaces;
using Microsoft.JSInterop;
using System.Text.Json;
namespace BlazorLocalStorageManager.Services
{
    internal sealed class LocalStorageService : ILocalStorageService
    {
        private readonly IJSRuntime _js;
        public LocalStorageService(IJSRuntime js)
        {
            _js = js;
        }
        public async ValueTask ClearAsync()
        {
            await _js.InvokeVoidAsync("localStorage.clear");
        }

        public async ValueTask<T?> GetAsync<T>(string key)
        {
            var json = await _js.InvokeAsync<string>("localStorage.getItem", key);

            return string.IsNullOrWhiteSpace(json) ? default : JsonSerializer.Deserialize<T>(json);
        }

        public async ValueTask RemoveAsync(string key)
        {
            await _js.InvokeVoidAsync("localStorage.removeItem", key);
        }

        public async ValueTask SetAsync<T>(string key, T value)
        {
            var json = JsonSerializer.Serialize(value);

            await _js.InvokeVoidAsync("localStorage.setItem", key, json);
        }
    }
}
