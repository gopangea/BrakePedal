using System.Net.Http;

namespace BrakePedal.Http
{
    public interface IHttpThrottlePolicy : IThrottlePolicy
    {
        bool Check(HttpRequestMessage request, out CheckResult result, bool increment = true);

        CheckResult Check(HttpRequestMessage request, bool increment = true);
    }
}