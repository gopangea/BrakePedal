using Microsoft.AspNetCore.Http;

namespace BrakePedal.Http
{
    public interface IHttpThrottlePolicy : IThrottlePolicy
    {
        bool Check(HttpRequest request, out CheckResult result, bool increment = true);

        CheckResult Check(HttpRequest request, bool increment = true);
    }
}