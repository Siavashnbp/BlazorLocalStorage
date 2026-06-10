namespace BlazorLocalStorageManager.Models
{
    internal sealed class StorageItem<T>
    {
        public T? Value { get; set; }
        public DateTimeOffset? ExpiresAt { get; set; }
    }
}
