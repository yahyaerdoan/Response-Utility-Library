using ResultHandler.Core.Enums;
using ResultHandler.Implementations.Error;
using Xunit;

namespace ResultHandler.Tests.Results;

public class ErrorResultTests
{
    [Fact]
    public void DefaultConstructor_SetsExpectedDefaults()
    {
        var result = new ErrorResult();

        Assert.False(result.IsSuccessful);
        Assert.Equal(ResultStatus.InternalServerError, result.Status);
        Assert.Equal("An error occurred.", result.Title);
        Assert.Null(result.Detail);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void TitleStatusDetailConstructor_SetsAllFields()
    {
        var result = new ErrorResult("Not found.", ResultStatus.NotFound, "The user does not exist.");

        Assert.False(result.IsSuccessful);
        Assert.Equal(ResultStatus.NotFound, result.Status);
        Assert.Equal("Not found.", result.Title);
        Assert.Equal("The user does not exist.", result.Detail);
    }

    [Fact]
    public void TitleStatusErrorsConstructor_PopulatesErrors()
    {
        var errors = new[] { "Field A is required.", "Field B is invalid." };
        var result = new ErrorResult("Validation failed.", ResultStatus.UnprocessableContent, errors);

        Assert.Equal(errors, result.Errors[string.Empty]);
        Assert.Null(result.Detail);
    }

    [Fact]
    public void ErrorDataResult_DataOnlyConstructor_UsesDefaultTitleAndStatus()
    {
        var result = new ErrorDataResult<string>("payload");

        Assert.False(result.IsSuccessful);
        Assert.Equal(ResultStatus.InternalServerError, result.Status);
        Assert.Equal("An error occurred.", result.Title);
        Assert.Equal("payload", result.Data);
    }

    [Fact]
    public void ErrorDataResult_NoDataConstructor_LeavesDataAtDefault()
    {
        var result = new ErrorDataResult<int>("Bad request.", ResultStatus.BadRequest);

        Assert.False(result.IsSuccessful);
        Assert.Equal(0, result.Data);
    }
}
