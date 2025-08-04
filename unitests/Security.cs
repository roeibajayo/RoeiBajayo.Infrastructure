using RoeiBajayo.Infrastructure.Security;
using Xunit;
using System;
using System.Threading.Tasks;

namespace UnitTestProject;


public class Security
{

    [Fact]
    public async Task Signature()
    {
        var key = "key1key2key3key4";

        var p1 = 1;
        var p2 = 5;
        var p3 = "helloworld";
        string p4 = null;

        var sign = SignatureValidator.Create(key, p1, p2, p3, p4);
        Assert.True(SignatureValidator.Validate(key, sign, TimeSpan.FromSeconds(1), p1, p2, p3, p4));

        await Task.Delay(2001);

        Assert.False(SignatureValidator.Validate(key, sign, TimeSpan.FromSeconds(1), p1, p2, p3, p4));

        sign = SignatureValidator.Create(key, p1, p2, p3, p4);
        Assert.True(SignatureValidator.Validate(key, sign, TimeSpan.FromSeconds(1), p1, p2, p3, p4));

        await Task.Delay(2001);

        Assert.False(SignatureValidator.Validate(key, sign, TimeSpan.FromSeconds(1), p1, p2, p3, p4));
    }

    [Fact]
    public void IsIdValid()
    {
        Assert.True(UserSecurity.IsValidId("204312771"));
        Assert.False(UserSecurity.IsValidId("204312770"));
        Assert.False(UserSecurity.IsValidId("204312772"));

        Assert.True(UserSecurity.IsValidId("514384197"));
        Assert.False(UserSecurity.IsValidId("514384196"));
        Assert.False(UserSecurity.IsValidId("514384198"));

        Assert.True(UserSecurity.IsValidId("515546224"));
        Assert.False(UserSecurity.IsValidId("515546223"));
        Assert.False(UserSecurity.IsValidId("515546225"));

        Assert.True(UserSecurity.IsValidId("580216885"));
        Assert.False(UserSecurity.IsValidId("580216886"));
        Assert.False(UserSecurity.IsValidId("580216884"));

    }
}
