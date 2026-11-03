using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace KWinHub.Hub.Api.InternalApi;

/// <summary>
/// Protects service-to-service internal endpoints with a shared secret header.
/// These endpoints are not reachable through the public JWT path and are only
/// meant for the sibling Platform API. Fail closed when no key is configured.
/// M1 may replace this with authenticated service credentials.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class InternalApiKeyAttribute : Attribute, IAuthorizationFilter
{
    public const string HeaderName = "X-Internal-Api-Key";

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        var expected = configuration["InternalApis:ApiKey"];

        var provided = context.HttpContext.Request.Headers[HeaderName].ToString();

        if (string.IsNullOrWhiteSpace(expected)
            || !string.Equals(provided, expected, StringComparison.Ordinal))
        {
            context.Result = new UnauthorizedObjectResult(new { error = "invalid_internal_api_key" });
        }
    }
}
