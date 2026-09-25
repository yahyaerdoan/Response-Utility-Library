using ResultHandler.Core.Abstractions;
using ResultHandler.Core.Base;
using ResultHandler.Core.Enums;
using ResultHandler.Facade;
using ResultHandler.Functional;
using ResultHandler.Implementations.Error;
using ResultHandler.Implementations.Success;
using Xunit;

namespace ResultHandler.Tests.Functional;

public class ResultExtensionsAsyncTests
{
    [Fact]
    public async Task MatchAsync_TaskSource_Success_InvokesOnSuccess()
    {
        var resultTask = Task.FromResult<IOperationResult<int>>(new SuccessDataResult<int>(42, "Ok.", ResultStatus.Ok));

        var output = await resultTask.MatchAsync(data => $"got {data}", failure => "failed");

        Assert.Equal("got 42", output);
    }

    [Fact]
    public async Task MatchAsync_TaskSource_Failure_InvokesOnFailure()
    {
        var resultTask = Task.FromResult<IOperationResult<int>>(new ErrorDataResult<int>("Not found.", ResultStatus.NotFound));

        var output = await resultTask.MatchAsync(data => "success", failure => $"failed: {failure.Title}");

        Assert.Equal("failed: Not found.", output);
    }

    [Fact]
    public async Task OnSuccessAsync_SyncSourceAsyncAction_RunsOnlyWhenSuccessful()
    {
        var ran = false;
        IOperationResult<int> success = new SuccessDataResult<int>(42, "Ok.", ResultStatus.Ok);
        IOperationResult<int> failure = new ErrorDataResult<int>("Not found.", ResultStatus.NotFound);

        await success.OnSuccessAsync(_ =>
        {
            ran = true;
            return Task.CompletedTask;
        });
        Assert.True(ran);

        ran = false;
        await failure.OnSuccessAsync(_ =>
        {
            ran = true;
            return Task.CompletedTask;
        });
        Assert.False(ran);
    }

    [Fact]
    public async Task OnFailureAsync_TaskSource_RunsOnlyWhenFailed()
    {
        var ran = false;
        var successTask = Task.FromResult<IOperationResult>(new SuccessResult());
        var failureTask = Task.FromResult<IOperationResult>(new ErrorResult());

        await failureTask.OnFailureAsync(_ => ran = true);
        Assert.True(ran);

        ran = false;
        await successTask.OnFailureAsync(_ => ran = true);
        Assert.False(ran);
    }

    [Fact]
    public async Task MapAsync_TaskSource_AsyncMapper_TransformsData()
    {
        var resultTask = Task.FromResult<IOperationResult<int>>(new SuccessDataResult<int>(2, "Ok.", ResultStatus.Ok));

        var mapped = await resultTask.MapAsync(value => Task.FromResult(value * 10));

        Assert.True(mapped.IsSuccessful);
        Assert.Equal(20, mapped.Data);
    }

    [Fact]
    public async Task MapAsync_Failure_PropagatesErrorWithoutInvokingMapper()
    {
        var resultTask = Task.FromResult<IOperationResult<int>>(new ErrorDataResult<int>("Not found.", ResultStatus.NotFound, "Missing."));
        var invoked = false;

        var mapped = await resultTask.MapAsync(value =>
        {
            invoked = true;
            return Task.FromResult(value * 10);
        });

        Assert.False(invoked);
        Assert.False(mapped.IsSuccessful);
        Assert.Equal(ResultStatus.NotFound, mapped.Status);
        Assert.Equal("Missing.", mapped.Detail);
    }

    [Fact]
    public async Task BindAsync_TaskSource_AsyncBinder_ChainsIntoNextOperation()
    {
        var resultTask = Task.FromResult<IOperationResult<int>>(new SuccessDataResult<int>(2, "Ok.", ResultStatus.Ok));

        var chained = await resultTask.BindAsync(value => Task.FromResult<IOperationResult<string>>(new SuccessDataResult<string>($"value={value}")));

        Assert.True(chained.IsSuccessful);
        Assert.Equal("value=2", chained.Data);
    }

