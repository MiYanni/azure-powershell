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

namespace Microsoft.Azure.Commands.Scheduler.Utilities
{
    using System;
    using System.Collections.Generic;
    using Models;
    using Management.Scheduler.Models;

    public static class Converter
    {
        #region Convert Job Collection to PS
        /// <summary>
        /// Convert JobCollectionDefintion result to powershell PSJobCollectionDefinition.
        /// </summary>
        /// <param name="jobCollectionDefinition">Job collection definition.</param>
        /// <returns>PowerShell job collection definition.</returns>
        internal static IList<PSJobCollectionDefinition> ConvertJobCollectionDefinitionListToPSList(IList<JobCollectionDefinition> jobCollectionDefinitionList)
        {
            if (jobCollectionDefinitionList == null)
            {
                throw new ArgumentNullException("jobCollectionDefinition");
            }

            var psJobCollectionDefinitionList = new List<PSJobCollectionDefinition>();

            foreach (var jobCollectionDefinition in jobCollectionDefinitionList)
            {
                if (jobCollectionDefinition != null)
                {
                    psJobCollectionDefinitionList.Add(ConvertJobCollectionDefinitionToPS(jobCollectionDefinition));
                }
            }

            return psJobCollectionDefinitionList;
        }

        /// <summary>
        /// Convert collection of JobCollectionDefintion result to collection of powershell PSJobCollectionDefinition.
        /// </summary>
        /// <param name="jobCollectionDefinition">Collection of job collection definition.</param>
        /// <returns>Collection of PowerShell job collection definition.</returns>
        internal static PSJobCollectionDefinition ConvertJobCollectionDefinitionToPS(JobCollectionDefinition jobCollectionDefinition)
        {
            if (jobCollectionDefinition == null)
            {
                throw new ArgumentNullException("jobCollectionDefinition");
            }

            var psJobCollectionDefinition = new PSJobCollectionDefinition
            {
                ResourceGroupName = jobCollectionDefinition.Id.Split('/')[4],
                JobCollectionName = jobCollectionDefinition.Name,
                Location = jobCollectionDefinition.Location,
                Plan = jobCollectionDefinition.Properties.Sku.Name.ToString(),
                MaxJobCount = jobCollectionDefinition.Properties.Quota.MaxJobCount.ToString(),
                MaxRecurrence = GetMaxRecurrenceInString(jobCollectionDefinition.Properties.Quota),
                State = jobCollectionDefinition.Properties.State.ToString(),
                Uri = jobCollectionDefinition.Id,
            };

            return psJobCollectionDefinition;
        }

        /// <summary>
        /// Get maximum recurrence in string.
        /// </summary>
        /// <param name="jobCollectionQuota">The job collection quota.</param>
        /// <returns>Maximum recurrence in string.</returns>
        internal static string GetMaxRecurrenceInString(JobCollectionQuota jobCollectionQuota)
        {
            if (jobCollectionQuota != null &&
                jobCollectionQuota.MaxRecurrence != null)
            {
                return "Every " + jobCollectionQuota.MaxRecurrence.Interval.ToString() + " " +
                       jobCollectionQuota.MaxRecurrence.Frequency.ToString();
            }

            return null;
        }

        #endregion

        #region Convert Job to PS
        /// <summary>
        /// Converts job history list to PowerShell job history.
        /// </summary>
        /// <param name="jobsDefinitionList">Job history list.</param>
        /// <returns>List of PowerShell job history.</returns>
        internal static IList<PSJobHistory> ConvertJobHistoryListToPSList(IList<JobHistoryDefinition> jobHistoryList)
        {
            if (jobHistoryList == null)
            {
                throw new ArgumentNullException("jobHistoryList");
            }

            var psJobHistoryList = new List<PSJobHistory>();

            foreach (JobHistoryDefinition jobHistory in jobHistoryList)
            {
                if (jobHistory != null)
                {
                    psJobHistoryList.Add(ConvertJobHistoryDefinitionToPSJobHistory(jobHistory));
                }
            }

            return psJobHistoryList;
        }

