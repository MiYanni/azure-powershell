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
    using Extensions;
    using WindowsAzure.Commands.Common;
    using Newtonsoft.Json.Linq;
    using ProjectResources = Properties.Resources;
    using System.Collections;
    using System.Management.Automation;

    /// <summary>
    /// A cmdlet that invokes a resource action.
    /// </summary>
    [Cmdlet(VerbsLifecycle.Invoke, "AzureRmResourceAction", SupportsShouldProcess = true, DefaultParameterSetName = ResourceIdParameterSet), OutputType(typeof(PSObject))]
    public sealed class InvokAzureResourceActionCmdlet : ResourceManipulationCmdletBase
    {
        /// <summary>
        /// Gets or sets the property object.
        /// </summary>
        [Alias("Object")]
        [Parameter(Mandatory = false, HelpMessage = "A hash table which represents resource properties.")]
        [ValidateNotNullOrEmpty]
        public Hashtable Parameters { get; set; }

        /// <summary>
        /// Gets or sets the property object.
        /// </summary>
        [Alias("ActionName")]
        [Parameter(Mandatory = true, HelpMessage = "The name of the action to invoke.")]
        [ValidateNotNullOrEmpty]
        public string Action { get; set; }

        /// <summary>
        /// Executes the cmdlet.
        /// </summary>
        protected override void OnProcessRecord()
        {
            base.OnProcessRecord();

            var resourceId = GetResourceId();

            ConfirmAction(
                Force,
                string.Format(ProjectResources.ConfirmInvokeAction, Action, resourceId),
                string.Format(ProjectResources.InvokingResourceAction, Action),
                resourceId,
                () =>
                {
                    var apiVersion = DetermineApiVersion(resourceId).Result;
                    var parameters = GetParameters();

                    var operationResult = GetResourcesClient()
                        .InvokeActionOnResource<JObject>(
                            resourceId,
                            Action,
                            apiVersion,
                            parameters: parameters,
                            cancellationToken: CancellationToken.Value,
                            odataQuery: ODataQuery)
                        .Result;

                    var managementUri = GetResourcesClient()
                        .GetResourceManagementRequestUri(
                            resourceId,
                            apiVersion,
                            Action,
                            ODataQuery);

                    var activity = string.Format("POST {0}", managementUri.PathAndQuery);
                    var resultString = GetLongRunningOperationTracker(activity, false)
                        .WaitOnOperation(operationResult);

                    TryConvertAndWriteObject(resultString);
                });
        }

        /// <summary>
        /// Gets the resource body from the parameters.
        /// </summary>
        private JToken GetParameters()
        {
            return Parameters.ToDictionary(false).ToJToken();
        }
    }
}
