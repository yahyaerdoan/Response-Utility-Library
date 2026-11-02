namespace ResultHandler.AspNetCore.ErrorCodes;

/// <summary>What an error code in a failed response means, as the error code catalog serves it.</summary>
/// <param name="Code">The code, as written in a failed response's <c>"code"</c>.</param>
/// <param name="Status">The HTTP status every failure with this code has.</param>
/// <param name="Title">The status's reason phrase.</param>
/// <param name="Description">What the code tells a client, and what it can do about it.</param>
/// <param name="General">Whether any endpoint can answer with the code, rather than only the endpoints of its own subject.</param>
public sealed record ErrorCodeDescription(string Code, int Status, string Title, string Description, bool General);
