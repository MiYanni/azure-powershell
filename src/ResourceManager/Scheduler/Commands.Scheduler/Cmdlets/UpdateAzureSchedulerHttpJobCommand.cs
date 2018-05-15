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

namespace Microsoft.Azure.Commands.Scheduler.Cmdlets
{
    using System;
    using System.Collections;
    using System.Management.Automation;
    using Models;
    using Properties;
    using Utilities;
    using SchedulerModels = Management.Scheduler.Models;
    using ResourceManager.Common.ArgumentCompleters;

    /// <summary>
    /// Updates existing job.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "AzureRmSchedulerHttpJob", SupportsShouldProcess = true), OutputType(typeof(PSSchedulerJobDefinition))]
    public class UpdateAzureSchedulerHttpJobCommand : JobBaseCmdlet, IDynamicParameters
    {
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The targeted resource group for job.")]
        [ResourceGroupCompleter]
        [ValidateNotNullOrEmpty]
        public string ResourceGroupName { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The name of the job collection.")]
        [Alias("Name", "ResourceName")]
        [ValidateNotNullOrEmpty]
        public string JobCollectionName { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The name of the job")]
        [ValidateNotNullOrEmpty]
        public string JobName { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "The Method for Http and Https Action types (GET, PUT, POST, HEAD or DELETE).")]
        [ValidateNotNullOrEmpty]
        [ValidateSet(Constants.HttpMethodGET, Constants.HttpMethodPUT, Constants.HttpMethodPOST, Constants.HttpMethodDELETE, IgnoreCase = true)]
        public string Method { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "The Uri for job action.")]
        [ValidateNotNullOrEmpty]
        public Uri Uri { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "The Body for PUT and POST job actions.")]
        [ValidateNotNullOrEmpty]
        public string RequestBody { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The header collection.")]
        [ValidateNotNullOrEmpty]
        public Hashtable Headers { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, HelpMessage = "The Http Authentication type (None or ClientCertificate).")]
        [ValidateSet(Constants.HttpAuthenticationNone, Constants.HttpAuthenticationClientCertificate, Constants.HttpAuthenticationActiveDirectoryOAuth, Constants.HttpAuthenticationBasic, IgnoreCase = true)]
        public string HttpAuthenticationType { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "The Start Time")]
        [ValidateNotNullOrEmpty]
        public DateTime? StartTime { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Interval of the recurrence at the given frequency")]
        [ValidateNotNullOrEmpty]
        public int? Interval { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "The frequency of recurrence")]
        [ValidateNotNullOrEmpty]
        [ValidateSet(Constants.FrequencyTypeMinute, Constants.FrequencyTypeHour, Constants.FrequencyTypeDay, Constants.FrequencyTypeWeek, Constants.FrequencyTypeMonth, IgnoreCase = true)]
        public string Frequency { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, HelpMessage = "The End Time")]
        [ValidateNotNullOrEmpty]
        public DateTime? EndTime { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Count of occurrences that will execute. Optional. Default will recur infinitely")]
        [ValidateNotNullOrEmpty]
        public int? ExecutionCount { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The job state.")]
        [ValidateSet(Constants.JobStateEnabled, Constants.JobStateDisabled, IgnoreCase = true)]
        public string JobState { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Error action settings")]
        [ValidateNotNullOrEmpty]
        [ValidateSet(Constants.HttpAction, Constants.HttpsAction, Constants.StorageQueueAction, Constants.ServiceBusQueueAction, Constants.ServiceBusTopicAction, IgnoreCase = true)]
        public string ErrorActionType { get; set; }

        /// <summary>
        /// Executes the command.
        /// </summary>
        public override void ExecuteCmdlet()
        {
            base.ExecuteCmdlet();

            var httpJobAction = new PSHttpJobActionParams
            {
                RequestMethod = Method,
                Uri = Uri,
                RequestBody = RequestBody,
                RequestHeaders = Headers,
                RequestAuthentication = GetAuthenticationParams(),
            };

            var jobActionType = SchedulerModels.JobActionType.Http;

            if (Uri != null)
            {
                if (Uri.Scheme.Equals(Constants.HttpScheme, StringComparison.InvariantCultureIgnoreCase) ||
                    Uri.Scheme.Equals(Constants.HttpsScheme, StringComparison.InvariantCultureIgnoreCase))
                {
                    jobActionType = (SchedulerModels.JobActionType)Enum.Parse(typeof(SchedulerModels.JobActionType), Uri.Scheme, true);
                }
                else
                {
                    throw new PSArgumentException(string.Format(Resources.SchedulerInvalidUriScheme, Uri.Scheme));
                }
            }

            var jobAction = new PSJobActionParams
            {
                JobActionType = jobActionType,
                HttpJobAction = httpJobAction
            };

            var jobRecurrence = new PSJobRecurrenceParams
            {
                Interval = Interval,
                Frequency = Frequency,
                EndTime = EndTime,
                ExecutionCount = ExecutionCount
            };

            var jobParams = new PSJobParams
            {
                ResourceGroupName = ResourceGroupName,
                JobCollectionName = JobCollectionName,
                JobName = JobName,
                JobState = JobState,
                StartTime = StartTime,
                JobAction = jobAction,
                JobRecurrence = jobRecurrence,
                JobErrorAction = GetErrorActionParamsValue(ErrorActionType)
            };

            ConfirmAction(
                string.Format(Resources.UpdateHttpJobResourceDescription, JobName),
                JobCollectionName,
                () =>
                    {
                        WriteObject(SchedulerClient.UpdateJob(jobParams));
                    }
            );            
        }

        /// <summary>
        /// Get conditional parameters depending on specified ErrorAction and/or type of http authentication.
        /// </summary>
        /// <returns>List of Powershell dynamic parameters.</returns>
        public object GetDynamicParameters()
        {
            var runtimeDefinedParameterDictionary = new RuntimeDefinedParameterDictionary();

            if (!string.IsNullOrWhiteSpace(HttpAuthenticationType))
            {
                if (HttpAuthenticationType == Constants.HttpAuthenticationClientCertificate)
                {
                    runtimeDefinedParameterDictionary.AddRange(JobDynamicParameters.AddHttpClientCertificateAuthenticationTypeParameters(false));
                }
                else if (HttpAuthenticationType == Constants.HttpAuthenticationBasic)
                {
                    runtimeDefinedParameterDictionary.AddRange(JobDynamicParameters.AddHttpBasicAuthenticationTypeParameters(false));
                }
                else if (HttpAuthenticationType == Constants.HttpAuthenticationActiveDirectoryOAuth)
                {
                    runtimeDefinedParameterDictionary.AddRange(JobDynamicParameters.AddHttpActiveDirectoryOAuthAuthenticationTypeParameters(false));
                }
            }

            if (!string.IsNullOrWhiteSpace(ErrorActionType))
            {
                runtimeDefinedParameterDictionary.AddRange(AddErrorActionParameters(ErrorActionType, false));
            }
            
            return runtimeDefinedParameterDictionary;
        }

        /// <summary>
        /// Gets http authentication.
        /// </summary>
        /// <returns>PSHttpJobAuthenticationParams instance.</returns>
        private PSHttpJobAuthenticationParams GetAuthenticationParams()
        {
            if (!string.IsNullOrWhiteSpace(HttpAuthenticationType))
            {
                var jobAuthentication = new PSHttpJobAuthenticationParams
                {
                    HttpAuthType = HttpAuthenticationType,
                    ClientCertPfx = string.IsNullOrWhiteSpace(JobDynamicParameters.ClientCertificatePfx) ? null : SchedulerUtility.GetCertData(this.ResolvePath(JobDynamicParameters.ClientCertificatePfx), JobDynamicParameters.ClientCertificatePassword),
                    ClientCertPassword = JobDynamicParameters.ClientCertificatePassword,
                    Username = JobDynamicParameters.BasicUsername,
                    Password = JobDynamicParameters.BasicPassword,
                    Secret = JobDynamicParameters.OAuthSecret,
                    Tenant = JobDynamicParameters.OAuthTenant,
                    Audience = JobDynamicParameters.OAuthAudience,
                    ClientId = JobDynamicParameters.OAuthClientId
                };

                return jobAuthentication;
            }

            return null;
        }
    }
}
