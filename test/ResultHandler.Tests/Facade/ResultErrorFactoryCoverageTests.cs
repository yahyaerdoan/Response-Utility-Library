using ResultHandler.Core.Enums;
using ResultHandler.Facade;
using ResultHandler.Implementations.Error;
using Xunit;

namespace ResultHandler.Tests.Facade;

/// <summary>Closes the coverage gap left by <see cref="ResultFacadeTests"/>: every 4xx/5xx named factory in Result.Error.cs, non-generic and generic, gets its status/title/detail checked here instead of only the handful ResultFacadeTests spot-checks.</summary>
public class ResultErrorFactoryCoverageTests
{
    public static TheoryData<Func<string, ErrorResult>, ResultStatus, string> ErrorFactories() => new()
    {
        { Result.BadRequest, ResultStatus.BadRequest, ResultTitles.BadRequest },
        { Result.Unauthorized, ResultStatus.Unauthorized, ResultTitles.Unauthorized },
        { Result.PaymentRequired, ResultStatus.PaymentRequired, ResultTitles.PaymentRequired },
        { Result.Forbidden, ResultStatus.Forbidden, ResultTitles.Forbidden },
        { Result.NotFound, ResultStatus.NotFound, ResultTitles.NotFound },
        { Result.MethodNotAllowed, ResultStatus.MethodNotAllowed, ResultTitles.MethodNotAllowed },
        { Result.NotAcceptable, ResultStatus.NotAcceptable, ResultTitles.NotAcceptable },
        { Result.ProxyAuthenticationRequired, ResultStatus.ProxyAuthenticationRequired, ResultTitles.ProxyAuthenticationRequired },
        { Result.RequestTimeout, ResultStatus.RequestTimeout, ResultTitles.RequestTimeout },
        { Result.Conflict, ResultStatus.Conflict, ResultTitles.Conflict },
        { Result.Gone, ResultStatus.Gone, ResultTitles.Gone },
        { Result.LengthRequired, ResultStatus.LengthRequired, ResultTitles.LengthRequired },
        { Result.PreconditionFailed, ResultStatus.PreconditionFailed, ResultTitles.PreconditionFailed },
        { Result.ContentTooLarge, ResultStatus.ContentTooLarge, ResultTitles.ContentTooLarge },
        { Result.UriTooLong, ResultStatus.UriTooLong, ResultTitles.UriTooLong },
        { Result.UnsupportedMediaType, ResultStatus.UnsupportedMediaType, ResultTitles.UnsupportedMediaType },
        { Result.RangeNotSatisfiable, ResultStatus.RangeNotSatisfiable, ResultTitles.RangeNotSatisfiable },
        { Result.ExpectationFailed, ResultStatus.ExpectationFailed, ResultTitles.ExpectationFailed },
        { Result.ImATeapot, ResultStatus.ImATeapot, ResultTitles.ImATeapot },
        { Result.MisdirectedRequest, ResultStatus.MisdirectedRequest, ResultTitles.MisdirectedRequest },
        { Result.UnprocessableContent, ResultStatus.UnprocessableContent, ResultTitles.UnprocessableContent },
        { Result.Locked, ResultStatus.Locked, ResultTitles.Locked },
        { Result.FailedDependency, ResultStatus.FailedDependency, ResultTitles.FailedDependency },
        { Result.TooEarly, ResultStatus.TooEarly, ResultTitles.TooEarly },
        { Result.UpgradeRequired, ResultStatus.UpgradeRequired, ResultTitles.UpgradeRequired },
        { Result.PreconditionRequired, ResultStatus.PreconditionRequired, ResultTitles.PreconditionRequired },
        { Result.TooManyRequests, ResultStatus.TooManyRequests, ResultTitles.TooManyRequests },
        { Result.RequestHeaderFieldsTooLarge, ResultStatus.RequestHeaderFieldsTooLarge, ResultTitles.RequestHeaderFieldsTooLarge },
        { Result.UnavailableForLegalReasons, ResultStatus.UnavailableForLegalReasons, ResultTitles.UnavailableForLegalReasons },
        { Result.InternalServerError, ResultStatus.InternalServerError, ResultTitles.InternalServerError },
        { Result.NotImplemented, ResultStatus.NotImplemented, ResultTitles.NotImplemented },
        { Result.BadGateway, ResultStatus.BadGateway, ResultTitles.BadGateway },
        { Result.ServiceUnavailable, ResultStatus.ServiceUnavailable, ResultTitles.ServiceUnavailable },
        { Result.GatewayTimeout, ResultStatus.GatewayTimeout, ResultTitles.GatewayTimeout },
        { Result.HttpVersionNotSupported, ResultStatus.HttpVersionNotSupported, ResultTitles.HttpVersionNotSupported },
        { Result.VariantAlsoNegotiates, ResultStatus.VariantAlsoNegotiates, ResultTitles.VariantAlsoNegotiates },
        { Result.InsufficientStorage, ResultStatus.InsufficientStorage, ResultTitles.InsufficientStorage },
        { Result.LoopDetected, ResultStatus.LoopDetected, ResultTitles.LoopDetected },
        { Result.NotExtended, ResultStatus.NotExtended, ResultTitles.NotExtended },
        { Result.NetworkAuthenticationRequired, ResultStatus.NetworkAuthenticationRequired, ResultTitles.NetworkAuthenticationRequired },
    };

