using BitCoin.API.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BitCoin.API.Tests;

[TestClass]
public class BaseControllerTests
{
    [TestMethod]
    public void ShouldRequireAuthorizationForApiControllers()
    {
        var authorizeAttribute = typeof(BaseController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .Cast<AuthorizeAttribute>()
            .SingleOrDefault();

        Assert.IsNotNull(authorizeAttribute);
    }
}