        /// <summary>
        /// Convert job history definition to PowerShell job history.
        /// </summary>
        /// <param name="jobHistory">Job history definition.</param>
        /// <returns>Powershell job history.</returns>
        internal static PSJobHistory ConvertJobHistoryDefinitionToPSJobHistory(JobHistoryDefinition jobHistory)
        {
            if (jobHistory == null)
            {
                throw new ArgumentNullException("jobHistory");
            }

            var psJobHistory = new PSJobHistory
            {
                JobName = jobHistory.Name,
                Status = jobHistory.Properties.Status.ToString(),
                StartTime = jobHistory.Properties.StartTime,
                EndTime = jobHistory.Properties.EndTime,
                Occurence = jobHistory.Properties.RepeatCount,
                Retry = jobHistory.Properties.RetryCount
            };

            PSJobActionHistory psJobActionHistory = GetHistoryDetails(jobHistory.Properties.Message);
            psJobActionHistory.JobHistoryActionName = jobHistory.Properties.ActionName.ToString();

            psJobHistory.ActionHistory = psJobActionHistory;

            return psJobHistory;
        }

        /// <summary>
        /// Gets job history deatils.
        /// </summary>
        /// <param name="message">Job history message.</param>
        /// <returns>PSJobActionHistory.</returns>
        internal static PSJobActionHistory GetHistoryDetails(string message)
        {
            if (message == null)
            {
                return null;
            }

            if (message.Contains("Http Action"))
            {
                int beginIndexHostName = message.IndexOf(Constants.ApostropheSeparator) + 1;
                int endIndexHostName = message.IndexOf(Constants.ApostropheSeparator, beginIndexHostName + 1);

                var psHttpJobActionHistory = new PSHttpJobActionHistory
                {
                    JobActionType = JobActionType.Http.ToString(),
                    HostName = message.Substring(beginIndexHostName, endIndexHostName - beginIndexHostName)
                };

                if (message.Contains("Request to host") && message.Contains("failed:"))
                {
                    if (message.Contains("Request to host") && message.Contains("failed:"))
                    {
                        psHttpJobActionHistory.Response = "Failed";
                        psHttpJobActionHistory.ResponseBody = message;
                    }
                }
                else
                {
                    int beginIndexResponse = message.IndexOf(Constants.ApostropheSeparator, endIndexHostName + 1) + 1;
                    int endIndexResponse = message.IndexOf(Constants.ApostropheSeparator, beginIndexResponse + 1);
                    int bodyIndex = message.IndexOf("Body: ") + 6;

                    psHttpJobActionHistory.Response = message.Substring(beginIndexResponse, endIndexResponse - beginIndexResponse);
                    psHttpJobActionHistory.ResponseBody = message.Substring(bodyIndex);
                }

                return psHttpJobActionHistory;
            }
            if (message.Contains("StorageQueue Action"))
            {
                var psStorageJobActionHistory = new PSStorageJobActionHistory
                {
                    JobActionType = JobActionType.StorageQueue.ToString(),
                };

                if (message.Contains("does not exist"))
                {
                    int storageQueueNameBeginIndex = message.IndexOf(Constants.ApostropheSeparator) + 1;
                    int storageQueueNameEndIndex = message.IndexOf(Constants.ApostropheSeparator, storageQueueNameBeginIndex);

                    psStorageJobActionHistory.StorageAccountName = string.Empty;
                    psStorageJobActionHistory.StorageQueueName = message.Substring(storageQueueNameBeginIndex, storageQueueNameEndIndex - storageQueueNameBeginIndex);
                    psStorageJobActionHistory.Response = "Failed";
                    psStorageJobActionHistory.ResponseBody = message;
                }
                else if (message.Contains("could not be found"))
                {
                    int storageQueueAccountBeginIndex = message.IndexOf(Constants.ApostropheSeparator) + 1;
                    int storageQueueAccountEndIndex = message.IndexOf(Constants.ApostropheSeparator, storageQueueAccountBeginIndex);

                    psStorageJobActionHistory.StorageAccountName = string.Empty;
                    psStorageJobActionHistory.StorageQueueName = message.Substring(storageQueueAccountBeginIndex, storageQueueAccountEndIndex - storageQueueAccountBeginIndex);
                    psStorageJobActionHistory.Response = "Failed";
                    psStorageJobActionHistory.ResponseBody = message;
                }
                else if (message.Contains("succeeded"))
                {
                    int storageQueueAccountBeginIndex = message.IndexOf(Constants.ApostropheSeparator) + 1;
                    int storageQueueAccountEndIndex = message.IndexOf(Constants.ApostropheSeparator, storageQueueAccountBeginIndex);
                    int storageQueueNameBeginIndex = message.IndexOf(Constants.ApostropheSeparator, storageQueueAccountEndIndex + 1) + 1;
                    int storageQueueNameEndIndex = message.IndexOf(Constants.ApostropheSeparator, storageQueueNameBeginIndex);

                    psStorageJobActionHistory.StorageAccountName = message.Substring(storageQueueAccountBeginIndex, storageQueueAccountEndIndex - storageQueueAccountBeginIndex);
                    psStorageJobActionHistory.StorageQueueName = message.Substring(storageQueueNameBeginIndex, storageQueueNameEndIndex - storageQueueNameBeginIndex);
                    psStorageJobActionHistory.Response = message.Substring(storageQueueNameEndIndex + 2);
                    psStorageJobActionHistory.ResponseBody = message;
                }
                else
                {
                    psStorageJobActionHistory.ResponseBody = message;
                }

                return psStorageJobActionHistory;
            }
            if (message.Contains("ServiceBusQueue Action"))
            {
                int serviceBusQueueNameBeginIndex = message.IndexOf(Constants.ApostropheSeparator) + 1;
                int serviceBusQueueNameEndIndex = message.IndexOfAny(Constants.ApostropheSemicolonSeparator, serviceBusQueueNameBeginIndex);
                int serviceBusNamespaceBeginIndex = message.IndexOf(Constants.ApostropheSeparator, serviceBusQueueNameEndIndex + 1) + 1;
                int serviceBusNamespaceEndIndex = message.IndexOf(Constants.ApostropheSeparator, serviceBusNamespaceBeginIndex);

                var psServiceBusQueueHistory = new PSServiceBusJobActionHistory
                {
                    JobActionType = JobActionType.ServiceBusQueue.ToString(),
                    ServiceBusQueueName = message.Substring(serviceBusQueueNameBeginIndex, serviceBusQueueNameEndIndex - serviceBusQueueNameBeginIndex),
                    ServiceBusNamespace = message.Substring(serviceBusNamespaceBeginIndex, serviceBusNamespaceEndIndex - serviceBusNamespaceBeginIndex),
                    Response = message.Substring(0, message.IndexOf(Constants.ColonSeparator)),
                    ResponseBody = message
                };

                return psServiceBusQueueHistory;
            }
            if (message.Contains("ServiceBusTopic Action"))
            {
                int serviceBusTopicPathBeginIndex = message.IndexOf(Constants.ApostropheSeparator) + 1;
                int serviceBusTopicPathEndIndex = message.IndexOfAny(Constants.ApostropheSemicolonSeparator, serviceBusTopicPathBeginIndex);
                int serviceBusNamespaceBeginIndex = message.IndexOf(Constants.ApostropheSeparator, serviceBusTopicPathEndIndex + 1) + 1;
                int serviceBusNamespaceEndIndex = message.IndexOf(Constants.ApostropheSeparator, serviceBusNamespaceBeginIndex);

                var psServiceBusTopicHistory = new PSServiceBusJobActionHistory
                {
                    JobActionType = JobActionType.ServiceBusTopic.ToString(),
                    ServiceBusTopicPath = message.Substring(serviceBusTopicPathBeginIndex, serviceBusTopicPathEndIndex - serviceBusTopicPathBeginIndex),
                    ServiceBusNamespace = message.Substring(serviceBusNamespaceBeginIndex, serviceBusNamespaceEndIndex - serviceBusNamespaceBeginIndex),
                    Response = message.Substring(0, message.IndexOf(Constants.ColonSeparator)),
                    ResponseBody = message
                };

                return psServiceBusTopicHistory;
            }

            return null;
        }

