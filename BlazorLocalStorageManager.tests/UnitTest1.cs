using BlazorLocalStorageManager.Models;
using BlazorLocalStorageManager.Services;
using Microsoft.JSInterop;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Threading;
using Xunit;
using FluentAssertions;

namespace BlazorLocalStorageManager.tests
{
    public class UnitTest1
    {
        [Fact]
        public async Task SetAndGetAsync_ShouldRoundtripValue()
        {
            var jsMock = new Mock<IJSRuntime>();

            var options = Options.Create(new BrowserStorageOptions());

            // Setup InvokeAsync<IJSVoidResult> for setItem (cover both overloads)
            jsMock.Setup(js => js.InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
                It.Is<string>(s => s.EndsWith("setItem")),
                It.IsAny<CancellationToken>(),
                It.IsAny<object[]>())).Returns(new ValueTask<Microsoft.JSInterop.Infrastructure.IJSVoidResult>((Microsoft.JSInterop.Infrastructure.IJSVoidResult?)null));

            jsMock.Setup(js => js.InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
                It.Is<string>(s => s.EndsWith("setItem")),
                It.IsAny<object[]>())).Returns(new ValueTask<Microsoft.JSInterop.Infrastructure.IJSVoidResult>((Microsoft.JSInterop.Infrastructure.IJSVoidResult?)null));

            // Prepare stored JSON for getItem
            var item = new StorageItem<int> { Value = 42, ExpiresAt = null };
            var json = System.Text.Json.JsonSerializer.Serialize(item, options.Value.JsonSerializerOptions);

            jsMock.Setup(js => js.InvokeAsync<string>(
                It.Is<string>(s => s.EndsWith("getItem")),
                It.IsAny<CancellationToken>(),
                It.IsAny<object[]>()))
                .ReturnsAsync(json);

            jsMock.Setup(js => js.InvokeAsync<string>(
                It.Is<string>(s => s.EndsWith("getItem")),
                It.IsAny<object[]>()))
                .ReturnsAsync(json);

            var storage = new BrowserStorage(jsMock.Object, options);

            await storage.SetAsync("key", 42);
            var value = await storage.GetAsync<int>("key");

