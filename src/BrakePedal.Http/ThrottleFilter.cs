using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;

namespace BrakePedal.Http
{
    public class ThrottleFilter : IAuthorizationFilter
    {
        public ThrottleFilter(IHttpThrottlePolicy throttlePolicy)
        {
            ThrottlePolicy = throttlePolicy;
        }

        protected IHttpThrottlePolicy ThrottlePolicy { get; private set; }

        public static bool AllowMultiple
        {
            get { return true; }
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            try
            {
                CheckResult(context);
            }
            catch
            {
                context.Result = new ForbidResult();
            }
        }

        protected virtual void CheckResult(AuthorizationFilterContext actionContext)
        {
            HttpRequest request = actionContext.HttpContext.Request;
            CheckResult checkResult = ThrottlePolicy.Check(request);
            if (checkResult.IsThrottled)
            {
                actionContext.HttpContext.Response.Headers.Add(HeaderNames.RetryAfter, checkResult.Limiter.Period.ToString());

                actionContext.Result = ThrottledResponse(checkResult);
            }
            else if (checkResult.IsLocked)
            {
                actionContext.HttpContext.Response.Headers.Add(HeaderNames.RetryAfter, checkResult.Limiter.LockDuration.Value.ToString());

                actionContext.Result = LockedResponse(checkResult);
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