        /// <summary>
        /// Converts job definition result to PowerShell job definition.
        /// </summary>
        /// <param name="jobsDefinitionList">List of jobs.</param>
        /// <returns>List of PowerShell job definition.</returns>
        internal static IList<PSSchedulerJobDefinition> ConvertJobDefinitionListToPSList(IList<JobDefinition> jobsDefinitionList)
        {
            if (jobsDefinitionList == null)
            {
                throw new ArgumentNullException("jobsDefinitionList");
            }

            var psJobDefinitionList = new List<PSSchedulerJobDefinition>();

            foreach (JobDefinition jobDefinition in jobsDefinitionList)
            {
                if (jobDefinition != null)
                {
                    psJobDefinitionList.Add(ConvertJobDefinitionToPS(jobDefinition));
                }
            }

            return psJobDefinitionList;
        }

        /// <summary>
        /// Converts job definition result to PowerShell job definition.
        /// </summary>
        /// <param name="jobsDefinitionList">Job definition.</param>
        /// <returns>PowerShell job definition.</returns>
        internal static PSSchedulerJobDefinition ConvertJobDefinitionToPS(JobDefinition jobDefinition)
        {
            if (jobDefinition == null)
            {
                throw new ArgumentNullException("jobDefinition");
            }

            var psSchedulerJobDefinition = new PSSchedulerJobDefinition
            {
                ResourceGroupName = jobDefinition.Id.Split('/')[4],
                JobCollectionName = jobDefinition.Name.Split('/')[0],
                JobName = jobDefinition.Name.Split('/')[1],
                Status = jobDefinition.Properties.State.ToString(),
                StartTime = jobDefinition.Properties.StartTime,
                Recurrence = ConvertRecurrenceToString(jobDefinition.Properties.Recurrence),
                EndSchedule = GetEndSchedule(jobDefinition.Properties.Recurrence),
            };

            if (jobDefinition.Properties.Status != null)
            {
                psSchedulerJobDefinition.Lastrun = jobDefinition.Properties.Status.LastExecutionTime;
                psSchedulerJobDefinition.Nextrun = jobDefinition.Properties.Status.NextExecutionTime;
                psSchedulerJobDefinition.Failures = jobDefinition.Properties.Status.FailureCount;
                psSchedulerJobDefinition.Faults = jobDefinition.Properties.Status.FaultedCount;
                psSchedulerJobDefinition.Executions = jobDefinition.Properties.Status.ExecutionCount;
            }

            psSchedulerJobDefinition.JobAction = GetSchedulerJobActionDetails(jobDefinition.Properties.Action);
            psSchedulerJobDefinition.JobErrorAction = GetSchedulerJobErrorActionDetails(jobDefinition.Properties.Action.ErrorAction);

            return psSchedulerJobDefinition;
        }

