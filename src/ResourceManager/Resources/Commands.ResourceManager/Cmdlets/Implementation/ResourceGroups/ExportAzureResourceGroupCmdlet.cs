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
    using Entities.ErrorResponses;
    using Entities.ResourceGroups;
    using Extensions;
    using Utilities;
    using WindowsAzure.Commands.Utilities.Common;
    using Newtonsoft.Json.Linq;
    using System.Management.Automation;

    /// <summary>
    /// Captures the specifies resource group as a template and saves it to a file on disk.
    /// </summary>
    [Cmdlet(VerbsData.Export, "AzureRmResourceGroup", SupportsShouldProcess = true), 
        OutputType(typeof(PSObject))]
    public class ExportAzureResourceGroupCmdlet : ResourceManagerCmdletBase
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
        /// Gets or sets the file path.
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The output path of the template file.")]
        [ValidateNotNullOrEmpty]
        public string Path { get; set; }

        /// <summary>
        /// Export template parameter with default value.
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "Export template parameter with default value.")]
        public SwitchParameter IncludeParameterDefaultValue { get; set; }

        /// <summary>
        /// Export template with comments.
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "Export template with comments.")]
        public SwitchParameter IncludeComments { get; set; }

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
            if (ShouldProcess(ResourceGroupName, VerbsData.Export))
            {

                var resourceId = GetResourceId();

                var apiVersion = DetermineApiVersion(resourceId).Result;

                var parameters = new ExportTemplateParameters
                {
                    Resources = new[] {"*"},
                    Options = GetExportOptions() ?? null
                };

                var operationResult = GetResourcesClient()
                    .InvokeActionOnResource<JObject>(
                        resourceId,
                        Constants.ExportTemplate,
                        parameters: parameters.ToJToken(),
                        apiVersion: apiVersion,
                        cancellationToken: CancellationToken.Value)
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

                if (JObject.Parse(resultString)["error"] != null)
                {
                    ExtendedErrorInfo error;
                    if (JObject.Parse(resultString)["error"].TryConvertTo(out error))
                    {
                        WriteWarning(string.Format("{0} : {1}", error.Code, error.Message));
                        foreach (var detail in error.Details)
                        {
                            WriteWarning(string.Format("{0} : {1}", detail.Code, detail.Message));
                        }
                    }
                }

                string path = FileUtility.SaveTemplateFile(
                    ResourceGroupName,
                    template.ToString(),
                    string.IsNullOrEmpty(Path)
                            ? System.IO.Path.Combine(CurrentPath(), ResourceGroupName)
                            : this.TryResolvePath(Path),
                    Force.IsPresent,
                    ShouldContinue);

                WriteObject(PowerShellUtilities.ConstructPSObject(null, "Path", path));
            }
        }

        /// <summary>
        /// Gets the export template options
        /// </summary>
        private string GetExportOptions()
        {
            string options = string.Empty;
            if (IncludeComments.IsPresent)
            {
                options += "IncludeComments";
            }
            if (IncludeParameterDefaultValue.IsPresent)
            {
                options = string.IsNullOrEmpty(options) ? "IncludeParameterDefaultValue" : options + ",IncludeParameterDefaultValue";
            }
            return string.IsNullOrEmpty(options) ? null : options;
        }

        /// <summary>
        /// Gets the resource Id from the supplied PowerShell parameters.
        /// </summary>
        protected string GetResourceId()
        {
            return ResourceIdUtility.GetResourceId(
                DefaultContext.Subscription.GetId(),
                ResourceGroupName,
                null,
                null);
        }
    }
}
