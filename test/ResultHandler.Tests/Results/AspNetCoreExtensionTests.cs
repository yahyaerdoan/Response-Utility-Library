using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ResultHandler.AspNetCore.Extensions;
using ResultHandler.Core.Abstractions;
using ResultHandler.Core.Enums;
using ResultHandler.Implementations.Error;
using ResultHandler.Implementations.Success;
using Xunit;

namespace ResultHandler.Tests.Results;

public class AspNetCoreExtensionTests
{
    [Fact]
    public void ToActionResultOfT_SuccessWithNonOkStatus_PreservesStatusCode()
    {
        var result = new SuccessDataResult<string>("new-id", "Created.", ResultStatus.Created);

        var actionResult = Assert.IsType<ObjectResult>(result.ToActionResult());

        Assert.Equal("new-id", actionResult.Value);
        Assert.Equal(201, actionResult.StatusCode);
    }

    [Fact]
    public void ToEnvelopedActionResult_SuccessWithNonOkStatus_PreservesStatusCode()
    {
        var result = new SuccessDataResult<int>(42, "Accepted.", ResultStatus.Accepted);

        var actionResult = Assert.IsType<ObjectResult>(result.ToEnvelopedActionResult());

        Assert.Same(result, actionResult.Value);
        Assert.Equal(202, actionResult.StatusCode);
    }

    [Fact]
    public void ToResult_Success_NoContent_ReturnsNoContentResult()
    {
        var result = new SuccessResult("Deleted.", ResultStatus.NoContent);

        Assert.IsType<NoContent>(result.ToResult());
    }

    [Fact]
    public void ToResultOfT_SuccessWithNonOkStatus_PreservesStatusCode()
    {
        var result = new SuccessDataResult<string>("new-id", "Created.", ResultStatus.Created);

        var httpResult = Assert.IsType<JsonHttpResult<string>>(result.ToResult());

        Assert.Equal("new-id", httpResult.Value);
        Assert.Equal(201, httpResult.StatusCode);
    }

    [Fact]
    public void ToEnvelopedResult_SuccessWithNonOkStatus_PreservesStatusCode()
    {
        var result = new SuccessDataResult<int>(42, "Accepted.", ResultStatus.Accepted);

        var httpResult = Assert.IsType<JsonHttpResult<IOperationResult>>(result.ToEnvelopedResult());

        Assert.Same(result, httpResult.Value);
        Assert.Equal(202, httpResult.StatusCode);
    }

    [Fact]
    public void ToProblemResult_Failure_ReturnsJsonHttpResultWithProblemDetails()
    {
        var result = new ErrorResult("Not found.", ResultStatus.NotFound, "The user does not exist.");

        var httpResult = Assert.IsType<JsonHttpResult<ProblemDetails>>(result.ToProblemResult());
        var problemDetails = Assert.IsType<ProblemDetails>(httpResult.Value);

        Assert.Equal(404, httpResult.StatusCode);
        Assert.Equal(404, problemDetails.Status);
        Assert.Equal("Not found.", problemDetails.Title);
        Assert.Equal("The user does not exist.", problemDetails.Detail);
    }

    [Fact]
    public void ToActionResult_Success_NoContent_ReturnsNoContentResult()
    {
        var result = new SuccessResult("Deleted.", ResultStatus.NoContent);

        var actionResult = result.ToActionResult();

        Assert.IsType<NoContentResult>(actionResult);
    }

    [Fact]
    public void ToActionResult_Success_NotModified_Returns304()
    {
        var result = new SuccessResult("Not modified.", ResultStatus.NotModified);

        var actionResult = Assert.IsType<StatusCodeResult>(result.ToActionResult());

        Assert.Equal(304, actionResult.StatusCode);
    }

