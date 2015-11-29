using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace BrakePedal.Http
{
    internal static class HttpResponseHelper
    {
        public static HttpResponseMessage Throttled(HttpRequestMessage request, CheckResult checkResult)
        {
            const string format = "Requests throttled; maximum allowed {0} per {1}.";
            string message = string.Format(format, checkResult.Limiter.Count, checkResult.Limiter.Period);
            HttpResponseMessage response = request.CreateResponse((HttpStatusCode)429, message);
            response.Headers.RetryAfter = new RetryConditionHeaderValue(checkResult.Limiter.Period);
            return response;
        }

        public static HttpResponseMessage Locked(HttpRequestMessage request, CheckResult checkResult)
        {
            const string format = "Requests throttled; maximum allowed {0} per {1}. Requests blocked for {2}.";
            string message = string.Format(format, checkResult.Limiter.Count, checkResult.Limiter.Period,
                checkResult.Limiter.LockDuration.Value);
            HttpResponseMessage response = request.CreateResponse((HttpStatusCode)429, message);
            response.Headers.RetryAfter = new RetryConditionHeaderValue(checkResult.Limiter.LockDuration.Value);
            return response;
        }
    }
}