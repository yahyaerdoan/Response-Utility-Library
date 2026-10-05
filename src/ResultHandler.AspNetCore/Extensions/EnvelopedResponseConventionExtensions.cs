using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using ResultHandler.Core.Base;

namespace ResultHandler.AspNetCore.Extensions;

/// <summary>Documents, for OpenAPI generators, the success body that <c>ToEnvelopedResult</c> writes, so API docs and generated clients see the envelope and the data inside it.</summary>
public static class EnvelopedResponseConventionExtensions
{
    private const string _jsonContentType = "application/json";

    /// <summary>Declares an <see cref="OperationDataResult{T}"/> success body (<c>resultData</c> plus status metadata) for an endpoint that returns <c>ToEnvelopedResult</c> on a data result. Use 201 for creates; bodyless statuses such as 204 are declared with <c>Produces(204)</c>.</summary>
    /// <example>
    /// <code>
    /// api.MapGet("/products/{id:int}", (int id, ProductService products) =&gt; products.GetById(id).ToEnvelopedResult())
    ///     .ProducesEnveloped&lt;ProductResponse&gt;();
    /// </code>
    /// </example>
    public static RouteHandlerBuilder ProducesEnveloped<TData>(this RouteHandlerBuilder builder, int statusCode = StatusCodes.Status200OK)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.Produces<OperationDataResult<TData>>(statusCode, _jsonContentType);
    }

    /// <summary>Declares an <see cref="OperationResult"/> success body (status metadata, no data) for an endpoint that returns <c>ToEnvelopedResult</c> on a result without data.</summary>
    public static RouteHandlerBuilder ProducesEnveloped(this RouteHandlerBuilder builder, int statusCode = StatusCodes.Status200OK)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.Produces<OperationResult>(statusCode, _jsonContentType);
    }
}
