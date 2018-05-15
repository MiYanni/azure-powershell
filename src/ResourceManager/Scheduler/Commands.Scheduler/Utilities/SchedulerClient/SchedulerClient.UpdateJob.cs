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
    using Models;
    using Properties;
    using Management.Scheduler;
    using Management.Scheduler.Models;
    using PSManagement = System.Management.Automation;

    public partial class SchedulerClient
    {
        /// <summary>
        /// Update an existing job.
        /// </summary>
        /// <param name="updateJobParams">Update job properties specified via PowerShell.</param>
        /// <returns>The job definition.</returns>
        public PSSchedulerJobDefinition UpdateJob(PSJobParams updateJobParams)
        {
            if (string.IsNullOrWhiteSpace(updateJobParams.ResourceGroupName))
            {
                throw new PSManagement.PSArgumentNullException("ResourceGroupName");
            }

            if (string.IsNullOrWhiteSpace(updateJobParams.JobCollectionName))
            {
                throw new PSManagement.PSArgumentNullException("JobCollectionName");
            }

            if (string.IsNullOrWhiteSpace(updateJobParams.JobName))
            {
                throw new PSManagement.PSArgumentNullException("JobName");
            }

            JobDefinition existingJobDefinition = GetJob(updateJobParams.ResourceGroupName, updateJobParams.JobCollectionName, updateJobParams.JobName);

            if (existingJobDefinition == null)
            {
                throw new PSManagement.PSArgumentException(string.Format(Resources.JobDoesnotExist, updateJobParams.ResourceGroupName, updateJobParams.JobCollectionName, updateJobParams.JobName));
            }

            PopulateExistingJobParams(updateJobParams, existingJobDefinition);

            var jobDefinitionResult = SchedulerManagementClient.Jobs.Patch(updateJobParams.ResourceGroupName, updateJobParams.JobCollectionName, updateJobParams.JobName, existingJobDefinition);

            return Converter.ConvertJobDefinitionToPS(jobDefinitionResult);
        }

        /// <summary>
        /// Populates job properties with valid values.
        /// </summary>
        /// <param name="updateJobParams">Job properties specified via PowerShell.</param>
        /// <param name="existingJobDefinition">Existing job definition.</param>
        private void PopulateExistingJobParams(PSJobParams updateJobParams, JobDefinition existingJobDefinition)
        {
            JobProperties jobProperties = GetExistingJobPropertiesParams(updateJobParams, existingJobDefinition.Properties);
            existingJobDefinition.Properties = jobProperties;
        }

        /// <summary>
        /// Get job properties.
        /// </summary>
        /// <param name="updateJobParams">Job properties specified via PowerShell.</param>
        /// <param name="existingJobProperties">Exsting job properties.</param>
        /// <returns></returns>
        private JobProperties GetExistingJobPropertiesParams(PSJobParams updateJobParams, JobProperties existingJobProperties)
        {
            var newJobProperties = new JobProperties
            {
                Action = GetExistingJobAction(updateJobParams.JobAction, existingJobProperties.Action),
                Recurrence = GetExistingJobRecurrence(updateJobParams.JobRecurrence, existingJobProperties.Recurrence),
                StartTime = updateJobParams.StartTime ?? existingJobProperties.StartTime,
            };

            newJobProperties.Action.ErrorAction = GetExistingJobErrorAction(updateJobParams.JobErrorAction, existingJobProperties.Action.ErrorAction);

            newJobProperties.State = updateJobParams.JobState.GetValueOrDefaultEnum<JobState?>(null);

            return newJobProperties;
        }

        /// <summary>
        /// Get job action.
        /// </summary>
        /// <param name="updateJobActionParams">Job action properties specified via PowerShell.</param>
        /// <param name="existingJobAction">Job action properties from existing job.</param>
        /// <returns>JobAction object.</returns>
        private JobAction GetExistingJobAction(PSJobActionParams updateJobActionParams, JobAction existingJobAction)
        {
            if (updateJobActionParams != null)
            {
                if (existingJobAction != null &&
                    (existingJobAction.Type == updateJobActionParams.JobActionType ||
                    (existingJobAction.Type == JobActionType.Http || existingJobAction.Type == JobActionType.Https) &&
                    (updateJobActionParams.JobActionType == JobActionType.Http || updateJobActionParams.JobActionType == JobActionType.Https)))
                {
                    switch (updateJobActionParams.JobActionType)
                    {
                        case JobActionType.Http:
                        case JobActionType.Https:
                            PSHttpJobActionParams httpJobAction = updateJobActionParams.HttpJobAction;
                            HttpRequest existinghHttpRequest = existingJobAction.Request;
                            if (httpJobAction.Uri != null)
                            {
                                existinghHttpRequest.Uri = httpJobAction.Uri.OriginalString;
                                existingJobAction.Type = updateJobActionParams.JobActionType;
                            }

                            existinghHttpRequest.Method = httpJobAction.RequestMethod.GetValueOrDefault(existinghHttpRequest.Method);
                            existinghHttpRequest.Body = httpJobAction.RequestBody.GetValueOrDefault(existinghHttpRequest.Body);
                            existinghHttpRequest.Headers = httpJobAction.RequestHeaders != null ? httpJobAction.RequestHeaders.ToDictionary() : existinghHttpRequest.Headers;
                            existinghHttpRequest.Authentication = GetExistingAuthentication(httpJobAction.RequestAuthentication, existinghHttpRequest.Authentication);
                            break;

                        case JobActionType.StorageQueue:
                            PSStorageJobActionParams storageJobAction = updateJobActionParams.StorageJobAction;
                            StorageQueueMessage existingStorageQueue = existingJobAction.QueueMessage;
                            storageJobAction.StorageAccount = storageJobAction.StorageAccount.GetValueOrDefault(existingStorageQueue.StorageAccount);
                            storageJobAction.StorageQueueMessage = storageJobAction.StorageQueueMessage.GetValueOrDefault(existingStorageQueue.Message);
                            storageJobAction.StorageQueueName = storageJobAction.StorageQueueName.GetValueOrDefault(existingStorageQueue.QueueName);
                            storageJobAction.StorageSasToken = storageJobAction.StorageSasToken.GetValueOrDefault(existingStorageQueue.SasToken);
                            break;

                        case JobActionType.ServiceBusQueue:
                            PSServiceBusParams serviceBusQueueParams = updateJobActionParams.ServiceBusAction;
                            ServiceBusQueueMessage existingServiceBusQueueMessage = existingJobAction.ServiceBusQueueMessage;
                            UpdateServiceBus(serviceBusQueueParams, existingServiceBusQueueMessage);
                            existingServiceBusQueueMessage.QueueName = serviceBusQueueParams.QueueName.GetValueOrDefault(existingServiceBusQueueMessage.QueueName);
                            break;

                        case JobActionType.ServiceBusTopic:
                            PSServiceBusParams serviceBusTopicParams = updateJobActionParams.ServiceBusAction;
                            ServiceBusTopicMessage existingServiceBusTopicMessage = existingJobAction.ServiceBusTopicMessage;
                            UpdateServiceBus(serviceBusTopicParams, existingServiceBusTopicMessage);
                            existingServiceBusTopicMessage.TopicPath = serviceBusTopicParams.TopicPath.GetValueOrDefault(existingServiceBusTopicMessage.TopicPath);
                            break;
                    }
                }
                else
                {
                    PopulateJobAction(updateJobActionParams, ref existingJobAction);
                }
            }

            return existingJobAction;
        }

        /// <summary>
        /// Get job error action.
        /// </summary>
        /// <param name="updateJobErrorActionParams">Error action properties specified via PowerShell.</param>
        /// <param name="existingJobErrorAction">Existing error action properties.</param>
        /// <returns>JobErrorAction object.</returns>
        private JobErrorAction GetExistingJobErrorAction(PSJobActionParams updateJobErrorActionParams, JobErrorAction existingJobErrorAction)
        {
            if (updateJobErrorActionParams != null)
            {
                if (existingJobErrorAction != null &&
                    (existingJobErrorAction.Type == updateJobErrorActionParams.JobActionType ||
                    (existingJobErrorAction.Type == JobActionType.Http || existingJobErrorAction.Type == JobActionType.Https) &&
                    (updateJobErrorActionParams.JobActionType == JobActionType.Http || updateJobErrorActionParams.JobActionType == JobActionType.Https)))
                {
                    switch (updateJobErrorActionParams.JobActionType)
                    {
                        case JobActionType.Http:
                        case JobActionType.Https:
                            PSHttpJobActionParams httpJobAction = updateJobErrorActionParams.HttpJobAction;
                            HttpRequest existinghHttpRequest = existingJobErrorAction.Request;
                            existingJobErrorAction.Type = updateJobErrorActionParams.JobActionType;
                            existinghHttpRequest.Method = httpJobAction.RequestMethod.GetValueOrDefault(existinghHttpRequest.Method);
                            existinghHttpRequest.Uri = httpJobAction.Uri != null ? httpJobAction.Uri.OriginalString : existinghHttpRequest.Uri;
                            existinghHttpRequest.Body = httpJobAction.RequestBody.GetValueOrDefault(existinghHttpRequest.Body);
                            existinghHttpRequest.Authentication = GetExistingAuthentication(httpJobAction.RequestAuthentication, existinghHttpRequest.Authentication);
                            break;

                        case JobActionType.StorageQueue:
                            PSStorageJobActionParams storageJobAction = updateJobErrorActionParams.StorageJobAction;
                            StorageQueueMessage existingStorageQueue = existingJobErrorAction.QueueMessage;
                            existingStorageQueue.StorageAccount = storageJobAction.StorageAccount.GetValueOrDefault(existingStorageQueue.StorageAccount);
                            existingStorageQueue.Message = storageJobAction.StorageQueueMessage.GetValueOrDefault(existingStorageQueue.Message);
                            existingStorageQueue.QueueName = storageJobAction.StorageQueueName.GetValueOrDefault(existingStorageQueue.QueueName);
                            existingStorageQueue.SasToken = storageJobAction.StorageSasToken.GetValueOrDefault(existingStorageQueue.SasToken);
                            break;

                        case JobActionType.ServiceBusQueue:
                            PSServiceBusParams serviceBusParams = updateJobErrorActionParams.ServiceBusAction;
                            ServiceBusQueueMessage existingServiceBusQueueMessage = existingJobErrorAction.ServiceBusQueueMessage;
                            UpdateServiceBus(serviceBusParams, existingServiceBusQueueMessage);
                            existingServiceBusQueueMessage.QueueName = serviceBusParams.QueueName.GetValueOrDefault(existingServiceBusQueueMessage.QueueName);
                            break;

                        case JobActionType.ServiceBusTopic:
                            PSServiceBusParams serviceBusTopicParams = updateJobErrorActionParams.ServiceBusAction;
                            ServiceBusTopicMessage existingServiceBusTopicMessage = existingJobErrorAction.ServiceBusTopicMessage;
                            UpdateServiceBus(serviceBusTopicParams, existingServiceBusTopicMessage);
                            existingServiceBusTopicMessage.TopicPath = serviceBusTopicParams.TopicPath.GetValueOrDefault(existingServiceBusTopicMessage.TopicPath);
                            break;
                    }
                }
                else
                {
                    PopulateJobErrorAction(updateJobErrorActionParams, ref existingJobErrorAction);
                }
            }

            return existingJobErrorAction;
        }

        /// <summary>
        /// Update service bus properties.
        /// </summary>
        /// <param name="serviceBusParams">Service bus message properties specified via PowerShell.</param>
        /// <param name="existingServiceBusMessage">Existing job's service bus message properties.</param>
        private void UpdateServiceBus(PSServiceBusParams serviceBusParams, ServiceBusMessage existingServiceBusMessage)
        {
            if (existingServiceBusMessage != null)
            {
                existingServiceBusMessage.TransportType = serviceBusParams.TransportType.GetValueOrDefaultEnum<ServiceBusTransportType?>(existingServiceBusMessage.TransportType);
                existingServiceBusMessage.NamespaceProperty = serviceBusParams.NamespaceProperty.GetValueOrDefault(existingServiceBusMessage.NamespaceProperty);
                existingServiceBusMessage.Message = serviceBusParams.Message.GetValueOrDefault(existingServiceBusMessage.Message);
                
                PopulateServiceBusAuthentication(serviceBusParams.Authentication, existingServiceBusMessage.Authentication);
            }
        }

        /// <summary>
        /// Populates service bus authentication properties.
        /// </summary>
        /// <param name="serviceBusAuthentication">Service bus authentication properties specified via PowerShell.</param>
        /// <param name="existingServiceBusAuthentication">Existing job's service bus authentication properties.</param>
        private void PopulateServiceBusAuthentication(PSServiceBusAuthenticationParams serviceBusAuthentication, ServiceBusAuthentication existingServiceBusAuthentication)
        {
            if (serviceBusAuthentication != null)
            {
                existingServiceBusAuthentication.SasKey = serviceBusAuthentication.SasKey.GetValueOrDefault(existingServiceBusAuthentication.SasKey);
                existingServiceBusAuthentication.SasKeyName = serviceBusAuthentication.SasKeyName.GetValueOrDefault(existingServiceBusAuthentication.SasKeyName);
                existingServiceBusAuthentication.Type = serviceBusAuthentication.Type.GetValueOrDefaultEnum<ServiceBusAuthenticationType?>(existingServiceBusAuthentication.Type);
            }
        }

        /// <summary>
        /// Gets existing job recurrence.
        /// </summary>
        /// <param name="updateRecurrenceParams">Recurrence properties specified via PowerShell.</param>
        /// <param name="recurrence">Existing job recurrence property.</param>
        /// <returns>JobRecurrence object.</returns>
        private JobRecurrence GetExistingJobRecurrence(PSJobRecurrenceParams updateRecurrenceParams, JobRecurrence recurrence)
        {
            if (updateRecurrenceParams != null)
            {
                if (recurrence == null)
                {
                    return GetJobRecurrence(updateRecurrenceParams);
                }

                recurrence.Count = updateRecurrenceParams.ExecutionCount ?? recurrence.Count;
                recurrence.EndTime = updateRecurrenceParams.EndTime ?? recurrence.EndTime;
                recurrence.Frequency = updateRecurrenceParams.Frequency.GetValueOrDefaultEnum<RecurrenceFrequency?>(recurrence.Frequency);
                recurrence.Interval = updateRecurrenceParams.Interval ?? recurrence.Interval;
            }

            return recurrence;
        }

        /// <summary>
        /// Gets http authentication.
        /// </summary>
        /// <param name="updateJobAuthenticationParams">Http authentication properties specified via PowerShell.</param>
        /// <param name="authentication">Existing job http authentication.</param>
        /// <returns>HttpAuthentication object.</returns>
        private HttpAuthentication GetExistingAuthentication(PSHttpJobAuthenticationParams updateJobAuthenticationParams, HttpAuthentication authentication)
        {
            // With current SDK design (limits within Swagger for inheritance), we won't be able to update Authentication params. 
            // In order for user to update Authentication params, the user must enter the credentials again.
            if (updateJobAuthenticationParams != null && updateJobAuthenticationParams.HttpAuthType != null)
            {
                return PopulateHttpAuthentication(updateJobAuthenticationParams);
            }

            return null;
        }
    }
}
