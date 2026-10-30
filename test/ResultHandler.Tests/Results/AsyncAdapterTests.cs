using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ResultHandler.AspNetCore.Envelopes;
using ResultHandler.AspNetCore.Extensions;
using ResultHandler.Core.Abstractions;
using ResultHandler.Facade;
using Xunit;

namespace ResultHandler.Tests.Results;

public class AsyncAdapterTests
{
    [Fact]
    public async Task ToResultAsync_ConcreteDataTask_ReturnsTheRawData()
    {
        var httpResult = await Task.FromResult(Result.Success<string>("receipt")).ToResultAsync();

        var json = Assert.IsType<JsonHttpResult<string>>(httpResult);
        Assert.Equal("receipt", json.Value);
        Assert.Equal(200, json.StatusCode);
    }

    [Fact]
    public async Task ToResultAsync_InterfaceDataTask_Failure_ReturnsProblemDetails()
    {
        var httpResult = await Task.FromResult<IOperationResult<string>>(Result.NotFound<string>("Missing.")).ToResultAsync();

        Assert.Equal(404, Assert.IsAssignableFrom<IStatusCodeHttpResult>(httpResult).StatusCode);
        Assert.Equal("Missing.", Assert.IsAssignableFrom<IValueHttpResult<ProblemDetails>>(httpResult).Value?.Detail);
    }

    [Fact]
    public async Task ToResultAsync_CommandTasks_AreBodyless()
    {
        Assert.IsType<NoContent>(await Task.FromResult(Result.NoContent()).ToResultAsync());
        Assert.IsType<NoContent>(await Task.FromResult<IOperationResult>(Result.NoContent()).ToResultAsync());
    }

    [Fact]
    public async Task ToEnvelopedResultAsync_WrapsTheWholeResult_ForEveryTaskShape()
    {
        Assert.IsType<JsonHttpResult<ResultEnvelope>>(await Task.FromResult(Result.Success<string>("x")).ToEnvelopedResultAsync());
        Assert.IsType<JsonHttpResult<ResultEnvelope>>(await Task.FromResult<IOperationResult<string>>(Result.Success<string>("x")).ToEnvelopedResultAsync());
        Assert.IsType<JsonHttpResult<ResultEnvelope>>(await Task.FromResult(Result.Success()).ToEnvelopedResultAsync());
        Assert.IsType<JsonHttpResult<ResultEnvelope>>(await Task.FromResult<IOperationResult>(Result.Success()).ToEnvelopedResultAsync());
    }

    [Fact]
    public async Task ToActionResultAsync_MapsDataAndFailures()
    {
        var success = Assert.IsType<ObjectResult>(await Task.FromResult(Result.Success<string>("receipt")).ToActionResultAsync());
        var failure = Assert.IsAssignableFrom<ObjectResult>(await Task.FromResult<IOperationResult>(Result.Conflict("Locked.")).ToActionResultAsync());

        Assert.Equal("receipt", success.Value);
        Assert.Equal(409, failure.StatusCode);
        Assert.IsType<ProblemDetails>(failure.Value);
        Assert.IsType<NoContentResult>(await Task.FromResult(Result.NoContent()).ToActionResultAsync());
        Assert.IsType<ObjectResult>(await Task.FromResult<IOperationResult<int>>(Result.Success(1)).ToActionResultAsync());
    }

    [Fact]
    public async Task ToEnvelopedActionResultAsync_WrapsTheWholeResult()
    {
        var data = Assert.IsType<ObjectResult>(await Task.FromResult(Result.Success<string>("x")).ToEnvelopedActionResultAsync());
        var command = Assert.IsType<ObjectResult>(await Task.FromResult(Result.Success()).ToEnvelopedActionResultAsync());

        Assert.IsType<ResultEnvelope<string>>(data.Value);
        Assert.IsType<ResultEnvelope>(command.Value);
        Assert.IsType<ObjectResult>(await Task.FromResult<IOperationResult<string>>(Result.Success<string>("x")).ToEnvelopedActionResultAsync());
        Assert.IsType<ObjectResult>(await Task.FromResult<IOperationResult>(Result.Success()).ToEnvelopedActionResultAsync());
    }
}
