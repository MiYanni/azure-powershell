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

using System.Net;
using System.Management.Automation;
using Microsoft.Azure.Commands.Insights.OutputClasses;
using Microsoft.Azure.Commands.ResourceManager.Common.ArgumentCompleters;

namespace Microsoft.Azure.Commands.Insights.ActivityLogAlert
{
    /// <summary>
    /// Remove an activity log alert.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "AzureRmActivityLogAlert", SupportsShouldProcess = true), OutputType(typeof(AzureOperationResponse))]
    public class RemoveAzureRmActivityLogAlertCommand : ManagementCmdletBase
    {
        internal const string RemoveActivityLogAlertDefaultParamGroup = "RemoveByNameAndResourceGroup";
        internal const string RemoveActivityLogAlertFromPipeParamGroup = "RemoveByInputObject";
        internal const string RemoveActivityLogAlertFromResourceIdParamGroup = "RemoveByResourceId";

        #region Parameter declaration

        /// <summary>
        /// Gets or sets the ResourceGroupName parameter of the cmdlet
        /// </summary>
        [Parameter(ParameterSetName = RemoveActivityLogAlertDefaultParamGroup, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The resource group name")]
        [ResourceGroupCompleter]
        [ValidateNotNullOrEmpty]
        public string ResourceGroupName { get; set; }

        /// <summary>
        /// Gets or sets the rule name parameter of the cmdlet
        /// </summary>
        [Parameter(ParameterSetName = RemoveActivityLogAlertDefaultParamGroup, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The activity log alert name")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the InputObject parameter of the cmdlet
        /// </summary>
        [Parameter(ParameterSetName = RemoveActivityLogAlertFromPipeParamGroup, Mandatory = true, ValueFromPipeline = true, HelpMessage = "The activity log alert resource from the pipe")]
        [ValidateNotNull]
        public PSActivityLogAlertResource InputObject { get; set; }

        /// <summary>
        /// Gets or sets the ResourceId parameter of the cmdlet
        /// </summary>
        [Parameter(ParameterSetName = RemoveActivityLogAlertFromResourceIdParamGroup, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The resource Id from the pipe by property name")]
        [ValidateNotNullOrEmpty]
        public string ResourceId { get; set; }

        #endregion

        /// <summary>
        /// Execute the cmdlet
        /// </summary>
        protected override void ProcessRecordInternal()
        {
            if (ShouldProcess(
                    string.Format("Delete activity logs alert: {0} from resource group: {1}", Name, ResourceGroupName),
                    "Delete activity logs alert"))
            {
                string resourceGroupName = ResourceGroupName;
                string activityLogAlertName = Name;

                // Using value from the pipe
                if (MyInvocation.BoundParameters.ContainsKey("InputObject") || InputObject != null)
                {
                    ActivityLogAlertUtilities.ProcessPipeObject(
                        InputObject,
                        out resourceGroupName,
                        out activityLogAlertName);
                }
                else if (MyInvocation.BoundParameters.ContainsKey("ResourceId") || !string.IsNullOrWhiteSpace(ResourceId))
                {
                    ActivityLogAlertUtilities.ProcessPipeObject(
                        ResourceId,
                        out resourceGroupName,
                        out activityLogAlertName);
                }

                var result = MonitorManagementClient.ActivityLogAlerts.DeleteWithHttpMessagesAsync(resourceGroupName, activityLogAlertName).Result;
                var response = new AzureOperationResponse
                {
                    RequestId = result.RequestId,
                    StatusCode = result.Response != null ? result.Response.StatusCode : HttpStatusCode.OK
                };

                WriteObject(response);
            }
        }
    }
}
