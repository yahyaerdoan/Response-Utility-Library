using ResultHandler.Core.Base;
using ResultHandler.Core.Enums;
using ResultHandler.Facade;
using ResultHandler.Functional;
using Xunit;

namespace ResultHandler.Tests.Functional;

/// <summary>
/// Closes the coverage gap left by <see cref="ResultFailureFactoryTests"/>: every named shortcut gets
/// checked here against the matching <see cref="Result"/> facade method it delegates to, verifying the
/// "one source of truth" claim in <see cref="ResultFailureFactory"/>'s own documentation.
/// </summary>
public class ResultFailureFactoryCoverageTests
{
    public static TheoryData<Func<string, OperationResult>, Func<string, OperationResult>> ShortcutsAgainstFacade() => new()
    {
        { detail => ResultFailureFactory.BadRequest<OperationResult>(detail), detail => Result.BadRequest(detail) },
        { detail => ResultFailureFactory.Unauthorized<OperationResult>(detail), detail => Result.Unauthorized(detail) },
        { detail => ResultFailureFactory.PaymentRequired<OperationResult>(detail), detail => Result.PaymentRequired(detail) },
        { detail => ResultFailureFactory.Forbidden<OperationResult>(detail), detail => Result.Forbidden(detail) },
        { detail => ResultFailureFactory.NotFound<OperationResult>(detail), detail => Result.NotFound(detail) },
        { detail => ResultFailureFactory.MethodNotAllowed<OperationResult>(detail), detail => Result.MethodNotAllowed(detail) },
        { detail => ResultFailureFactory.NotAcceptable<OperationResult>(detail), detail => Result.NotAcceptable(detail) },
        { detail => ResultFailureFactory.ProxyAuthenticationRequired<OperationResult>(detail), detail => Result.ProxyAuthenticationRequired(detail) },
        { detail => ResultFailureFactory.RequestTimeout<OperationResult>(detail), detail => Result.RequestTimeout(detail) },
        { detail => ResultFailureFactory.Conflict<OperationResult>(detail), detail => Result.Conflict(detail) },
        { detail => ResultFailureFactory.Gone<OperationResult>(detail), detail => Result.Gone(detail) },
        { detail => ResultFailureFactory.LengthRequired<OperationResult>(detail), detail => Result.LengthRequired(detail) },
        { detail => ResultFailureFactory.PreconditionFailed<OperationResult>(detail), detail => Result.PreconditionFailed(detail) },
        { detail => ResultFailureFactory.ContentTooLarge<OperationResult>(detail), detail => Result.ContentTooLarge(detail) },
        { detail => ResultFailureFactory.UriTooLong<OperationResult>(detail), detail => Result.UriTooLong(detail) },
        { detail => ResultFailureFactory.UnsupportedMediaType<OperationResult>(detail), detail => Result.UnsupportedMediaType(detail) },
        { detail => ResultFailureFactory.RangeNotSatisfiable<OperationResult>(detail), detail => Result.RangeNotSatisfiable(detail) },
        { detail => ResultFailureFactory.ExpectationFailed<OperationResult>(detail), detail => Result.ExpectationFailed(detail) },
        { detail => ResultFailureFactory.ImATeapot<OperationResult>(detail), detail => Result.ImATeapot(detail) },
        { detail => ResultFailureFactory.MisdirectedRequest<OperationResult>(detail), detail => Result.MisdirectedRequest(detail) },
        { detail => ResultFailureFactory.UnprocessableContent<OperationResult>(detail), detail => Result.UnprocessableContent(detail) },
        { detail => ResultFailureFactory.Locked<OperationResult>(detail), detail => Result.Locked(detail) },
        { detail => ResultFailureFactory.FailedDependency<OperationResult>(detail), detail => Result.FailedDependency(detail) },
        { detail => ResultFailureFactory.TooEarly<OperationResult>(detail), detail => Result.TooEarly(detail) },
        { detail => ResultFailureFactory.UpgradeRequired<OperationResult>(detail), detail => Result.UpgradeRequired(detail) },
        { detail => ResultFailureFactory.PreconditionRequired<OperationResult>(detail), detail => Result.PreconditionRequired(detail) },
        { detail => ResultFailureFactory.TooManyRequests<OperationResult>(detail), detail => Result.TooManyRequests(detail) },
        { detail => ResultFailureFactory.RequestHeaderFieldsTooLarge<OperationResult>(detail), detail => Result.RequestHeaderFieldsTooLarge(detail) },
        { detail => ResultFailureFactory.UnavailableForLegalReasons<OperationResult>(detail), detail => Result.UnavailableForLegalReasons(detail) },
        { detail => ResultFailureFactory.InternalServerError<OperationResult>(detail), detail => Result.InternalServerError(detail) },
        { detail => ResultFailureFactory.NotImplemented<OperationResult>(detail), detail => Result.NotImplemented(detail) },
        { detail => ResultFailureFactory.BadGateway<OperationResult>(detail), detail => Result.BadGateway(detail) },
        { detail => ResultFailureFactory.ServiceUnavailable<OperationResult>(detail), detail => Result.ServiceUnavailable(detail) },
        { detail => ResultFailureFactory.GatewayTimeout<OperationResult>(detail), detail => Result.GatewayTimeout(detail) },
        { detail => ResultFailureFactory.HttpVersionNotSupported<OperationResult>(detail), detail => Result.HttpVersionNotSupported(detail) },
        { detail => ResultFailureFactory.VariantAlsoNegotiates<OperationResult>(detail), detail => Result.VariantAlsoNegotiates(detail) },
        { detail => ResultFailureFactory.InsufficientStorage<OperationResult>(detail), detail => Result.InsufficientStorage(detail) },
        { detail => ResultFailureFactory.LoopDetected<OperationResult>(detail), detail => Result.LoopDetected(detail) },
        { detail => ResultFailureFactory.NotExtended<OperationResult>(detail), detail => Result.NotExtended(detail) },
        { detail => ResultFailureFactory.NetworkAuthenticationRequired<OperationResult>(detail), detail => Result.NetworkAuthenticationRequired(detail) },
    };

    [Theory]
    [MemberData(nameof(ShortcutsAgainstFacade))]
    public void Shortcut_MatchesTitleStatusAndDetailOfMatchingFacadeMethod(Func<string, OperationResult> shortcut, Func<string, OperationResult> facadeMethod)
    {
        var fromShortcut = shortcut("detail text");
        var fromFacade = facadeMethod("detail text");

        Assert.False(fromShortcut.IsSuccessful);
        Assert.Equal(fromFacade.Status, fromShortcut.Status);
        Assert.Equal(fromFacade.Title, fromShortcut.Title);
        Assert.Equal(fromFacade.Detail, fromShortcut.Detail);
    }
}
