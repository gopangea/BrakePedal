using System.Net.Http;

namespace BrakePedal.Http
{
    public class HttpThrottlePolicy<T> : HttpThrottlePolicyBase where T : HttpRequestKey, new()
    {
        public HttpThrottlePolicy(IThrottleRepository repository)
            : base(repository)
        {
        }

        public virtual T CreateIdentity(HttpRequestMessage request)
        {
            var identity = new T();
            identity.Initialize(request);
            return identity;
        }

        public override bool Check(HttpRequestMessage request, out CheckResult result, bool increment = true)
        {
            result = Check(request, increment);
            return result.IsThrottled;
        }

        public override CheckResult Check(HttpRequestMessage request, bool increment = true)
        {
            T identity = CreateIdentity(request);
            return base.Check(identity, increment);
        }
    }
}