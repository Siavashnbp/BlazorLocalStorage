using BlazorLocalStorageManager.Abstraction;
using BlazorLocalStorageManager.Enums;
using BlazorLocalStorageManager.Models;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using System.Text.Json;

namespace BlazorLocalStorageManager.Services
{
    internal sealed class BrowserStorage : IBrowserStorage
    {
        private readonly IJSRuntime _js;
        private readonly BrowserStorageOptions _options;
        public BrowserStorage(IJSRuntime js, IOptions<BrowserStorageOptions> options)
        {
            _js = js;
            _options = options.Value;
        }

        public ValueTask ClearAsync(StorageType storageType = StorageType.Local, CancellationToken cancellationToken = default)
        {
            return _js.InvokeVoidAsync($"{GetStorage(storageType)}.clear", cancellationToken);
        }

        public async ValueTask<bool> ExistsAsync(string key, StorageType storageType = StorageType.Local, CancellationToken cancellationToken = default)
        {
            // Read the raw stored JSON so we can inspect the expiration timestamp.
            // Use the overload without CancellationToken to better match callers/mocks that use the object[] overload.
            var json = await _js.InvokeAsync<string>($"{GetStorage(storageType)}.getItem", BuildKey(key));

            if (json is null)
                return false;

            var item = JsonSerializer.Deserialize<StorageItem<object>>(json, _options.JsonSerializerOptions);

            if (item?.ExpiresAt is not null && item.ExpiresAt <= DateTimeOffset.UtcNow)
            {
                // Remove the expired item (this uses the cancellationToken overload for removal)
                await RemoveAsync(key, storageType, cancellationToken);
                return false;
            }

            return item is not null;
        }

        public async ValueTask<T?> GetAsync<T>(string key, StorageType storageType = StorageType.Local, CancellationToken cancellationToken = default)
        {
            var json = await _js.InvokeAsync<string>($"{GetStorage(storageType)}.getItem", cancellationToken, BuildKey(key));
            if (string.IsNullOrWhiteSpace(json))
            {
                return default;
            }
            var item = JsonSerializer.Deserialize<StorageItem<T>>(json, _options.JsonSerializerOptions);
            if (item?.ExpiresAt is not null &&
                item.ExpiresAt <= DateTimeOffset.UtcNow)
            {
                await RemoveAsync(key, storageType, cancellationToken);
                return default;
            }

            return item is null ? default : item.Value;
        }

        public ValueTask RemoveAsync(string key, StorageType storageType = StorageType.Local, CancellationToken cancellationToken = default)
        {
            return _js.InvokeVoidAsync($"{GetStorage(storageType)}.removeItem", cancellationToken, BuildKey(key));
        }

        public async ValueTask RemoveManyAsync(IEnumerable<string> keys, StorageType storageType = StorageType.Local, CancellationToken cancellationToken = default)
        {
            foreach (var key in keys)
            {
                await RemoveAsync(key, storageType, cancellationToken);
            }
        }

        public async ValueTask SetAsync<T>(string key, T value, StorageType storageType = StorageType.Local, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            var item = new StorageItem<T>
            {
                Value = value,
                ExpiresAt = expiration is null ? null : DateTimeOffset.UtcNow.Add(expiration.Value)
            };

            var json = JsonSerializer.Serialize(item, _options.JsonSerializerOptions);

            await _js.InvokeVoidAsync($"{GetStorage(storageType)}.setItem", cancellationToken, BuildKey(key), json);
        }

        private string BuildKey(string key)
        {
            if (string.IsNullOrWhiteSpace(_options.KeyPrefix))
                return key;

            return $"{_options.KeyPrefix}:{key}";
        }
        private string GetStorage(StorageType storageType)
        {
            return storageType switch
            {
                StorageType.Local => "localStorage",
                StorageType.Session => "sessionStorage",
                _ => throw new ArgumentOutOfRangeException(nameof(storageType), storageType, null)
            };
        }
    }
}