        /// <summary>
        /// Gets end time schedule.
        /// </summary>
        /// <param name="recurrence">Job recurrence values.</param>
        /// <returns>End time in string.</returns>
        internal static string GetEndSchedule(JobRecurrence recurrence)
        {
            if (recurrence == null)
            {
                return "Run once";
            }
            if (recurrence.Count == null && recurrence.EndTime == null)
            {
                return "None";
            }
            if (recurrence.Count != null && recurrence.EndTime != null)
            {
                return "Until " + recurrence.Count + " executions. Or on " + recurrence.EndTime + " whichever occurs first.";
            }
            if (recurrence.Count != null)
            {
                return "Until " + recurrence.Count + " executions.";
            }
            return "On " + recurrence.EndTime + ".";
        }

        /// <summary>
        /// Converts recurrence to string.
        /// </summary>
        /// <param name="recurrence"></param>
        /// <returns></returns>
        internal static string ConvertRecurrenceToString(JobRecurrence recurrence)
        {
            if (recurrence == null)
            {
                return null;
            }

            return "Every " + recurrence.Interval + " " + recurrence.Frequency.ToString() + "s";
        }

        /// <summary>
        /// Get scheduler job action details.
        /// </summary>
        /// <param name="jobAction">Job action.</param>
        /// <returns>PSJobActionDetail.</returns>
        internal static PSJobActionDetails GetSchedulerJobActionDetails(JobAction jobAction)
        {
            if (jobAction == null)
            {
                throw new ArgumentNullException("jobAction");
            }

            switch (jobAction.Type)
            {
                case JobActionType.Http:
                case JobActionType.Https:
                    return GetSchedulerHttpJobActionDetails(jobAction.Type.Value, jobAction.Request);

                case JobActionType.StorageQueue:
                    return GetSchedulerStorageJobActionDetails(jobAction.Type.Value, jobAction.QueueMessage);

                case JobActionType.ServiceBusQueue:
                    return GetSchedulerServiceBusQueueJobActionDetails(jobAction.Type.Value, jobAction.ServiceBusQueueMessage);

                case JobActionType.ServiceBusTopic:
                    return GetSchedulerServiceBusTopicJobActionDetails(jobAction.Type.Value, jobAction.ServiceBusTopicMessage);

                default:
                    return null;
            }
        }