    [Fact]
    public async Task BindAsync_Failure_ShortCircuitsWithoutInvokingBinder()
    {
        var resultTask = Task.FromResult<IOperationResult<int>>(new ErrorDataResult<int>("Not found.", ResultStatus.NotFound));
        var invoked = false;

        var chained = await resultTask.BindAsync(value =>
        {
            invoked = true;
            return Task.FromResult<IOperationResult<string>>(new SuccessDataResult<string>("unused"));
        });

        Assert.False(invoked);
        Assert.False(chained.IsSuccessful);
        Assert.Equal(ResultStatus.NotFound, chained.Status);
    }

    [Fact]
    public async Task EnsureAsync_TaskSource_SyncPredicateFails_ReturnsValidationFailure()
    {
        var resultTask = Task.FromResult<IOperationResult<int>>(new SuccessDataResult<int>(-5, "Ok.", ResultStatus.Ok));

        var ensured = await resultTask.EnsureAsync(value => value > 0, "Value must be positive.");

        Assert.False(ensured.IsSuccessful);
        Assert.Equal(ResultStatus.UnprocessableContent, ensured.Status);
        Assert.Equal("Value must be positive.", ensured.Detail);
    }

    [Fact]
    public async Task EnsureAsync_SyncSourceAsyncPredicate_PassesThrough()
    {
        IOperationResult<int> result = new SuccessDataResult<int>(5, "Ok.", ResultStatus.Ok);

        var ensured = await result.EnsureAsync(value => Task.FromResult(value > 0), "Value must be positive.");

        Assert.Same(result, ensured);
    }

    [Fact]
    public async Task EnsureAsync_TaskSourceAsyncPredicateFails_ShortCircuitsChain()
    {
        var chained = await GetOrderTotalAsync()
            .EnsureAsync(total => Task.FromResult(total > 0), "Order total must be positive.")
            .MapAsync(total => $"total={total}");

        Assert.False(chained.IsSuccessful);
        Assert.Equal(ResultStatus.UnprocessableContent, chained.Status);

        static Task<IOperationResult<decimal>> GetOrderTotalAsync()
            => Task.FromResult<IOperationResult<decimal>>(new SuccessDataResult<decimal>(-1m, "Ok.", ResultStatus.Ok));
    }

    [Fact]
    public async Task FluentChain_MapAsyncThenBindAsync_ComposesAcrossAwaitsWithoutIntermediateAwait()
    {
        var chained = await GetUserIdAsync()
            .MapAsync(id => $"user-{id}")
            .BindAsync(name => ValidateAsync(name));

        Assert.True(chained.IsSuccessful);
        Assert.Equal("user-7", chained.Data);

        static Task<IOperationResult<int>> GetUserIdAsync()
            => Task.FromResult<IOperationResult<int>>(new SuccessDataResult<int>(7, "Ok.", ResultStatus.Ok));

        static Task<IOperationResult<string>> ValidateAsync(string name)
            => Task.FromResult<IOperationResult<string>>(new SuccessDataResult<string>(name));
    }

    [Fact]
    public async Task BindAsync_ConcreteTaskSource_ConcreteTaskBinder_ChainsAndKeepsConcreteType()
    {
        OperationDataResult<string> result = await SendGetUserId(7).BindAsync(SendGetUserName);

        Assert.True(result.IsSuccessful);
        Assert.Equal("user-7", result.Data);
    }

    [Fact]
    public async Task BindAsync_ConcreteTaskSource_Failure_ShortCircuitsWithoutInvokingBinder()
    {
        var invoked = false;
        var source = Task.FromResult<OperationDataResult<int>>(Result.NotFound<int>("Missing."));

        var result = await source.BindAsync(id =>
        {
            invoked = true;
            return SendGetUserName(id);
        });

        Assert.False(invoked);
        Assert.Equal(ResultStatus.NotFound, result.Status);
        Assert.Equal("Missing.", result.Detail);
    }

