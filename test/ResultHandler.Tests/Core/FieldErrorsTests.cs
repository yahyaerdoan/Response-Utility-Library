using System.Text.Json;
using ResultHandler.Core.Abstractions;
using ResultHandler.Core.Base;
using ResultHandler.Core.Enums;
using ResultHandler.Implementations.Error;
using Xunit;

namespace ResultHandler.Tests.Core;

public class FieldErrorsTests
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> SampleFieldErrors =
        new Dictionary<string, IReadOnlyList<string>>
        {
            ["UserNameOrEmail"] = ["'User Name Or Email' must not be empty."],
            ["Password"] = ["'Password' must not be empty.", "'Password' must be at least 8 characters."],
        };

    [Fact]
    public void OperationResult_Failure_WithFieldErrors_SetsValidationFailedTitleAndUnprocessableStatus()
    {
        var failure = OperationResult.Failure(SampleFieldErrors);

        Assert.False(failure.IsSuccessful);
        Assert.Equal("Validation Failed", failure.Title);
        Assert.Equal(ResultStatus.UnprocessableContent, failure.Status);
    }

    [Fact]
    public void OperationResult_Failure_WithFieldErrors_FlattensIntoErrors()
    {
        var failure = OperationResult.Failure(SampleFieldErrors);

        Assert.Equal(3, failure.Errors.Count);
        Assert.Contains("'User Name Or Email' must not be empty.", failure.Errors);
        Assert.Contains("'Password' must not be empty.", failure.Errors);
        Assert.Contains("'Password' must be at least 8 characters.", failure.Errors);
    }

    [Fact]
    public void OperationResult_Failure_WithFieldErrors_ExposesFieldErrorsViaIHasFieldErrors()
    {
        var failure = OperationResult.Failure(SampleFieldErrors);

        var withFieldErrors = Assert.IsAssignableFrom<IHasFieldErrors>(failure);
        Assert.Equal(2, withFieldErrors.FieldErrors.Count);
        Assert.Equal(SampleFieldErrors["UserNameOrEmail"], withFieldErrors.FieldErrors["UserNameOrEmail"]);
        Assert.Equal(SampleFieldErrors["Password"], withFieldErrors.FieldErrors["Password"]);
    }

    [Fact]
    public void OperationDataResult_Failure_WithFieldErrors_BehavesLikeOperationResult()
    {
        var failure = OperationDataResult<string>.Failure(SampleFieldErrors);

        Assert.False(failure.IsSuccessful);
        Assert.Equal("Validation Failed", failure.Title);
        Assert.Equal(ResultStatus.UnprocessableContent, failure.Status);
        Assert.Equal(3, failure.Errors.Count);

        var withFieldErrors = Assert.IsAssignableFrom<IHasFieldErrors>(failure);
        Assert.Equal(2, withFieldErrors.FieldErrors.Count);
    }

    [Fact]
    public void Failure_WithNoFieldErrors_DoesNotImplementIHasFieldErrorsWithContent()
    {
        var failure = OperationResult.Failure(["A plain error."]);

        // Still implements IHasFieldErrors (inherited from OperationResult), but with an empty map -
        // this is what ToProblemDetails checks before adding the "fieldErrors" extension.
        var withFieldErrors = Assert.IsAssignableFrom<IHasFieldErrors>(failure);
        Assert.Empty(withFieldErrors.FieldErrors);
    }

    [Fact]
    public void GenericFieldFailureFactory_Failure_ReprojectsIntoGenericTSelf()
    {
        var failure = CallFieldFailure<OperationResult>(SampleFieldErrors);

        Assert.False(failure.IsSuccessful);
        Assert.Equal(ResultStatus.UnprocessableContent, failure.Status);
    }

    [Fact]
    public void ErrorResult_ConstructedWithFieldErrors_MatchesFailureFactoryOutput()
    {
        var viaConstructor = new ErrorResult("Validation Failed", ResultStatus.UnprocessableContent, SampleFieldErrors);
        var viaFactory = OperationResult.Failure(SampleFieldErrors);

        Assert.Equal(viaFactory, viaConstructor);
    }

    [Fact]
    public void TwoFailures_WithSameFieldErrors_AreEqual()
    {
        var a = OperationResult.Failure(SampleFieldErrors);
        var b = OperationResult.Failure(SampleFieldErrors);

        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void TwoFailures_WithDifferentFieldErrors_AreNotEqual()
    {
        var a = OperationResult.Failure(SampleFieldErrors);
        var b = OperationResult.Failure(new Dictionary<string, IReadOnlyList<string>>
        {
            ["UserNameOrEmail"] = ["'User Name Or Email' must not be empty."],
        });

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void FailureWithFieldErrors_AndFailureWithOnlyFlatErrors_AreNotEqual()
    {
        var withFieldErrors = OperationResult.Failure(SampleFieldErrors);
        IReadOnlyList<string> flatMessages =
        [
            "'User Name Or Email' must not be empty.",
            "'Password' must not be empty.",
            "'Password' must be at least 8 characters.",
        ];
        var flatOnly = OperationResult.Failure(flatMessages);

        Assert.NotEqual(withFieldErrors, flatOnly);
    }

    [Fact]
    public void Serialization_IncludesFieldErrorsUnderExpectedPropertyName()
    {
        var failure = OperationResult.Failure(SampleFieldErrors);

        using var document = JsonDocument.Parse(JsonSerializer.Serialize(failure));
        var fieldErrors = document.RootElement.GetProperty("fieldErrors");

        Assert.Equal(
            "'User Name Or Email' must not be empty.",
            fieldErrors.GetProperty("UserNameOrEmail")[0].GetString());
        Assert.Equal(2, fieldErrors.GetProperty("Password").GetArrayLength());
    }

    [Fact]
    public void Serialization_WithoutFieldErrors_StillIncludesEmptyFieldErrorsObject()
    {
        var failure = OperationResult.Failure(["A plain error."]);

        using var document = JsonDocument.Parse(JsonSerializer.Serialize(failure));
        var fieldErrors = document.RootElement.GetProperty("fieldErrors");

        Assert.Equal(JsonValueKind.Object, fieldErrors.ValueKind);
        Assert.Empty(fieldErrors.EnumerateObject());
    }

    private static TSelf CallFieldFailure<TSelf>(IReadOnlyDictionary<string, IReadOnlyList<string>> fieldErrors)
        where TSelf : IOperationResult, IFieldFailureFactory<TSelf>
        => TSelf.Failure(fieldErrors);
}
