using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace BrakePedal.Http
{
    public class ThrottleHandler : DelegatingHandler
    {
        protected IHttpThrottlePolicy ThrottlePolicy;

        public ThrottleHandler(IHttpThrottlePolicy throttlePolicy)
        {
            ThrottlePolicy = throttlePolicy;
        }

        protected virtual Task<HttpResponseMessage> CheckResult(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            CheckResult checkResult = ThrottlePolicy.Check(request);
            if (checkResult.IsThrottled)
                return Task.FromResult(ThrottledResponse(request, checkResult));
            if (checkResult.IsLocked)
                return Task.FromResult(LockedResponse(request, checkResult));
            return base.SendAsync(request, cancellationToken);
        }

        protected virtual HttpResponseMessage ThrottledResponse(HttpRequestMessage request, CheckResult checkResult)
        {
            return HttpResponseHelper.Throttled(request, checkResult);
        }

        protected virtual HttpResponseMessage LockedResponse(HttpRequestMessage request, CheckResult checkResult)
        {
            return HttpResponseHelper.Locked(request, checkResult);
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return CheckResult(request, cancellationToken);
        }
    }
}