using System.Collections;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using Microsoft.Azure.Commands.Common.Authentication;
using Microsoft.Azure.Commands.Common.Authentication.Abstractions;

namespace Microsoft.Azure.Commands.Profile.Utilities
{
    internal static class HttpRequestExtensions
    {
        public static void InjectAzureAuthentication(this HttpRequestMessage request, IAzureContext context, CancellationToken token) => 
            AzureSession.Instance.AuthenticationFactory.GetServiceClientCredentials(context).ProcessHttpRequestAsync(request, token).Wait(token);

        public static void AddRange(this HttpRequestHeaders headers, IDictionary collection)
        {
            foreach (DictionaryEntry pair in collection)
            {
                headers.Add(pair.Key.ToString(), pair.Value.ToString());
            }
        }
    }
}
