using ResultHandler.Core.Enums;
using ResultHandler.Implementations.Success;

namespace ResultHandler.Facade;

public static partial class Result
{
    /// <summary>Success (100 Continue).</summary>
    public static SuccessResult Continue(string title = ResultTitles.Continue)
        => new(title, ResultStatus.Continue);

    /// <summary>Success (101 Switching Protocols).</summary>
    public static SuccessResult SwitchingProtocols(string title = ResultTitles.SwitchingProtocols)
        => new(title, ResultStatus.SwitchingProtocols);

    /// <summary>Success (102 Processing).</summary>
    public static SuccessResult Processing(string title = ResultTitles.Processing)
        => new(title, ResultStatus.Processing);

    /// <summary>Success (103 Early Hints).</summary>
    public static SuccessResult EarlyHints(string title = ResultTitles.EarlyHints)
        => new(title, ResultStatus.EarlyHints);

    /// <summary>Success (200 Ok) with the default title, no data.</summary>
    public static SuccessResult Success()
        => new();

    /// <summary>Success (200 Ok) with a custom title, no data.</summary>
    public static SuccessResult Success(string title)
        => new(title);

    /// <summary>Success (200 Ok) carrying <paramref name="data"/>, with the default title.</summary>
    public static SuccessDataResult<T> Success<T>(T data)
        => new(data);

    /// <summary>Success (200 Ok) carrying <paramref name="data"/>, with a custom title.</summary>
    public static SuccessDataResult<T> Success<T>(T data, string title)
        => new(data, title);

    /// <summary>Success (201 Created), no data.</summary>
    public static SuccessResult Created(string title = ResultTitles.Created)
        => new(title, ResultStatus.Created);

    /// <summary>Success (201 Created) carrying the newly-created <paramref name="data"/>.</summary>
    public static SuccessDataResult<T> Created<T>(T data, string title = ResultTitles.Created)
        => new(data, title, ResultStatus.Created);

    /// <summary>Success (202 Accepted), no data.</summary>
    public static SuccessResult Accepted(string title = ResultTitles.Accepted)
        => new(title, ResultStatus.Accepted);

    /// <summary>Success (202 Accepted) carrying <paramref name="data"/>.</summary>
    public static SuccessDataResult<T> Accepted<T>(T data, string title = ResultTitles.Accepted)
        => new(data, title, ResultStatus.Accepted);

    /// <summary>Success (204 No Content).</summary>
    public static SuccessResult NoContent()
        => new(ResultTitles.NoContent, ResultStatus.NoContent);

    /// <summary>Success (205 Reset Content).</summary>
    public static SuccessResult ResetContent()
        => new(ResultTitles.ResetContent, ResultStatus.ResetContent);

    /// <summary>Success (203 Non-Authoritative Information), no data.</summary>
    public static SuccessResult NonAuthoritativeInformation()
        => new(ResultTitles.NonAuthoritativeInformation, ResultStatus.NonAuthoritativeInformation);

    /// <summary>Success (203 Non-Authoritative Information) carrying <paramref name="data"/>.</summary>
    public static SuccessDataResult<T> NonAuthoritativeInformation<T>(T data, string title = ResultTitles.NonAuthoritativeInformation)
        => new(data, title, ResultStatus.NonAuthoritativeInformation);

    /// <summary>Success (206 Partial Content), no data.</summary>
    public static SuccessResult PartialContent()
        => new(ResultTitles.PartialContent, ResultStatus.PartialContent);

    /// <summary>Success (206 Partial Content) carrying <paramref name="data"/>.</summary>
    public static SuccessDataResult<T> PartialContent<T>(T data, string title = ResultTitles.PartialContent)
        => new(data, title, ResultStatus.PartialContent);

    /// <summary>Success (207 Multi-Status), no data.</summary>
    public static SuccessResult MultiStatus()
        => new(ResultTitles.MultiStatus, ResultStatus.MultiStatus);

    /// <summary>Success (207 Multi-Status) carrying <paramref name="data"/>.</summary>
    public static SuccessDataResult<T> MultiStatus<T>(T data, string title = ResultTitles.MultiStatus)
        => new(data, title, ResultStatus.MultiStatus);

    /// <summary>Success (208 Already Reported), no data.</summary>
    public static SuccessResult AlreadyReported()
        => new(ResultTitles.AlreadyReported, ResultStatus.AlreadyReported);

    /// <summary>Success (208 Already Reported) carrying <paramref name="data"/>.</summary>
    public static SuccessDataResult<T> AlreadyReported<T>(T data, string title = ResultTitles.AlreadyReported)
        => new(data, title, ResultStatus.AlreadyReported);

