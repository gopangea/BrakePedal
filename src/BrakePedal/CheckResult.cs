namespace BrakePedal
{
    public class CheckResult
    {
        public static readonly CheckResult NotThrottled = new CheckResult { IsThrottled = false, IsLocked = false };
        public string ThrottleKey { get; set; }
        public string LockKey { get; set; }
        public Limiter Limiter { get; set; }
        public bool IsThrottled { get; set; }
        public bool IsLocked { get; set; }
    }
}