    [Fact]
    public async Task BindAsync_ConcreteTaskSource_Failure_CarriesFieldErrors()
    {
        var fieldErrors = new Dictionary<string, IReadOnlyList<string>> { ["Name"] = ["Name is required."] };
        var source = Task.FromResult(OperationDataResult<int>.Failure(fieldErrors));

        var result = await source.BindAsync(SendGetUserName);

        Assert.Equal(["Name is required."], result.FieldErrors["Name"]);
    }

    [Fact]
    public async Task BindAsync_ConcreteTaskSource_InterfaceBinders_Resolve()
    {
        IOperationResult<string> viaSync = await SendGetUserId(3).BindAsync(id => (IOperationResult<string>)Result.Success<string>($"s{id}"));
        IOperationResult<string> viaAsync = await SendGetUserId(3).BindAsync(id => Task.FromResult<IOperationResult<string>>(Result.Success<string>($"a{id}")));

        Assert.Equal("s3", viaSync.Data);
        Assert.Equal("a3", viaAsync.Data);
    }

    [Fact]
    public async Task BindAsync_InterfaceSources_ConcreteTaskBinder_Resolve()
    {
        IOperationResult<int> value = Result.Success<int>(5);
        var fromValue = await value.BindAsync(SendGetUserName);
        var fromTask = await Task.FromResult(value).BindAsync(SendGetUserName);

        Assert.Equal("user-5", fromValue.Data);
        Assert.Equal("user-5", fromTask.Data);
    }

    [Fact]
    public async Task BindAsync_InterfaceSource_FailureWithConcreteTaskBinder_ShortCircuits()
    {
        IOperationResult<int> value = Result.Conflict<int>("Taken.");

        var result = await value.BindAsync(SendGetUserName);

        Assert.Equal(ResultStatus.Conflict, result.Status);
    }

    [Fact]
    public async Task BindAsync_InterfaceTaskSource_KeepsInterfaceReturnTypeForBothBinderShapes()
    {
        Task<IOperationResult<int>> source = Task.FromResult<IOperationResult<int>>(Result.Success<int>(1));

        Task<IOperationResult<int>> viaInterfaceBinder = source.BindAsync(x => Task.FromResult<IOperationResult<int>>(Result.Success<int>(x + 1)));
        Task<IOperationResult<int>> viaConcreteBinder = source.BindAsync(x => Task.FromResult<OperationDataResult<int>>(Result.Success<int>(x + 2)));

        Assert.Equal(2, (await viaInterfaceBinder).Data);
        Assert.Equal(3, (await viaConcreteBinder).Data);
    }

    [Fact]
    public async Task MapAsync_ConcreteTaskSource_SyncAndAsyncMappers_KeepTitleAndStatus()
    {
        var source = Task.FromResult<OperationDataResult<int>>(Result.Created<int>(4));

        OperationDataResult<string> mapped = await source.MapAsync(x => $"#{x}");
        OperationDataResult<int> mappedAsync = await source.MapAsync(x => Task.FromResult(x * 10));

        Assert.Equal("#4", mapped.Data);
        Assert.Equal(ResultStatus.Created, mapped.Status);
        Assert.Equal(40, mappedAsync.Data);
        Assert.Equal(ResultStatus.Created, mappedAsync.Status);
    }

