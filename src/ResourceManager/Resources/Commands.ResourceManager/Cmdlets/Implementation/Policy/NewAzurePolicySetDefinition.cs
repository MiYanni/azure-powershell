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
    using WindowsAzure.Commands.Utilities.Common;
    using Newtonsoft.Json.Linq;
    using System.IO;
    using System.Management.Automation;
    using WindowsAzure.Commands.Common;

    /// <summary>
    /// Creates the policy definition.
    /// </summary>
    [Cmdlet(VerbsCommon.New, "AzureRmPolicySetDefinition", SupportsShouldProcess = true), OutputType(typeof(PSObject))]
    public class NewAzurePolicySetDefinitionCmdlet : PolicyCmdletBase
    {
        /// <summary>
        /// Gets or sets the policy set definition name parameter.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The policy set definition name.")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the policy definition display name parameter
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The display name for policy set definition.")]
        [ValidateNotNullOrEmpty]
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the policy definition description parameter
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The description for policy set definition.")]
        [ValidateNotNullOrEmpty]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the policy set definition metadata parameter
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The metadata for policy set definition. This can either be a path to a file name containing the metadata, or the metadata as string.")]
        [ValidateNotNullOrEmpty]
        public string Metadata { get; set; }

        /// <summary>
        /// Gets or sets the policy definition parameter
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The policy definition. This can either be a path to a file name containing the policy definitions, or the policy set definition as string.")]
        [ValidateNotNullOrEmpty]
        public string PolicyDefinition { get; set; }

        /// <summary>
        /// Gets or sets the policy definition parameters parameter
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The parameters declaration for policy set definition. This can either be a path to a file name containing the parameters declaration, or the parameters declaration as string.")]
        [ValidateNotNullOrEmpty]
        public string Parameter { get; set; }

        /// <summary>
        /// Executes the cmdlet.
        /// </summary>
        protected override void OnProcessRecord()
        {
            base.OnProcessRecord();

            if(ShouldProcess(Name, "Create Policy Set Definition"))
            {
                string resourceId = GetResourceId();

                var apiVersion = string.IsNullOrWhiteSpace(ApiVersion) ? Constants.PolicySetDefintionApiVersion : ApiVersion;

                var operationResult = GetResourcesClient()
                    .PutResource(
                        resourceId,
                        apiVersion,
                        GetResource(),
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
        /// Gets the resource Id
        /// </summary>
        private string GetResourceId()
        {
            var subscriptionId = DefaultContext.Subscription.Id;
            return string.Format("/subscriptions/{0}/providers/{1}/{2}",
                subscriptionId.ToString(),
                Constants.MicrosoftAuthorizationPolicySetDefinitionType,
                Name);
        }

        /// <summary>
        /// Constructs the resource
        /// </summary>
        private JToken GetResource()
        {
            var policySetDefinitionObject = new PolicySetDefinition
            {
                Name = Name,
                Properties = new PolicySetDefinitionProperties
                {
                    Description = Description ?? null,
                    DisplayName = DisplayName ?? null,
                    PolicyDefinitions = JArray.Parse(GetObjectFromParameter(PolicyDefinition).ToString()),
                    Metadata = Metadata == null ? null : JObject.Parse(GetObjectFromParameter(Metadata).ToString()),
                    Parameters = Parameter == null ? null : JObject.Parse(GetObjectFromParameter(Parameter).ToString())
                }
            };

            return policySetDefinitionObject.ToJToken();
        }
    }
}
