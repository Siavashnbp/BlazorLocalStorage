namespace BlazorLocalStorageManager.Interfaces
{
    public interface ILocalStorageService
    {
        ValueTask SetAsync<T>(string key, T value);
        ValueTask<T?> GetAsync<T>(string key);
        ValueTask RemoveAsync(string key);
        ValueTask ClearAsync();
    }
}
