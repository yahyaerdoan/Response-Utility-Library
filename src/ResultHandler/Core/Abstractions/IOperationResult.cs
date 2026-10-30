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

    /// <summary>Optional list of individual error messages (e.g. validation failures).</summary>
    IReadOnlyList<string> Errors { get; }

    /// <summary>Optional machine-readable error code (e.g. <c>"Tenant.SlugTaken"</c>) so clients can tell failures with the same status apart without parsing <see cref="Detail"/>. <see langword="null"/> unless set with <c>WithCode</c>.</summary>
    string? Code { get; }
}
