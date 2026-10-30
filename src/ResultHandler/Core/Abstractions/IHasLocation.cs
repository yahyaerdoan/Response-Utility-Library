namespace ResultHandler.Core.Abstractions;

/// <summary>Optional companion to <see cref="IOperationResult"/> for results that point at another resource: the new resource of a 201, the status monitor of a 202, the target of a 3xx. Written as the HTTP <c>Location</c> header; check with <c>result is IHasLocation { Location: { } location }</c>.</summary>
public interface IHasLocation
{
    /// <summary>The URI the result points at, absolute or relative to the request; <see langword="null"/> when there is none.</summary>
    string? Location { get; }
}
