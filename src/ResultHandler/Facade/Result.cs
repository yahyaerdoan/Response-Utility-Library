namespace ResultHandler.Facade;

/// <summary>Static factory facade for building results by status (e.g. <c>Result.NotFound(...)</c>), a non-generic overload for bodyless outcomes and a generic <c>&lt;T&gt;</c> overload for ones that carry data. Split across <c>Result.Success.cs</c>/<c>Result.Error.cs</c>/<c>Result.Combine.cs</c> for readability only — one logical facade.</summary>
public static partial class Result
{
}
