using System.Globalization;
using ResultHandler.Core.Base;
using ResultHandler.Core.Enums;
using ResultHandler.Facade;
using ResultHandler.Functional;
using ResultHandler.Implementations.Error;
using Xunit;

namespace ResultHandler.Tests.Core;

public class ErrorCodeTests
{
    [Fact]
    public void Code_IsNullByDefault()
    {
        Assert.Null(Result.NotFound("Missing.").Code);
    }

    [Fact]
    public void WithCode_SetsCode_AndKeepsEverythingElse()
    {
        var original = Result.Conflict("Slug taken.");

        var coded = original.WithCode("Tenant.SlugTaken");

        Assert.Equal("Tenant.SlugTaken", coded.Code);
        Assert.Equal(original.Title, coded.Title);
        Assert.Equal(original.Detail, coded.Detail);
        Assert.Equal(original.Status, coded.Status);
        Assert.Null(original.Code);
    }

    [Fact]
    public void WithCode_PreservesRuntimeType()
    {
        var coded = Result.NotFound<int>("Missing.").WithCode("Order.NotFound");

        Assert.IsType<ErrorDataResult<int>>(coded);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void WithCode_RejectsBlankCode(string code)
    {
        Assert.Throws<ArgumentException>(() => Result.Conflict("x").WithCode(code));
    }

    [Fact]
    public void Equality_ConsidersCode()
    {
        var first = Result.Conflict("x").WithCode("A");

        Assert.Equal(first, Result.Conflict("x").WithCode("A"));
        Assert.NotEqual(first, Result.Conflict("x").WithCode("B"));
        Assert.NotEqual(first, Result.Conflict("x"));
        Assert.Equal(first.GetHashCode(), Result.Conflict("x").WithCode("A").GetHashCode());
    }

    [Fact]
    public void Bind_CarriesCodeIntoTheNewResultType()
    {
        var failed = Result.Conflict<int>("Slug taken.").WithCode("Tenant.SlugTaken");

        var bound = failed.Bind(value => Result.Success(value.ToString(CultureInfo.InvariantCulture)));

        Assert.Equal("Tenant.SlugTaken", bound.Code);
        Assert.Equal(ResultStatus.Conflict, bound.Status);
    }

    [Fact]
    public void ToErrorDataResult_CarriesCode()
    {
        var failed = Result.Forbidden("No.").WithCode("Billing.ModuleNotBought");

        var projected = failed.ToErrorDataResult<string>();

        Assert.Equal("Billing.ModuleNotBought", projected.Code);
    }

    [Fact]
    public void ToErrorDataResult_WithoutCode_LeavesCodeNull()
    {
        Assert.Null(Result.Forbidden("No.").ToErrorDataResult<string>().Code);
    }
}
