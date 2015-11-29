using System.Collections.Generic;

namespace BrakePedal
{
    public interface IThrottlePolicy
    {
        string Name { get; set; }
        string[] Prefixes { get; set; }
        ICollection<Limiter> Limiters { get; set; }

        CheckResult Check(IThrottleKey key, bool increment = true);

        bool IsThrottled(IThrottleKey key, out CheckResult result, bool increment = true);

        bool IsLocked(IThrottleKey key, out CheckResult result, bool increment = true);
    }
}