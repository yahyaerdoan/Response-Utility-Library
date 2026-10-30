using System.Text.Json;
using ResultHandler.Core.Abstractions;
using ResultHandler.Core.Base;
using ResultHandler.Core.Enums;
using ResultHandler.Facade;
using ResultHandler.Functional;
using ResultHandler.Implementations.Error;
using ResultHandler.Implementations.Success;
using Xunit;

namespace ResultHandler.Tests.Core;

public class ErrorsTests
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> SampleErrors =
        new Dictionary<string, IReadOnlyList<string>>
        {
            ["UserNameOrEmail"] = ["'User Name Or Email' must not be empty."],
            ["Password"] = ["'Password' must not be empty.", "'Password' must be at least 8 characters."],
        };

    [Fact]
    public void Failure_WithFieldErrors_IsAValidationFailure_KeyedByField()
    {
        var failure = OperationResult.Failure(SampleErrors);

        Assert.False(failure.IsSuccessful);
        Assert.Equal("Validation Failed", failure.Title);
        Assert.Equal(ResultStatus.UnprocessableContent, failure.Status);
        Assert.Equal(SampleErrors["Password"], failure.Errors["Password"]);
        Assert.Equal(2, failure.Errors.Count);
    }

    [Fact]
    public void Failure_WithMessagesOnly_PutsThemUnderTheGeneralKey()
    {
        var failure = OperationResult.Failure(["A plain error.", "Another."]);

        var only = Assert.Single(failure.Errors);
        Assert.Equal(string.Empty, only.Key);
        Assert.Equal(["A plain error.", "Another."], only.Value);
    }

    [Fact]
    public void Errors_AreEmpty_WhenThereAreNone()
    {
        Assert.Empty(Result.Success().Errors);
        Assert.Empty(Result.NotFound("Missing.").Errors);
        Assert.Empty(OperationResult.Failure(Array.Empty<string>()).Errors);
    }

    [Fact]
    public void Errors_SkipFieldsWithoutMessages()
    {
        var errors = new Dictionary<string, IReadOnlyList<string>>
        {
            ["Name"] = ["Name is required."],
            ["Empty"] = [],
#pragma warning disable CS8625 // Deliberately malformed input: the guard must skip a null value instead of throwing.
            ["Null"] = null,
#pragma warning restore CS8625
        };

        var failure = OperationResult.Failure(errors);

        Assert.Equal(["Name"], failure.Errors.Keys);
    }

    [Fact]
    public void DataFailure_BehavesLikeOperationResult()
    {
        var failure = OperationDataResult<int>.Failure(SampleErrors);

        Assert.False(failure.IsSuccessful);
        Assert.Equal(default, failure.Data);
        Assert.Equal(SampleErrors["UserNameOrEmail"], failure.Errors["UserNameOrEmail"]);
    }

    [Fact]
    public void GenericFailureFactory_BuildsTheRequestedType()
    {
        var failure = Build<OperationDataResult<string>>();

        Assert.IsType<ErrorDataResult<string>>(failure);
        Assert.Equal(2, failure.Errors.Count);

        static TSelf Build<TSelf>()
            where TSelf : IOperationResult, IResultFailureFactory<TSelf>
            => TSelf.Failure(SampleErrors);
    }

    [Fact]
    public void ErrorResultConstructor_MatchesTheFailureFactory()
    {
        var constructed = new ErrorResult("Validation Failed", ResultStatus.UnprocessableContent, SampleErrors);

        Assert.Equal(OperationResult.Failure(SampleErrors), constructed);
    }

    [Fact]
    public void Equality_ComparesErrorsByContent_RegardlessOfInsertionOrder()
    {
        var reordered = new Dictionary<string, IReadOnlyList<string>>
        {
            ["Password"] = ["'Password' must not be empty.", "'Password' must be at least 8 characters."],
            ["UserNameOrEmail"] = ["'User Name Or Email' must not be empty."],
        };

        var a = OperationResult.Failure(SampleErrors);
        var b = OperationResult.Failure(reordered);

        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
        Assert.NotEqual(a, OperationResult.Failure(["'Password' must not be empty."]));
    }

    [Fact]
    public void Serialization_WritesErrorsAsTheFieldMap()
    {
        using var document = JsonDocument.Parse(JsonSerializer.Serialize(OperationResult.Failure(SampleErrors)));
        var errors = document.RootElement.GetProperty("errors");

        Assert.Equal(JsonValueKind.Object, errors.ValueKind);
        Assert.Equal(2, errors.GetProperty("Password").GetArrayLength());
        Assert.False(document.RootElement.TryGetProperty("fieldErrors", out _));
    }

    [Fact]
    public void Serialization_RoundTripsTheErrorMap()
    {
        var mixed = new Dictionary<string, IReadOnlyList<string>>
        {
            ["Name"] = ["Name is required."],
            [string.Empty] = ["The catalog is closed."],
        };
        var original = OperationResult.Failure(mixed);

        var restored = JsonSerializer.Deserialize<OperationResult>(JsonSerializer.Serialize(original));

        Assert.NotNull(restored);
        Assert.Equal(["Name is required."], restored.Errors["Name"]);
        Assert.Equal(["The catalog is closed."], restored.Errors[string.Empty]);
    }

    [Fact]
    public void ToErrorDataResult_KeepsErrorsDetailAndCode()
    {
        var failure = new ErrorResult("Validation Failed", ResultStatus.UnprocessableContent, SampleErrors).WithCode("Signup.Invalid");

        var projected = failure.ToErrorDataResult<string>();

        Assert.Equal(2, projected.Errors.Count);
        Assert.Equal("Signup.Invalid", projected.Code);
        Assert.Equal(ResultStatus.UnprocessableContent, projected.Status);
    }

    [Fact]
    public void ToErrorDataResult_KeepsTheDetailNextToErrors()
    {
        var failure = new OperationResult(false, ResultStatus.UnprocessableContent, "Validation Failed", "Two fields are invalid.", SampleErrors);

        var projected = failure.ToErrorDataResult<string>();

        Assert.Equal("Two fields are invalid.", projected.Detail);
        Assert.Equal(2, projected.Errors.Count);
    }

    [Fact]
    public void MapAndBind_KeepErrors()
    {
        var failure = OperationDataResult<int>.Failure(SampleErrors);

        Assert.Equal(2, failure.Map(value => value * 10).Errors.Count);
        Assert.Equal(2, failure.Bind(value => new SuccessDataResult<string>($"value={value}")).Errors.Count);
    }
}
