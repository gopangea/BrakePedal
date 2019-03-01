using System;
using System.Net;
using System.Net.Http;
using System.ServiceModel.Channels;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace BrakePedal.Http
{
    public class HttpRequestKey : IThrottleKey
    {
        public HttpRequestMessage Request { get; private set; }
        public IPAddress ClientIp { get; protected set; }
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

        private IPAddress GetClientIp()
        {
            
            if (Request.Properties.ContainsKey("MS_HttpContext"))
            {
                return ((HttpContext) Request.Properties["MS_HttpContext"]).Features.Get<IHttpConnectionFeature>()
                    ?.RemoteIpAddress;
            }

            if (Request.Properties.ContainsKey(RemoteEndpointMessageProperty.Name))
            {
                return
                    IPAddress.Parse(
                        ((RemoteEndpointMessageProperty)Request.Properties[RemoteEndpointMessageProperty.Name]).Address);
            }

            throw new InvalidOperationException("Ip not found");
        }
    }
}