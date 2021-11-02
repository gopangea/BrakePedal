# Brake Pedal 

`Brake Pedal` is a general purpose throttling and rate limiting library. The code was forked from [WebApiThrottle](https://github.com/stefanprodan/WebApiThrottle). It's currently in use at [Pangea](https://gopangea.com).

The core library provides the following features:

- Throttling: limit `X attempts` over `Y time period`.
- Locking: after `X attempts` over `Y time period` block future attempts for `Z time period`.

### What's with the name?

The main purpose of this library is to "throttle". "Throttle" as a library name would be too generic. "Throttle" is also commonly used to refer to a car's gas pedal. However, "Gas Pedal" generally means increasing the speed, but this library is trying to limit speeds. "Brake Pedal" better denotes the purpose of the library. 

Sticking to the whole car analogy, maybe calling it "Cops" or "Police" would make more sense since the police would stop you if you go over the speed limit (throttling) and arrest you disallowing you to drive (locking)..?

If you'd like to suggest a name change, feel free to open a PR.

### Packages

Current Version: `2.0.0`

Target Framework: `.NET 5 and above`.

- `BrakePedal` is the main package that contains all the logic as well as an in-memory repository.
    - [nuget.org/packages/BrakePedal](https://www.nuget.org/packages/BrakePedal)
- `BrakePedal.Http` contains code to use the main package in a web application as a handler or filter.
    - [nuget.org/packages/BrakePedal.Http](https://www.nuget.org/packages/BrakePedal.Http)
- `BrakePedal.Redis` contains an implementation of a Redis throttle repository which uses [StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis).
    - [nuget.org/packages/BrakePedal.Redis](https://www.nuget.org/packages/BrakePedal.Redis)

###  CI

[![](https://travis-ci.org/gopangea/BrakePedal.svg?branch=master)](https://travis-ci.org/gopangea/BrakePedal)

https://travis-ci.org/gopangea/BrakePedal 

### To Do

#### Documentation

- Add XML documentation to public functions.
- Figure out how to include debug symbols from from xbuild (it's generating mdb only).

#### Refactoring 

- Provide a simpler way to define throttle keys (maybe through a lamda) instead of having to subclass.
- Use something other than `System.Runetime.Caching.ObjectCache` for the in-memory repository to allow creating a PCL.

## Usage

### Basic Usage

Let's assume we want to throttle login attempts. 

1. Start with a throttle policy:

        var loginPolicy = new ThrottlePolicy()     // An empty constructor uses an in-memory repository
        {
            Name = "Login Attempts",                // Give it a name so it can be used for logging purposes.
            Prefixes = new[] { "login:attempts" },  // Use prefixes to differentiate tracked keys for this policy from others.
                                                    // It might make sense to get rid of this property and just use the Name property. 
                                                
            // Set the limits for this policy. 
            // We want to limit logins to 1 per second and 4 per minute.
            PerSecond = 1,
            PerMinute = 4
        };

2. You can also configure the policy to block requests for a predetermined amount of time:

        var loginPolicyWithLocking = new ThrottlePolicy() 
        {
            Name = "Login Attempts Locking",
            Prefixes = new[] { "login:attempts:locking" }, 
                                                
            Limiters = new Limiter[]
            {
                new Limiter()
                    .Limit(10)                        // Limit 10 requests,
                    .Over(TimeSpan.FromMinutes(15))     // over a 15 minute period,
                    .LockFor(TimeSpan.FromMinutes(15))  // disallow new requests for 15 minutes
                                                        // from when the initial limits are hit
            }
        };
                
Once instantiated, the policies can be used as follows:

1. Create a key that can uniquely identify the requester. This is used to track the number of requests made:

        var key = new SimpleThrottleKey("username");   // Determine a key to track the requester like user name
                                                       // or user ID or IP address etc or a combination of values.

2. Login policy with throttling only:

        var check = loginPolicy.Check(key); // NOTE: by default, calling the check method will increment the counter.
                                            // If you want to check the status of a policy but not increment the counter
                                            // pass in false to the increment parameter as follows.
                                            // loginPolicy.Check(key, increment = false); 
        
        if (check.IsThrottled)
        {
            throw new Exception($"Requests throttled. Maximum allowed { check.Limiter.Count } per { check.Limiter.Period }.");
        }

3. Login policy with locking:

        var check = loginPolicyWithLocking.Check(key);  
        
        if (check.IsLocked)
        {
            throw new Exception($"Requests blocked for { check.Limiter.LockDuration }. Maximum allowed { check.Limiter.Count } per { check.Limiter.Period }.");
        }
    
### Http Usage

The `BrakePedal.Http` package provides an `HttpThrottlePolicy` for use in an HTTP environment like APIs and web applications.

The `HttpThrottlePolicy` expects a throttle key that's built from values in a request object like IP, endpoint, and other HTTP values.

#### Using as a handler

First define the throttle key we want to use for this policy:
    
    // HttpRequestKey is part of the library and contains helpful values like IP address.
    public class IpRequestKey : HttpRequestKey 
    {
        public override void Initialize(HttpRequestMessage request)
        {
            base.Initialize(request);
    
            string forwardedFor = "X-Forwarded-For"; 
            if (request.Headers.Contains(forwardedFor))
            {
                // Use the forwarded IP address if sitting behind a load balancer
                string ip = request.Headers.GetValues(forwardedFor).First().Trim();
                
                // The base class HttpRequestKey gets the IP from other sources.
                ClientIp = IPAddress.Parse(ip);
            }
        }
    
        public override object[] Values
        {
            get
            {
                return new object[] 
                {
                    ClientIp
                };
            }
        } 
    }
    
The `IpRequestKey` defined above uses the IP of the request to track the number of requests and locking. Use it to create the policy:
    
    var apiRequestPolicy = new HttpThrottlePolicy<IpRequestKey>()
    {
        Name = "Requests",
        Prefixes = new[] { "requests" },
    
        PerSecond = 50 // Only allow 50 requests per second per IP
    };

Create a delegating handler using the policy above and add it to the request pipeline:
 
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Other code truncated
            
            var throttleHandler = new HttpThrottleHandler(apiRequestPolicy);
            config.MessageHandlers.Add(throttleHandler);
            
            // Other code truncated
        }
    }

### Redis Storage

An in-memory repository (using `System.Runtime.Caching.ObjectCache`) ships with the main package. The `BrakePedal.Redis` package ships with a Redis repository using [StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis).

The `ThrottlePolicy` constructor accepts an `IThrottleRepository`. To use Redis as storage:
    
    var configurationOptions = new ConfigurationOptions
    {
        AbortOnConnectFail = false
    };
    configurationOptions.EndPoints.Add("redishost", 6379);
    var connection = ConnectionMultiplexer.Connect(configurationOptions);
    var redisDatabaseIndex = 2;
    var database = connection.GetDatabase(2);
    
    var repository = new RedisThrottleRepository(database);
    
    var apiRequestPolicy = new HttpThrottlePolicy<IpRequestKey>(repository) // Pass in the Redis repository
    {
        Name = "Requests",
        Prefixes = new[] { "requests" },
    
        PerSecond = 50 // Only allow 50 requests per second per IP
    };

#### Author and License

Released by the [Pangea engineering team](http://engineering.gopangea.com) under the MIT License (see LICENSE file).