    /// <summary>Success (226 IM Used), no data.</summary>
    public static SuccessResult ImUsed()
        => new(ResultTitles.ImUsed, ResultStatus.ImUsed);

    /// <summary>Success (226 IM Used) carrying <paramref name="data"/>.</summary>
    public static SuccessDataResult<T> ImUsed<T>(T data, string title = ResultTitles.ImUsed)
        => new(data, title, ResultStatus.ImUsed);

    /// <summary>Redirect (300 Multiple Choices), no data.</summary>
    public static SuccessResult MultipleChoices(string detail)
        => new(detail, ResultStatus.MultipleChoices);

    /// <summary>Redirect (300 Multiple Choices) carrying <paramref name="data"/>.</summary>
    public static SuccessDataResult<T> MultipleChoices<T>(T data, string detail)
        => new(data, detail, ResultStatus.MultipleChoices);

    /// <summary>Redirect (301 Moved Permanently); <paramref name="location"/> is interpolated into the title.</summary>
    public static SuccessResult MovedPermanently(string location)
        => new(string.Format(ResultTitles.MovedPermanentlyTemplate, location), ResultStatus.MovedPermanently);

    /// <inheritdoc cref="MovedPermanently(string)"/>
    public static SuccessDataResult<T> MovedPermanently<T>(T data, string location)
        => new(data, string.Format(ResultTitles.MovedPermanentlyTemplate, location), ResultStatus.MovedPermanently);

    /// <summary>Redirect (302 Found); <paramref name="location"/> is interpolated into the title.</summary>
    public static SuccessResult Found(string location)
        => new(string.Format(ResultTitles.FoundTemplate, location), ResultStatus.Found);

    /// <inheritdoc cref="Found(string)"/>
    public static SuccessDataResult<T> Found<T>(T data, string location)
        => new(data, string.Format(ResultTitles.FoundTemplate, location), ResultStatus.Found);

    /// <summary>Redirect (303 See Other); <paramref name="location"/> is interpolated into the title.</summary>
    public static SuccessResult SeeOther(string location)
        => new(string.Format(ResultTitles.SeeOtherTemplate, location), ResultStatus.SeeOther);

    /// <inheritdoc cref="SeeOther(string)"/>
    public static SuccessDataResult<T> SeeOther<T>(T data, string location)
        => new(data, string.Format(ResultTitles.SeeOtherTemplate, location), ResultStatus.SeeOther);

    /// <summary>Redirect (305 Use Proxy); <paramref name="proxy"/> is interpolated into the title.</summary>
    public static SuccessResult UseProxy(string proxy)
        => new(string.Format(ResultTitles.UseProxyTemplate, proxy), ResultStatus.UseProxy);

    /// <inheritdoc cref="UseProxy(string)"/>
    public static SuccessDataResult<T> UseProxy<T>(T data, string proxy)
        => new(data, string.Format(ResultTitles.UseProxyTemplate, proxy), ResultStatus.UseProxy);

    /// <summary>Success (304 Not Modified), no data.</summary>
    public static SuccessResult NotModified()
        => new(ResultTitles.NotModified, ResultStatus.NotModified);

    /// <summary>Success (304 Not Modified) carrying <paramref name="data"/>.</summary>
    public static SuccessDataResult<T> NotModified<T>(T data)
        => new(data, ResultTitles.NotModified, ResultStatus.NotModified);

    /// <summary>Redirect (307 Temporary Redirect); <paramref name="location"/> is interpolated into the title.</summary>
    public static SuccessResult TemporaryRedirect(string location)
        => new(string.Format(ResultTitles.TemporaryRedirectTemplate, location), ResultStatus.TemporaryRedirect);

    /// <inheritdoc cref="TemporaryRedirect(string)"/>
    public static SuccessDataResult<T> TemporaryRedirect<T>(T data, string location)
        => new(data, string.Format(ResultTitles.TemporaryRedirectTemplate, location), ResultStatus.TemporaryRedirect);

    /// <summary>Redirect (308 Permanent Redirect); <paramref name="location"/> is interpolated into the title.</summary>
    public static SuccessResult PermanentRedirect(string location)
        => new(string.Format(ResultTitles.PermanentRedirectTemplate, location), ResultStatus.PermanentRedirect);

    /// <inheritdoc cref="PermanentRedirect(string)"/>
    public static SuccessDataResult<T> PermanentRedirect<T>(T data, string location)
        => new(data, string.Format(ResultTitles.PermanentRedirectTemplate, location), ResultStatus.PermanentRedirect);
}
