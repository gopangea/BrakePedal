using Microsoft.AspNetCore.Mvc;

namespace BrakePedal.Http
{
    internal static class HttpResponseHelper
    {
        public static IActionResult Throttled(CheckResult checkResult)
        {
            const string format = "Requests throttled; maximum allowed {0} per {1}.";
            string message = string.Format(format, checkResult.Limiter.Count, checkResult.Limiter.Period);

            return new ContentResult()
            {
                Content = message,
                StatusCode = 429
            };
        }

        public static IActionResult Locked(CheckResult checkResult)
        {
            const string format = "Requests throttled; maximum allowed {0} per {1}. Requests blocked for {2}.";
            string message = string.Format(format, checkResult.Limiter.Count, checkResult.Limiter.Period,
                checkResult.Limiter.LockDuration.Value);

            return new ContentResult()
            {
                Content = message,
                StatusCode = 429
            };
        }
    }
}