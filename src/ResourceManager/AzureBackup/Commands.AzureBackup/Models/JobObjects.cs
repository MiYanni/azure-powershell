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

using Microsoft.Azure.Commands.AzureBackup.Cmdlets;
using System;
using System.Collections.Generic;
using Mgmt = Microsoft.Azure.Management.BackupServices.Models;

namespace Microsoft.Azure.Commands.AzureBackup.Models
{
    public class AzureRMBackupJob : AzureBackupVaultContextObject
    {
        public string InstanceId { get; private set; }

        public string WorkloadType { get; set; }

        public string Operation { get; set; }

        public string Status { get; set; }

        public string WorkloadName { get; set; }

        public TimeSpan Duration { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public bool IsCancellable { get; private set; }

        public bool IsRetriable { get; private set; }

        public List<ErrorInfo> ErrorDetails { get; set; }

        public AzureRMBackupJob(AzureRMBackupVault vault, Mgmt.CSMJobProperties serviceJob, string jobName)
            : base(vault)
        {
            InstanceId = jobName;
            WorkloadType = AzureBackupJobHelper.GetTypeForPS(serviceJob.WorkloadType);
            WorkloadName = serviceJob.EntityFriendlyName;
            Operation = serviceJob.Operation;
            Status = serviceJob.Status;
            Duration = serviceJob.Duration;
            StartTime = serviceJob.StartTimestamp;
            EndTime = serviceJob.EndTimestamp;
            ErrorDetails = new List<ErrorInfo>();

            if (serviceJob.ErrorDetails != null)
            {
                foreach (Mgmt.CSMJobErrorInfo error in serviceJob.ErrorDetails)
                {
                    ErrorDetails.Add(new ErrorInfo(error));
                }
            }

            IsRetriable = IsCancellable = false;

            if (serviceJob.ActionsInfo != null)
            {
                for (int i = 0; i < serviceJob.ActionsInfo.Count; i++)
                {
                    if (serviceJob.ActionsInfo[i] == Mgmt.JobSupportedAction.Cancellable)
                        IsCancellable = true;
                    else if (serviceJob.ActionsInfo[i] == Mgmt.JobSupportedAction.Retriable)
                        IsRetriable = true;
                }
            }
        }
    }

    public class ErrorInfo
    {
        public int ErrorCode { get; set; }

        public string ErrorMessage { get; set; }

        // Not including ErrorTitle because we are not filling anything in it.

        public List<string> Recommendations { get; set; }

        public ErrorInfo(Mgmt.CSMJobErrorInfo serviceErrorInfo)
        {
            ErrorCode = serviceErrorInfo.ErrorCode;
            ErrorMessage = serviceErrorInfo.ErrorString;
            Recommendations = new List<string>();
            foreach (string recommendation in serviceErrorInfo.Recommendations)
            {
                Recommendations.Add(recommendation);
            }
        }
    }

    public class AzureRMBackupJobDetails : AzureRMBackupJob
    {
        public Dictionary<string, string> Properties { get; set; }

        public List<AzureBackupJobSubTask> SubTasks { get; set; }

        public AzureRMBackupJobDetails(AzureRMBackupVault vault, Mgmt.CSMJobDetailedProperties serviceJobProperties, string jobName)
            : base(vault, serviceJobProperties, jobName)
        {
            if (serviceJobProperties.PropertyBag != null)
                Properties = new Dictionary<string, string>(serviceJobProperties.PropertyBag);
            else
                Properties = new Dictionary<string, string>();

            SubTasks = new List<AzureBackupJobSubTask>();
            if (serviceJobProperties.TasksList != null)
            {
                foreach (Mgmt.CSMJobTaskDetails serviceSubTask in serviceJobProperties.TasksList)
                {
                    SubTasks.Add(new AzureBackupJobSubTask(serviceSubTask));
                }
            }
        }
    }

    public class AzureBackupJobSubTask
    {
        public string Name { get; set; }

        public string Status { get; set; }

        // Not adding other fields because service is not filling them today.

        public AzureBackupJobSubTask(Mgmt.CSMJobTaskDetails serviceTask)
        {
            Name = serviceTask.TaskId;
            Status = serviceTask.Status;
        }
    }
}