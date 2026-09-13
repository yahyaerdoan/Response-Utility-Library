using System.Net;
using ResultHandler.Core.Enums;

namespace ResultHandler.Mapping;

/// <summary>Converts <see cref="HttpStatusCode"/> (or a raw status code) to <see cref="ResultStatus"/>.</summary>
public static class HttpStatusCodeExtensions
{
    /// <inheritdoc cref="ToResultStatus(int)"/>
    public static ResultStatus ToResultStatus(this HttpStatusCode httpStatusCode)
        => ((int)httpStatusCode).ToResultStatus();

    /// <summary>Maps <paramref name="httpStatusCode"/> to the matching <see cref="ResultStatus"/>, or the nearest generic status for its code class if there's no exact match.</summary>
    public static ResultStatus ToResultStatus(this int httpStatusCode)
    {
        if (ResultStatusRegistry.FromHttpCode.TryGetValue(httpStatusCode, out var status))
        {
            return status;
        }

        return httpStatusCode switch
        {
            >= 100 and < 200 => ResultStatus.Processing,
            >= 200 and < 300 => ResultStatus.Ok,
            >= 300 and < 400 => ResultStatus.Found,
            >= 400 and < 500 => ResultStatus.BadRequest,
            _ => ResultStatus.InternalServerError,
        };
    }
}
