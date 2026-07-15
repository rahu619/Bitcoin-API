using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BitCoin.API.Tests;

[TestClass]
public sealed class OptionsValidationTests
{
    [TestMethod]
    public void HostStartup_Throws_WhenJwtKeyIsTooShort()
    {
        using var factory = new BitcoinApiWebApplicationFactory();
        using var shortKeyFactory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("Jwt:Key", "too-short"));

        Assert.ThrowsExactly<OptionsValidationException>(() => shortKeyFactory.CreateClient());
    }

    [TestMethod]
    public void HostStartup_Throws_WhenJwtIssuerIsMissing()
    {
        using var factory = new BitcoinApiWebApplicationFactory();
        using var missingIssuerFactory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("Jwt:Issuer", string.Empty));

        Assert.ThrowsExactly<OptionsValidationException>(() => missingIssuerFactory.CreateClient());
    }
}
