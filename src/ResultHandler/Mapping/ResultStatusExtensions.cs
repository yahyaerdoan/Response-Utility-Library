using System.Net;
using ResultHandler.Core.Enums;

namespace ResultHandler.Mapping;

/// <summary>Converts <see cref="ResultStatus"/> to <see cref="HttpStatusCode"/>.</summary>
public static class ResultStatusExtensions
{
    /// <summary>Maps <paramref name="status"/> to its <see cref="HttpStatusCode"/>, or <see cref="HttpStatusCode.InternalServerError"/> if unmapped.</summary>
    public static HttpStatusCode ToHttpStatusCode(this ResultStatus status)
        => ResultStatusRegistry.ToHttpCode.TryGetValue(status, out var httpStatusCode)
            ? httpStatusCode
            : HttpStatusCode.InternalServerError;
}
