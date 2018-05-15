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
    using Entities.Locks;
    using Extensions;
    using Newtonsoft.Json.Linq;
    using System.Management.Automation;

    /// <summary>
    /// The new azure resource lock cmdlet.
    /// </summary>
    [Cmdlet(VerbsCommon.New, "AzureRmResourceLock", SupportsShouldProcess = true, DefaultParameterSetName = ScopeLevelLock), OutputType(typeof(PSObject))]
    public class NewAzureResourceLockCmdlet : ResourceLockManagementCmdletBase
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
        /// Gets or sets the extension resource name parameter.
        /// </summary>
        [Alias("Level")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The level of the lock.")]
        [ValidateNotNullOrEmpty]
        public LockLevel LockLevel { get; set; }

        /// <summary>
        /// Gets or sets the extension resource name parameter.
        /// </summary>
        [Alias("Notes")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The notes of the lock.")]
        [ValidateNotNullOrEmpty]
        public string LockNotes { get; set; }

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
                GetActionMessage(resourceId),
                GetProccessMessage(),
                resourceId,
                () =>
                {
                    var resourceBody = GetResourceBody();

                    var operationResult = GetResourcesClient()
                        .PutResource(
                            resourceId,
                            LockApiVersion,
                            resourceBody,
                            CancellationToken.Value)
                        .Result;

                    var managementUri = GetResourcesClient()
                      .GetResourceManagementRequestUri(
                          resourceId,
                          LockApiVersion);

                    var activity = string.Format("PUT {0}", managementUri.PathAndQuery);
                    var result = GetLongRunningOperationTracker(activity, true)
                        .WaitOnOperation(operationResult);

                    WriteObject(GetOutputObjects(JObject.Parse(result)), true);
                });
        }

        /// <summary>
        /// Gets the action message.
        /// </summary>
        /// <param name="resourceId">The resource Id.</param>
        protected virtual string GetActionMessage(string resourceId)
        {
            return "Are you sure you want to create the following lock: " + resourceId;
        }

        /// <summary>
        /// Gets the process message.
        /// </summary>
        protected virtual string GetProccessMessage()
        {
            return "Creating the lock.";
        }

        /// <summary>
        /// Gets the lock.
        /// </summary>
        private JToken GetResourceBody()
        {
            var lockObject = new Lock
            {
                Properties = new LockProperties
                {
                    Level = LockLevel,
                    Notes = LockNotes,
                }
            };

            return lockObject.ToJToken();
        }
    }
}