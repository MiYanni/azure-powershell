using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Security.Permissions;
using Microsoft.Azure.Commands.Automation.Model;
using Microsoft.Azure.Commands.Automation.Common;


namespace Microsoft.Azure.Commands.Automation.Cmdlet
{
    [Cmdlet(VerbsCommon.Get, "AzureRMAutomationHybridWorkerGroup", DefaultParameterSetName = AutomationCmdletParameterSets.ByAll)]
    [OutputType(typeof(HybridRunbookWorkerGroup))]
    public class GetAzureAutomationHybridWorkerGroup : AzureAutomationBaseCmdlet
    {
        [Parameter(ParameterSetName = AutomationCmdletParameterSets.ByName,Position = 2,  Mandatory = false, ValueFromPipeline = true, HelpMessage = "The Hybrid Runbook Worker Group name")]
        [Alias("Group")]
        public string Name { get; set; }

        protected override void AutomationProcessRecord()
        {
            if (ParameterSetName == AutomationCmdletParameterSets.ByName)
            {
                IEnumerable<HybridRunbookWorkerGroup> ret = null;
                ret = new List<HybridRunbookWorkerGroup> {

                    AutomationClient.GetHybridRunbookWorkerGroup(ResourceGroupName, AutomationAccountName, Name)
                };
                GenerateCmdletOutput(ret);
            }
            else if(ParameterSetName == AutomationCmdletParameterSets.ByAll)
            {
                var nextLink = string.Empty;
                do
                {
                    var results = AutomationClient.ListHybridRunbookWorkerGroups(ResourceGroupName, AutomationAccountName, ref nextLink);
                    GenerateCmdletOutput(results);
                }while (!string.IsNullOrEmpty(nextLink));
            }
        }
    }
}
