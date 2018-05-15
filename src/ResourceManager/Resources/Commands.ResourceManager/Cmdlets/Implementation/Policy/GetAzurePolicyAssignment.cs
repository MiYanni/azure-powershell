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
    using Extensions;
    using Common;
    using Newtonsoft.Json.Linq;
    using System.Management.Automation;
    using System.Threading.Tasks;

    /// <summary>
    /// Gets the policy assignment.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureRmPolicyAssignment", DefaultParameterSetName = ParameterlessSet), OutputType(typeof(PSObject))]
    public class GetAzurePolicyAssignmentCmdlet : PolicyCmdletBase
    {
        /// <summary>
        /// The policy Id parameter set.
        /// </summary>
        internal const string PolicyAssignmentIdParameterSet = "GetPolicyAssignmentId";

        /// <summary>
        /// The policy name parameter set.
        /// </summary>
        internal const string PolicyAssignmentNameParameterSet = "GetPolicyAssignmentName";

        /// <summary>
        /// The list all policy parameter set.
        /// </summary>
        internal const string ParameterlessSet = "GetAllPolicyAssignments";

        /// <summary>
        /// Gets or sets the policy assignment name parameter.
        /// </summary>
        [Parameter(ParameterSetName = PolicyAssignmentNameParameterSet, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The policy assignment name.")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the policy assignment scope parameter.
        /// </summary>
        [Parameter(ParameterSetName = PolicyAssignmentNameParameterSet, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The policy assignment name.")]
        [ValidateNotNullOrEmpty]
        public string Scope { get; set; }

        /// <summary>
        /// Gets or sets the policy assignment id parameter
        /// </summary>
        [Alias("ResourceId")]
        [Parameter(ParameterSetName = PolicyAssignmentIdParameterSet, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The fully qualified policy assignment Id, including the subscription. e.g. /subscriptions/{subscriptionId}/resourcegroups/{resourceGroupName}")]
        [ValidateNotNullOrEmpty]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the policy assignment policy definition id parameter
        /// </summary>
        [Parameter(ParameterSetName = PolicyAssignmentIdParameterSet, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The fully qualified policy assignment Id, including the subscription. e.g. /subscriptions/{subscriptionId}/resourcegroups/{resourceGroupName}")]
        [Parameter(ParameterSetName = PolicyAssignmentNameParameterSet, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The fully qualified policy assignment Id, including the subscription. e.g. /subscriptions/{subscriptionId}/resourcegroups/{resourceGroupName}")]
        [ValidateNotNullOrEmpty]
        public string PolicyDefinitionId { get; set; }

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
                resources => WriteObject(GetOutputObjects("PolicyAssignmentId", resources), true));
        }

        /// <summary>
        /// Queries the ARM cache and returns the cached resource that match the query specified.
        /// </summary>
        private async Task<ResponseWithContinuation<JObject[]>> GetResources()
        {
            string resourceId = Id ?? GetResourceId();

            var apiVersion = string.IsNullOrWhiteSpace(ApiVersion) ? Constants.PolicyAssignmentApiVersion : ApiVersion;

            if (IsResourceGet(resourceId))
            {
                var resource = await GetResourcesClient()
                    .GetResource<JObject>(
                        resourceId,
                        apiVersion,
                        CancellationToken.Value,
                        null)
                    .ConfigureAwait(false);
                ResponseWithContinuation<JObject[]> retVal;
                return resource.TryConvertTo(out retVal) && retVal.Value != null
                    ? retVal
                    : new ResponseWithContinuation<JObject[]> { Value = resource.AsArray() };
            }
            if (IsScopeLevelList(resourceId))//If only scope is given, list assignments call
            {
                string filter = "$filter=atScope()";
                return await GetResourcesClient()
                    .ListObjectColleciton<JObject>(
                        resourceId,
                        apiVersion,
                        CancellationToken.Value,
                        filter)
                    .ConfigureAwait(false);
            }
            else
            {
                string filter = string.IsNullOrEmpty(PolicyDefinitionId)
                    ? null
                    : string.Format("$filter=policydefinitionid eq '{0}'", PolicyDefinitionId);

                return await GetResourcesClient()
                    .ListObjectColleciton<JObject>(
                        resourceId,
                        apiVersion,
                        CancellationToken.Value,
                        filter)
                    .ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Returns true if it is scope level policy assignment list call
        /// </summary>
        private bool IsScopeLevelList(string resourceId)
        {
            return !string.IsNullOrEmpty(Scope) && string.IsNullOrEmpty(Name)
                || !string.IsNullOrEmpty(Scope) && string.IsNullOrEmpty(ResourceIdUtility.GetResourceName(resourceId));
        }

        /// <summary>
        /// Returns true if it is a single policy assignment get
        /// </summary>
        /// <param name="resourceId"></param>
        private bool IsResourceGet(string resourceId)
        {
            return !string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(Scope)
                || !string.IsNullOrEmpty(ResourceIdUtility.GetResourceName(resourceId));
        }

        /// <summary>
        /// Gets the resource Id
        /// </summary>
        private string GetResourceId()
        {
            var subscriptionId = DefaultContext.Subscription.Id;
            if (string.IsNullOrEmpty(Name) && string.IsNullOrEmpty(Scope))
            {
                return string.Format("/subscriptions/{0}/providers/{1}",
                    subscriptionId.ToString(),
                    Constants.MicrosoftAuthorizationPolicyAssignmentType);
            }
            if (string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(Scope))
            {
                return ResourceIdUtility.GetResourceId(
                    Scope,
                    Constants.MicrosoftAuthorizationPolicyAssignmentType,
                    null);
            }
            return ResourceIdUtility.GetResourceId(
                Scope,
                Constants.MicrosoftAuthorizationPolicyAssignmentType,
                Name);
        }
    }
}