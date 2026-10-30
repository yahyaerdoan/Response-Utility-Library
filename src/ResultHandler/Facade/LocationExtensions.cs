using ResultHandler.Core.Base;

namespace ResultHandler.Facade;

/// <summary>Attaches the URI a result points at, written as the HTTP <c>Location</c> header.</summary>
public static class LocationExtensions
{
    /// <summary>Returns a copy of <paramref name="result"/>, same runtime type, with <see cref="OperationResult.Location"/> set to <paramref name="location"/>: the new resource of a 201 or the status monitor of a 202. The redirect factories (<c>MovedPermanently</c>, <c>Found</c>, ...) set it already.</summary>
    /// <example>
    /// <code>
    /// return Result.Created(order.Id).WithLocation($"/api/v1/orders/{order.Id}");
    /// </code>
    /// </example>
    public static TResult WithLocation<TResult>(this TResult result, string location)
        where TResult : OperationResult
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentException.ThrowIfNullOrWhiteSpace(location);
        return (TResult)result.CloneWithLocation(location);
    }
}
