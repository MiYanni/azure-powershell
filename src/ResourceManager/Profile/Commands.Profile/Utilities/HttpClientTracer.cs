using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Rest;

namespace Microsoft.Azure.Commands.Profile.Utilities
{
    // A lightweight instance wrapper on the static ServiceClientTracing
    internal class HttpClientTracer
    {
        public string InvocationId { get; private set; }

        public static void Configuration(string source, string name, string value) =>
            ServiceClientTracing.Configuration(source, name, value);

        public static void AddTracingInterceptor(IServiceClientTracingInterceptor interceptor) =>
            ServiceClientTracing.AddTracingInterceptor(interceptor);

        public static bool RemoveTracingInterceptor(IServiceClientTracingInterceptor interceptor) =>
            ServiceClientTracing.RemoveTracingInterceptor(interceptor);

        public static void Information(string message, params object[] parameters) =>
            ServiceClientTracing.Information(message, parameters);

        public static void Information(string message) =>
            ServiceClientTracing.Information(message);

        public void Enter(object instance, string method, IDictionary<string, object> parameters = null)
        {
            if (!ServiceClientTracing.IsEnabled || InvocationId != null) return;

            InvocationId = ServiceClientTracing.NextInvocationId.ToString();
            ServiceClientTracing.Enter(InvocationId, instance, method, parameters ?? new Dictionary<string, object>());
        }

        public void ReceiveResponse(HttpResponseMessage response)
        {
            if (!ServiceClientTracing.IsEnabled || InvocationId == null) return;

            ServiceClientTracing.ReceiveResponse(InvocationId, response);
        }

        public void SendRequest(HttpRequestMessage request)
        {
            if (!ServiceClientTracing.IsEnabled || InvocationId == null) return;

            ServiceClientTracing.SendRequest(InvocationId, request);
        }

        public void Error(Exception ex)
        {
            if (!ServiceClientTracing.IsEnabled || InvocationId == null) return;

            ServiceClientTracing.Error(InvocationId, ex);
        }

        public void Exit(object result)
        {
            if (!ServiceClientTracing.IsEnabled || InvocationId == null) return;

            ServiceClientTracing.Exit(InvocationId, result);
            InvocationId = null;
        }
    }
}