        /// <summary>
        /// Gets error action details.
        /// </summary>
        /// <param name="erroJobAction">Job error action.</param>
        /// <returns>PSJobActionDetail.</returns>
        internal static PSJobActionDetails GetSchedulerJobErrorActionDetails(JobErrorAction erroJobAction)
        {
            if (erroJobAction == null)
            {
                return null;
            }

            switch (erroJobAction.Type)
            {
                case JobActionType.Http:
                case JobActionType.Https:
                    return GetSchedulerHttpJobActionDetails(erroJobAction.Type.Value, erroJobAction.Request);

                case JobActionType.StorageQueue:
                    return GetSchedulerStorageJobActionDetails(erroJobAction.Type.Value, erroJobAction.QueueMessage);

                case JobActionType.ServiceBusQueue:
                    return GetSchedulerServiceBusQueueJobActionDetails(erroJobAction.Type.Value, erroJobAction.ServiceBusQueueMessage);

                case JobActionType.ServiceBusTopic:
                    return GetSchedulerServiceBusTopicJobActionDetails(erroJobAction.Type.Value, erroJobAction.ServiceBusTopicMessage);

                default:
                    return null;
            }
        }

        /// <summary>
        /// Gets http job action details.
        /// </summary>
        /// <param name="jobActionType">Job action type.</param>
        /// <param name="request">Job actions http request.</param>
        /// <returns>PSHttpJobActionDetails</returns>
        internal static PSHttpJobActionDetails GetSchedulerHttpJobActionDetails(JobActionType jobActionType, HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            var psHttpJobActionDetails = new PSHttpJobActionDetails(jobActionType)
            {
                RequestBody = request.Body,
                RequestHeaders = request.Headers,
                RequestMethod = request.Method,
                Uri = request.Uri,
                HttpJobAuthentication = GetSchedulerHttpJobAuthenticationDetails(request.Authentication),
            };

            return psHttpJobActionDetails;
        }

        /// <summary>
        /// Get http job authentication details.
        /// </summary>
        /// <param name="authentication">Http authentication.</param>
        /// <returns>PSHttpJobAuthenticationDetails</returns>
        internal static PSHttpJobAuthenticationDetails GetSchedulerHttpJobAuthenticationDetails(HttpAuthentication authentication)
        {
            if (authentication == null)
            {
                return null;
            }

            switch (authentication.Type)
            {
                case HttpAuthenticationType.ClientCertificate:
                    var clientCertAuthentication = authentication as ClientCertAuthentication;

                    var psClientCertAuthentication = new PSHttpJobClientCertAuthenticationDetails
                    {
                        HttpAuthType = Constants.HttpAuthenticationClientCertificate,
                    };

                    if (clientCertAuthentication != null)
                    {
                        psClientCertAuthentication.ClientCertExpiryDate = clientCertAuthentication.CertificateExpirationDate.ToString();
                        psClientCertAuthentication.ClientCertSubjectName = clientCertAuthentication.CertificateSubjectName;
                        psClientCertAuthentication.ClientCertThumbprint = clientCertAuthentication.CertificateThumbprint;
                    }

                    return psClientCertAuthentication;

                case HttpAuthenticationType.Basic:
                    var basicAuthentication = authentication as BasicAuthentication;

                    var psBasicAuthentication = new PSHttpJobBasicAuthenticationDetails
                    {
                        HttpAuthType = Constants.HttpAuthenticationBasic,
                        Username = basicAuthentication == null ? null : basicAuthentication.Username,
                    };

                    return psBasicAuthentication;

                case HttpAuthenticationType.ActiveDirectoryOAuth:
                    var oAuthAuthentication = authentication as OAuthAuthentication;

                    var psOAuthAuthentication = new PSHttpJobOAuthAuthenticationDetails
                    {
                        HttpAuthType = Constants.HttpAuthenticationActiveDirectoryOAuth,
                    };

                    if (oAuthAuthentication != null)
                    {
                        psOAuthAuthentication.Audience = oAuthAuthentication.Audience;
                        psOAuthAuthentication.ClientId = oAuthAuthentication.ClientId;
                        psOAuthAuthentication.Tenant = oAuthAuthentication.Tenant;
                    }

                    return psOAuthAuthentication;

                default:
                    return new PSHttpJobAuthenticationDetails
                    {
                        HttpAuthType = Constants.HttpAuthenticationNone
                    };
            }
        }

