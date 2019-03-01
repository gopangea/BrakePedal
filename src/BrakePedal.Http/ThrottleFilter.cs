using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BrakePedal.Http
{
    public class ThrottleFilter : IAuthorizationFilter
    {
        public ThrottleFilter(IHttpThrottlePolicy throttlePolicy)
        {
            ThrottlePolicy = throttlePolicy;
        }

        protected IHttpThrottlePolicy ThrottlePolicy { get; private set; }

        public bool AllowMultiple
        {
            get { return true; }
        }

        /*Task<HttpResponseMessage> IAuthorizationFilter.ExecuteAuthorizationFilterAsync(HttpActionContext actionContext,
            CancellationToken cancellationToken, Func<Task<HttpResponseMessage>> continuation)
        {
            if (actionContext == null)
            {
                throw new ArgumentNullException("actionContext");
            }
            if (continuation == null)
            {
                throw new ArgumentNullException("continuation");
            }

            try
            {
                CheckResult(actionContext);
            }
            catch (Exception e)
            {
                return FromError<HttpResponseMessage>(e);
            }

            if (actionContext.Response != null)
            {
                return Task.FromResult(actionContext.Response);
            }
            return continuation();
        }*/

        protected virtual void CheckResult(AuthorizationFilterContext actionContext)
        {
            HttpRequest request = actionContext.HttpContext.Request;
            CheckResult checkResult = ThrottlePolicy.Check(request);
            if (checkResult.IsThrottled)
                actionContext.HttpContext.Response = ThrottledResponse(request, checkResult);
            else if (checkResult.IsLocked)
                actionContext.Response = LockedResponse(request, checkResult);
        }

        protected virtual HttpResponseMessage ThrottledResponse(HttpRequestMessage request, CheckResult checkResult)
        {
            return HttpResponseHelper.Throttled(request, checkResult);
        }

        protected virtual HttpResponseMessage LockedResponse(HttpRequestMessage request, CheckResult checkResult)
        {
            return HttpResponseHelper.Locked(request, checkResult);
        }

        private static Task<TResult> FromError<TResult>(Exception exception)
        {
            var tcs = new TaskCompletionSource<TResult>();
            tcs.SetException(exception);
            return tcs.Task;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (context.ActionDescriptor == null)
            {
                throw new ArgumentNullException("actionContext");
            }
            if (continuation == null)
            {
                throw new ArgumentNullException("continuation");
            }

            try
            {
                CheckResult(context);
            }
            catch (Exception e)
            {
                return FromError<HttpResponseMessage>(e);
            }

            if (actionContext.Response != null)
            {
                return Task.FromResult(actionContext.Response);
            }
            return continuation();
        }
    }
}