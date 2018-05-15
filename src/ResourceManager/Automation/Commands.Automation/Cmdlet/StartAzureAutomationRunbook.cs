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

using Microsoft.Azure.Commands.Automation.Common;
using Microsoft.Azure.Commands.Automation.Properties;
using System;
using System.Collections;
using System.Globalization;
using System.Management.Automation;
using System.Security.Permissions;
using System.Threading;
using Job = Microsoft.Azure.Commands.Automation.Model.Job;


namespace Microsoft.Azure.Commands.Automation.Cmdlet
{
    /// <summary>
    /// Starts an Azure automation runbook.
    /// </summary>
    [Cmdlet(VerbsLifecycle.Start, "AzureRmAutomationRunbook", DefaultParameterSetName = AutomationCmdletParameterSets.ByAsynchronousReturnJob)]
    [OutputType(typeof(Job), ParameterSetName = new[] { AutomationCmdletParameterSets.ByAsynchronousReturnJob })]
    [OutputType(typeof(PSObject), ParameterSetName = new[] { AutomationCmdletParameterSets.BySynchronousReturnJobOutput })]
    public class StartAzureAutomationRunbook : AzureAutomationBaseCmdlet
    {
        /// <summary>
        /// True to wait for job output
        /// </summary>        
        private bool wait;

        /// <summary>
        /// Timeout value
        /// </summary>        
        private int timeout = Constants.MaxWaitSeconds;

        /// <summary>
        /// Polling interval
        /// </summary>        
        private const int pollingIntervalInSeconds = 5;

        /// <summary>
        /// Gets or sets the runbook name
        /// </summary>
        [Parameter(ParameterSetName = AutomationCmdletParameterSets.ByAsynchronousReturnJob, Position = 2, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The runbook name.")]
        [Parameter(ParameterSetName = AutomationCmdletParameterSets.BySynchronousReturnJobOutput, Position = 2, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The runbook name.")]
        [ValidateNotNullOrEmpty]
        [Alias("RunbookName")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the runbook parameters.
        /// </summary>
        [Parameter(ParameterSetName = AutomationCmdletParameterSets.ByAsynchronousReturnJob, Mandatory = false, HelpMessage = "The runbook parameters.")]
        [Parameter(ParameterSetName = AutomationCmdletParameterSets.BySynchronousReturnJobOutput, Mandatory = false, HelpMessage = "The runbook parameters.")]
        public IDictionary Parameters { get; set; }

        /// <summary>
        /// Gets or sets the optional hybrid agent friendly name upon which the runbook should be executed.
        /// </summary>
        [Parameter(ParameterSetName = AutomationCmdletParameterSets.ByAsynchronousReturnJob, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Optional name of the hybrid agent which should execute the runbook")]
        [Parameter(ParameterSetName = AutomationCmdletParameterSets.BySynchronousReturnJobOutput, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Optional name of the hybrid agent which should execute the runbook")]
        [Alias("HybridWorker")]
        public string RunOn { get; set; }


        /// <summary>
        /// Gets or sets the switch parameter to wait for job output
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = AutomationCmdletParameterSets.BySynchronousReturnJobOutput, HelpMessage = "Wait for job to complete, suspend, or fail.")]
        public SwitchParameter Wait
        {
            get { return wait; }
            set { wait = value; }
        }

        /// <summary>
        /// Gets or sets the switch parameter to wait for job output
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = AutomationCmdletParameterSets.BySynchronousReturnJobOutput, HelpMessage = "Maximum time in seconds to wait for job completion. Default max wait time is 10800 seconds.")]
        public int MaxWaitSeconds
        {
            get { return timeout; }
            set { timeout = value; }
        }

        /// <summary>
        /// Execute this cmdlet.
        /// </summary>
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        protected override void AutomationProcessRecord()
        {
            var job = AutomationClient.StartRunbook(ResourceGroupName, AutomationAccountName, Name, Parameters, RunOn);

            // if no wait, return job object
            if (!Wait.IsPresent)
            {
                WriteObject(job);
                return;
            }

            // wait for job completion
            if (!PollJobCompletion(job.JobId))
            {
                throw new ResourceCommonException(typeof(Job), string.Format(CultureInfo.CurrentCulture, Resources.JobCompletionMaxWaitReached));
            }

            // retrieve job streams
            var nextLink = string.Empty;
            do
            {
                var jobOutputList = AutomationClient.GetJobStream(ResourceGroupName, AutomationAccountName, job.JobId, null, StreamType.Output.ToString(), ref nextLink);
                foreach (var jobOutputRecord in jobOutputList)
                {
                    var response = AutomationClient.GetJobStreamRecordAsPsObject(ResourceGroupName, AutomationAccountName, job.JobId, jobOutputRecord.StreamRecordId);
                    GenerateCmdletOutput(response);
                }
            } while (!string.IsNullOrEmpty(nextLink));
        }

        private bool PollJobCompletion(Guid jobId)
        {
            var timeoutIncrement = 0;
            do
            {
                Thread.Sleep(TimeSpan.FromSeconds(pollingIntervalInSeconds));
                timeoutIncrement += pollingIntervalInSeconds;

                var job = AutomationClient.GetJob(ResourceGroupName, AutomationAccountName, jobId);
                if (!IsJobTerminalState(job))
                {
                    if (IsVerbose()) WriteVerbose(string.Format(Resources.JobProgressState, job.JobId, job.Status, DateTime.Now));
                }
                else
                {
                    if (IsVerbose()) WriteVerbose(string.Format(Resources.JobTerminalState, job.JobId, job.Status, DateTime.Now));
                    return true;
                }
            } while (MaxWaitSeconds < 0 || timeoutIncrement <= MaxWaitSeconds);

            return false;
        }

        private static bool IsJobTerminalState(Job job)
        {
            return job.Status.Equals(JobState.Completed.ToString(), StringComparison.OrdinalIgnoreCase) ||
                   job.Status.Equals(JobState.Failed.ToString(), StringComparison.OrdinalIgnoreCase) ||
                   job.Status.Equals(JobState.Stopped.ToString(), StringComparison.OrdinalIgnoreCase) ||
                   job.Status.Equals(JobState.Suspended.ToString(), StringComparison.OrdinalIgnoreCase);
        }
    }
}
