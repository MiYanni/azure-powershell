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
    /// A cmdlet that removes an azure resource.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "AzureRmResource", SupportsShouldProcess = true, DefaultParameterSetName = ResourceIdParameterSet), OutputType(typeof(bool))]
    public class RemoveAzureResourceCmdlet : ResourceManipulationCmdletBase
    {
        [Parameter(Mandatory = false, HelpMessage = "Run cmdlet in the background")]
        public SwitchParameter AsJob { get; set; }

        /// <summary>
        /// Executes the cmdlet.
        /// </summary>
        protected override void OnProcessRecord()
        {
            base.OnProcessRecord();

            var resourceId = GetResourceId();

            ConfirmAction(
                Force,
                string.Format("Are you sure you want to delete the following resource: {0}", resourceId),
                "Deleting the resource...",
                resourceId,
                () =>
                {
                    var apiVersion = DetermineApiVersion(resourceId).Result;

                    var operationResult = GetResourcesClient()
                        .DeleteResource(
                            resourceId,
                            apiVersion,
                            CancellationToken.Value,
                            ODataQuery)
                        .Result;

                    var managementUri = GetResourcesClient()
                        .GetResourceManagementRequestUri(
                            resourceId,
                            apiVersion,
                            odataQuery: ODataQuery);

                    var activity = string.Format("DELETE {0}", managementUri.PathAndQuery);

                    GetLongRunningOperationTracker(activity, false)
                        .WaitOnOperation(operationResult);

                    WriteObject(true);
                });
        }
    }
}