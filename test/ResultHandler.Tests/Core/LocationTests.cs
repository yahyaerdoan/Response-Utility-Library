using System.Text.Json;
using ResultHandler.Core.Abstractions;
using ResultHandler.Facade;
using ResultHandler.Implementations.Success;
using Xunit;

namespace ResultHandler.Tests.Core;

public class LocationTests
{
    [Fact]
    public void Location_IsNullByDefault()
    {
        Assert.Null(Result.Created(5).Location);
    }

    [Fact]
    public void WithLocation_SetsIt_AndKeepsTypeAndData()
    {
        var created = Result.Created(5).WithLocation("/api/orders/5");

        Assert.IsType<SuccessDataResult<int>>(created);
        Assert.Equal("/api/orders/5", created.Location);
        Assert.Equal(5, created.Data);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void WithLocation_RejectsBlank(string location)
    {
        Assert.Throws<ArgumentException>(() => Result.Created().WithLocation(location));
    }

    [Fact]
    public void RedirectFactories_SetTheLocation()
    {
        Assert.Equal("https://example.com/a", Result.MovedPermanently("https://example.com/a").Location);
        Assert.Equal("https://example.com/b", Result.Found(1, "https://example.com/b").Location);
        Assert.Equal("/c", Result.SeeOther("/c").Location);
        Assert.Equal("/d", Result.TemporaryRedirect("/d").Location);
        Assert.Equal("/e", Result.PermanentRedirect("/e").Location);
        Assert.Equal("http://proxy:8080", Result.UseProxy("http://proxy:8080").Location);
        Assert.IsAssignableFrom<IHasLocation>(Result.Found("/f"));
    }

    [Fact]
    public void Equality_ConsidersLocation()
    {
        Assert.Equal(Result.Created().WithLocation("/a"), Result.Created().WithLocation("/a"));
        Assert.NotEqual(Result.Created().WithLocation("/a"), Result.Created().WithLocation("/b"));
    }

    [Fact]
    public void Location_IsNotSerialized()
    {
        var json = JsonSerializer.Serialize(Result.Created(5).WithLocation("/api/orders/5"));

        using var document = JsonDocument.Parse(json);
        Assert.False(document.RootElement.TryGetProperty("location", out _));
        Assert.False(document.RootElement.TryGetProperty("Location", out _));
    }
}
