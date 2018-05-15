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

namespace Microsoft.Azure.Commands.ResourceManager.Cmdlets.Implementation
{
    using Commands.Common.Authentication.Abstractions;
    using Common.ArgumentCompleters;
    using Components;
    using Extensions;
    using Common;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Management.Automation;
    using System.Threading.Tasks;

    /// <summary>
    /// Gets the deployment operation.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureRmResourceGroupDeploymentOperation"), OutputType(typeof(PSObject))]
    public class GetAzureResourceGroupDeploymentOperationCmdlet : ResourceManagerCmdletBase
    {
        /// <summary>
        /// Gets or sets the deployment name parameter.
        /// </summary>
        [Alias("Name")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The deployment name.")]
        [ValidateNotNullOrEmpty]
        public string DeploymentName { get; set; }

        /// <summary>
        /// Gets or sets the subscription id parameter.
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The subscription to use.")]
        [ValidateNotNullOrEmpty]
        public Guid? SubscriptionId { get; set; }

        /// <summary>
        /// Gets or sets the resource group name parameter.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The resource group name.")]
        [ResourceGroupCompleter]
        [ValidateNotNullOrEmpty]
        public string ResourceGroupName { get; set; }

        /// <summary>
        /// Executes the cmdlet.
        /// </summary>
        protected override void OnProcessRecord()
        {
            base.OnProcessRecord();

            if (SubscriptionId == null)
            {
                SubscriptionId = DefaultContext.Subscription.GetId();
            }

            RunCmdlet();
        }

        /// <summary>
        /// Contains the cmdlet's execution logic.
        /// </summary>
        private void RunCmdlet()
        {
            PaginatedResponseHelper.ForEach(
                () => GetResources(),
                nextLink => GetNextLink<JObject>(nextLink),
                CancellationToken,
                resources => WriteObject(resources.CoalesceEnumerable().SelectArray(resource => resource.ToPsObject("System.Management.Automation.PSCustomObject#DeploymentOperation")), true));
        }

        /// <summary>
        /// Queries the ARM cache and returns the cached resource that match the query specified.
        /// </summary>
        private async Task<ResponseWithContinuation<JObject[]>> GetResources()
        {
            var resourceId = GetResourceId();

            var apiVersion = string.IsNullOrWhiteSpace(ApiVersion) ? Constants.DeploymentOperationApiVersion : ApiVersion;

            return await GetResourcesClient()
                .ListObjectColleciton<JObject>(
                    resourceId,
                    apiVersion,
                    CancellationToken.Value)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the next set of resources using the <paramref name="nextLink"/>
        /// </summary>
        /// <param name="nextLink">The next link.</param>
        private Task<ResponseWithContinuation<TType[]>> GetNextLink<TType>(string nextLink)
        {
            return GetResourcesClient()
                .ListNextBatch<TType>(nextLink, CancellationToken.Value);
        }

        /// <summary>
        /// Gets the resource Id from the supplied PowerShell parameters.
        /// </summary>
        protected string GetResourceId()
        {
            return ResourceIdUtility.GetResourceId(
                SubscriptionId,
                ResourceGroupName,
                Constants.MicrosoftResourcesDeploymentOperationsType,
                DeploymentName);
        }
    }
}
