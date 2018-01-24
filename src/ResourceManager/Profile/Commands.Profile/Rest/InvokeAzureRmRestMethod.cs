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
using System.Management.Automation;
using System.Net.Http;
using System.Threading;
using Microsoft.Azure.Commands.Profile.Utilities;
using Microsoft.Azure.Commands.ResourceManager.Common;
using Microsoft.Rest;

// ReSharper disable once CheckNamespace
namespace Microsoft.Azure.Commands.Profile
{


    [Cmdlet(VerbsLifecycle.Invoke, "AzureRmRestMethod", DefaultParameterSetName = "Default"), OutputType(typeof(PSObject))]
    public class InvokeAzureRmRestMethod : AzureRmLongRunningCmdlet
    {
        //[Parameter(Mandatory = true, HelpMessage = "Name of a specific billing period to get.", ParameterSetName = Constants.ParameterSetNames.SingleItemParameterSet)]
        //[ValidateNotNullOrEmpty]
        //public List<string> Name { get; set; }

        //[Parameter(Mandatory = false, HelpMessage = "Determine the maximum number of records to return.", ParameterSetName = Constants.ParameterSetNames.ListParameterSet)]
        //[ValidateNotNull]
        //[ValidateRange(1, 100)]
        //public int? MaxCount { get; set; }

        [Parameter(Position = 0, Mandatory = true, HelpMessage = "Specifies the Uniform Resource Identifier (URI) of the Azure resource to which the web request is sent.")]
        [ValidateNotNullOrEmpty]
        public Uri Uri { get; set; }

        [Parameter(ParameterSetName = "Default", HelpMessage = "Specifies the method used for the web request.")]
        public WebRequestMethod Method { get; set; }

        [Parameter(ValueFromPipeline = true, HelpMessage = "Specifies the body of the request.")]
        public object Body { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [Parameter(HelpMessage = "Specifies the headers of the web request.")]
        public IDictionary Headers { get; set; }

        public override void ExecuteCmdlet()
        {
            var cancellationToken = new CancellationToken();
            var httpRequest = new HttpRequestMessage(Method.ToHttpMethod(), Uri);
            httpRequest.InjectAzureAuthentication(DefaultContext, cancellationToken);
            httpRequest.Headers.AddRange(Headers ?? new Dictionary<string, string>());
            if (Body != null)
            {
                httpRequest.Content = new StringContent(Body.ToString());
            }

            var shouldTrace = ServiceClientTracing.IsEnabled;
            string invocationId = null;
            if (shouldTrace)
            {
                invocationId = ServiceClientTracing.NextInvocationId.ToString();
                ServiceClientTracing.Enter(invocationId, this, VerbsLifecycle.Invoke, new Dictionary<string, object> { { "cancellationToken", cancellationToken } });
            }

            // Send Request
            if (shouldTrace)
            {
                ServiceClientTracing.SendRequest(invocationId, httpRequest);
            }
            cancellationToken.ThrowIfCancellationRequested();
            using (var client = new HttpClient())
            {
                var httpResponse = client.SendAsync(httpRequest, cancellationToken).Result;

                if (shouldTrace)
                {
                    ServiceClientTracing.ReceiveResponse(invocationId, httpResponse);
                }
                //var statusCode = httpResponse.StatusCode;
                cancellationToken.ThrowIfCancellationRequested();

                WriteObject($"Status: {httpResponse.StatusCode}{Environment.NewLine}Reason: {httpResponse.ReasonPhrase}{Environment.NewLine}Content: {httpResponse.Content.ReadAsStringAsync().Result}");
            }
            
            

            //AzureSession.Instance.ClientFactory.CreateHttpClient(Uri.ToString(), )

        }



        //public override void ExecuteCmdlet()
        //{
        //    if (ParameterSetName.Equals(Constants.ParameterSetNames.ListParameterSet))
        //    {
        //        try
        //        {
        //            WriteObject(BillingManagementClient.BillingPeriods.List(top: MaxCount).Select(x => new PSBillingPeriod(x)), true);
        //        }
        //        catch (ErrorResponseException error)
        //        {
        //            WriteWarning(error.Body.Error.Message);
        //        }
        //        return;
        //    }

        //    if (ParameterSetName.Equals(Constants.ParameterSetNames.SingleItemParameterSet))
        //    {
        //        foreach (var billingPeriodName in Name)
        //        {
        //            try
        //            {
        //                var billingPeriod = new PSBillingPeriod(BillingManagementClient.BillingPeriods.Get(billingPeriodName));
        //                WriteObject(billingPeriod);
        //            }
        //            catch (ErrorResponseException error)
        //            {
        //                WriteWarning(billingPeriodName + ": " + error.Body.Error.Message);
        //                // continue with the next
        //            }
        //        }
        //    }
        //}
    }
}
