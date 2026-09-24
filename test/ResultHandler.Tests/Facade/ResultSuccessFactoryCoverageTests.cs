using ResultHandler.Core.Base;
using ResultHandler.Core.Enums;
using ResultHandler.Facade;
using ResultHandler.Implementations.Success;
using Xunit;

namespace ResultHandler.Tests.Facade;

/// <summary>Closes the coverage gap left by <see cref="ResultFacadeTests"/>: every named factory in Result.Success.cs, non-generic and generic, gets its status checked here.</summary>
public class ResultSuccessFactoryCoverageTests
{
    public static TheoryData<Func<OperationResult>, ResultStatus> ParameterlessFactories() => new()
    {
        { () => Result.Continue(), ResultStatus.Continue },
        { () => Result.SwitchingProtocols(), ResultStatus.SwitchingProtocols },
        { () => Result.Processing(), ResultStatus.Processing },
        { () => Result.EarlyHints(), ResultStatus.EarlyHints },
        { () => Result.Created(), ResultStatus.Created },
        { () => Result.Accepted(), ResultStatus.Accepted },
        { Result.NoContent, ResultStatus.NoContent },
        { Result.ResetContent, ResultStatus.ResetContent },
        { Result.NonAuthoritativeInformation, ResultStatus.NonAuthoritativeInformation },
        { Result.PartialContent, ResultStatus.PartialContent },
        { Result.MultiStatus, ResultStatus.MultiStatus },
        { Result.AlreadyReported, ResultStatus.AlreadyReported },
        { Result.ImUsed, ResultStatus.ImUsed },
        { Result.NotModified, ResultStatus.NotModified },
    };

    public static TheoryData<Func<int, OperationDataResult<int>>, ResultStatus> ParameterlessDataFactories() => new()
    {
        { data => Result.Created(data), ResultStatus.Created },
        { data => Result.Accepted(data), ResultStatus.Accepted },
        { data => Result.NonAuthoritativeInformation(data), ResultStatus.NonAuthoritativeInformation },
        { data => Result.PartialContent(data), ResultStatus.PartialContent },
        { data => Result.MultiStatus(data), ResultStatus.MultiStatus },
        { data => Result.AlreadyReported(data), ResultStatus.AlreadyReported },
        { data => Result.ImUsed(data), ResultStatus.ImUsed },
        { data => Result.NotModified(data), ResultStatus.NotModified },
    };

    public static TheoryData<Func<string, OperationResult>, ResultStatus> RedirectFactories() => new()
    {
        { Result.MovedPermanently, ResultStatus.MovedPermanently },
        { Result.Found, ResultStatus.Found },
        { Result.SeeOther, ResultStatus.SeeOther },
        { Result.UseProxy, ResultStatus.UseProxy },
        { Result.TemporaryRedirect, ResultStatus.TemporaryRedirect },
        { Result.PermanentRedirect, ResultStatus.PermanentRedirect },
    };

    public static TheoryData<Func<int, string, OperationDataResult<int>>, ResultStatus> RedirectDataFactories() => new()
    {
        { Result.MovedPermanently, ResultStatus.MovedPermanently },
        { Result.Found, ResultStatus.Found },
        { Result.SeeOther, ResultStatus.SeeOther },
        { Result.UseProxy, ResultStatus.UseProxy },
        { Result.TemporaryRedirect, ResultStatus.TemporaryRedirect },
        { Result.PermanentRedirect, ResultStatus.PermanentRedirect },
    };

    [Theory]
    [MemberData(nameof(ParameterlessFactories))]
    public void ParameterlessFactory_SetsExpectedStatus(Func<OperationResult> factory, ResultStatus expectedStatus)
    {
        var result = factory();

        Assert.True(result.IsSuccessful);
        Assert.Equal(expectedStatus, result.Status);
    }

    [Theory]
    [MemberData(nameof(ParameterlessDataFactories))]
    public void DataFactory_CarriesDataAndSetsExpectedStatus(Func<int, OperationDataResult<int>> factory, ResultStatus expectedStatus)
    {
        var result = factory(42);

        Assert.True(result.IsSuccessful);
        Assert.Equal(expectedStatus, result.Status);
        Assert.Equal(42, result.Data);
    }

    [Theory]
    [MemberData(nameof(RedirectFactories))]
    public void RedirectFactory_InterpolatesLocationIntoTitleAndSetsExpectedStatus(Func<string, OperationResult> factory, ResultStatus expectedStatus)
    {
        var result = factory("https://example.com/target");

        Assert.True(result.IsSuccessful);
        Assert.Equal(expectedStatus, result.Status);
        Assert.Contains("https://example.com/target", result.Title);
    }

    [Theory]
    [MemberData(nameof(RedirectDataFactories))]
    public void RedirectDataFactory_CarriesDataAndInterpolatesLocation(Func<int, string, OperationDataResult<int>> factory, ResultStatus expectedStatus)
    {
        var result = factory(42, "https://example.com/target");

        Assert.True(result.IsSuccessful);
        Assert.Equal(expectedStatus, result.Status);
        Assert.Equal(42, result.Data);
        Assert.Contains("https://example.com/target", result.Title);
    }

    [Fact]
    public void CreatedAndAccepted_DefaultAndCustomTitles()
    {
        Assert.Equal(ResultTitles.Created, Result.Created().Title);
        Assert.Equal("Order placed.", Result.Created("Order placed.").Title);
        Assert.Equal(ResultTitles.Created, Result.Created(7).Title);
        Assert.Equal("Order placed.", Result.Created(7, "Order placed.").Title);
        Assert.Equal(ResultTitles.Accepted, Result.Accepted().Title);
        Assert.Equal("Queued.", Result.Accepted("Queued.").Title);
        Assert.Equal(ResultTitles.Accepted, Result.Accepted(7).Title);
        Assert.Equal("Queued.", Result.Accepted(7, "Queued.").Title);
        Assert.Equal("Queued.", Result.Accepted(7, title: "Queued.").Title);
    }

    [Fact]
    public void CreatedAndAccepted_AreUsableAsMethodGroups()
    {
        Func<OperationResult> created = Result.Created;
        Func<int, OperationDataResult<int>> accepted = Result.Accepted;

        Assert.Equal(ResultStatus.Created, created().Status);
        Assert.Equal(ResultStatus.Accepted, accepted(1).Status);
    }

    [Fact]
    public void MultipleChoices_UsesGivenDetailAsTitle()
    {
        var result = Result.MultipleChoices("Pick one of the listed options.");

        Assert.True(result.IsSuccessful);
        Assert.Equal(ResultStatus.MultipleChoices, result.Status);
        Assert.Equal("Pick one of the listed options.", result.Title);
    }

    [Fact]
    public void MultipleChoicesOfT_CarriesDataAndUsesGivenDetailAsTitle()
    {
        var result = Result.MultipleChoices(42, "Pick one of the listed options.");

        Assert.True(result.IsSuccessful);
        Assert.Equal(ResultStatus.MultipleChoices, result.Status);
        Assert.Equal(42, result.Data);
        Assert.Equal("Pick one of the listed options.", result.Title);
    }
}
