using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace BrakePedal.Http
{
    public class ThrottleMiddleware
    {
        private readonly RequestDelegate _next;
        protected IHttpThrottlePolicy ThrottlePolicy { get; private set; }

        public ThrottleMiddleware(RequestDelegate next, IHttpThrottlePolicy throttlePolicy)
        {
            _next = next;
            ThrottlePolicy = throttlePolicy;
        }

        public async Task Invoke(HttpContext context)
        {
            await CheckResult(context);
            await _next.Invoke(context);
        }

        protected async Task CheckResult(HttpContext httpContext)
        {
            HttpRequest request = httpContext.Request;
            CheckResult checkResult = ThrottlePolicy.Check(request);
            if (checkResult.IsThrottled)
            {
                httpContext.Response.Headers.Add(HeaderNames.RetryAfter, checkResult.Limiter.Period.ToString());

                await httpContext.Response.WriteAsync(JsonConvert.SerializeObject(ThrottledResponse(checkResult)));
            }
            else if (checkResult.IsLocked)
            {
                httpContext.Response.Headers.Add(HeaderNames.RetryAfter, checkResult.Limiter.LockDuration.Value.ToString());

                await httpContext.Response.WriteAsync(JsonConvert.SerializeObject(LockedResponse(checkResult)));
            }
        }

        protected virtual IActionResult ThrottledResponse(CheckResult checkResult)
        {
            return HttpResponseHelper.Throttled(checkResult);
        }

        protected virtual IActionResult LockedResponse(CheckResult checkResult)
        {
            return HttpResponseHelper.Locked(checkResult);
        }
    }
}
