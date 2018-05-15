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

using Microsoft.Azure.Commands.Insights.OutputClasses;
using Microsoft.Azure.Commands.ResourceManager.Common.ArgumentCompleters;
using Microsoft.Azure.Management.Monitor.Management;
using Microsoft.Azure.Management.Monitor.Management.Models;
using System.Collections.Generic;
using System.Management.Automation;

namespace Microsoft.Azure.Commands.Insights.ActivityLogAlert
{
    /// <summary>
    /// Enable an activity log alert
    /// </summary>
    [Cmdlet("Enable", "AzureRmActivityLogAlert", SupportsShouldProcess = true), OutputType(typeof(PSActivityLogAlertResource))]
    public class EnableAzureRmActivityLogAlertCommand : ManagementCmdletBase
    {
        internal const string EnableActivityLogAlertDefaultParamGroup = "EnableByNameAndResourceGroup";
        internal const string EnableActivityLogAlertFromPipeParamGroup = "EnableByInputObject";
        internal const string EnableActivityLogAlertFromResourceIdParamGroup = "EnableByResourceId";

        #region Cmdlet parameters

        /// <summary>
        /// Gets or sets the alert name parameter of the cmdlet
        /// </summary>
        [Parameter(ParameterSetName = EnableActivityLogAlertDefaultParamGroup, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The activity log rule name")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        /// <summary>
        /// Gets or set the resource group name
        /// </summary>
        [Parameter(ParameterSetName = EnableActivityLogAlertDefaultParamGroup, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The resource group name of the activity log rule resource")]
        [ResourceGroupCompleter]
        [ValidateNotNullOrEmpty]
        public string ResourceGroupName { get; set; }

        /// <summary>
        /// Gets or sets the InputObject parameter of the cmdlet
        /// </summary>
        [Parameter(ParameterSetName = EnableActivityLogAlertFromPipeParamGroup, Mandatory = true, ValueFromPipeline = true, HelpMessage = "The activity log alert resource from the pipe")]
        [ValidateNotNull]
        public PSActivityLogAlertResource InputObject { get; set; }

        /// <summary>
        /// Gets or sets the ResourceId parameter of the cmdlet
        /// </summary>
        [Parameter(ParameterSetName = EnableActivityLogAlertFromResourceIdParamGroup, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The resource Id from the pipe by property name")]
        [ValidateNotNullOrEmpty]
        public string ResourceId { get; set; }

        #endregion

        /// <summary>
        /// Execute the cmdlet
        /// </summary>
        protected override void ProcessRecordInternal()
        {
            if (ShouldProcess(
                    string.Format("Patch activity logs alert: {0} from resource group: {1}", Name, ResourceGroupName),
                    "Patch activity logs alert"))
            {
                string resourceGroupName = ResourceGroupName;
                string activityLogAlertName = Name;
                IDictionary<string, string> tags = null;

                // Using value from the pipe
                if (MyInvocation.BoundParameters.ContainsKey("InputObject") || InputObject != null)
                {
                    ActivityLogAlertUtilities.ProcessPipeObject(
                        InputObject,
                        out resourceGroupName,
                        out activityLogAlertName);

                    tags = InputObject.Tags;
                }
                else if (MyInvocation.BoundParameters.ContainsKey("ResourceId") || !string.IsNullOrWhiteSpace(ResourceId))
                {
                    ActivityLogAlertUtilities.ProcessPipeObject(
                        ResourceId,
                        out resourceGroupName,
                        out activityLogAlertName);
                }

                WriteObject(
                    MonitorManagementClient.ActivityLogAlerts.Update(
                        resourceGroupName,
                        activityLogAlertName,
                        CreateActivityLogAlertPatchBody(true, tags)));
            }
        }

        private ActivityLogAlertPatchBody CreateActivityLogAlertPatchBody(bool enableAlert, IDictionary<string, string> tags)
        {
            return new ActivityLogAlertPatchBody(
                tags,
                enableAlert);
        }
    }
}