    [Fact]
    public async Task MapAsync_ConcreteTaskSource_Failure_SkipsMapper()
    {
        var invoked = false;
        var source = Task.FromResult<OperationDataResult<int>>(Result.NotFound<int>("Missing."));

        var result = await source.MapAsync(x =>
        {
            invoked = true;
            return x;
        });

        Assert.False(invoked);
        Assert.Equal(ResultStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task ConcreteChain_BindThenMap_ComposesLikeAMediatRPipeline()
    {
        var result = await SendGetUserId(9)
            .BindAsync(SendGetUserName)
            .MapAsync(name => name.ToUpperInvariant());

        Assert.Equal("USER-9", result.Data);
    }

    [Fact]
    public async Task BindAsync_AsyncLambdaWithMixedReturns_ResolvesWithExplicitTypeArguments()
    {
        var found = await SendGetUserId(2).BindAsync<int, string>(async id =>
        {
            await Task.Yield();
            if (id < 0)
            {
                return Result.NotFound<string>("Missing.");
            }

            return Result.Success<string>($"user-{id}");
        });

        IOperationResult<int> value = Result.Success<int>(-1);
        var missing = await value.BindAsync<int, string>(async id =>
        {
            await Task.Yield();
            if (id < 0)
            {
                return Result.NotFound<string>("Missing.");
            }

            return Result.Success<string>($"user-{id}");
        });

        Assert.Equal("user-2", found.Data);
        Assert.Equal(ResultStatus.NotFound, missing.Status);
    }

    [Fact]
    public async Task BindAsync_AsyncLambdaWithExplicitReturnType_Resolves()
    {
        var result = await SendGetUserId(4).BindAsync(async Task<OperationDataResult<string>> (int id) =>
        {
            await Task.Yield();
            return id > 0 ? Result.Success<string>($"user-{id}") : Result.NotFound<string>("Missing.");
        });

        Assert.Equal("user-4", result.Data);
    }

    [Fact]
    public async Task MatchAsync_ConcreteTaskSources_ReduceBothOutcomes()
    {
        var command = Task.FromResult<OperationResult>(Result.Conflict("Taken."));

        var commandOutput = await command.MatchAsync(_ => "ok", failure => failure.Detail ?? "none");
        var commandOutputAsync = await command.MatchAsync(_ => Task.FromResult("ok"), failure => Task.FromResult(failure.Status.ToString()));
        var queryOutput = await SendGetUserId(6).MatchAsync(id => id * 2, _ => -1);
        var queryOutputAsync = await SendGetUserId(6).MatchAsync(id => Task.FromResult(id * 3), _ => Task.FromResult(-1));

        Assert.Equal("Taken.", commandOutput);
        Assert.Equal(nameof(ResultStatus.Conflict), commandOutputAsync);
        Assert.Equal(12, queryOutput);
        Assert.Equal(18, queryOutputAsync);
    }

    [Fact]
    public async Task OnSuccessAsync_ConcreteTaskSources_RunOnlyOnSuccessAndKeepConcreteType()
    {
        var seen = new List<string>();

        OperationResult command = await Task.FromResult<OperationResult>(Result.Success())
            .OnSuccessAsync(_ => seen.Add("sync"));
        await Task.FromResult<OperationResult>(Result.Success())
            .OnSuccessAsync(_ => Task.Run(() => seen.Add("async")));
        OperationDataResult<int> query = await SendGetUserId(8)
            .OnSuccessAsync(id => seen.Add($"data-{id}"));
        await SendGetUserId(9)
            .OnSuccessAsync(id => Task.Run(() => seen.Add($"async-data-{id}")));
        await Task.FromResult<OperationDataResult<int>>(Result.NotFound<int>("Missing."))
            .OnSuccessAsync(_ => seen.Add("never"));

        Assert.True(command.IsSuccessful);
        Assert.Equal(8, query.Data);
        Assert.Equal(["sync", "async", "data-8", "async-data-9"], seen);
    }

    [Fact]
    public async Task OnFailureAsync_ConcreteTaskSources_RunOnlyOnFailureAndKeepConcreteType()
    {
        var seen = new List<ResultStatus>();

        OperationResult command = await Task.FromResult<OperationResult>(Result.Conflict("Taken."))
            .OnFailureAsync(failure => seen.Add(failure.Status));
        await Task.FromResult<OperationResult>(Result.Forbidden("No."))
            .OnFailureAsync(failure => Task.Run(() => seen.Add(failure.Status)));
        OperationDataResult<int> query = await Task.FromResult<OperationDataResult<int>>(Result.NotFound<int>("Missing."))
            .OnFailureAsync(failure => seen.Add(failure.Status));
        await Task.FromResult<OperationDataResult<int>>(Result.Unauthorized<int>("Who?"))
            .OnFailureAsync(failure => Task.Run(() => seen.Add(failure.Status)));
        await SendGetUserId(1).OnFailureAsync(failure => seen.Add(failure.Status));

        Assert.False(command.IsSuccessful);
        Assert.Equal(ResultStatus.NotFound, query.Status);
        Assert.Equal([ResultStatus.Conflict, ResultStatus.Forbidden, ResultStatus.NotFound, ResultStatus.Unauthorized], seen);
    }

    [Fact]
    public async Task EnsureAsync_ConcreteTaskSource_RejectsWithDetailAndKeepsConcreteType()
    {
        OperationDataResult<int> passed = await SendGetUserId(3).EnsureAsync(id => id > 0, "Id must be positive.");
        OperationDataResult<int> rejected = await SendGetUserId(-3).EnsureAsync(id => Task.FromResult(id > 0), "Id must be positive.");

        Assert.True(passed.IsSuccessful);
        Assert.Equal(ResultStatus.UnprocessableContent, rejected.Status);
        Assert.Equal("Id must be positive.", rejected.Detail);
        Assert.Empty(rejected.Errors);
    }

    [Fact]
    public async Task EnsureAsync_ConcreteTaskSource_AlreadyFailed_SkipsPredicate()
    {
        var invoked = false;
        var source = Task.FromResult<OperationDataResult<int>>(Result.NotFound<int>("Missing."));

        var result = await source.EnsureAsync(
            _ =>
            {
                invoked = true;
                return true;
            },
            "Unused.");

        Assert.False(invoked);
        Assert.Equal(ResultStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task ConcreteChain_AllOperators_ComposeFromMediatRStyleSources()
    {
        var log = new List<string>();

        var output = await SendGetUserId(5)
            .EnsureAsync(id => id > 0, "Id must be positive.")
            .OnSuccessAsync(id => log.Add($"id={id}"))
            .BindAsync(SendGetUserName)
            .OnFailureAsync(failure => log.Add(failure.Title))
            .MatchAsync(name => name, failure => failure.Title);

        Assert.Equal("user-5", output);
        Assert.Equal(["id=5"], log);
    }

    [Fact]
    public async Task BindAsync_ConcreteTaskSource_NonGenericCommand_ChainsAndKeepsConcreteType()
    {
        OperationResult result = await SendGetUserId(3).BindAsync(SendDeleteAddress);

        Assert.True(result.IsSuccessful);
        Assert.Equal(ResultStatus.NoContent, result.Status);
    }

    [Fact]
    public async Task BindAsync_ConcreteTaskSource_NonGenericCommand_FailureShortCircuitsAsErrorResult()
    {
        var invoked = false;
        var fieldErrors = new Dictionary<string, IReadOnlyList<string>> { ["Email"] = ["Email is required."] };
        var source = Task.FromResult(OperationDataResult<int>.Failure(fieldErrors));

        var result = await source.BindAsync(id =>
        {
            invoked = true;
            return SendDeleteAddress(id);
        });

        Assert.False(invoked);
        Assert.IsType<ErrorResult>(result);
        Assert.Equal(ResultStatus.UnprocessableContent, result.Status);
        Assert.Equal(["Email is required."], result.FieldErrors["Email"]);
    }

    [Fact]
    public async Task BindAsync_ConcreteTaskSource_NonGenericCommandFailure_IsReturnedUnchanged()
    {
        var result = await SendGetUserId(-1).BindAsync(SendDeleteAddress);

        Assert.Equal(ResultStatus.Conflict, result.Status);
        Assert.Equal("Address is in use.", result.Detail);
    }

    [Fact]
    public async Task BindAsync_InterfaceSources_NonGenericBinders_Resolve()
    {
        IOperationResult<int> value = Result.Success(2);
        var interfaceTask = Task.FromResult(value);

        IOperationResult fromValueConcrete = await value.BindAsync(SendDeleteAddress);
        IOperationResult fromTaskConcrete = await interfaceTask.BindAsync(SendDeleteAddress);
        IOperationResult fromValueInterface = await value.BindAsync(id => Task.FromResult<IOperationResult>(Result.Accepted()));
        IOperationResult fromTaskInterface = await interfaceTask.BindAsync(id => Task.FromResult<IOperationResult>(Result.Accepted()));
        IOperationResult fromTaskSync = await interfaceTask.BindAsync(id => (IOperationResult)Result.Accepted());
        IOperationResult fromConcreteSync = await SendGetUserId(2).BindAsync(id => (IOperationResult)Result.Accepted());
        IOperationResult fromConcreteInterface = await SendGetUserId(2).BindAsync(id => Task.FromResult<IOperationResult>(Result.Accepted()));

        Assert.Equal(ResultStatus.NoContent, fromValueConcrete.Status);
        Assert.Equal(ResultStatus.NoContent, fromTaskConcrete.Status);
        Assert.All([fromValueInterface, fromTaskInterface, fromTaskSync, fromConcreteSync, fromConcreteInterface], r => Assert.Equal(ResultStatus.Accepted, r.Status));
    }

    [Fact]
    public async Task BindAsync_AsyncLambdaReturningNonGenericResults_BindsToNonGenericOverload()
    {
        OperationResult result = await SendGetUserId(5).BindAsync(async id =>
        {
            await Task.Yield();
            return id > 0 ? Result.NoContent() : Result.Conflict("Blocked.");
        });

        Assert.Equal(ResultStatus.NoContent, result.Status);
    }

    [Fact]
    public async Task BindAsync_LambdaWithOnlyHandBuiltSubclasses_FallsBackToNonGenericOverload()
    {
        IOperationResult<int> source = Result.Success(1);

        var bound = await source.BindAsync(async id =>
        {
            await Task.Yield();
            if (id < 0)
            {
                return new ErrorDataResult<string>("Missing.", ResultStatus.NotFound);
            }

            return new SuccessDataResult<string>($"#{id}");
        });

        var (staticType, value) = Describe(bound);

        Assert.Equal(typeof(IOperationResult), staticType);
        Assert.IsType<SuccessDataResult<string>>(value);
    }

    [Fact]
    public async Task BindAsync_DataBinders_StillBindToDataOverloads()
    {
        OperationDataResult<string> concrete = await SendGetUserId(1).BindAsync(SendGetUserName);
        OperationDataResult<int> asyncLambda = await SendGetUserId(1).BindAsync(async id =>
        {
            await Task.Yield();
            return Result.Success(id + 1);
        });
        IOperationResult<string> syncLambda = await SendGetUserId(1).BindAsync(id => Result.Success<string>($"#{id}"));

        Assert.Equal("user-1", concrete.Data);
        Assert.Equal(2, asyncLambda.Data);
        Assert.Equal("#1", syncLambda.Data);
    }

    private static Task<OperationDataResult<int>> SendGetUserId(int id)
        => Task.FromResult<OperationDataResult<int>>(Result.Success<int>(id));

    private static Task<OperationDataResult<string>> SendGetUserName(int id)
        => Task.FromResult<OperationDataResult<string>>(id > 0 ? Result.Success<string>($"user-{id}") : Result.NotFound<string>("User not found."));

    private static Task<OperationResult> SendDeleteAddress(int id)
        => Task.FromResult(id > 0 ? Result.NoContent() : Result.Conflict("Address is in use."));

    private static (Type StaticType, object? Value) Describe<TValue>(TValue value)
        => (typeof(TValue), value);
}
