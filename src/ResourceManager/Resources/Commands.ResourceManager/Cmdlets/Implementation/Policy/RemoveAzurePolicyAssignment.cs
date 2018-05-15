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

namespace Microsoft.Azure.Commands.ResourceManager.Cmdlets.Implementation
{
    using Components;
    using System.Management.Automation;

    /// <summary>
    /// Removes the policy assignment.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "AzureRmPolicyAssignment", SupportsShouldProcess = true, 
        DefaultParameterSetName = PolicyAssignmentNameParameterSet), 
        OutputType(typeof(bool))]
    public class RemoveAzurePolicyAssignmentCmdlet : PolicyCmdletBase
    {
        /// <summary>
        /// The policy assignment Id parameter set.
        /// </summary>
        internal const string PolicyAssignmentIdParameterSet = "RemoveByPolicyAssignmentId";

        /// <summary>
        /// The policy assignment name parameter set.
        /// </summary>
        internal const string PolicyAssignmentNameParameterSet = "RemoveByPolicyAssignmentName";

        /// <summary>
        /// Gets or sets the policy assignment name parameter.
        /// </summary>
        [Parameter(ParameterSetName = PolicyAssignmentNameParameterSet, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The policy assignment name.")]
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
            base.OnProcessRecord();
            string resourceId = Id ?? GetResourceId();
            var apiVersion = string.IsNullOrWhiteSpace(ApiVersion) ? Constants.PolicyAssignmentApiVersion : ApiVersion;

            ConfirmAction(
                "Deleting the policy assignment...",
                resourceId,
                () =>
                {
                    var operationResult = GetResourcesClient()
                        .DeleteResource(
                            resourceId,
                            apiVersion,
                            CancellationToken.Value,
                            null)
                        .Result;

                    var managementUri = GetResourcesClient()
                        .GetResourceManagementRequestUri(
                            resourceId,
                            apiVersion,
                            odataQuery: null);

                    var activity = string.Format("DELETE {0}", managementUri.PathAndQuery);

                    GetLongRunningOperationTracker(activity, false)
                        .WaitOnOperation(operationResult);

                    WriteObject(true);
                });
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