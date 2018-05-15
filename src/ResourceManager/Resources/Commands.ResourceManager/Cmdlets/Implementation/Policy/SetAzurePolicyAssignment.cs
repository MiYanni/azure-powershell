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
    using Components;
    using Entities.Policy;
    using Extensions;
    using WindowsAzure.Commands.Common;
    using Newtonsoft.Json.Linq;
    using System.Collections;
    using System.Management.Automation;
    using System.Threading.Tasks;

    /// <summary>
    /// Sets the policy assignment.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "AzureRmPolicyAssignment", DefaultParameterSetName = PolicyAssignmentNameParameterSet), OutputType(typeof(PSObject))]
    public class SetAzurePolicyAssignmentCmdlet : PolicyCmdletBase
    {
        /// <summary>
        /// The policy Id parameter set.
        /// </summary>
        internal const string PolicyAssignmentIdParameterSet = "SetByPolicyAssignmentId";

        /// <summary>
        /// The policy name parameter set.
        /// </summary>
        internal const string PolicyAssignmentNameParameterSet = "SetByPolicyAssignmentName";

        /// <summary>
        /// Gets or sets the policy assignment name parameter.
        /// </summary>
        [Parameter(ParameterSetName = PolicyAssignmentNameParameterSet, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The policy assignment name.")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the policy assignment scope parameter.
        /// </summary>
        [Parameter(ParameterSetName = PolicyAssignmentNameParameterSet, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The policy assignment scope.")]
        [ValidateNotNullOrEmpty]
        public string Scope { get; set; }

        /// <summary>
        /// Gets or sets the policy assignment not scopes parameter.
        /// </summary>
        [Parameter(ParameterSetName = PolicyAssignmentNameParameterSet, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The policy assignment not scopes.")]
        [ValidateNotNullOrEmpty]
        public string[] NotScope { get; set; }

        /// <summary>
        /// Gets or sets the policy assignment id parameter
        /// </summary>
        [Alias("ResourceId")]
        [Parameter(ParameterSetName = PolicyAssignmentIdParameterSet, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The fully qualified policy assignment Id, including the subscription. e.g. /subscriptions/{subscriptionId}/resourcegroups/{resourceGroupName}")]
        [ValidateNotNullOrEmpty]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the policy assignment display name parameter
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The display name for policy assignment.")]
        [ValidateNotNullOrEmpty]
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the policy assignment description parameter
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The description for policy assignment.")]
        [ValidateNotNullOrEmpty]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the policy sku object.
        /// </summary>
        [Alias("SkuObject")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "A hash table which represents sku properties.")]
        [ValidateNotNullOrEmpty]
        public Hashtable Sku { get; set; }

        /// <summary>
        /// Executes the cmdlet.
        /// </summary>
        protected override void OnProcessRecord()
        {
            base.OnProcessRecord();
            string resourceId = Id ?? GetResourceId();
            var apiVersion = string.IsNullOrWhiteSpace(ApiVersion) ? Constants.PolicyAssignmentApiVersion : ApiVersion;

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

            WriteObject(GetOutputObjects("PolicyAssignmentId", JObject.Parse(result)), true);
        }

        /// <summary>
        /// Constructs the resource
        /// </summary>
        private JToken GetResource(string resourceId, string apiVersion)
        {
            var resource = GetExistingResource(resourceId, apiVersion).Result.ToResource();

            var policyAssignmentObject = new PolicyAssignment
            {
                Name = Name ?? ResourceIdUtility.GetResourceName(Id),
                Sku = Sku != null
                    ? Sku.ToDictionary(false).ToJson().FromJson<PolicySku>()
                    : (resource.Sku == null ? new PolicySku { Name = "A0", Tier = "Free" } : resource.Sku.ToJson().FromJson<PolicySku>()),
                Properties = new PolicyAssignmentProperties
                {
                    DisplayName = DisplayName ?? (resource.Properties["displayName"] != null
                        ? resource.Properties["displayName"].ToString()
                        : null),
                    Description = Description ?? (resource.Properties["description"] != null
                        ? resource.Properties["description"].ToString()
                        : null),
                    Scope = resource.Properties["scope"].ToString(),
                    NotScopes = NotScope ?? (resource.Properties["NotScopes"] == null ? null : resource.Properties["NotScopes"].ToString().Split(',')),
                    PolicyDefinitionId = resource.Properties["policyDefinitionId"].ToString()
                }
            };

            return policyAssignmentObject.ToJToken();
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
            return ResourceIdUtility.GetResourceId(
                Scope,
                Constants.MicrosoftAuthorizationPolicyAssignmentType,
                Name);
        }
    }
}
