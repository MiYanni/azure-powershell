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
    using Common.ArgumentCompleters;
    using Components;
    using Extensions;
    using Common;
    using Newtonsoft.Json.Linq;
    using System.Management.Automation;
    using System.Threading.Tasks;

    /// <summary>
    /// Gets the managed application definition.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureRmManagedApplicationDefinition", DefaultParameterSetName = ManagedApplicationDefinitionNameParameterSet), OutputType(typeof(PSObject))]
    public class GetAzureManagedApplicationDefinitionCmdlet : ManagedApplicationCmdletBase
    {
        /// <summary>
        /// The managed application definition Id parameter set.
        /// </summary>
        internal const string ManagedApplicationDefinitionIdParameterSet = "GetById";

        /// <summary>
        /// The managed application definition name parameter set.
        /// </summary>
        internal const string ManagedApplicationDefinitionNameParameterSet = "GetByNameAndResourceGroup";

        /// <summary>
        /// Gets or sets the managed application definition name parameter.
        /// </summary>
        [Parameter(ParameterSetName = ManagedApplicationDefinitionNameParameterSet, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The managed application definition name.")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the managed application definition resource group parameter
        /// </summary>
        [Parameter(ParameterSetName = ManagedApplicationDefinitionNameParameterSet, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The resource group name.")]
        [ResourceGroupCompleter]
        [ValidateNotNullOrEmpty]
        public string ResourceGroupName { get; set; }

        /// <summary>
        /// Gets or sets the managed application definition id parameter
        /// </summary>
        [Alias("ResourceId", "ManagedApplicationDefinitionId")]
        [Parameter(ParameterSetName = ManagedApplicationDefinitionIdParameterSet, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The fully qualified managed application definition Id, including the subscription. e.g. /subscriptions/{subscriptionId}/resourcegroups/{resourceGroupName}")]
        [ValidateNotNullOrEmpty]
        public string Id { get; set; }

        /// <summary>
        /// Executes the cmdlet.
        /// </summary>
        protected override void OnProcessRecord()
        {
            base.OnProcessRecord();

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
                resources => WriteObject(GetOutputObjects("ManagedApplicationDefinitionId", resources), true));
        }

        /// <summary>
        /// Queries the ARM cache and returns the cached resource that match the query specified.
        /// </summary>
        private async Task<ResponseWithContinuation<JObject[]>> GetResources()
        {
            string resourceId = Id ?? GetResourceId();

            var apiVersion = string.IsNullOrWhiteSpace(ApiVersion) ? Constants.ApplicationApiVersion : ApiVersion;

            if (!string.IsNullOrEmpty(ResourceIdUtility.GetResourceName(resourceId)))
            {
                var resource = await GetResourcesClient()
                    .GetResource<JObject>(
                        resourceId,
                        apiVersion,
                        CancellationToken.Value)
                    .ConfigureAwait(false);
                ResponseWithContinuation<JObject[]> retVal;
                return resource.TryConvertTo(out retVal) && retVal.Value != null
                    ? retVal
                    : new ResponseWithContinuation<JObject[]> { Value = resource.AsArray() };
            }
            return await GetResourcesClient()
                .ListObjectColleciton<JObject>(
                    resourceId,
                    apiVersion,
                    CancellationToken.Value)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the resource Id
        /// </summary>
        private string GetResourceId()
        {
            var subscriptionId = DefaultContext.Subscription.Id;
            if(string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(ResourceGroupName))
            {
                return string.Format("/subscriptions/{0}/resourcegroups/{1}/providers/{2}",
                    subscriptionId.ToString(),
                    ResourceGroupName,
                    Constants.MicrosoftApplicationDefinitionType);
            }
            if (!string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(ResourceGroupName))
            {
                return string.Format("/subscriptions/{0}/resourcegroups/{1}/providers/{2}/{3}",
                    subscriptionId.ToString(),
                    ResourceGroupName,
                    Constants.MicrosoftApplicationDefinitionType,
                    Name);
            }
            return Id;
        }
    }
}
