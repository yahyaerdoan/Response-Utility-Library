using ResultHandler.Core.Base;
using ResultHandler.Core.Enums;
using ResultHandler.Implementations.Success;

namespace ResultHandler.Facade;

public static partial class Result
{
    /// <summary>Success (200 Ok) with the default title, no data.</summary>
    public static OperationResult Success()
        => new SuccessResult();

    /// <summary>Success (200 Ok) with a custom title, no data.</summary>
    public static OperationResult Success(string title)
        => new SuccessResult(title);

    /// <summary>Success (200 Ok) carrying <paramref name="data"/>, with the default title.</summary>
    public static OperationDataResult<T> Success<T>(T data)
        => new SuccessDataResult<T>(data);

    /// <summary>Success (200 Ok) carrying <paramref name="data"/>, with a custom title.</summary>
    public static OperationDataResult<T> Success<T>(T data, string title)
        => new SuccessDataResult<T>(data, title);

    /// <summary>Success (201 Created) with the default title, no data.</summary>
    public static OperationResult Created()
        => Created(ResultTitles.Created);

    /// <summary>Success (201 Created) with a custom title, no data.</summary>
    public static OperationResult Created(string title)
        => new SuccessResult(title, ResultStatus.Created);

    /// <summary>Success (201 Created) carrying the newly-created <paramref name="data"/>, with the default title.</summary>
    public static OperationDataResult<T> Created<T>(T data)
        => Created(data, ResultTitles.Created);

    /// <summary>Success (201 Created) carrying the newly-created <paramref name="data"/>, with a custom title.</summary>
    public static OperationDataResult<T> Created<T>(T data, string title)
        => new SuccessDataResult<T>(data, title, ResultStatus.Created);

    /// <summary>Success (202 Accepted) with the default title, no data.</summary>
    public static OperationResult Accepted()
        => Accepted(ResultTitles.Accepted);

    /// <summary>Success (202 Accepted) with a custom title, no data.</summary>
    public static OperationResult Accepted(string title)
        => new SuccessResult(title, ResultStatus.Accepted);

    /// <summary>Success (202 Accepted) carrying <paramref name="data"/>, with the default title.</summary>
    public static OperationDataResult<T> Accepted<T>(T data)
        => Accepted(data, ResultTitles.Accepted);

    /// <summary>Success (202 Accepted) carrying <paramref name="data"/>, with a custom title.</summary>
    public static OperationDataResult<T> Accepted<T>(T data, string title)
        => new SuccessDataResult<T>(data, title, ResultStatus.Accepted);

    /// <summary>Success (204 No Content).</summary>
    public static OperationResult NoContent()
        => new SuccessResult(ResultTitles.NoContent, ResultStatus.NoContent);

    /// <summary>Success (205 Reset Content).</summary>
    public static OperationResult ResetContent()
        => new SuccessResult(ResultTitles.ResetContent, ResultStatus.ResetContent);

    /// <summary>Success (203 Non-Authoritative Information), no data.</summary>
    public static OperationResult NonAuthoritativeInformation()
        => new SuccessResult(ResultTitles.NonAuthoritativeInformation, ResultStatus.NonAuthoritativeInformation);

    /// <summary>Success (203 Non-Authoritative Information) carrying <paramref name="data"/>.</summary>
    public static OperationDataResult<T> NonAuthoritativeInformation<T>(T data, string title = ResultTitles.NonAuthoritativeInformation)
        => new SuccessDataResult<T>(data, title, ResultStatus.NonAuthoritativeInformation);

    /// <summary>Success (206 Partial Content), no data.</summary>
    public static OperationResult PartialContent()
        => new SuccessResult(ResultTitles.PartialContent, ResultStatus.PartialContent);

    /// <summary>Success (206 Partial Content) carrying <paramref name="data"/>.</summary>
    public static OperationDataResult<T> PartialContent<T>(T data, string title = ResultTitles.PartialContent)
        => new SuccessDataResult<T>(data, title, ResultStatus.PartialContent);

    /// <summary>Success (207 Multi-Status), no data.</summary>
    public static OperationResult MultiStatus()
        => new SuccessResult(ResultTitles.MultiStatus, ResultStatus.MultiStatus);

    /// <summary>Success (207 Multi-Status) carrying <paramref name="data"/>.</summary>
    public static OperationDataResult<T> MultiStatus<T>(T data, string title = ResultTitles.MultiStatus)
        => new SuccessDataResult<T>(data, title, ResultStatus.MultiStatus);

    /// <summary>Success (208 Already Reported), no data.</summary>
    public static OperationResult AlreadyReported()
        => new SuccessResult(ResultTitles.AlreadyReported, ResultStatus.AlreadyReported);

