using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;

using BitCoin.Application.Constants;
using BitCoin.Domain;

using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BitCoin.API.Tests;

[TestClass]
public sealed class BitCoinControllerTests
{
    private const string LatestRoute = "/api/v1/bitcoin/latest";

    private BitcoinApiWebApplicationFactory _factory = null!;

    [TestInitialize]
    public void Initialize() => _factory = new BitcoinApiWebApplicationFactory();

    [TestCleanup]
    public void Cleanup() => _factory.Dispose();

    [TestMethod]
    public async Task GetLatest_ReturnsOk_WhenCacheHasData()
    {
        IReadOnlyList<BitCoinPriceIndexHistoryModel> seeded = [new() { Date = "2026-07-14", USD = 65000m }];
        await _factory.CacheProvider.SetValueAsync(CacheKeys.ApiLatest, seeded);

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateValidToken());

        using var response = await client.GetAsync(LatestRoute);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task GetLatest_ReturnsNotFound_WhenCacheIsEmpty()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateValidToken());

        using var response = await client.GetAsync(LatestRoute);

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task GetLatest_ReturnsUnauthorized_WhenNoTokenProvided()
    {
        using var client = _factory.CreateClient();

        using var response = await client.GetAsync(LatestRoute);

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private static string CreateValidToken()
    {
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(BitcoinApiWebApplicationFactory.ValidJwtKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: BitcoinApiWebApplicationFactory.ValidJwtIssuer,
            audience: BitcoinApiWebApplicationFactory.ValidJwtAudience,
            claims: [new Claim(ClaimTypes.NameIdentifier, "test-client")],
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