    public static TheoryData<Func<string, ErrorDataResult<int>>, ResultStatus, string> ErrorDataFactories() => new()
    {
        { Result.BadRequest<int>, ResultStatus.BadRequest, ResultTitles.BadRequest },
        { Result.Unauthorized<int>, ResultStatus.Unauthorized, ResultTitles.Unauthorized },
        { Result.PaymentRequired<int>, ResultStatus.PaymentRequired, ResultTitles.PaymentRequired },
        { Result.Forbidden<int>, ResultStatus.Forbidden, ResultTitles.Forbidden },
        { Result.NotFound<int>, ResultStatus.NotFound, ResultTitles.NotFound },
        { Result.MethodNotAllowed<int>, ResultStatus.MethodNotAllowed, ResultTitles.MethodNotAllowed },
        { Result.NotAcceptable<int>, ResultStatus.NotAcceptable, ResultTitles.NotAcceptable },
        { Result.ProxyAuthenticationRequired<int>, ResultStatus.ProxyAuthenticationRequired, ResultTitles.ProxyAuthenticationRequired },
        { Result.RequestTimeout<int>, ResultStatus.RequestTimeout, ResultTitles.RequestTimeout },
        { Result.Conflict<int>, ResultStatus.Conflict, ResultTitles.Conflict },
        { Result.Gone<int>, ResultStatus.Gone, ResultTitles.Gone },
        { Result.LengthRequired<int>, ResultStatus.LengthRequired, ResultTitles.LengthRequired },
        { Result.PreconditionFailed<int>, ResultStatus.PreconditionFailed, ResultTitles.PreconditionFailed },
        { Result.ContentTooLarge<int>, ResultStatus.ContentTooLarge, ResultTitles.ContentTooLarge },
        { Result.UriTooLong<int>, ResultStatus.UriTooLong, ResultTitles.UriTooLong },
        { Result.UnsupportedMediaType<int>, ResultStatus.UnsupportedMediaType, ResultTitles.UnsupportedMediaType },
        { Result.RangeNotSatisfiable<int>, ResultStatus.RangeNotSatisfiable, ResultTitles.RangeNotSatisfiable },
        { Result.ExpectationFailed<int>, ResultStatus.ExpectationFailed, ResultTitles.ExpectationFailed },
        { Result.ImATeapot<int>, ResultStatus.ImATeapot, ResultTitles.ImATeapot },
        { Result.MisdirectedRequest<int>, ResultStatus.MisdirectedRequest, ResultTitles.MisdirectedRequest },
        { Result.UnprocessableContent<int>, ResultStatus.UnprocessableContent, ResultTitles.UnprocessableContent },
        { Result.Locked<int>, ResultStatus.Locked, ResultTitles.Locked },
        { Result.FailedDependency<int>, ResultStatus.FailedDependency, ResultTitles.FailedDependency },
        { Result.TooEarly<int>, ResultStatus.TooEarly, ResultTitles.TooEarly },
        { Result.UpgradeRequired<int>, ResultStatus.UpgradeRequired, ResultTitles.UpgradeRequired },
        { Result.PreconditionRequired<int>, ResultStatus.PreconditionRequired, ResultTitles.PreconditionRequired },
        { Result.TooManyRequests<int>, ResultStatus.TooManyRequests, ResultTitles.TooManyRequests },
        { Result.RequestHeaderFieldsTooLarge<int>, ResultStatus.RequestHeaderFieldsTooLarge, ResultTitles.RequestHeaderFieldsTooLarge },
        { Result.UnavailableForLegalReasons<int>, ResultStatus.UnavailableForLegalReasons, ResultTitles.UnavailableForLegalReasons },
        { Result.InternalServerError<int>, ResultStatus.InternalServerError, ResultTitles.InternalServerError },
        { Result.NotImplemented<int>, ResultStatus.NotImplemented, ResultTitles.NotImplemented },
        { Result.BadGateway<int>, ResultStatus.BadGateway, ResultTitles.BadGateway },
        { Result.ServiceUnavailable<int>, ResultStatus.ServiceUnavailable, ResultTitles.ServiceUnavailable },
        { Result.GatewayTimeout<int>, ResultStatus.GatewayTimeout, ResultTitles.GatewayTimeout },
        { Result.HttpVersionNotSupported<int>, ResultStatus.HttpVersionNotSupported, ResultTitles.HttpVersionNotSupported },
        { Result.VariantAlsoNegotiates<int>, ResultStatus.VariantAlsoNegotiates, ResultTitles.VariantAlsoNegotiates },
        { Result.InsufficientStorage<int>, ResultStatus.InsufficientStorage, ResultTitles.InsufficientStorage },
        { Result.LoopDetected<int>, ResultStatus.LoopDetected, ResultTitles.LoopDetected },
        { Result.NotExtended<int>, ResultStatus.NotExtended, ResultTitles.NotExtended },
        { Result.NetworkAuthenticationRequired<int>, ResultStatus.NetworkAuthenticationRequired, ResultTitles.NetworkAuthenticationRequired },
    };

    [Theory]
    [MemberData(nameof(ErrorFactories))]
    public void ErrorFactory_SetsExpectedStatusTitleAndDetail(Func<string, ErrorResult> factory, ResultStatus expectedStatus, string expectedTitle)
    {
        var result = factory("detail text");

        Assert.False(result.IsSuccessful);
        Assert.Equal(expectedStatus, result.Status);
        Assert.Equal(expectedTitle, result.Title);
        Assert.Equal("detail text", result.Detail);
    }

    [Theory]
    [MemberData(nameof(ErrorDataFactories))]
    public void ErrorDataFactory_SetsExpectedStatusTitleAndDefaultData(Func<string, ErrorDataResult<int>> factory, ResultStatus expectedStatus, string expectedTitle)
    {
        var result = factory("detail text");

        Assert.False(result.IsSuccessful);
        Assert.Equal(expectedStatus, result.Status);
        Assert.Equal(expectedTitle, result.Title);
        Assert.Equal(0, result.Data);
    }
}
