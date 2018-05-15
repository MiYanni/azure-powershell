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
    using System.Management.Automation;

    /// <summary>
    /// Removes the managed application definition.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "AzureRmManagedApplicationDefinition", SupportsShouldProcess = true,
        DefaultParameterSetName = ManagedApplicationDefinitionNameParameterSet), 
        OutputType(typeof(bool))]
    public class RemoveAzureManagedApplicationDefinitionCmdlet : ManagedApplicationCmdletBase
    {
        /// <summary>
        /// The policy Id parameter set.
        /// </summary>
        internal const string ManagedApplicationDefinitionIdParameterSet = "RemoveById";

        /// <summary>
        /// The policy name parameter set.
        /// </summary>
        internal const string ManagedApplicationDefinitionNameParameterSet = "RemoveByNameAndResourceGroup";

        /// <summary>
        /// Gets or sets the managed application definition name parameter.
        /// </summary>
        [Parameter(ParameterSetName = ManagedApplicationDefinitionNameParameterSet, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The managed application definition name.")]
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
        [Alias("ResourceId")]
        [Parameter(ParameterSetName = ManagedApplicationDefinitionIdParameterSet, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The fully qualified managed application definition Id, including the subscription. e.g. /subscriptions/{subscriptionId}/resourcegroups/{resourceGroupName}")]
        [ValidateNotNullOrEmpty]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the force parameter.
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "Do not ask for confirmation.")]
        public SwitchParameter Force { get; set; }

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
            var apiVersion = string.IsNullOrWhiteSpace(ApiVersion) ? Constants.ApplicationApiVersion : ApiVersion;

            ConfirmAction(
                Force,
                string.Format("Are you sure you want to delete the following managed application definition: {0}", resourceId),
                "Deleting the managed application definition...",
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
            var subscriptionId = DefaultContext.Subscription.Id;
            return string.Format("/subscriptions/{0}/resourcegroups/{1}/providers/{2}/{3}",
                subscriptionId.ToString(),
                ResourceGroupName,
                Constants.MicrosoftApplicationDefinitionType,
                Name);
        }
    }
}
