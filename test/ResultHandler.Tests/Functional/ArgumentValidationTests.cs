using Microsoft.AspNetCore.Http;
using ResultHandler.AspNetCore.Extensions;
using ResultHandler.Core.Abstractions;
using ResultHandler.Core.Base;
using ResultHandler.Facade;
using ResultHandler.Functional;
using Xunit;

namespace ResultHandler.Tests.Functional;

#pragma warning disable CS8600, CS8625 // These tests pass null on purpose: public methods must reject it with ArgumentNullException.
public class ArgumentValidationTests
{
    private static readonly OperationDataResult<int> Success = Result.Success(1);

    [Fact]
    public void SyncOperators_RejectNullDelegates_WithTheParameterName()
    {
        Assert.Equal("mapper", Assert.Throws<ArgumentNullException>(() => Success.Map<int, int>(null)).ParamName);
        Assert.Equal("binder", Assert.Throws<ArgumentNullException>(() => Success.Bind<int, int>(null)).ParamName);
        Assert.Equal("predicate", Assert.Throws<ArgumentNullException>(() => Success.Ensure(null, "message")).ParamName);
        Assert.Equal("result", Assert.Throws<ArgumentNullException>(() => ((IOperationResult<int>)null).Map(value => value)).ParamName);
    }

    [Fact]
    public void AsyncOperators_RejectNullArguments_BeforeTheTaskIsAwaited()
    {
        var source = Task.FromResult(Success);

        var mapError = Assert.Throws<ArgumentNullException>(() => { _ = source.MapAsync((Func<int, int>)null); });
        var taskError = Assert.Throws<ArgumentNullException>(() => { _ = ((Task<OperationDataResult<int>>)null).MapAsync(value => value); });
        var matchError = Assert.Throws<ArgumentNullException>(() => { _ = source.MatchAsync<int, string>(null, _ => "failure"); });

        Assert.Equal("mapper", mapError.ParamName);
        Assert.Equal("resultTask", taskError.ParamName);
        Assert.Equal("onSuccess", matchError.ParamName);
    }

    [Fact]
    public void Combine_RejectsNull()
    {
        Assert.Throws<ArgumentNullException>(() => Result.Combine((IEnumerable<IOperationResult>)null));
        Assert.Throws<ArgumentNullException>(() => Result.Combine<int, int>(Success, null));
    }

    [Fact]
    public void HttpMappings_RejectANullResult_ButAcceptAMissingHttpContext()
    {
        Assert.Throws<ArgumentNullException>(() => ((IOperationResult)null).ToResult());
        Assert.Throws<ArgumentNullException>(() => ((IOperationResult)null).ToActionResult());
        Assert.NotNull(Success.ToResult(httpContext: null));
        Assert.NotNull(Success.ToEnvelopedResult((HttpContext?)null));
    }
}
#pragma warning restore CS8600, CS8625
