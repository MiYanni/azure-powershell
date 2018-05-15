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
    using Commands.Common.Authentication;
    using WindowsAzure.Commands.Utilities.Common;
    using Newtonsoft.Json.Linq;
    using System.Management.Automation;
    using System;
    using System.Linq;
    using System.Collections;
    using WindowsAzure.Commands.Common;
    using Commands.Common.Authentication.Abstractions;

    /// <summary>
    /// Creates a policy assignment.
    /// </summary>
    [Cmdlet(VerbsCommon.New, "AzureRmPolicyAssignment", DefaultParameterSetName = ParameterlessPolicyParameterSetName), OutputType(typeof(PSObject))]
    public class NewAzurePolicyAssignmentCmdlet : PolicyCmdletBase, IDynamicParameters
    {
        protected RuntimeDefinedParameterDictionary dynamicParameters = new RuntimeDefinedParameterDictionary();

        protected const string PolicyParameterObjectParameterSetName = "CreateWithPolicyParameterObject";
        protected const string PolicyParameterStringParameterSetName = "CreateWithPolicyParameterString";
        protected const string PolicySetParameterObjectParameterSetName = "CreateWithPolicySetParameterObject";
        protected const string PolicySetParameterStringParameterSetName = "CreateWithPolicySetParameterString";
        protected const string ParameterlessPolicyParameterSetName = "CreateWithoutParameters";

        /// <summary>
        /// Gets or sets the policy assignment name parameter.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The policy assignment name.")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the policy assignment scope parameter
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The scope for policy assignment.")]
        [ValidateNotNullOrEmpty]
        public string Scope { get; set; }

        /// <summary>
        /// Gets or sets the policy assignment not scopes parameter
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The not scopes for policy assignment.")]
        [ValidateNotNullOrEmpty]
        public string[] NotScope { get; set; }

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
        /// Gets or sets the policy assignment policy definition parameter.
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The policy definition object.")]
        [Parameter(ParameterSetName = ParameterlessPolicyParameterSetName,
            Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The policy definition object.")]
        [Parameter(ParameterSetName = PolicyParameterObjectParameterSetName,
            Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The policy definition object.")]
        [Parameter(ParameterSetName = PolicyParameterStringParameterSetName,
            Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The policy definition object.")]
        public PSObject PolicyDefinition { get; set; }

        /// <summary>
        /// Gets or sets the policy assignment policy set definition parameter.
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The policy set definition object.")]
        [Parameter(ParameterSetName = ParameterlessPolicyParameterSetName,
            Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The policy set definition object.")]
        [Parameter(ParameterSetName = PolicySetParameterObjectParameterSetName,
            Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The policy set definition object.")]
        [Parameter(ParameterSetName = PolicySetParameterStringParameterSetName,
            Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The policy set definition object.")]
        public PSObject PolicySetDefinition { get; set; }

        /// <summary>
        /// Gets or sets the policy assignment policy parameter object.
        /// </summary>
        [Parameter(ParameterSetName = PolicyParameterObjectParameterSetName,
            Mandatory = true, ValueFromPipelineByPropertyName = false, HelpMessage = "The policy parameter object.")]
        [Parameter(ParameterSetName = PolicySetParameterObjectParameterSetName,
            Mandatory = true, ValueFromPipelineByPropertyName = false, HelpMessage = "The policy parameter object.")]
        public Hashtable PolicyParameterObject { get; set; }

        /// <summary>
        /// Gets or sets the policy assignment policy parameter file path or policy parameter string.
        /// </summary>
        [Parameter(ParameterSetName = PolicyParameterStringParameterSetName, 
            Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The policy parameter file path or policy parameter string.")]
        [Parameter(ParameterSetName = PolicySetParameterStringParameterSetName,
            Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The policy parameter file path or policy parameter string.")]
        [ValidateNotNullOrEmpty]
        public string PolicyParameter { get; set; }

        /// <summary>
        /// Gets or sets the policy sku object.
        /// </summary>
        [Alias("SkuObject")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "A hash table which represents sku properties. Defaults to Free Sku: Name = A0, Tier = Free")]
        [ValidateNotNullOrEmpty]
        public Hashtable Sku { get; set; }

        /// <summary>
        /// Executes the cmdlet.
        /// </summary>
        protected override void OnProcessRecord()
        {
            base.OnProcessRecord();
            if(PolicyDefinition !=null && PolicySetDefinition !=null)
            {
                throw new PSInvalidOperationException("Only one of PolicyDefinition or PolicySetDefinition can be specified, not both.");
            }
            if (PolicyDefinition !=null && PolicyDefinition.Properties["policyDefinitionId"] == null)
            {
                throw new PSInvalidOperationException("The supplied PolicyDefinition object is invalid.");
            }
            if (PolicySetDefinition != null && PolicySetDefinition.Properties["policySetDefinitionId"] == null)
            {
                throw new PSInvalidOperationException("The supplied PolicySetDefinition object is invalid.");
            }
            string resourceId = GetResourceId();

            var apiVersion = string.IsNullOrWhiteSpace(ApiVersion) ? Constants.PolicyAssignmentApiVersion : ApiVersion;

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

            WriteObject(GetOutputObjects("PolicyAssignmentId", JObject.Parse(result)), true);
        }

        /// <summary>
        /// Gets the resource Id
        /// </summary>
        private string GetResourceId()
        {
            return ResourceIdUtility.GetResourceId(
                Scope,
                Constants.MicrosoftAuthorizationPolicyAssignmentType,
                Name);
        }

        /// <summary>
        /// Constructs the resource
        /// </summary>
        private JToken GetResource()
        {
            var policyassignmentObject = new PolicyAssignment
            {
                Name = Name,
                Sku = Sku == null? new PolicySku { Name = "A0", Tier = "Free" } : Sku.ToDictionary(false).ToJson().FromJson<PolicySku>(),
                Properties = new PolicyAssignmentProperties
                {
                    DisplayName = DisplayName ?? null,
                    Description = Description ?? null,
                    Scope = Scope,
                    NotScopes = NotScope ?? null,
                    Parameters = GetParameters()
                }
            };

            if(PolicyDefinition != null)
            {
                policyassignmentObject.Properties.PolicyDefinitionId = PolicyDefinition.Properties["policyDefinitionId"].Value.ToString();
            }
            else if(PolicySetDefinition != null)
            {
                policyassignmentObject.Properties.PolicyDefinitionId = PolicySetDefinition.Properties["policySetDefinitionId"].Value.ToString();
            }

            return policyassignmentObject.ToJToken();
        }

        object IDynamicParameters.GetDynamicParameters()
        {
            PSObject parameters = null;
            if (PolicyDefinition != null)
            {
                parameters = PolicyDefinition.GetPSObjectProperty("Properties.parameters") as PSObject;
            }
            else if(PolicySetDefinition != null)
            {
                parameters = PolicySetDefinition.GetPSObjectProperty("Properties.parameters") as PSObject;
            }
            if (parameters != null)
            {
                foreach (var param in parameters.Properties)
                {
                    var type = (param.Value as PSObject).Properties["type"];
                    var typeString = type != null ? type.Value.ToString() : string.Empty;
                    var description = (param.Value as PSObject).GetPSObjectProperty("metadata.description");
                    var helpString = description != null ? description.ToString() : string.Format("The {0} policy parameter.", param.Name);
                    var dp = new RuntimeDefinedParameter
                    {
                        Name = param.Name,
                        ParameterType = typeString.Equals("array", StringComparison.OrdinalIgnoreCase) ? typeof(string[]) : typeof(string)
                    };
                    dp.Attributes.Add(new ParameterAttribute
                    {
                        ParameterSetName = ParameterlessPolicyParameterSetName,
                        Mandatory = true,
                        ValueFromPipelineByPropertyName = false,
                        HelpMessage = helpString
                    });
                    dynamicParameters.Add(param.Name, dp);
                }
            }

            RegisterDynamicParameters(dynamicParameters);

            return dynamicParameters;
        }

        private JObject GetParameters()
        {
            // Load parameters from local file or literal
            if (PolicyParameter != null)
            {
                string policyParameterFilePath = this.TryResolvePath(PolicyParameter);
                return FileUtilities.DataStore.FileExists(policyParameterFilePath)
                    ? JObject.Parse(FileUtilities.DataStore.ReadFileAsText(policyParameterFilePath))
                    : JObject.Parse(PolicyParameter);
            }

            // Load from PS object
            if (PolicyParameterObject != null)
            {
                return PolicyParameterObject.ToJObjectWithValue();
            }

            // Load dynamic parameters
            var parameters = PowerShellUtilities.GetUsedDynamicParameters(AsJobDynamicParameters, MyInvocation);
            if (parameters.Count() > 0)
            {
                return MyInvocation.BoundParameters.ToJObjectWithValue(parameters.Select(p => p.Name));
            }

            return null;
        }
    }
}
