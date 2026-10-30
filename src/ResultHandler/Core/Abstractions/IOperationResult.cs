using System.Net;
using ResultHandler.Core.Enums;

namespace ResultHandler.Core.Abstractions;

/// <summary>The outcome of an operation: success flag, HTTP-mappable status, a title, optional detail, and optional error messages.</summary>
public interface IOperationResult
{
    /// <summary>Whether the operation succeeded.</summary>
    bool IsSuccessful { get; }

    /// <summary>The outcome status, decoupled from <see cref="HttpStatusCode"/>; see <see cref="Mapping.ResultStatusExtensions.ToHttpStatusCode"/>.</summary>
    ResultStatus Status { get; }

    /// <summary>A short, human-readable summary of the result.</summary>
    string Title { get; }

    /// <summary>Optional additional context beyond <see cref="Title"/>.</summary>
    string? Detail { get; }

    /// <summary>Error messages by field (<c>"Address.City"</c>, <c>"Items[0].Name"</c>), with messages that belong to no field under the <c>""</c> key. Empty when there are none. The same map as <c>"errors"</c> in validation Problem Details and in the serialized result.</summary>
    IReadOnlyDictionary<string, IReadOnlyList<string>> Errors { get; }

    /// <summary>The URI the result points at, absolute or relative to the request: the new resource of a 201, the status monitor of a 202, the target of a 3xx. Written as the HTTP <c>Location</c> header; <see langword="null"/> when there is none.</summary>
    string? Location { get; }

    /// <summary>Optional machine-readable error code (e.g. <c>"Tenant.SlugTaken"</c>) so clients can tell failures with the same status apart without parsing <see cref="Detail"/>. <see langword="null"/> unless set with <c>WithCode</c>.</summary>
    string? Code { get; }
}
