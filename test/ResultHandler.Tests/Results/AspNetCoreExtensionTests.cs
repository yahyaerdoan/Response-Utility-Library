using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ResultHandler.AspNetCore.Extensions;
using ResultHandler.Core.Abstractions;
using ResultHandler.Core.Base;
using ResultHandler.Core.Enums;
using ResultHandler.Facade;
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
    public void ToProblemResult_Failure_ReturnsProblemJsonResultWithProblemDetails()
    {
        var result = new ErrorResult("Not found.", ResultStatus.NotFound, "The user does not exist.");

        var httpResult = result.ToProblemResult();
        var problemDetails = Assert.IsAssignableFrom<IValueHttpResult<ProblemDetails>>(httpResult).Value;

        Assert.NotNull(problemDetails);
        Assert.Equal(404, Assert.IsAssignableFrom<IStatusCodeHttpResult>(httpResult).StatusCode);
        Assert.Equal("application/problem+json", Assert.IsAssignableFrom<IContentTypeHttpResult>(httpResult).ContentType);
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

        var actionResult = Assert.IsAssignableFrom<ObjectResult>(result.ToActionResult());
        var problemDetails = Assert.IsType<ProblemDetails>(actionResult.Value);

        Assert.Equal(404, actionResult.StatusCode);
        Assert.Equal(404, problemDetails.Status);
        Assert.Equal("Not found.", problemDetails.Title);
        Assert.Equal("The user does not exist.", problemDetails.Detail);
    }

    [Fact]
    public void ToProblemDetails_WithFlatErrors_PutsThemUnderTheGeneralKey()
    {
        var result = new ErrorResult("Validation failed.", ResultStatus.UnprocessableContent, ["Field A is required."]);

        var problemDetails = Assert.IsType<HttpValidationProblemDetails>(result.ToProblemDetails());

        var only = Assert.Single(problemDetails.Errors);
        Assert.Equal(string.Empty, only.Key);
        Assert.Equal(["Field A is required."], only.Value);
    }

    [Fact]
    public void ToProblemDetails_WithFieldErrors_WritesTheErrorsMapOnly()
    {
        var fieldErrors = new Dictionary<string, IReadOnlyList<string>>
        {
            ["Title"] = ["Title is required."],
            ["Icon"] = ["Icon is required."],
        };
        var result = new ErrorResult("Validation Failed", ResultStatus.UnprocessableContent, fieldErrors);

        var problemDetails = Assert.IsType<HttpValidationProblemDetails>(result.ToProblemDetails());

        Assert.NotEmpty(result.Errors);
        Assert.Equal(["Icon", "Title"], problemDetails.Errors.Keys.Order());
        Assert.Equal(["Title is required."], problemDetails.Errors["Title"]);
        Assert.Empty(problemDetails.Extensions);
    }

    [Fact]
    public void ToProblemDetails_CombinedFailures_KeepFieldMessagesOnTheirFieldAndTheRestUnderTheGeneralKey()
    {
        var fieldFailure = OperationResult.Failure(new Dictionary<string, IReadOnlyList<string>> { ["Name"] = ["Name is required."] });
        var otherFailure = Result.Conflict("Name already exists.");

        var problemDetails = Assert.IsType<HttpValidationProblemDetails>(Result.Combine(fieldFailure, otherFailure).ToProblemDetails());

        Assert.Equal(["Name is required."], problemDetails.Errors["Name"]);
        Assert.Equal(["Name already exists."], problemDetails.Errors[string.Empty]);
    }

    [Fact]
    public void ToProblemDetails_WithoutErrors_IsPlainProblemDetails()
    {
        var result = new ErrorResult("Not found.", ResultStatus.NotFound, "The user does not exist.");

        var problemDetails = result.ToProblemDetails();

        Assert.IsType<ProblemDetails>(problemDetails);
        Assert.Empty(problemDetails.Extensions);
    }

    [Fact]
    public void ToProblemDetails_WithCode_AddsCodeExtension()
    {
        var problemDetails = Result.Conflict("Slug taken.").WithCode("Tenant.SlugTaken").ToProblemDetails();

        Assert.Equal("Tenant.SlugTaken", problemDetails.Extensions["code"]);
    }

    [Fact]
    public void ToProblemDetails_WithHttpContext_AddsInstanceAndTraceId()
    {
        var httpContext = new DefaultHttpContext { TraceIdentifier = "trace-1" };
        httpContext.Request.Path = "/api/things";

        var problemDetails = Result.NotFound("Missing.").ToProblemDetails(httpContext);

        Assert.Equal("/api/things", problemDetails.Instance);
        Assert.Equal(Activity.Current?.Id ?? "trace-1", problemDetails.Extensions["traceId"]);
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
