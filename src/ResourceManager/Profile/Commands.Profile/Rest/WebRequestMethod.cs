using System.Linq;
using System.Net.Http;

// ReSharper disable once CheckNamespace
namespace Microsoft.Azure.Commands.Profile
{
    // TODO: When we use an updated Microsoft.PowerShell.Commands reference, this enum should be removed and references should use that one instead.
    // Copied from Microsoft.PowerShell.Commands.WebRequestMethod
    public enum WebRequestMethod
    {
        Default,
        Get,
        Head,
        Post,
        Put,
        Delete,
        Trace,
        Options,
        Merge,
        Patch
    }

    internal static class WebRequestMethodExtensions
    {
        private static readonly HttpMethod HttpMethodMerge = new HttpMethod("MERGE");
        private static readonly HttpMethod HttpMethodPatch = new HttpMethod("PATCH");

        private static readonly (WebRequestMethod WebRequest, HttpMethod Http)[] Mapper =
        {
            (WebRequestMethod.Default, HttpMethod.Get),
            (WebRequestMethod.Get, HttpMethod.Get),
            (WebRequestMethod.Head, HttpMethod.Head),
            (WebRequestMethod.Post, HttpMethod.Post),
            (WebRequestMethod.Put, HttpMethod.Put),
            (WebRequestMethod.Delete, HttpMethod.Delete),
            (WebRequestMethod.Trace, HttpMethod.Trace),
            (WebRequestMethod.Options, HttpMethod.Options),
            (WebRequestMethod.Merge, HttpMethodMerge),
            (WebRequestMethod.Patch, HttpMethodPatch)
        };

        public static HttpMethod ToHttpMethod(this WebRequestMethod webRequest) =>
            Mapper.First(m => m.WebRequest == webRequest).Http;
    }
}
