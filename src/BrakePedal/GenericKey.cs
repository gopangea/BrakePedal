namespace BrakePedal
{
    public class SimpleThrottleKey : IThrottleKey
    {
        public SimpleThrottleKey(params object[] values)
        {
            Values = values;
        }

        public object[] Values { get; private set; }
    }
}