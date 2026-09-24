using ResultHandler.Core.Abstractions;
using ResultHandler.Core.Base;
using ResultHandler.Core.Enums;
using ResultHandler.Facade;
using ResultHandler.Functional;
using ResultHandler.Implementations.Error;
using ResultHandler.Implementations.Success;
using Xunit;

namespace ResultHandler.Tests.Facade;

public class ResultFacadeReturnTypeTests
{
    [Fact]
    public void ConditionalExpression_MixingSuccessAndFailure_HasCommonType()
    {
        static OperationDataResult<int> Parse(string input)
            => int.TryParse(input, out var value) ? Result.Success(value) : Result.BadRequest<int>("Not a number.");

        Assert.Equal(42, Parse("42").Data);
        Assert.Equal(ResultStatus.BadRequest, Parse("x").Status);
    }

    [Fact]
    public void Bind_LambdaMixingSuccessAndFailure_InfersTypeArguments()
    {
        IOperationResult<int> source = Result.Success(-1);

        var bound = source.Bind(value =>
        {
            if (value < 0)
            {
                return Result.NotFound<string>("Missing.");
            }

            return Result.Success<string>($"#{value}");
        });

        Assert.Equal(ResultStatus.NotFound, bound.Status);
    }

    [Fact]
    public async Task BindAsync_AsyncLambdaMixingSuccessAndFailure_InfersTypeArguments()
    {
        var source = Task.FromResult(Result.Success(-1));

        var bound = await source.BindAsync(async value =>
        {
            await Task.Yield();
            if (value < 0)
            {
                return Result.NotFound<string>("Missing.");
            }

            return Result.Success<string>($"#{value}");
        });

        Assert.Equal(ResultStatus.NotFound, bound.Status);
    }

    [Fact]
    public async Task BindAsync_AsyncLambdaMixingFacadeAndSubclass_InfersTypeArguments()
    {
        IOperationResult<int> source = Result.Success(7);

        var bound = await source.BindAsync(async value =>
        {
            await Task.Yield();
            if (value < 0)
            {
                return new ErrorDataResult<string>("Missing.", ResultStatus.NotFound);
            }

            return Result.Success<string>($"#{value}");
        });

        Assert.Equal("#7", bound.Data);
    }

    [Fact]
    public void NonGenericConditional_MixingSuccessAndFailure_HasCommonType()
    {
        static OperationResult Delete(bool exists)
            => exists ? Result.NoContent() : Result.NotFound("Missing.");

        Assert.True(Delete(true).IsSuccessful);
        Assert.Equal(ResultStatus.NotFound, Delete(false).Status);
    }

    [Fact]
    public void Factories_StillCreateTheConcreteSubclassesAtRuntime()
    {
        Assert.IsType<SuccessDataResult<int>>(Result.Success(1));
        Assert.IsType<ErrorDataResult<int>>(Result.Conflict<int>("Taken."));
        Assert.IsType<SuccessResult>(Result.Success());
        Assert.IsType<ErrorResult>(Result.Forbidden());
    }
}
