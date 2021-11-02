using Microsoft.AspNetCore.Http;

namespace BrakePedal.Http
{
    public class HttpThrottlePolicy<T> : HttpThrottlePolicyBase where T : HttpRequestKey, new()
    {
        public HttpThrottlePolicy(IThrottleRepository repository)
            : base(repository)
        {
        }

        public virtual T CreateIdentity(HttpRequest request)
        {
            var identity = new T();
            identity.Initialize(request);
            return identity;
        }

        public override bool Check(HttpRequest request, out CheckResult result, bool increment = true)
        {
            result = Check(request, increment);
            return result.IsThrottled;
        }

        public override CheckResult Check(HttpRequest request, bool increment = true)
        {
            T identity = CreateIdentity(request);
            return base.Check(identity, increment);
        }
    }
}