        /// <summary>
        /// Gets storage queue job action details.
        /// </summary>
        /// <param name="jobActionType">Job action type.</param>
        /// <param name="storageQueue">Storage queue message.</param>
        /// <returns>PSStorageJobActionDetails.</returns>
        internal static PSStorageJobActionDetails GetSchedulerStorageJobActionDetails(JobActionType jobActionType, StorageQueueMessage storageQueue)
        {
            if (storageQueue == null)
            {
                throw new ArgumentNullException("storageQueue");
            }

            var psStorageJobActionDetails = new PSStorageJobActionDetails(jobActionType)
            {
                StorageAccount = storageQueue.StorageAccount,
                StorageQueueMessage = storageQueue.Message,
                StorageQueueName = storageQueue.QueueName,
                StorageSasToken = storageQueue.SasToken
            };

            return psStorageJobActionDetails;
        }

        /// <summary>
        /// Gets service bus queue job action details.
        /// </summary>
        /// <param name="jobActionType">Job action type.</param>
        /// <param name="storageQueue">Stervice bus queue message.</param>
        /// <returns>PSServiceBusJobActionDetails.</returns>
        internal static PSServiceBusJobActionDetails GetSchedulerServiceBusQueueJobActionDetails(JobActionType jobActionType, ServiceBusQueueMessage serviceBusQueueMessage)
        {
            var psServiceBusjobActionDetails = GetServiceBusJobActionDetails(jobActionType, serviceBusQueueMessage);
            psServiceBusjobActionDetails.ServiceBusQueueName = serviceBusQueueMessage.QueueName;

            return psServiceBusjobActionDetails;
        }

        /// <summary>
        /// Gets service bus topic job action details.
        /// </summary>
        /// <param name="jobActionType">Job action type.</param>
        /// <param name="storageQueue">Stervice bus topic message.</param>
        /// <returns>PSServiceBusJobActionDetails.</returns>
        internal static PSServiceBusJobActionDetails GetSchedulerServiceBusTopicJobActionDetails(JobActionType jobActionType, ServiceBusTopicMessage serviceBusTopicMessage)
        {
            var psServiceBusjobActionDetails = GetServiceBusJobActionDetails(jobActionType, serviceBusTopicMessage);
            psServiceBusjobActionDetails.ServiceBusTopicPath = serviceBusTopicMessage.TopicPath;

            return psServiceBusjobActionDetails;
        }

        /// <summary>
        /// Gets service bus action details.
        /// </summary>
        /// <param name="jobActionType">Job action type.</param>
        /// <param name="serviceBusMessage">Service bus message.</param>
        /// <returns>PSServiceBusJobActionDetails</returns>
        internal static PSServiceBusJobActionDetails GetServiceBusJobActionDetails(JobActionType jobActionType, ServiceBusMessage serviceBusMessage)
        {
            if (serviceBusMessage == null)
            {
                throw new ArgumentNullException("serviceBusMessage");
            }

            var psServieBusJobActionDetails = new PSServiceBusJobActionDetails(jobActionType)
            {
                ServiceBusAuthentication = GetServiceBusActionAuthenticationDetails(serviceBusMessage.Authentication),
                ServiceBusMessage = serviceBusMessage.Message,
                ServiceBusNamespaceProperty = serviceBusMessage.NamespaceProperty,
                ServiceBusTransportType = serviceBusMessage.TransportType.ToString()
            };

            return psServieBusJobActionDetails;
        }

        /// <summary>
        /// Get service bus authentication details.
        /// </summary>
        /// <param name="authentication">Service bus authentication.</param>
        /// <returns>PSServiceBusJobActionAuthenticationDetails</returns>
        internal static PSServiceBusJobActionAuthenticationDetails GetServiceBusActionAuthenticationDetails(ServiceBusAuthentication authentication)
        {
            if (authentication == null)
            {
                throw new ArgumentNullException("authentication");
            }

            var psServiceBusAuthentication = new PSServiceBusJobActionAuthenticationDetails
            {
                ServiceBusSasKey = authentication.SasKey,
                ServiceBusSasKeyName = authentication.SasKeyName,
                ServiceBusType = authentication.Type.ToString(),
            };

            return psServiceBusAuthentication;
        }

        #endregion
    }
}
