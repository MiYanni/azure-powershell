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
    using System.Management.Automation;

    /// <summary>
    /// The remove azure resource lock cmdlet.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "AzureRmResourceLock", SupportsShouldProcess = true, DefaultParameterSetName = LockIdParameterSet), OutputType(typeof(PSObject))]
    public class RemoveAzureResourceLockCmdlet : ResourceLockManagementCmdletBase
    {
        /// <summary>
        /// Gets or sets the extension resource name parameter.
        /// </summary>
        [Alias("ExtensionResourceName", "Name")]
        [Parameter(ParameterSetName = ResourceGroupLevelLock, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The name of the lock.")]
        [Parameter(ParameterSetName = ResourceGroupResourceLevelLock, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The name of the lock.")]
        [Parameter(ParameterSetName = ScopeLevelLock, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The name of the lock.")]
        [Parameter(ParameterSetName = SubscriptionLevelLock, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The name of the lock.")]
        [Parameter(ParameterSetName = SubscriptionResourceLevelLock, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The name of the lock.")]
        [Parameter(ParameterSetName = TenantResourceLevelLock, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The name of the lock.")]
        [ValidateNotNullOrEmpty]
        public string LockName { get; set; }

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
            var resourceId = GetResourceId(LockName);
            ConfirmAction(
                Force,
                string.Format("Are you sure you want to delete the following lock: {0}", resourceId),
                "Deleting the lock...",
                resourceId,
                () =>
                {
                    var operationResult = GetResourcesClient()
                        .DeleteResource(
                            resourceId,
                            LockApiVersion,
                            CancellationToken.Value)
                        .Result;

                    if (operationResult.HttpStatusCode == System.Net.HttpStatusCode.NoContent)
                    {
                        throw new PSInvalidOperationException(string.Format("The resource lock '{0}' could not be found.", resourceId));
                    }

                    var managementUri = GetResourcesClient()
                        .GetResourceManagementRequestUri(
                            resourceId,
                            LockApiVersion);

                    var activity = string.Format("DELETE {0}", managementUri.PathAndQuery);

                    var result = GetLongRunningOperationTracker(activity, false)
                        .WaitOnOperation(operationResult);

                    WriteObject(true);
                });
        }
    }
}
