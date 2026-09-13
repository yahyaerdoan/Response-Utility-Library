using ResultHandler.Core.Abstractions;
using ResultHandler.Core.Base;
using ResultHandler.Core.Enums;
using ResultHandler.Facade;
using ResultHandler.Implementations.Error;
using Xunit;

namespace ResultHandler.Tests.Facade;

public class ResultFacadeTests
{
    [Fact]
    public void Success_NoArgs_UsesDefaults()
    {
        var result = Result.Success();

        Assert.True(result.IsSuccessful);
        Assert.Equal(ResultStatus.Ok, result.Status);
    }

    [Fact]
    public void Success_WithData_CarriesData()
    {
        var result = Result.Success(42);

        Assert.True(result.IsSuccessful);
        Assert.Equal(42, result.Data);
    }

    [Fact]
    public void MovedPermanently_InterpolatesLocationIntoTitle()
    {
        var result = Result.MovedPermanently("https://example.com/new");

        Assert.True(result.IsSuccessful);
        Assert.Equal(ResultStatus.MovedPermanently, result.Status);
        Assert.Contains("https://example.com/new", result.Title);
    }

    [Fact]
    public void NotFound_UsesGivenDetail()
    {
        var result = Result.NotFound("The user does not exist.");

        Assert.False(result.IsSuccessful);
        Assert.Equal(ResultStatus.NotFound, result.Status);
        Assert.Equal("The user does not exist.", result.Detail);
    }

    [Fact]
    public void Unauthorized_HasDefaultDetail()
    {
        var result = Result.Unauthorized();

        Assert.Equal("Authentication is required to access this resource.", result.Detail);
    }

    [Fact]
    public void NotFoundOfT_ReturnsGenericErrorDataResult()
    {
        var result = Result.NotFound<string>("Missing.");

        Assert.False(result.IsSuccessful);
        Assert.Equal(ResultStatus.NotFound, result.Status);
        Assert.Null(result.Data);
    }

    [Fact]
    public void Invalid_ParamsOverload_SetsErrors()
    {
        var result = Result.Invalid("Name is required.", "Email is invalid.");

        Assert.Equal(ResultStatus.UnprocessableContent, result.Status);
        Assert.Equal(2, result.Errors.Count);
    }

    [Fact]
    public void Invalid_ReadOnlyListOverload_SetsErrors()
    {
        IReadOnlyList<string> errors = ["Name is required."];

        var result = Result.Invalid(errors);

        Assert.Equal(errors, result.Errors);
    }

    [Fact]
    public void Failure_EscapeHatch_BuildsCustomErrorResult()
    {
        var result = Result.Failure("Custom title", "Custom detail", ResultStatus.Conflict);

        Assert.Equal("Custom title", result.Title);
        Assert.Equal("Custom detail", result.Detail);
        Assert.Equal(ResultStatus.Conflict, result.Status);
    }

    [Fact]
    public void FailureOfT_WithData_BuildsCustomErrorDataResult()
    {
        var result = Result.Failure(-1, "Custom title", "Custom detail", ResultStatus.Conflict);

        Assert.Equal(-1, result.Data);
        Assert.Equal(ResultStatus.Conflict, result.Status);
    }

    [Fact]
    public void Combine_AllSucceed_ReturnsSuccess()
    {
        var result = Result.Combine(Result.Success(), Result.Success("Ok."), Result.Created());

        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void Combine_NoResults_ReturnsSuccess()
    {
        var result = Result.Combine();

        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void Combine_SomeFail_CollectsErrorsFromEveryFailureNotJustTheFirst()
    {
        var result = Result.Combine(
            Result.Success(),
            Result.Invalid("Name is required."),
            Result.NotFound("Category 7 does not exist."),
            Result.Invalid("Price must be greater than zero."));

        Assert.False(result.IsSuccessful);
        Assert.Equal(ResultStatus.UnprocessableContent, result.Status);
        Assert.Equal(
            ["Name is required.", "Category 7 does not exist.", "Price must be greater than zero."],
            result.Errors);
    }

    [Fact]
    public void Combine_FailureWithoutDetailOrErrors_FallsBackToTitle()
    {
        var result = Result.Combine(new ErrorResult("Something went wrong.", ResultStatus.Conflict));

        Assert.Equal(["Something went wrong."], result.Errors);
    }

    [Fact]
    public void Combine_SomeFailWithFieldErrors_MergesFieldErrorsByKey()
    {
        var result = Result.Combine(
            OperationResult.Failure(new Dictionary<string, IReadOnlyList<string>> { ["Name"] = ["Name is required."] }),
            OperationResult.Failure(new Dictionary<string, IReadOnlyList<string>> { ["Price"] = ["Price must be greater than zero."] }));

        var withFieldErrors = Assert.IsAssignableFrom<IHasFieldErrors>(result);
        Assert.Equal(["Name is required."], withFieldErrors.FieldErrors["Name"]);
        Assert.Equal(["Price must be greater than zero."], withFieldErrors.FieldErrors["Price"]);
        Assert.Equal(["Name is required.", "Price must be greater than zero."], result.Errors);
    }

    [Fact]
    public void Combine_MixOfFieldAndPlainFailures_KeepsPlainErrorsAndFieldErrors()
    {
        var result = Result.Combine(
            OperationResult.Failure(new Dictionary<string, IReadOnlyList<string>> { ["Name"] = ["Name is required."] }),
            Result.NotFound("Category 7 does not exist."));

        var withFieldErrors = Assert.IsAssignableFrom<IHasFieldErrors>(result);
        Assert.Equal(["Name is required."], withFieldErrors.FieldErrors["Name"]);
        Assert.Equal(["Name is required.", "Category 7 does not exist."], result.Errors);
    }

    [Fact]
    public void CombineOfT_AllSucceed_ReturnsTupleOfData()
    {
        var result = Result.Combine(Result.Success<string>("Alice"), Result.Success(42));

        Assert.True(result.IsSuccessful);
        Assert.Equal(("Alice", 42), result.Data);
    }

    [Fact]
    public void CombineOfT_SomeFail_ReturnsAggregatedValidationFailure()
    {
        var result = Result.Combine(Result.Invalid<string>("Name is required."), Result.Success(42));

        Assert.False(result.IsSuccessful);
        Assert.Equal(ResultStatus.UnprocessableContent, result.Status);
        Assert.Equal(["Name is required."], result.Errors);
    }

    [Fact]
    public void CombineOfT_ThreeAndFourArity_ReturnTupleOfData()
    {
        var three = Result.Combine(Result.Success(1), Result.Success<string>("two"), Result.Success(3.0));
        var four = Result.Combine(Result.Success(1), Result.Success<string>("two"), Result.Success(3.0), Result.Success(true));

        Assert.Equal((1, "two", 3.0), three.Data);
        Assert.Equal((1, "two", 3.0, true), four.Data);
    }
}
