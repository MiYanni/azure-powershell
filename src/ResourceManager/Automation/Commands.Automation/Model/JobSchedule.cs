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
using System;
using System.Collections;
using System.Globalization;
using System.Linq;

namespace Microsoft.Azure.Commands.Automation.Model
{
    /// <summary>
    /// The Job Schedule.
    /// </summary>
    public class JobSchedule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JobSchedule"/> class.
        /// </summary>
        /// <param name="resourceGroupName">
        /// The resource group name.
        /// </param>
        /// <param name="automationAccountName">
        /// The account name.
        /// </param>
        /// <param name="jobSchedule">
        /// The job schedule.
        /// </param>
        public JobSchedule(string resourceGroupName, string automationAccountName, Management.Automation.Models.JobSchedule jobSchedule)
        {
            Requires.Argument("jobSchedule", jobSchedule).NotNull();
            ResourceGroupName = resourceGroupName;
            AutomationAccountName = automationAccountName;
            JobScheduleId = jobSchedule.Properties.Id.ToString();
            RunbookName = jobSchedule.Properties.Runbook.Name;
            ScheduleName = jobSchedule.Properties.Schedule.Name;
            Parameters = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
            HybridWorker = jobSchedule.Properties.RunOn;
            foreach (var kvp in jobSchedule.Properties.Parameters.Where(kvp => 0 != String.Compare(kvp.Key, Constants.JobStartedByParameterName, CultureInfo.InvariantCulture,
                CompareOptions.IgnoreCase)))
            {
                Parameters.Add(kvp.Key, PowerShellJsonConverter.Deserialize(kvp.Value));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HourlySchedule"/> class.
        /// </summary>
        public JobSchedule()
        {
        }

        /// <summary>
        /// Gets or sets the resource group name.
        /// </summary>
        public string ResourceGroupName { get; set; }

        public string RunOn { get; set; }

        /// <summary>
        /// Gets or sets the automation account name.
        /// </summary>
        public string AutomationAccountName { get; set; }

        /// <summary>
        /// Gets or sets the job schedule id.
        /// </summary>
        public string JobScheduleId { get; set; }

        /// <summary>
        /// Gets or sets the runbook name.
        /// </summary>
        public string RunbookName { get; set; }

        /// <summary>
        /// Gets or sets the schedule name.
        /// </summary>
        public string ScheduleName { get; set; }

        /// <summary>
        /// Gets or sets the runbook parameters.
        /// </summary>
        public Hashtable Parameters { get; set; }

        /// <summary>
        /// Gets or sets the HybridWorker.
        /// </summary>
        public string HybridWorker { get; set; }
    }
}
