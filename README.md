# Siavash.Blazor.Storage

A lightweight browser storage abstraction for Blazor applications.

`Siavash.Blazor.Storage` provides a simple and strongly-typed API for working with browser storage (`localStorage` / `sessionStorage`) through JavaScript interop.

It is designed to work with Blazor Server, Blazor WebAssembly, and Blazor Web Apps.

---

## Features

* ✅ Strongly typed storage API
* ✅ JSON serialization/deserialization
* ✅ Support for `localStorage`
* ✅ Support for `sessionStorage`
* ✅ Expiration support
* ✅ Automatic removal of expired items
* ✅ Async API
* ✅ Cancellation token support
* ✅ Simple dependency injection setup

---

## Installation

Install the package from NuGet:

```bash
dotnet add package Siavash.Blazor.Storage
```

or using Package Manager:

```powershell
Install-Package Siavash.Blazor.Storage
```

---

## Configuration

Register the service in `Program.cs`:

```csharp
builder.Services.AddBrowserStorage();
```

---

## Usage

Inject the storage service:

```razor
@inject IBrowserStorage Storage
```

---

## Store Data

```csharp
await Storage.SetAsync(
    "username",
    "siavash"
);
```

The value will be stored as JSON.

---

## Retrieve Data

```csharp
var username =
    await Storage.GetAsync<string>("username");
```

If the key does not exist, the result will be:

```csharp
null
```

---

## Store Objects

Any serializable object can be stored:

```csharp
public class UserSettings
{
    public string Theme { get; set; } = "";
    public bool Notifications { get; set; }
}
```

Store:

```csharp
await Storage.SetAsync(
    "settings",
    new UserSettings
    {
        Theme = "dark",
        Notifications = true
    });
```

Retrieve:

```csharp
var settings =
    await Storage.GetAsync<UserSettings>("settings");
```

---

## Expiration

You can define an expiration time:

```csharp
await Storage.SetAsync(
    "temporary-data",
    "hello",
    expiration: TimeSpan.FromMinutes(10)
);
```

When the item expires:

* `GetAsync` returns the default value
* The expired item is automatically removed

Example:

```csharp
var value =
    await Storage.GetAsync<string>("temporary-data");
```

---

## Storage Type

By default, data is stored in `localStorage`.

You can specify the storage type:

```csharp
await Storage.SetAsync(
    "token",
    "abc123",
    StorageType.Session
);
```

Retrieve:

```csharp
var token =
    await Storage.GetAsync<string>(
        "token",
        StorageType.Session
    );
```

---

## Remove Data

Remove a single item:

```csharp
await Storage.RemoveAsync("username");
```

Clear all stored values:

```csharp
await Storage.ClearAsync();
```

---

## Example: Language Preference

A common use case is saving the selected application language:

```csharp
await Storage.SetAsync(
    "culture",
    "fa-IR"
);
```

Later:

```csharp
var culture =
    await Storage.GetAsync<string>("culture");
```

---

## Notes for Blazor Server / Prerendering

Browser storage requires JavaScript interop, which is only available after the application is connected to the browser.

Avoid calling storage methods during:

```csharp
OnInitializedAsync()
```

when prerendering is enabled.

Use:

```csharp
OnAfterRenderAsync(firstRender: true)
```

or call storage operations after the application becomes interactive.

---

## License

MIT License

---

## Contributing

Issues and pull requests are welcome.

If you find a bug or have a feature request, please open an issue in the GitHub repository.