    [Fact]
    public void ToActionResult_Success_Ok_ReturnsStatusCodeResultWithoutBody()
    {
        var result = new SuccessResult("Ok.", ResultStatus.Ok);

        var actionResult = Assert.IsType<StatusCodeResult>(result.ToActionResult());

        Assert.Equal(200, actionResult.StatusCode);
    }

    [Fact]
    public void ToActionResultOfT_Success_ReturnsBodyWithData()
    {
        var result = new SuccessDataResult<string>("hello", "Ok.", ResultStatus.Ok);

        var actionResult = Assert.IsType<ObjectResult>(result.ToActionResult());

        Assert.Equal("hello", actionResult.Value);
        Assert.Equal(200, actionResult.StatusCode);
    }

    [Fact]
    public void ToActionResult_Failure_ReturnsProblemDetailsObjectResult()
    {
        var result = new ErrorResult("Not found.", ResultStatus.NotFound, "The user does not exist.");

        var actionResult = Assert.IsType<ObjectResult>(result.ToActionResult());
        var problemDetails = Assert.IsType<ProblemDetails>(actionResult.Value);

        Assert.Equal(404, actionResult.StatusCode);
        Assert.Equal(404, problemDetails.Status);
        Assert.Equal("Not found.", problemDetails.Title);
        Assert.Equal("The user does not exist.", problemDetails.Detail);
    }

    [Fact]
    public void ToProblemDetails_WithErrors_AddsErrorsExtension()
    {
        var errors = new[] { "Field A is required." };
        var result = new ErrorResult("Validation failed.", ResultStatus.UnprocessableContent, errors);

        var problemDetails = result.ToProblemDetails();

        Assert.True(problemDetails.Extensions.TryGetValue("errors", out var errorsExtension));
        Assert.Equal(errors, errorsExtension);
    }

    [Fact]
    public void ToProblemDetails_WithFieldErrors_AddsFieldErrorsExtension()
    {
        var fieldErrors = new Dictionary<string, IReadOnlyList<string>>
        {
            ["UserNameOrEmail"] = ["'User Name Or Email' must not be empty."],
        };
        var result = new ErrorResult("Validation Failed", ResultStatus.UnprocessableContent, fieldErrors);

        var problemDetails = result.ToProblemDetails();

        Assert.True(problemDetails.Extensions.TryGetValue("fieldErrors", out var fieldErrorsExtension));
        Assert.Equal(fieldErrors, fieldErrorsExtension);
    }

    [Fact]
    public void ToProblemDetails_WithoutFieldErrors_OmitsFieldErrorsExtension()
    {
        var result = new ErrorResult("Not found.", ResultStatus.NotFound, "The user does not exist.");

        var problemDetails = result.ToProblemDetails();

        Assert.False(problemDetails.Extensions.ContainsKey("fieldErrors"));
    }

    [Fact]
    public void ToProblemDetails_UnmappedStatus_FallsBackToAboutBlank()
    {
        var result = new SuccessResult("Ok.", ResultStatus.Ok);

        var problemDetails = result.ToProblemDetails();

        Assert.Equal("about:blank", problemDetails.Type);
    }

    [Fact]
    public void ToEnvelopedActionResult_Success_WrapsWholeResult()
    {
        var result = new SuccessDataResult<int>(42, "Fetched.", ResultStatus.Ok);

        var actionResult = Assert.IsType<ObjectResult>(result.ToEnvelopedActionResult());

        Assert.Same(result, actionResult.Value);
        Assert.Equal(200, actionResult.StatusCode);
    }

    [Fact]
    public void ProblemTypes_CoversEveryResultStatus()
    {
        var missing = Enum.GetValues<ResultStatus>()
            .Where(status => !AspNetCoreResultExtensions._problemTypes.ContainsKey(status))
            .ToArray();

        var message = $"Missing problem-type mapping for: {string.Join(", ", missing)}. " +
            $"A missing entry silently falls back to \"about:blank\" instead of failing loudly.";
        Assert.True(missing.Length == 0, message);
    }
}
