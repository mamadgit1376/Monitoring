using Monitoring_Support_Server.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
namespace Monitoring_Support_Server.MiddleWare
{
    public class ValidateSiteRequestAttribute : Attribute, IAsyncActionFilter
    {
        private const string HeaderName = "X-Site-Signature";

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var httpContext = context.HttpContext;
            var headers = httpContext.Request.Headers;

            if (!headers.TryGetValue(HeaderName, out var signatureFromRequest) || string.IsNullOrWhiteSpace(signatureFromRequest))
            {
                context.Result = new ForbidResult("Missing Site Signature");
                return;
            }

            var services = httpContext.RequestServices;
            var config = services.GetRequiredService<IOptions<SecuritySettings>>();
            var cache = services.GetRequiredService<IMemoryCache>();

            var siteKey = config.Value.SiteKey;
            var cacheKey = $"SiteSignature:{DateTime.UtcNow:yyyyMMdd}";

            var expectedSignature = cache.GetOrCreate(cacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1);
                return GenerateSignature(siteKey);
            });

            if (!string.Equals(signatureFromRequest, expectedSignature, StringComparison.OrdinalIgnoreCase))
            {
                context.Result = new ForbidResult("Invalid Site Signature");
                return;
            }

            await next();
        }
        private static string GenerateSignature(string siteKey)
        {
            // بخش ساعت (HH) را حذف کنید
            var now = DateTime.UtcNow.ToString("yyyyMMdd"); // مثلا 20250525
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(siteKey));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(now));
            return Convert.ToHexString(hash);
        }
    }
}
