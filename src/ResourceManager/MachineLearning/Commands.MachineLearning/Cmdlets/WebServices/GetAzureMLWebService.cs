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
using System.Collections.Generic;
using System.Management.Automation;
using System.Threading.Tasks;
using Microsoft.Azure.Management.MachineLearning.WebServices.Models;
using Microsoft.Azure.Commands.ResourceManager.Common;
using Microsoft.Azure.Commands.ResourceManager.Common.ArgumentCompleters;

namespace Microsoft.Azure.Commands.MachineLearning
{
    [Cmdlet(VerbsCommon.Get, CommandletSuffix)]
    [OutputType(typeof(WebService), typeof(WebService[]))]
    public class GetAzureMLWebService : WebServicesCmdletBase
    {
        [Parameter(
            Mandatory = false, 
            HelpMessage = "The name of the resource group for the Azure ML web service.")]
        [ResourceGroupCompleter]
        [ValidateNotNullOrEmpty]
        public string ResourceGroupName { get; set; }

        [Parameter(
            Mandatory = false, 
            HelpMessage = "The name of the web service.")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        [Parameter(
            Mandatory = false,
            HelpMessage = "The name of region")]
        [LocationCompleter("Microsoft.MachineLearning/webServices")]
        [ValidateNotNullOrEmpty]
        public string Region { get; set; }

        protected override void RunCmdlet()
        {
            if (!string.IsNullOrWhiteSpace(Name))
            {
                if (string.IsNullOrWhiteSpace(ResourceGroupName))
                {
                    // If there is only web service name but no resource group name, it is invalid input.
                    throw new PSArgumentNullException(ResourceGroupName, Resources.MissingResourceGroupName);
                }

                // If this is a simple get web service by name operation, resolve it as such
                WebService service = WebServicesClient.GetAzureMlWebService(ResourceGroupName, Name, Region);
                WriteObject(service);
            }
            else
            {
                // This is a collection of web services get call, so determine which flavor it is
                Func<Task<ResponseWithContinuation<WebService[]>>> getFirstServicesPageCall = () => WebServicesClient.GetAzureMlWebServicesBySubscriptionAsync(null, CancellationToken);
                Func<string, Task<ResponseWithContinuation<WebService[]>>> getNextPageCall = nextLink => WebServicesClient.GetAzureMlWebServicesBySubscriptionAsync(nextLink, CancellationToken);

                if (!string.IsNullOrWhiteSpace(ResourceGroupName))
                {
                    // This is a call for resource retrieval within a resource group
                    getFirstServicesPageCall = () => WebServicesClient.GetAzureMlWebServicesBySubscriptionAndGroupAsync(ResourceGroupName, null, CancellationToken);
                    getNextPageCall = nextLink => WebServicesClient.GetAzureMlWebServicesBySubscriptionAndGroupAsync(ResourceGroupName, nextLink, CancellationToken);
                }

                PaginatedResponseHelper.ForEach<WebService>(
                    getFirstServicesPageCall,
                    getNextPageCall,
                    CancellationToken,
                    webServices =>
                    {
                        foreach (var service in webServices)
                        {
                            WriteObject(service, true);
                        }
                    });
            }
        }
    }
}