    /// <summary>Success (208 Already Reported) carrying <paramref name="data"/>.</summary>
    public static OperationDataResult<T> AlreadyReported<T>(T data, string title = ResultTitles.AlreadyReported)
        => new SuccessDataResult<T>(data, title, ResultStatus.AlreadyReported);

    /// <summary>Success (226 IM Used), no data.</summary>
    public static OperationResult ImUsed()
        => new SuccessResult(ResultTitles.ImUsed, ResultStatus.ImUsed);

    /// <summary>Success (226 IM Used) carrying <paramref name="data"/>.</summary>
    public static OperationDataResult<T> ImUsed<T>(T data, string title = ResultTitles.ImUsed)
        => new SuccessDataResult<T>(data, title, ResultStatus.ImUsed);

    /// <summary>Redirect (300 Multiple Choices), no data.</summary>
    public static OperationResult MultipleChoices(string detail)
        => new SuccessResult(detail, ResultStatus.MultipleChoices);

    /// <summary>Redirect (300 Multiple Choices) carrying <paramref name="data"/>.</summary>
    public static OperationDataResult<T> MultipleChoices<T>(T data, string detail)
        => new SuccessDataResult<T>(data, detail, ResultStatus.MultipleChoices);

    /// <summary>Redirect (301 Moved Permanently); <paramref name="location"/> is interpolated into the title.</summary>
    public static OperationResult MovedPermanently(string location)
        => new SuccessResult(string.Format(ResultTitles.MovedPermanentlyTemplate, location), ResultStatus.MovedPermanently).WithLocation(location);

    /// <inheritdoc cref="MovedPermanently(string)"/>
    public static OperationDataResult<T> MovedPermanently<T>(T data, string location)
        => new SuccessDataResult<T>(data, string.Format(ResultTitles.MovedPermanentlyTemplate, location), ResultStatus.MovedPermanently).WithLocation(location);

    /// <summary>Redirect (302 Found); <paramref name="location"/> is interpolated into the title.</summary>
    public static OperationResult Found(string location)
        => new SuccessResult(string.Format(ResultTitles.FoundTemplate, location), ResultStatus.Found).WithLocation(location);

    /// <inheritdoc cref="Found(string)"/>
    public static OperationDataResult<T> Found<T>(T data, string location)
        => new SuccessDataResult<T>(data, string.Format(ResultTitles.FoundTemplate, location), ResultStatus.Found).WithLocation(location);

    /// <summary>Redirect (303 See Other); <paramref name="location"/> is interpolated into the title.</summary>
    public static OperationResult SeeOther(string location)
        => new SuccessResult(string.Format(ResultTitles.SeeOtherTemplate, location), ResultStatus.SeeOther).WithLocation(location);

    /// <inheritdoc cref="SeeOther(string)"/>
    public static OperationDataResult<T> SeeOther<T>(T data, string location)
        => new SuccessDataResult<T>(data, string.Format(ResultTitles.SeeOtherTemplate, location), ResultStatus.SeeOther).WithLocation(location);

    /// <summary>Success (304 Not Modified), no data.</summary>
    public static OperationResult NotModified()
        => new SuccessResult(ResultTitles.NotModified, ResultStatus.NotModified);

    /// <summary>Success (304 Not Modified) carrying <paramref name="data"/>.</summary>
    public static OperationDataResult<T> NotModified<T>(T data)
        => new SuccessDataResult<T>(data, ResultTitles.NotModified, ResultStatus.NotModified);

    /// <summary>Redirect (307 Temporary Redirect); <paramref name="location"/> is interpolated into the title.</summary>
    public static OperationResult TemporaryRedirect(string location)
        => new SuccessResult(string.Format(ResultTitles.TemporaryRedirectTemplate, location), ResultStatus.TemporaryRedirect).WithLocation(location);

    /// <inheritdoc cref="TemporaryRedirect(string)"/>
    public static OperationDataResult<T> TemporaryRedirect<T>(T data, string location)
        => new SuccessDataResult<T>(data, string.Format(ResultTitles.TemporaryRedirectTemplate, location), ResultStatus.TemporaryRedirect).WithLocation(location);

    /// <summary>Redirect (308 Permanent Redirect); <paramref name="location"/> is interpolated into the title.</summary>
    public static OperationResult PermanentRedirect(string location)
        => new SuccessResult(string.Format(ResultTitles.PermanentRedirectTemplate, location), ResultStatus.PermanentRedirect).WithLocation(location);

    /// <inheritdoc cref="PermanentRedirect(string)"/>
    public static OperationDataResult<T> PermanentRedirect<T>(T data, string location)
        => new SuccessDataResult<T>(data, string.Format(ResultTitles.PermanentRedirectTemplate, location), ResultStatus.PermanentRedirect).WithLocation(location);
}
