using ResultHandler.Core.Abstractions;
using ResultHandler.Core.Enums;
using ResultHandler.Facade;
using ResultHandler.Functional;
using ResultHandler.Implementations.Error;
using ResultHandler.Implementations.Success;
using Xunit;

namespace ResultHandler.Tests.Functional;

public class ResultExtensionsTests
{
    [Fact]
    public void Match_Success_InvokesOnSuccess()
    {
        var result = new SuccessDataResult<int>(42, "Ok.", ResultStatus.Ok);

        var output = result.Match(data => $"got {data}", failure => "failed");

        Assert.Equal("got 42", output);
    }

    [Fact]
    public void Match_Failure_InvokesOnFailure()
    {
        var result = new ErrorDataResult<int>("Not found.", ResultStatus.NotFound);

        var output = result.Match(data => "success", failure => $"failed: {failure.Title}");

        Assert.Equal("failed: Not found.", output);
    }

    [Fact]
    public void OnSuccess_RunsAction_OnlyWhenSuccessful()
    {
        var ran = false;
        var success = new SuccessResult();
        var failure = new ErrorResult();

        success.OnSuccess(_ => ran = true);
        Assert.True(ran);

        ran = false;
        failure.OnSuccess(_ => ran = true);
        Assert.False(ran);
    }

    [Fact]
    public void OnSuccessOfT_ExposesTypedDataAndRunsOnlyWhenSuccessful()
    {
        var received = 0;
        var success = new SuccessDataResult<int>(42, "Ok.", ResultStatus.Ok);
        var failure = new ErrorDataResult<int>("Not found.", ResultStatus.NotFound);

        success.OnSuccess(data => received = data);
        Assert.Equal(42, received);

        received = 0;
        failure.OnSuccess(data => received = data);
        Assert.Equal(0, received);
    }

    [Fact]
    public void OnFailure_RunsAction_OnlyWhenFailed()
    {
        var ran = false;
        var success = new SuccessResult();
        var failure = new ErrorResult();

        failure.OnFailure(_ => ran = true);
        Assert.True(ran);

        ran = false;
        success.OnFailure(_ => ran = true);
        Assert.False(ran);
    }

    [Fact]
    public void Map_Success_TransformsData()
    {
        var result = new SuccessDataResult<int>(2, "Ok.", ResultStatus.Ok);

        var mapped = result.Map(value => value * 10);

        Assert.True(mapped.IsSuccessful);
        Assert.Equal(20, mapped.Data);
    }

    [Fact]
    public void Map_Failure_PropagatesErrorWithoutInvokingMapper()
    {
        var result = new ErrorDataResult<int>("Not found.", ResultStatus.NotFound, "Missing.");

        var mapped = result.Map(value => value * 10);

        Assert.False(mapped.IsSuccessful);
        Assert.Equal(ResultStatus.NotFound, mapped.Status);
        Assert.Equal("Missing.", mapped.Detail);
    }

    [Fact]
    public void Bind_Success_ChainsIntoNextOperation()
    {
        var result = new SuccessDataResult<int>(2, "Ok.", ResultStatus.Ok);

        var chained = result.Bind(value => new SuccessDataResult<string>($"value={value}"));

        Assert.True(chained.IsSuccessful);
        Assert.Equal("value=2", chained.Data);
    }

    [Fact]
    public void Bind_Failure_ShortCircuitsWithoutInvokingBinder()
    {
        var result = new ErrorDataResult<int>("Not found.", ResultStatus.NotFound);
        var invoked = false;

        var chained = result.Bind(value =>
        {
            invoked = true;
            return new SuccessDataResult<string>("unused");
        });

        Assert.False(invoked);
        Assert.False(chained.IsSuccessful);
        Assert.Equal(ResultStatus.NotFound, chained.Status);
    }

    [Fact]
    public void Bind_NonGenericBinder_Success_ReturnsBinderResult()
    {
        IOperationResult<int> result = Result.Success(4);

        IOperationResult bound = result.Bind(id => id > 0 ? Result.NoContent() : Result.NotFound("Missing."));

        Assert.Equal(ResultStatus.NoContent, bound.Status);
    }

    [Fact]
    public void Bind_NonGenericBinder_Failure_CarriesTitleStatusDetailAndErrors()
    {
        IOperationResult<int> notFound = Result.NotFound<int>("Missing.");
        IOperationResult<int> invalid = Result.Invalid<int>("A is required.", "B is required.");

        var fromDetail = notFound.Bind(_ => Result.NoContent());
        var fromErrors = invalid.Bind(_ => Result.NoContent());

        Assert.IsType<ErrorResult>(fromDetail);
        Assert.Equal(ResultStatus.NotFound, fromDetail.Status);
        Assert.Equal("Missing.", fromDetail.Detail);
        Assert.Equal(["A is required.", "B is required."], fromErrors.Errors);
    }

    [Fact]
    public void Bind_DataBinder_StillReturnsDataResult()
    {
        IOperationResult<int> result = Result.Success(4);

        IOperationResult<string> bound = result.Bind(id => Result.Success<string>($"#{id}"));

        Assert.Equal("#4", bound.Data);
    }

    [Fact]
    public void Ensure_PredicateFails_ReturnsValidationFailure()
    {
        var result = new SuccessDataResult<int>(-5, "Ok.", ResultStatus.Ok);

        var ensured = result.Ensure(value => value > 0, "Value must be positive.");

        Assert.False(ensured.IsSuccessful);
        Assert.Equal(ResultStatus.UnprocessableContent, ensured.Status);
        Assert.Equal("Value must be positive.", ensured.Detail);
    }

    [Fact]
    public void Ensure_PredicatePasses_ReturnsOriginalResultUnchanged()
    {
        var result = new SuccessDataResult<int>(5, "Ok.", ResultStatus.Ok);

        var ensured = result.Ensure(value => value > 0, "Value must be positive.");

        Assert.Same(result, ensured);
    }

    [Fact]
    public void Ensure_AlreadyFailed_ShortCircuitsWithoutInvokingPredicate()
    {
        var result = new ErrorDataResult<int>("Not found.", ResultStatus.NotFound);
        var invoked = false;

        bool Predicate(int value)
        {
            invoked = true;
            return true;
        }

        var ensured = result.Ensure(Predicate, "Value must be positive.");

        Assert.False(invoked);
        Assert.Same(result, ensured);
    }

    [Fact]
    public void Ensure_WithCustomStatus_UsesGivenTitleDetailAndStatus()
    {
        var result = new SuccessDataResult<int>(5, "Ok.", ResultStatus.Ok);

        var ensured = result.Ensure(value => value == 42, "Forbidden.", "Only 42 is allowed.", ResultStatus.Forbidden);

        Assert.False(ensured.IsSuccessful);
        Assert.Equal(ResultStatus.Forbidden, ensured.Status);
        Assert.Equal("Forbidden.", ensured.Title);
        Assert.Equal("Only 42 is allowed.", ensured.Detail);
    }
}
