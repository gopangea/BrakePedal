using Microsoft.AspNetCore.Http;

namespace BrakePedal.Http
{
    public abstract class HttpThrottlePolicyBase : ThrottlePolicy, IHttpThrottlePolicy
    {
        public HttpThrottlePolicyBase()
            : this(new MemoryThrottleRepository())
        {
        }

        public HttpThrottlePolicyBase(IThrottleRepository throttleRepository)
            : base(throttleRepository)
        {
        }

        public abstract bool Check(HttpRequest request, out CheckResult result, bool increment = true);

        public abstract CheckResult Check(HttpRequest request, bool increment = true);
    }
}