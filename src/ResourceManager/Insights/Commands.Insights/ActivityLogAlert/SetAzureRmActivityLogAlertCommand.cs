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
    /// Create or update an Activity Log Alert
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "AzureRmActivityLogAlert", SupportsShouldProcess = true), OutputType(typeof(PSActivityLogAlertResource))]
    public class SetAzureRmActivityLogAlertCommand : ManagementCmdletBase
    {
        internal const string SetActivityLogAlertDefaultParamGroup = "SetByNameAndResourceGroup";
        internal const string SetActivityLogAlertFromPipeParamGroup = "SetByInputObject";
        internal const string SetActivityLogAlertFromResourceIdParamGroup = "SetByResourceId";

        #region Cmdlet parameters

        /// <summary>
        /// Gets or sets the Location parameter of the cmdlet
        /// </summary>
        [Parameter(ParameterSetName = SetActivityLogAlertDefaultParamGroup, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The location of the activity log rule resource")]
        [Parameter(ParameterSetName = SetActivityLogAlertFromResourceIdParamGroup, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The location of the activity log rule resource")]
        [LocationCompleter("Microsoft.Insights/activityLogAlerts")]
        [ValidateNotNullOrEmpty]
        public string Location { get; set; }

        /// <summary>
        /// Gets or sets the alert name parameter of the cmdlet
        /// </summary>
        [Parameter(ParameterSetName = SetActivityLogAlertDefaultParamGroup, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The activity log rule name")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        /// <summary>
        /// Gets or set the resource group name
        /// </summary>
        [Parameter(ParameterSetName = SetActivityLogAlertDefaultParamGroup, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The resource group name of the activity log rule resource")]
        [ResourceGroupCompleter]
        [ValidateNotNullOrEmpty]
        public string ResourceGroupName { get; set; }

        /// <summary>
        /// Gets or sets the scopes of the activity log alert
        /// </summary>
        [Parameter(ParameterSetName = SetActivityLogAlertDefaultParamGroup, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The list scopes")]
        [Parameter(ParameterSetName = SetActivityLogAlertFromPipeParamGroup, Mandatory = false, HelpMessage = "The list scopes")]
        [Parameter(ParameterSetName = SetActivityLogAlertFromResourceIdParamGroup, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The list scopes")]
        [ValidateNotNullOrEmpty]
        public List<string> Scope { get; set; }

        /// <summary>
        /// Gets or sets the conditions of the activity log alert
        /// </summary>
        [Parameter(ParameterSetName = SetActivityLogAlertDefaultParamGroup, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The list of leaf conditions of the activity log alert")]
        [Parameter(ParameterSetName = SetActivityLogAlertFromPipeParamGroup, Mandatory = false, HelpMessage = "The list of leaf conditions of the activity log alert")]
        [Parameter(ParameterSetName = SetActivityLogAlertFromResourceIdParamGroup, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The list of leaf conditions of the activity log alert")]
        [ValidateNotNullOrEmpty]
        public List<ActivityLogAlertLeafCondition> Condition { get; set; }

        /// <summary>
        /// Gets or sets the actions of the activity log alert
        /// </summary>
        [Parameter(ParameterSetName = SetActivityLogAlertDefaultParamGroup, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The list actions of the activity log alert")]
        [Parameter(ParameterSetName = SetActivityLogAlertFromPipeParamGroup, Mandatory = false, HelpMessage = "The list actions of the activity log alert")]
        [Parameter(ParameterSetName = SetActivityLogAlertFromResourceIdParamGroup, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The list actions of the activity log alert")]
        [ValidateNotNullOrEmpty]
        public List<ActivityLogAlertActionGroup> Action { get; set; }

        /// <summary>
        /// Gets or sets the DisableAlert flag.
        /// <para>If not given, the alert is created enabled, i.e. the default value.</para>
        /// </summary>
        [Parameter(ParameterSetName = SetActivityLogAlertDefaultParamGroup, Mandatory = false, HelpMessage = "The disable alert flag. Defaults to false, alerts are created enabled by default.")]
        [Parameter(ParameterSetName = SetActivityLogAlertFromResourceIdParamGroup, Mandatory = false, HelpMessage = "The disable alert flag. Defaults to false, alerts are created enabled by default.")]
        public SwitchParameter DisableAlert { get; set; }

        /// <summary>
        /// Gets or set the alert description
        /// </summary>
        [Parameter(ParameterSetName = SetActivityLogAlertDefaultParamGroup, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The description of the activity log rule resource")]
        [Parameter(ParameterSetName = SetActivityLogAlertFromPipeParamGroup, Mandatory = false, HelpMessage = "The description of the activity log rule resource")]
        [Parameter(ParameterSetName = SetActivityLogAlertFromResourceIdParamGroup, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The description of the activity log rule resource")]
        [ValidateNotNullOrEmpty]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the Tags of the activity log alert resource
        /// </summary>
        [Parameter(ParameterSetName = SetActivityLogAlertDefaultParamGroup, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The tags of the activity log alert resource")]
        [Parameter(ParameterSetName = SetActivityLogAlertFromPipeParamGroup, Mandatory = false, HelpMessage = "The tags of the activity log alert resource")]
        [Parameter(ParameterSetName = SetActivityLogAlertFromResourceIdParamGroup, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The tags of the activity log alert resource")]
        [ValidateNotNullOrEmpty]
        public Dictionary<string, string> Tag { get; set; }

        /// <summary>
        /// Gets or sets the InputObject parameter of the cmdlet
        /// </summary>
        [Parameter(ParameterSetName = SetActivityLogAlertFromPipeParamGroup, Mandatory = true, ValueFromPipeline = true, HelpMessage = "The activity log alert resource from the pipe")]
        [ValidateNotNull]
        public PSActivityLogAlertResource InputObject { get; set; }

        /// <summary>
        /// Gets or sets the ResourceId parameter of the cmdlet
        /// </summary>
        [Parameter(ParameterSetName = SetActivityLogAlertFromResourceIdParamGroup, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The resource Id from the pipe by property name")]
        [ValidateNotNullOrEmpty]
        public string ResourceId { get; set; }

        #endregion

        /// <summary>
        /// Execute the cmdlet
        /// </summary>
        protected override void ProcessRecordInternal()
        {
            if (ShouldProcess(
                    string.Format("Create/update an activity logs alert: {0} from resource group: {1}", Name, ResourceGroupName),
                    "Create/update an activity logs alert"))
            {
                string resourceGroupName = ResourceGroupName;
                string activityLogAlertName = Name;
                ActivityLogAlertResource requestBody = null;

                // Using value from the pipe
                if (MyInvocation.BoundParameters.ContainsKey("InputObject") || InputObject != null)
                {
                    WriteVerboseWithTimestamp("InputObject detected: creating request body based on it.");

                    ActivityLogAlertUtilities.ProcessPipeObject(
                        InputObject,
                        out resourceGroupName,
                        out activityLogAlertName);

                    requestBody = UpdateActivityLogAlertResource(InputObject);
                }
                else if (MyInvocation.BoundParameters.ContainsKey("ResourceId") || !string.IsNullOrWhiteSpace(ResourceId))
                {
                    WriteVerboseWithTimestamp("ResourceId detected: extracting name and resource group name based on it.");

                    // ResourceId is not enough to set an ActivityLogAlert
                    // First there is the need to try and find an existing alert and modify it
                    ActivityLogAlertUtilities.ProcessPipeObject(
                        ResourceId,
                        out resourceGroupName,
                        out activityLogAlertName);

                    WriteVerboseWithTimestamp("ResourceId detected: checking for the existence the given activity log alert.");

                    requestBody = MonitorManagementClient.ActivityLogAlerts.Get(resourceGroupName, activityLogAlertName);
                    if (requestBody == null)
                    {
                        WriteVerboseWithTimestamp("ResourceId detected: given activity log alert does not exist.");

                        // This can only happen if the user sent a resourceId of an alert that does not exist yet
                        if (string.IsNullOrWhiteSpace(Location))
                        {
                            // The user wants to create an activity log alert given ResourceId as argument, but the Location parameter was empty or null
                            throw new PSArgumentException("With ResourceId parameter used to create a new ActivityLogAlert, Location must contain a value", "Location");
                        }

                        requestBody = CreateActivityLogAlertResource(
                            activityLogAlertName,
                            Location);
                    }
                    else
                    {
                        WriteVerboseWithTimestamp("ResourceId detected: given activity log alert found, modifying its values with the parameters.");

                        requestBody = UpdateActivityLogAlertResource(requestBody);
                    }
                }
                else
                {
                    WriteVerboseWithTimestamp("No InputObject or ResourceId detected: following standard creation/update process.");

                    requestBody = CreateActivityLogAlertResource(
                        activityLogAlertName,
                        Location);
                }

                WriteObject(
                    MonitorManagementClient.ActivityLogAlerts.CreateOrUpdate(
                        resourceGroupName,
                        activityLogAlertName,
                        requestBody));
            }
        }

        private ActivityLogAlertResource UpdateActivityLogAlertResource(ActivityLogAlertResource requestBody)
        {
            // There was an ActivityLogAlert already there, just modify what can be modifed
            // NOTE: Location remains unchanged, the value of the Location parameter is ignored
            if (MyInvocation.BoundParameters.ContainsKey("Scope") || Scope != null)
            {
                requestBody.Scopes = Scope;
            }

            if (MyInvocation.BoundParameters.ContainsKey("Condition") || Condition != null)
            {
                requestBody.Condition = new ActivityLogAlertAllOfCondition(Condition);
            }

            if (MyInvocation.BoundParameters.ContainsKey("Action") || Action != null)
            {
                requestBody.Actions = new ActivityLogAlertActionList(Action);
            }

            if (DisableAlert.IsPresent)
            {
                requestBody.Enabled = false;
            }

            if (MyInvocation.BoundParameters.ContainsKey("Description") || Description != null)
            {
                requestBody.Description = Description;
            }

            if (MyInvocation.BoundParameters.ContainsKey("Tag") || Tag != null)
            {
                requestBody.Tags = Tag;
            }

            return requestBody;
        }

        private ActivityLogAlertResource CreateActivityLogAlertResource(string name, string location)
        {
            ActivityLogAlertResource newAlert = new ActivityLogAlertResource(
                name: name,
                location: location,
                scopes: Scope,
                condition: new ActivityLogAlertAllOfCondition(Condition),
                actions: new ActivityLogAlertActionList(Action));

            // EnableAlert defaults to true
            newAlert.Enabled = !DisableAlert.IsPresent;
            newAlert.Description = Description;
            newAlert.Tags = Tag;

            return newAlert;
        }
    }
}
