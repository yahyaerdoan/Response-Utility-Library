using System.Text.Json;
using ResultHandler.Core.Base;
using ResultHandler.Core.Enums;
using ResultHandler.Facade;
using ResultHandler.Implementations.Error;
using ResultHandler.Implementations.Success;
using ResultHandler.Mapping;
using Xunit;

namespace ResultHandler.Tests.Core;

public class SerializationTests
{
    [Fact]
    public void SuccessDataResult_SerializesWithExpectedPropertyNames()
    {
        var result = new SuccessDataResult<string>("payload", "Fetched.", ResultStatus.Ok);

        using var document = JsonDocument.Parse(JsonSerializer.Serialize(result));
        var root = document.RootElement;

        Assert.Equal((int)ResultStatus.Ok.ToHttpStatusCode(), root.GetProperty("statusCode").GetInt32());
        Assert.Equal("Fetched.", root.GetProperty("statusMessage").GetString());
        Assert.Equal("payload", root.GetProperty("resultData").GetString());
        Assert.True(root.GetProperty("isSuccessful").GetBoolean());
    }

    [Fact]
    public void Detail_IsOmittedFromJson_WhenNull()
    {
        var result = new ErrorResult("Bad Request", ResultStatus.BadRequest);

        using var document = JsonDocument.Parse(JsonSerializer.Serialize(result));

        Assert.False(document.RootElement.TryGetProperty("detail", out _));
    }

    [Fact]
    public void Detail_IsIncludedInJson_WhenSet()
    {
        var result = new ErrorResult("Bad Request", ResultStatus.BadRequest, "Field X is missing.");

        using var document = JsonDocument.Parse(JsonSerializer.Serialize(result));

        Assert.Equal("Field X is missing.", document.RootElement.GetProperty("detail").GetString());
    }

    [Fact]
    public void Success_HasNoErrorsProperty()
    {
        using var document = JsonDocument.Parse(JsonSerializer.Serialize(new SuccessDataResult<string>("payload", "Fetched.", ResultStatus.Ok)));

        Assert.Equal(["resultData", "isSuccessful", "statusCode", "statusMessage"], document.RootElement.EnumerateObject().Select(property => property.Name));
    }

    [Fact]
    public void DataResult_RoundTripsThroughJson()
    {
        var original = Result.Success(new[] { 1, 2, 3 }, "Fetched.");

        var restored = JsonSerializer.Deserialize<OperationDataResult<int[]>>(JsonSerializer.Serialize(original));

        Assert.NotNull(restored);
        Assert.Equal([1, 2, 3], restored.Data ?? []);
        Assert.Equal("Fetched.", restored.Title);
        Assert.True(restored.IsSuccessful);
    }

    [Fact]
    public void Deserialization_IgnoresUnknownProperties_SoResultsCachedBeforeV16StillRead()
    {
        const string cached = """
            {"resultData":[1,2],"isSuccessful":true,"statusCode":200,"statusMessage":"Fetched.","errors":[],"fieldErrors":{}}
            """;

        var restored = JsonSerializer.Deserialize<OperationDataResult<int[]>>(cached);

        Assert.NotNull(restored);
        Assert.Equal([1, 2], restored.Data ?? []);
        Assert.Equal(ResultStatus.Ok, restored.Status);
        Assert.Empty(restored.Errors);
    }

    [Fact]
    public void Deserialization_OfASubclass_IsNotSupported()
    {
        Assert.Throws<NotSupportedException>(() => JsonSerializer.Deserialize<SuccessResult>("{\"isSuccessful\":true,\"statusCode\":200,\"statusMessage\":\"Ok\"}"));
    }

    [Fact]
    public void Deserialization_WithoutStatusCode_Fails()
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<OperationResult>("{\"isSuccessful\":true}"));
    }

    [Fact]
    public void Code_IsOmittedFromJson_WhenNull()
    {
        var result = new ErrorResult("Conflict", ResultStatus.Conflict, "Slug taken.");

        using var document = JsonDocument.Parse(JsonSerializer.Serialize(result));

        Assert.False(document.RootElement.TryGetProperty("code", out _));
    }

    [Fact]
    public void Code_RoundTripsThroughJson()
    {
        var result = Result.Conflict<string>("Slug taken.").WithCode("Tenant.SlugTaken");

        var json = JsonSerializer.Serialize(result);
        var restored = JsonSerializer.Deserialize<OperationDataResult<string>>(json);

        using var document = JsonDocument.Parse(json);
        Assert.Equal("Tenant.SlugTaken", document.RootElement.GetProperty("code").GetString());
        Assert.NotNull(restored);
        Assert.Equal("Tenant.SlugTaken", restored.Code);
        Assert.Equal("Slug taken.", restored.Detail);
        Assert.Equal(ResultStatus.Conflict, restored.Status);
    }
}
