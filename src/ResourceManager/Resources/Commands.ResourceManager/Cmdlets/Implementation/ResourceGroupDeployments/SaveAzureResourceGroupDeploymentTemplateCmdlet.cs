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
    using Common.ArgumentCompleters;
    using Components;
    using Utilities;
    using WindowsAzure.Commands.Utilities.Common;
    using Newtonsoft.Json.Linq;
    using System.Management.Automation;

    /// <summary>
    /// Saves the deployment template to a file on disk.
    /// </summary>
    [Cmdlet(VerbsData.Save, "AzureRmResourceGroupDeploymentTemplate", SupportsShouldProcess = true), 
        OutputType(typeof(PSObject))]
    public class SaveAzureResourceGroupDeploymentTemplateCmdlet : ResourceManagerCmdletBase
    {
        /// <summary>
        /// Gets or sets the resource group name parameter.
        /// </summary>
        [Alias("ResourceGroup")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The resource group name.")]
        [ResourceGroupCompleter]
        [ValidateNotNullOrEmpty]
        public string ResourceGroupName { get; set; }

        /// <summary>
        /// Gets or sets the deployment name parameter.
        /// </summary>
        [Alias("Name")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The deployment name.")]
        [ValidateNotNullOrEmpty]
        public string DeploymentName { get; set; }

        /// <summary>
        /// Gets or sets the file path.
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The output path of the template file.")]
        [ValidateNotNullOrEmpty]
        public string Path { get; set; }

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
            if (ShouldProcess(DeploymentName, VerbsData.Save))
            {
                var resourceId = GetResourceId();

                var apiVersion = DetermineApiVersion(resourceId).Result;

                var operationResult = GetResourcesClient()
                    .InvokeActionOnResource<JObject>(
                        resourceId,
                        Constants.ExportTemplate,
                        apiVersion,
                        CancellationToken.Value)
                    .Result;

                var managementUri = GetResourcesClient()
                    .GetResourceManagementRequestUri(
                        resourceId,
                        apiVersion,
                        Constants.ExportTemplate);

                var activity = string.Format("POST {0}", managementUri.PathAndQuery);
                var resultString = GetLongRunningOperationTracker(activity,
                    false)
                    .WaitOnOperation(operationResult);

                var template = JToken.FromObject(JObject.Parse(resultString)["template"]);

                string path = FileUtility.SaveTemplateFile(
                    DeploymentName,
                    template.ToString(),
                    string.IsNullOrEmpty(Path)
                            ? System.IO.Path.Combine(CurrentPath(), DeploymentName)
                            : this.TryResolvePath(Path),
                    Force,
                    ShouldContinue);

                WriteObject(PowerShellUtilities.ConstructPSObject(null, "Path", path));
            }
        }

        /// <summary>
        /// Gets the resource Id from the supplied PowerShell parameters.
        /// </summary>
        protected string GetResourceId()
        {
            return ResourceIdUtility.GetResourceId(
                DefaultContext.Subscription.GetId(),
                ResourceGroupName,
                Constants.MicrosoftResourcesDeploymentType,
                DeploymentName);
        }
    }
}
