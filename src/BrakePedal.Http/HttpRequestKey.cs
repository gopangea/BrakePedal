using System;
using System.Net.Http;

namespace BrakePedal.Http
{
    public class HttpRequestKey : IThrottleKey
    {
        public HttpRequestMessage Request { get; private set; }
        public string ClientIp { get; protected set; }
        public Uri RequestUri { get; protected set; }
        public HttpMethod HttpMethod { get; protected set; }

        public virtual object[] Values
        {
            get
            {
                return new object[]
                {
                    ClientIp,
                    HttpMethod,
                    RequestUri.AbsolutePath
                };
            }
        }

        public virtual void Initialize(HttpRequestMessage request)
        {
            
            Request = request;
            ClientIp = GetClientIp();
            RequestUri = request.RequestUri;
            HttpMethod = request.Method;
        }

        private string GetClientIp()
        {
            return Request.GetOwinContext().Request.RemoteIpAddress;
        }
    }
}