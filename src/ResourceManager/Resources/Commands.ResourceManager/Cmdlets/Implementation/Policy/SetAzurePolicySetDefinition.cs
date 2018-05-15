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
    [Cmdlet(VerbsCommon.Set, "AzureRmPolicySetDefinition", DefaultParameterSetName = PolicySetDefinitionNameParameterSet, SupportsShouldProcess = true), 
        OutputType(typeof(PSObject))]
    public class SetAzurePolicySetDefinitionCmdlet : PolicyCmdletBase
    {
        /// <summary>
        /// The policy Id parameter set.
        /// </summary>
        internal const string PolicySetDefinitionIdParameterSet = "SetById";

        /// <summary>
        /// The policy name parameter set.
        /// </summary>
        internal const string PolicySetDefinitionNameParameterSet = "SetByNameAndResourceGroup";

        /// <summary>
        /// Gets or sets the policy set definition name parameter.
        /// </summary>
        [Parameter(ParameterSetName = PolicySetDefinitionNameParameterSet, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The policy set definition name.")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the policy set definition id parameter
        /// </summary>
        [Alias("ResourceId")]
        [Parameter(ParameterSetName = PolicySetDefinitionIdParameterSet, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The fully qualified policy set definition Id, including the subscription. e.g. /subscriptions/{subscriptionId}/resourcegroups/{resourceGroupName}")]
        [ValidateNotNullOrEmpty]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the policy set definition display name parameter
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The display name for policy set definition.")]
        [ValidateNotNullOrEmpty]
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the policy set definition description parameter
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The description for policy set definition.")]
        [ValidateNotNullOrEmpty]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the policy definition parameter
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The policy set definition. This can either be a path to a file name containing the policy definitions, or the policy set definition as string.")]
        [ValidateNotNullOrEmpty]
        public string PolicyDefinition { get; set; }

        /// <summary>
        /// Executes the cmdlet.
        /// </summary>
        protected override void OnProcessRecord()
        {
            base.OnProcessRecord();
            if (ShouldProcess(Name, "Update Policy Set Definition"))
            {
                string resourceId = Id ?? GetResourceId();
                var apiVersion = string.IsNullOrWhiteSpace(ApiVersion) ? Constants.PolicySetDefintionApiVersion : ApiVersion;

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

                WriteObject(GetOutputObjects("PolicySetDefinitionId", JObject.Parse(result)), true);
            }
        }

        /// <summary>
        /// Constructs the resource
        /// </summary>
        private JToken GetResource(string resourceId, string apiVersion)
        {
            var resource = GetExistingResource(resourceId, apiVersion).Result.ToResource();

            var policySetDefinitionObject = new PolicySetDefinition
            {
                Name = Name ?? ResourceIdUtility.GetResourceName(Id),
                Properties = new PolicySetDefinitionProperties
                {
                    Description = Description ?? (resource.Properties["description"] != null
                        ? resource.Properties["description"].ToString()
                        : null),
                    DisplayName = DisplayName ?? (resource.Properties["displayName"] != null
                        ? resource.Properties["displayName"].ToString()
                        : null)
                }
            };
            if (!string.IsNullOrEmpty(PolicyDefinition))
            {
                policySetDefinitionObject.Properties.PolicyDefinitions = GetPolicyDefinitionsObject();
            }
            else
            {
                policySetDefinitionObject.Properties.PolicyDefinitions = JArray.Parse(resource.Properties["policyDefinitions"].ToString());
            }

            return policySetDefinitionObject.ToJToken();
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
                Constants.MicrosoftAuthorizationPolicySetDefinitionType,
                Name);
        }

        /// <summary>
        /// Gets the policy definitions object
        /// </summary>
        private JArray GetPolicyDefinitionsObject()
        {
            string policyFilePath = this.TryResolvePath(PolicyDefinition);

            return File.Exists(policyFilePath)
                ? JArray.Parse(FileUtilities.DataStore.ReadFileAsText(policyFilePath))
                : JArray.Parse(PolicyDefinition);
        }
    }
}
