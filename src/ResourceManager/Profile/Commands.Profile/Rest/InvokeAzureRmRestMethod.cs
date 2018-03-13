// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Management.Automation;
using System.Net.Http;
using System.Text;
using System.Web;
using Microsoft.Azure.Commands.Profile.Utilities;
using Microsoft.Azure.Commands.ResourceManager.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static Microsoft.Azure.Commands.Profile.Properties.Resources;

// ReSharper disable once CheckNamespace
namespace Microsoft.Azure.Commands.Profile
{
    [Cmdlet(VerbsLifecycle.Invoke, "AzureRmRestMethod", SupportsShouldProcess = true), OutputType(typeof(PSObject))]
    public class InvokeAzureRmRestMethod : AzureRmLongRunningCmdlet
    {
        [Parameter(Position = 0, Mandatory = true, HelpMessage = "Specifies the Uniform Resource Identifier (URI) of the Azure RM resource to which the web request is sent.")]
        [ValidateNotNullOrEmpty]
        public Uri Uri { get; set; }

        [Parameter(HelpMessage = "Specifies the method used for the web request.")]
        public WebRequestMethod Method { get; set; }

        [Parameter(ValueFromPipeline = true, HelpMessage = "Specifies the body of the request. Azure RM REST API calls that require a body are in JSON format.")]
        [ValidateNotNullOrEmpty]
        public PSObject Body { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [Parameter(HelpMessage = "Specifies the headers of the web request.")]
        public IDictionary Headers { get; set; }

        private readonly HttpClientTracer _tracer = new HttpClientTracer();

        private static Uri ResolveAzureUri(Uri uri, Uri azureRmUri)
        {
            var uriString = uri.ToString();
            if (!Uri.IsWellFormedUriString(uriString, UriKind.RelativeOrAbsolute))
            {
                throw new PSArgumentException(RestMethodUriInvalid.AsFormatString(uriString));
            }

            var builder = new UriBuilder
            {
                Scheme = uri.IsAbsoluteUri ? uri.Scheme : azureRmUri.Scheme,
                Host = uri.IsAbsoluteUri ? uri.Host : azureRmUri.Host
            };

            if (builder.Scheme != azureRmUri.Scheme)
            {
                throw new PSArgumentException(RestMethodUriSchemeInvalid.AsFormatString(uri.Scheme, azureRmUri.Scheme));
            }
            if (builder.Host != azureRmUri.Host)
            {
                throw new PSArgumentException(RestMethodUriHostInvalid.AsFormatString(uri.Host, azureRmUri.Host));
            }
            var absoluteUri = new Uri(builder.Uri, uri.IsAbsoluteUri ? uri.PathAndQuery : uriString);
            var queryParts = HttpUtility.ParseQueryString(absoluteUri.Query);
            if (!queryParts.HasKeys() || !queryParts.AllKeys.Contains("api-version"))
            {
                throw new PSArgumentException(RestMethodApiVersionRequired);
            }

            return absoluteUri;
        }

        private static PSObject ResultToPsObject(string jsonText)
        {
            if (String.IsNullOrEmpty(jsonText)) return new PSObject();

            var json = JObject.Parse(jsonText);
            //https://stackoverflow.com/a/7216958/294804
            //if (json["value"]?.Type != JTokenType.Array) return JObjectToPsObject(json);
            if (!(json["value"] is JArray))
            {
                var psObject = new PSObject();
                foreach (var key in json.Properties().Select(p => p.Name))
                {
                    psObject.Members.Add(new PSNoteProperty(key, json[key].ToString(Formatting.None)));
                }
                return new PSObject(psObject);
            }

            var items = json.Value<JArray>("value").ToObject<List<JObject>>();

            //https://stackoverflow.com/a/13565373/294804
            //return new PSObject(elements.ToObject<List<JObject>>().Select(JObjectToHashtable).ToList());

            var result = new List<PSObject>();
            foreach (var item in items)
            {
                var psObject = new PSObject();
                foreach (var key in item.Properties().Select(p => p.Name))
                {
                    psObject.Members.Add(new PSNoteProperty(key, item[key].ToString(Formatting.None)));
                }
                result.Add(psObject);
            }

            return new PSObject(result);

            //Hashtable JObjectToHashtable(JObject jObject) =>
            //    new Hashtable(jObject.ToObject<List<KeyValuePair<string, JToken>>>().ToDictionary(tag => tag.Key, tag => tag.Value.ToString(Formatting.None)));
            //Hashtable JObjectToHashtable(JObject jObject)
            //{
            //    var hashtable = new Hashtable();
            //    //https://stackoverflow.com/a/29103652/294804
            //    foreach (var tag in jObject)
            //    {
            //        hashtable.Add(tag.Key, tag.Value.ToString(Formatting.None));
            //    }
            //    return hashtable;
            //}

            //PSObject JObjectToPsObject(JObject jObject)
            //{
            //    var result = new PSObject();
            //    //https://stackoverflow.com/a/29103652/294804
            //    foreach (var tag in jObject)
            //    {
            //        //https://stackoverflow.com/a/30988995/294804
            //        //result.Members.Add(new PSNoteProperty(tag.Key, tag.Value.ToString(Formatting.None)));
            //        //result.Members.Add(new PSNoteProperty(tag.Key, tag.Value.ToString(Formatting.None).Replace("\"", String.Empty)));

            //        var value = tag.Value.Type == JTokenType.Object
            //            ? (object)tag.Value.ToObject<JObject>()
            //            : tag.Value.ToObject<JValue>().ToString(Formatting.None).Replace("\"", String.Empty);

            //        result.Members.Add(new PSNoteProperty(tag.Key, value));
            //    }
            //    return result;
            //}

            //PSObject JObjectToPsObject(JObject jObject)
            //{
            //    var result = new PSObject();
            //    //https://stackoverflow.com/a/29103652/294804
            //    foreach (var tag in jObject)
            //    {
            //        //https://stackoverflow.com/a/30988995/294804
            //        //result.Members.Add(new PSNoteProperty(tag.Key, tag.Value.ToString(Formatting.None)));
            //        //result.Members.Add(new PSNoteProperty(tag.Key, tag.Value.ToString(Formatting.None).Replace("\"", String.Empty)));

            //        var value = tag.Value.Type == JTokenType.Object
            //            ? (object)tag.Value.ToObject<JObject>()
            //            : tag.Value.ToObject<JValue>().ToString(Formatting.None).Replace("\"", String.Empty);

            //        result.Members.Add(new PSNoteProperty(tag.Key, value));
            //    }
            //    return result;
            //}
        }

        public override void ExecuteCmdlet()
        {
            var uri = ResolveAzureUri(Uri, new Uri(DefaultContext.Environment.ResourceManagerUrl));
            var httpRequest = new HttpRequestMessage(Method.ToHttpMethod(), uri);
            httpRequest.InjectAzureAuthentication(DefaultContext);
            httpRequest.Headers.Add("x-ms-client-request-id", Guid.NewGuid().ToString());
            httpRequest.Headers.Add("accept-language", "en-US");
            httpRequest.Headers.AddUserAgent();
            httpRequest.Headers.AddRange(Headers ?? new Dictionary<string, string>());
            if (this.IsParamBound(c => c.Body))
            {
                httpRequest.Content = new StringContent(JsonConvert.SerializeObject(Body.BaseObject), Encoding.UTF8, "application/json");
            }
            
            using (var client = new HttpClient())
            {
                _tracer.Enter(this, VerbsLifecycle.Invoke);
                _tracer.SendRequest(httpRequest);
                var httpResponse = client.SendAsync(httpRequest).Result;
                _tracer.ReceiveResponse(httpResponse);

                //var resultText = httpResponse.Content.ReadAsStringAsync().Result;
                //var thing = JObject.Parse(resultText);
                //var thing2 = JObjectToPsObject(thing);
                var resultText = httpResponse.Content.ReadAsStringAsync().Result;
                var result = ResultToPsObject(resultText);
                WriteObject(result);
                _tracer.Exit(result);
            }
        }
    }
}
