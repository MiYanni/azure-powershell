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

using Microsoft.Azure.Commands.Common.Authentication;

namespace Microsoft.Azure.Commands.ResourceManager.Cmdlets.Implementation
{
    using Commands.Common.Authentication.Abstractions;
    using Components;
    using Entities.Policy;
    using Extensions;
    using WindowsAzure.Commands.Utilities.Common;
    using Newtonsoft.Json.Linq;
    using System.IO;
    using System.Management.Automation;
    using System.Threading.Tasks;
    using WindowsAzure.Commands.Common;

    /// <summary>
    /// Sets the policy definition.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "AzureRmPolicyDefinition", DefaultParameterSetName = PolicyDefinitionNameParameterSet), OutputType(typeof(PSObject))]
    public class SetAzurePolicyDefinitionCmdlet : PolicyCmdletBase
    {
        /// <summary>
        /// The policy Id parameter set.
        /// </summary>
        internal const string PolicyDefinitionIdParameterSet = "SetByPolicyDefinitionId";

        /// <summary>
        /// The policy name parameter set.
        /// </summary>
        internal const string PolicyDefinitionNameParameterSet = "SetByPolicyDefinitionName";

        /// <summary>
        /// Gets or sets the policy definition name parameter.
        /// </summary>
        [Parameter(ParameterSetName = GetAzurePolicyDefinitionCmdlet.PolicyDefinitionNameParameterSet, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The policy definition name.")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the policy definition id parameter
        /// </summary>
        [Alias("ResourceId")]
        [Parameter(ParameterSetName = GetAzurePolicyDefinitionCmdlet.PolicyDefinitionIdParameterSet, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The fully qualified policy definition Id, including the subscription. e.g. /subscriptions/{subscriptionId}/resourcegroups/{resourceGroupName}")]
        [ValidateNotNullOrEmpty]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the policy definition display name parameter
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The display name for policy definition.")]
        [ValidateNotNullOrEmpty]
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the policy definition description parameter
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The description for policy definition.")]
        [ValidateNotNullOrEmpty]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the policy rule parameter
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The rule for policy definition. This can either be a path to a file name or uri containing the rule, or the rule as string.")]
        [ValidateNotNullOrEmpty]
        public string Policy { get; set; }

        /// <summary>
        /// Gets or sets the policy definition metadata parameter
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The metadata for policy definition. This can either be a path to a file name containing the metadata, or the metadata as string.")]
        [ValidateNotNullOrEmpty]
        public string Metadata { get; set; }

        /// <summary>
        /// Gets or sets the policy definition parameters parameter
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The parameters declaration for policy definition. This can either be a path to a file name or uri containing the parameters declaration, or the parameters declaration as string.")]
        [ValidateNotNullOrEmpty]
        public string Parameter { get; set; }

        /// <summary>
        /// Executes the cmdlet.
        /// </summary>
        protected override void OnProcessRecord()
        {
            base.OnProcessRecord();
            string resourceId = Id ?? GetResourceId();
            var apiVersion = string.IsNullOrWhiteSpace(ApiVersion) ? Constants.PolicyDefinitionApiVersion : ApiVersion;

            var operationResult = GetResourcesClient()
                        .PutResource(
                            resourceId,
                            apiVersion,
                            GetResource(resourceId, apiVersion),
                            CancellationToken.Value,
                            null)
                        .Result;

            var managementUri = GetResourcesClient()
              .GetResourceManagementRequestUri(
                  resourceId,
                  apiVersion,
                  odataQuery: null);

            var activity = string.Format("PUT {0}", managementUri.PathAndQuery);
            var result = GetLongRunningOperationTracker(activity, true)
                .WaitOnOperation(operationResult);

            WriteObject(GetOutputObjects("PolicyDefinitionId", JObject.Parse(result)), true);
        }

        /// <summary>
        /// Constructs the resource
        /// </summary>
        private JToken GetResource(string resourceId, string apiVersion)
        {
            var resource = GetExistingResource(resourceId, apiVersion).Result.ToResource();

            var policyDefinitionObject = new PolicyDefinition
            {
                Name = Name ?? ResourceIdUtility.GetResourceName(Id),
                Properties = new PolicyDefinitionProperties
                {
                    Description = Description ?? (resource.Properties["description"] != null
                        ? resource.Properties["description"].ToString()
                        : null),
                    DisplayName = DisplayName ?? (resource.Properties["displayName"] != null
                        ? resource.Properties["displayName"].ToString()
                        : null)
                }
            };
            if (!string.IsNullOrEmpty(Policy))
            {
                policyDefinitionObject.Properties.PolicyRule = JObject.Parse(GetObjectFromParameter(Policy).ToString());
            }
            else
            {
                policyDefinitionObject.Properties.PolicyRule = JObject.Parse(resource.Properties["policyRule"].ToString());
            }
            if (!string.IsNullOrEmpty(Metadata))
            {
                policyDefinitionObject.Properties.Metadata = JObject.Parse(GetObjectFromParameter(Metadata).ToString());
            }
            else
            {
                policyDefinitionObject.Properties.Metadata = resource.Properties["metaData"] == null 
                    ? null
                    : JObject.Parse(resource.Properties["metaData"].ToString());
            }
            if (!string.IsNullOrEmpty(Parameter))
            {
                policyDefinitionObject.Properties.Parameters = JObject.Parse(GetObjectFromParameter(Parameter).ToString());
            }
            else
            {
                policyDefinitionObject.Properties.Parameters = resource.Properties["parameters"] == null
                    ? null
                    : JObject.Parse(resource.Properties["parameters"].ToString());
            }
            return policyDefinitionObject.ToJToken();
        }

        /// <summary>
        /// Gets a resource.
        /// </summary>
        private async Task<JObject> GetExistingResource(string resourceId, string apiVersion)
        {
            return await GetResourcesClient()
                .GetResource<JObject>(
                    resourceId,
                    apiVersion,
                    CancellationToken.Value)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the resource Id from the supplied PowerShell parameters.
        /// </summary>
        protected string GetResourceId()
        {
            var subscriptionId = DefaultContext.Subscription.Id;
            return string.Format("/subscriptions/{0}/providers/{1}/{2}",
                subscriptionId.ToString(),
                Constants.MicrosoftAuthorizationPolicyDefinitionType,
                Name);
        }
    }
}
