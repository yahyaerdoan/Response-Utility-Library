using System.Collections.Concurrent;
using ResultHandler.Core.Abstractions;
using ResultHandler.Mapping;

namespace ResultHandler.AspNetCore.Envelopes;

/// <summary>Builds the success body for a result: <see cref="ResultEnvelope{TData}"/> when it carries data (found through <see cref="IOperationResult{T}"/>, since callers often hold a plain <see cref="IOperationResult"/>), otherwise <see cref="ResultEnvelope"/>.</summary>
internal static class ResultEnvelopeFactory
{
    private static readonly ConcurrentDictionary<Type, IEnvelopeBuilder> _builders = new();

    private interface IEnvelopeBuilder
    {
        ResultEnvelope Build(IOperationResult result);
    }

    internal static ResultEnvelope Create(IOperationResult result)
        => _builders.GetOrAdd(result.GetType(), BuilderFor).Build(result);

    private static IEnvelopeBuilder BuilderFor(Type resultType)
    {
        var dataInterface = resultType.GetInterfaces().FirstOrDefault(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IOperationResult<>));
        if (dataInterface is null)
        {
            return new WithoutData();
        }

        var builderType = typeof(WithData<>).MakeGenericType(dataInterface.GetGenericArguments()[0]);
        return (IEnvelopeBuilder)(Activator.CreateInstance(builderType)
            ?? throw new InvalidOperationException($"Could not create the envelope builder for {resultType.Name}."));
    }

    private sealed class WithoutData : IEnvelopeBuilder
    {
        public ResultEnvelope Build(IOperationResult result) => new()
        {
            IsSuccessful = result.IsSuccessful,
            StatusCode = (int)result.Status.ToHttpStatusCode(),
            StatusMessage = result.Title,
            Detail = result.Detail,
            Code = result.Code,
        };
    }

    private sealed class WithData<TData> : IEnvelopeBuilder
    {
        public ResultEnvelope Build(IOperationResult result) => new ResultEnvelope<TData>
        {
            ResultData = ((IOperationResult<TData>)result).Data,
            IsSuccessful = result.IsSuccessful,
            StatusCode = (int)result.Status.ToHttpStatusCode(),
            StatusMessage = result.Title,
            Detail = result.Detail,
            Code = result.Code,
        };
    }
}
