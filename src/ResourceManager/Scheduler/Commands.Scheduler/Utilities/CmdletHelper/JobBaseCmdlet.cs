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
    using System.Management.Automation;
    using Models;
    using SchedulerModels = Management.Scheduler.Models;

    public class JobBaseCmdlet : SchedulerBaseCmdlet
    {
        /// <summary>
        /// Dynamic PowerShell parameters for creating a job.
        /// </summary>
        internal JobDynamicParameters JobDynamicParameters = new JobDynamicParameters();

        /// <summary>
        /// Gets service bus error action authentication.
        /// </summary>
        /// <returns></returns>
        private PSServiceBusAuthenticationParams GetServiceBusErrorActionAuthentication()
        {
            return new PSServiceBusAuthenticationParams
            {
                SasKey = JobDynamicParameters.ErrorActionServiceBusSasKeyValue,
                SasKeyName = JobDynamicParameters.ErrorActionServiceBusSasKeyName,
                Type = Constants.SharedAccessKey
            };
        }

        /// <summary>
        /// Gets service bus error action parameters.
        /// </summary>
        /// <returns></returns>
        private PSServiceBusParams GetServiceBusErrorActionParams()
        {
            return new PSServiceBusParams
            {
                Authentication = GetServiceBusErrorActionAuthentication(),
                Message = JobDynamicParameters.ErrorActionServiceBusMessage,
                NamespaceProperty = JobDynamicParameters.ErrorActionServiceBusNamespace,
                TransportType = JobDynamicParameters.ErrorActionServiceBusTransportType
            };
        }

        /// <summary>
        /// Populates error action paramenter values to PSJobActionParams.
        /// </summary>
        /// <param name="errorActionType">Error action type e.g. http, https, storage etc</param>
        /// <returns>PSJobActionParams.</returns>
        internal PSJobActionParams GetErrorActionParamsValue(string errorActionType)
        {
            if (!string.IsNullOrWhiteSpace(errorActionType))
            {
                var jobErrorActionType = (SchedulerModels.JobActionType)Enum.Parse(typeof(SchedulerModels.JobActionType), errorActionType, true);

                var jobErrorAction = new PSJobActionParams
                {
                    JobActionType = jobErrorActionType,
                };

                switch(jobErrorActionType)
                {
                    case SchedulerModels.JobActionType.Http:
                    case SchedulerModels.JobActionType.Https:
                        var jobErrorActionAuthentication = new PSHttpJobAuthenticationParams
                        {
                            HttpAuthType = JobDynamicParameters.ErrorActionHttpAuthenticationType,
                            ClientCertPfx = string.IsNullOrWhiteSpace(JobDynamicParameters.ErrorActionClientCertificatePfx) ? null : SchedulerUtility.GetCertData(this.ResolvePath(JobDynamicParameters.ErrorActionClientCertificatePfx), JobDynamicParameters.ErrorActionClientCertificatePassword),
                            ClientCertPassword = JobDynamicParameters.ErrorActionClientCertificatePassword,
                            Username = JobDynamicParameters.ErrorActionBasicUsername,
                            Password = JobDynamicParameters.ErrorActionBasicPassword,
                            Secret = JobDynamicParameters.ErrorActionOAuthSecret,
                            Tenant = JobDynamicParameters.ErrorActionOAuthTenant,
                            Audience = JobDynamicParameters.ErrorActionOAuthAudience,
                            ClientId = JobDynamicParameters.ErrorActionOAuthClientId
                        };

                        var httpJobErrorAction = new PSHttpJobActionParams
                        {
                            RequestMethod = JobDynamicParameters.ErrorActionMethod,
                            Uri = JobDynamicParameters.ErrorActionUri,
                            RequestBody = JobDynamicParameters.ErrorActionRequestBody,
                            RequestHeaders = JobDynamicParameters.ErrorActionHeaders,
                            RequestAuthentication = jobErrorActionAuthentication
                        };

                        jobErrorAction.HttpJobAction = httpJobErrorAction;
                        break;

                    case SchedulerModels.JobActionType.StorageQueue:
                        var storageQueueErrorAction = new PSStorageJobActionParams
                        {
                            StorageAccount = JobDynamicParameters.ErrorActionStorageAccount,
                            StorageQueueName = JobDynamicParameters.ErrorActionStorageQueue,
                            StorageSasToken = JobDynamicParameters.ErrorActionStorageSASToken,
                            StorageQueueMessage = JobDynamicParameters.ErrorActionQueueMessageBody,
                        };

                        jobErrorAction.StorageJobAction = storageQueueErrorAction;
                        break;

                    case SchedulerModels.JobActionType.ServiceBusQueue:
                        var serviceBusQueueErrorAction = GetServiceBusErrorActionParams();
                        serviceBusQueueErrorAction.QueueName = JobDynamicParameters.ErrorActionServiceBusQueueName;
                        jobErrorAction.ServiceBusAction = serviceBusQueueErrorAction;
                        break;

                    case SchedulerModels.JobActionType.ServiceBusTopic:
                        var serviceBusTopicErrorAction = GetServiceBusErrorActionParams();
                        serviceBusTopicErrorAction.TopicPath = JobDynamicParameters.ErrorActionServiceBusTopicPath;
                        jobErrorAction.ServiceBusAction = serviceBusTopicErrorAction;
                        break;
                }

                return jobErrorAction;
            }

            return null;
        }

        /// <summary>
        /// Adds error action Powershell Parameters.
        /// </summary>
        /// <param name="errorActionType">Error action type.</param>
        /// <param name="create">true</param>
        /// <returns>PowerShell parameters.</returns>
        public RuntimeDefinedParameterDictionary AddErrorActionParameters(string errorActionType, bool create)
        {
            var runtimeDefinedParameterDictionary = new RuntimeDefinedParameterDictionary();

            if (!string.IsNullOrWhiteSpace(errorActionType))
            {
                if (errorActionType.Equals(Constants.HttpAction, StringComparison.InvariantCultureIgnoreCase))
                {
                    runtimeDefinedParameterDictionary.AddRange(JobDynamicParameters.AddHttpErrorActionParameters(create));
                }
                else if (errorActionType.Equals(Constants.StorageQueueAction, StringComparison.InvariantCultureIgnoreCase))
                {
                    runtimeDefinedParameterDictionary.AddRange(JobDynamicParameters.AddStorageQueueErrorActionParameters(create));
                }
                else if (errorActionType.Equals(Constants.ServiceBusQueueAction, StringComparison.InvariantCultureIgnoreCase))
                {
                    runtimeDefinedParameterDictionary.AddRange(JobDynamicParameters.AddServiceBusQueueErrorActionParameters(create));
                }
                else if (errorActionType.Equals(Constants.ServiceBusTopicAction, StringComparison.InvariantCultureIgnoreCase))
                {
                    runtimeDefinedParameterDictionary.AddRange(JobDynamicParameters.AddServiceBusTopicErrorActionParameters(create));
                }
            }

            return runtimeDefinedParameterDictionary;
        }
    }
}
