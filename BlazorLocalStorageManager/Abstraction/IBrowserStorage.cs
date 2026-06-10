using BlazorLocalStorageManager.Enums;

namespace BlazorLocalStorageManager.Abstraction
{
    public interface IBrowserStorage
    {
        ValueTask SetAsync<T>(
            string key,
            T value,
            StorageType storageType = StorageType.Local,
            TimeSpan? expiration = null,
            CancellationToken cancellationToken = default);

        ValueTask<T?> GetAsync<T>(
            string key,
            StorageType storageType = StorageType.Local,
            CancellationToken cancellationToken = default);

        ValueTask<bool> ExistsAsync(
            string key,
            StorageType storageType = StorageType.Local,
            CancellationToken cancellationToken = default);

        ValueTask RemoveAsync(
            string key,
            StorageType storageType = StorageType.Local,
            CancellationToken cancellationToken = default);

        ValueTask RemoveManyAsync(
            IEnumerable<string> keys,
            StorageType storageType = StorageType.Local,
            CancellationToken cancellationToken = default);

        ValueTask ClearAsync(
            StorageType storageType = StorageType.Local,
            CancellationToken cancellationToken = default);
    }
}