            Assert.Equal(42, value);
        }

        [Fact]
        public async Task SetAsync_UsesKeyPrefix()
        {
            var jsMock = new Mock<IJSRuntime>();

            var options = Options.Create(new BrowserStorageOptions { KeyPrefix = "pref" });

            // Arrange: mock the underlying InvokeAsync<IJSVoidResult> used by the extension (both overloads)
            jsMock.Setup(js => js.InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
                    It.Is<string>(s => s.EndsWith("setItem")),
                    It.IsAny<CancellationToken>(),
                    It.IsAny<object[]>()))
                .Returns(new ValueTask<Microsoft.JSInterop.Infrastructure.IJSVoidResult>((Microsoft.JSInterop.Infrastructure.IJSVoidResult?)null));

            jsMock.Setup(js => js.InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
                    It.Is<string>(s => s.EndsWith("setItem")),
                    It.IsAny<object[]>()))
                .Returns(new ValueTask<Microsoft.JSInterop.Infrastructure.IJSVoidResult>((Microsoft.JSInterop.Infrastructure.IJSVoidResult?)null));

            var storage = new BrowserStorage(jsMock.Object, options);

            await storage.SetAsync("key", 1);

            // Verify: verify the same underlying InvokeAsync<IJSVoidResult> was called
            jsMock.Verify(js => js.InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
                    It.Is<string>(s => s.EndsWith("setItem")),
                    It.IsAny<CancellationToken>(),
                    It.Is<object[]>(arr => arr.Length >= 1 &&
                                           arr[0] != null &&
                                           arr[0].ToString() == "pref:key")),
                Times.Once);
        }

        [Fact]
        public async Task GetAsync_UsesKeyPrefix()
        {
            var jsMock = new Mock<IJSRuntime>();

            var options = Options.Create(new BrowserStorageOptions { KeyPrefix = "myprefix" });

            var item = new StorageItem<string> { Value = "x", ExpiresAt = null };
            var json = System.Text.Json.JsonSerializer.Serialize(item, options.Value.JsonSerializerOptions);

            jsMock.Setup(js => js.InvokeAsync<string>(
                It.Is<string>(s => s.EndsWith("getItem")),
                It.IsAny<CancellationToken>(),
                It.IsAny<object[]>())).ReturnsAsync(json);

            jsMock.Setup(js => js.InvokeAsync<string>(
                It.Is<string>(s => s.EndsWith("getItem")),
                It.IsAny<object[]>())).ReturnsAsync(json);

            var storage = new BrowserStorage(jsMock.Object, options);

            var value = await storage.GetAsync<string>("key");

            Assert.Equal("x", value);

            jsMock.Verify(js => js.InvokeAsync<string>(
                It.Is<string>(s => s.EndsWith("getItem")),
                It.IsAny<CancellationToken>(),
                It.Is<object[]>(arr => arr.Length >= 1 && arr[0] != null && arr[0].ToString() == "myprefix:key")
            ), Times.Once);
        }

        [Fact]
        public async Task GetAsync_RemovesExpiredItemAndReturnsDefault()
        {
            var jsMock = new Mock<IJSRuntime>();
            var options = Options.Create(new BrowserStorageOptions());

            string? storedJson = null;

            // Capture setItem json when SetAsync is called
            jsMock.Setup(js => js.InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
                It.Is<string>(s => s.EndsWith("setItem")),
                It.IsAny<object[]>()))
                .Callback<string, object[]>((identifier, args) =>
                {
                    // args: [key, json]
                    if (args.Length >= 2)
                        storedJson = args[1]?.ToString();
                })
                .Returns(new ValueTask<Microsoft.JSInterop.Infrastructure.IJSVoidResult>((Microsoft.JSInterop.Infrastructure.IJSVoidResult?)null));

            // Also capture the overload that includes a CancellationToken (used by InvokeVoidAsync extension)
            jsMock.Setup(js => js.InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
                It.Is<string>(s => s.EndsWith("setItem")),
                It.IsAny<System.Threading.CancellationToken>(),
                It.IsAny<object[]>()))
                .Callback<string, System.Threading.CancellationToken, object[]>((identifier, ct, args) =>
                {
                    if (args.Length >= 2)
                        storedJson = args[1]?.ToString();
                })
                .Returns(new ValueTask<Microsoft.JSInterop.Infrastructure.IJSVoidResult>((Microsoft.JSInterop.Infrastructure.IJSVoidResult?)null));

            // getItem should return whatever was stored

            jsMock.Setup(js => js.InvokeAsync<string>(
                It.Is<string>(s => s.EndsWith("getItem")),
                It.IsAny<CancellationToken>(),
                It.IsAny<object[]>()))
                .Returns(() => new ValueTask<string>(storedJson));

            // removeItem: just return void result (both overloads)
            jsMock.Setup(js => js.InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
                It.Is<string>(s => s.EndsWith("removeItem")),
                It.IsAny<object[]>()))
                .Returns(new ValueTask<Microsoft.JSInterop.Infrastructure.IJSVoidResult>((Microsoft.JSInterop.Infrastructure.IJSVoidResult?)null));

            jsMock.Setup(js => js.InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
                It.Is<string>(s => s.EndsWith("removeItem")),
                It.IsAny<System.Threading.CancellationToken>(),
                It.IsAny<object[]>()))
                .Returns(new ValueTask<Microsoft.JSInterop.Infrastructure.IJSVoidResult>((Microsoft.JSInterop.Infrastructure.IJSVoidResult?)null));

            var storage = new BrowserStorage(jsMock.Object, options);

            // Store an already-expired item by using a clearly negative expiration (one day ago) to avoid timing issues
            await storage.SetAsync("key", 1, expiration: TimeSpan.FromDays(-1));


            var value = await storage.GetAsync<int>("key");

            value.Should().Be(default);

            // Verify removeItem was called once with the key
            jsMock.Verify(js => js.InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
                It.Is<string>(s => s.EndsWith("removeItem")),
                It.IsAny<CancellationToken>(),
                It.Is<object[]>(arr => arr.Length >= 1 && arr[0] != null && arr[0].ToString() == "key")
            ), Times.Once);
        }

        [Fact]
        public async Task ExistsAsync_ReturnsFalseAndRemoves_WhenItemExpired()
        {
            var jsMock = new Mock<IJSRuntime>();
            var options = Options.Create(new BrowserStorageOptions());

            // Prepare an expired stored item JSON
            var expiredItem = new StorageItem<string> { Value = "v", ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(-10) };
            var expiredJson = System.Text.Json.JsonSerializer.Serialize(expiredItem, options.Value.JsonSerializerOptions);

            jsMock.Setup(js => js.InvokeAsync<string>(
                It.Is<string>(s => s.EndsWith("getItem")),
                It.IsAny<object[]>()))
                .Returns(() => new ValueTask<string>(expiredJson));

            jsMock.Setup(js => js.InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
                It.Is<string>(s => s.EndsWith("removeItem")),
                It.IsAny<object[]>()))
                .Returns(new ValueTask<Microsoft.JSInterop.Infrastructure.IJSVoidResult>((Microsoft.JSInterop.Infrastructure.IJSVoidResult?)null));

            var storage = new BrowserStorage(jsMock.Object, options);

            var exists = await storage.ExistsAsync("key");

            Assert.False(exists);

            jsMock.Verify(js => js.InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
                It.Is<string>(s => s.EndsWith("removeItem")),
                It.IsAny<CancellationToken>(),
                It.Is<object[]>(arr => arr.Length >= 1 && arr[0] != null && arr[0].ToString() == "key")
            ), Times.Once);
        }
    }
}
