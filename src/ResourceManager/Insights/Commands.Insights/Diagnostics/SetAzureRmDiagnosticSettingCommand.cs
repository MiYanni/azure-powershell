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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Threading;
using System.Xml;
using Microsoft.Azure.Commands.Insights.OutputClasses;
using Microsoft.Azure.Management.Monitor.Management;
using Microsoft.Azure.Management.Monitor.Management.Models;

namespace Microsoft.Azure.Commands.Insights.Diagnostics
{
    /// <summary>
    /// Get the list of events for at a subscription level.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "AzureRmDiagnosticSetting", SupportsShouldProcess = true), OutputType(typeof(PSServiceDiagnosticSettings))]
    public class SetAzureRmDiagnosticSettingCommand : ManagementCmdletBase
    {
        public const string StorageAccountIdParamName = "StorageAccountId";
        public const string ServiceBusRuleIdParamName = "ServiceBusRuleId";
        public const string EventHubRuleIdParamName = "EventHubAuthorizationRuleId";
        public const string WorkspacetIdParamName = "WorkspaceId";
        public const string EnabledParamName = "Enabled";

        #region Parameters declarations

        /// <summary>
        /// Gets or sets the resourceId parameter of the cmdlet
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The resource id")]
        [ValidateNotNullOrEmpty]
        public string ResourceId { get; set; }

        /// <summary>
        /// Gets or sets the storage account parameter of the cmdlet
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The storage account id")]
        public string StorageAccountId { get; set; }

        /// <summary>
        /// Gets or sets the service bus rule id parameter of the cmdlet
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The service bus rule id")]
        public string ServiceBusRuleId { get; set; }

        /// <summary>
        /// Gets or sets the event hub authorization rule id parameter of the cmdlet
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The event hub rule id")]
        public string EventHubAuthorizationRuleId { get; set; }

        /// <summary>
        /// Gets or sets the enable parameter of the cmdlet
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The value indicating whether the diagnostics should be enabled or disabled")]
        [ValidateNotNullOrEmpty]
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the categories parameter of the cmdlet
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The log categories")]
        [ValidateNotNullOrEmpty]
        public List<string> Categories { get; set; }

        /// <summary>
        /// Gets or sets the timegrain parameter of the cmdlet
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The timegrains")]
        [ValidateNotNullOrEmpty]
        public List<string> Timegrains { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether retention should be enabled
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The value indicating whether the retention should be enabled")]
        [ValidateNotNullOrEmpty]
        public bool? RetentionEnabled { get; set; }

        /// <summary>
        /// Gets or sets the OMS workspace Id
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The resource Id of the Log Analytics workspace to send logs/metrics to")]
        public string WorkspaceId { get; set; }

        /// <summary>
        /// Gets or sets the retention in days
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "The retention in days.")]
        public int? RetentionInDays { get; set; }

        #endregion

        private bool isStorageParamPresent;

        private bool isServiceBusParamPresent;

        private bool isEventHubRuleParamPresent;

        private bool isWorkspaceParamPresent;

        private bool isEnbledParameterPresent;

        protected override void ProcessRecordInternal()
        {
            HashSet<string> usedParams = new HashSet<string>(MyInvocation.BoundParameters.Keys, StringComparer.OrdinalIgnoreCase);

            isStorageParamPresent = usedParams.Contains(StorageAccountIdParamName);
            isServiceBusParamPresent = usedParams.Contains(ServiceBusRuleIdParamName);
            isEventHubRuleParamPresent = usedParams.Contains(EventHubRuleIdParamName);
            isWorkspaceParamPresent = usedParams.Contains(WorkspacetIdParamName);
            isEnbledParameterPresent = usedParams.Contains(EnabledParamName);

            if (!isStorageParamPresent &&
                !isServiceBusParamPresent &&
                !isEventHubRuleParamPresent &&
                !isWorkspaceParamPresent &&
                !isEnbledParameterPresent)
            {
                throw new ArgumentException("No operation is specified");
            }

            ServiceDiagnosticSettingsResource getResponse = MonitorManagementClient.ServiceDiagnosticSettings.GetAsync(ResourceId, CancellationToken.None).Result;

            ServiceDiagnosticSettingsResource properties = getResponse;

            SetStorage(properties);

            SetServiceBus(properties);

            SetEventHubRule(properties);

            SetWorkspace(properties);

            if (Categories == null && Timegrains == null)
            {
                SetAllCategoriesAndTimegrains(properties);
            }
            else
            {
                if (Categories != null)
                {
                    SetSelectedCategories(properties);
                }

                if (Timegrains != null)
                {
                    SetSelectedTimegrains(properties);
                }
            }

            if (RetentionEnabled.HasValue)
            {
                SetRetention(properties);
            }

            var putParameters = CopySettings(properties);

            if (ShouldProcess(
                string.Format("Create/update a diagnostic setting for resource Id: {0}", ResourceId),
                "Create/update a diagnostic setting"))
            {
                ServiceDiagnosticSettingsResource result = MonitorManagementClient.ServiceDiagnosticSettings.CreateOrUpdateAsync(ResourceId, putParameters, CancellationToken.None).Result;
                WriteObject(new PSServiceDiagnosticSettings(result));
            }
        }

        private static ServiceDiagnosticSettingsResource CopySettings(ServiceDiagnosticSettingsResource properties)
        {
            // Location is marked as required, but the get operation returns Location as null. So use an empty string instead of null to avoid validation errors
            var putParameters = new ServiceDiagnosticSettingsResource(properties.Location ?? string.Empty, name: properties.Name, id: properties.Id, type: properties.Type)
            {
                Logs = properties.Logs,
                Metrics = properties.Metrics,
                ServiceBusRuleId = properties.ServiceBusRuleId,
                StorageAccountId = properties.StorageAccountId,
                WorkspaceId = properties.WorkspaceId,
                Tags = properties.Tags,
                EventHubAuthorizationRuleId = properties.EventHubAuthorizationRuleId
            };
            return putParameters;
        }

        private void SetRetention(ServiceDiagnosticSettingsResource properties)
        {
            var retentionPolicy = new RetentionPolicy
            {
                Enabled = RetentionEnabled.Value,
                Days = RetentionInDays.Value
            };

            if (properties.Logs != null)
            {
                foreach (LogSettings logSettings in properties.Logs)
                {
                    logSettings.RetentionPolicy = retentionPolicy;
                }
            }

            if (properties.Metrics != null)
            {
                foreach (MetricSettings metricSettings in properties.Metrics)
                {
                    metricSettings.RetentionPolicy = retentionPolicy;
                }
            }
        }

        private void SetSelectedTimegrains(ServiceDiagnosticSettingsResource properties)
        {
            if (!isEnbledParameterPresent)
            {
                throw new ArgumentException("Parameter 'Enabled' is required by 'Timegrains' parameter.");
            }

            foreach (string timegrainString in Timegrains)
            {
                TimeSpan timegrain = XmlConvert.ToTimeSpan(timegrainString);
                MetricSettings metricSettings = properties.Metrics.FirstOrDefault(x => TimeSpan.Equals(x.TimeGrain, timegrain));

                if (metricSettings == null)
                {
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Metric timegrain '{0}' is not available", timegrainString));
                }
                metricSettings.Enabled = Enabled;
            }
        }

        private void SetSelectedCategories(ServiceDiagnosticSettingsResource properties)
        {
            if (!isEnbledParameterPresent)
            {
                throw new ArgumentException("Parameter 'Enabled' is required by 'Categories' parameter.");
            }

            foreach (string category in Categories)
            {
                LogSettings logSettings = properties.Logs.FirstOrDefault(x => string.Equals(x.Category, category, StringComparison.OrdinalIgnoreCase));

                if (logSettings == null)
                {
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Log category '{0}' is not available", category));
                }

                logSettings.Enabled = Enabled;
            }
        }

        private void SetAllCategoriesAndTimegrains(ServiceDiagnosticSettingsResource properties)
        {
            if (!isEnbledParameterPresent)
            {
                return;
            }

            foreach (var log in properties.Logs)
            {
                log.Enabled = Enabled;
            }

            foreach (var metric in properties.Metrics)
            {
                metric.Enabled = Enabled;
            }
        }

        private void SetWorkspace(ServiceDiagnosticSettingsResource properties)
        {
            if (isWorkspaceParamPresent)
            {
                properties.WorkspaceId = WorkspaceId;
            }
        }

        private void SetServiceBus(ServiceDiagnosticSettingsResource properties)
        {
            if (isServiceBusParamPresent)
            {
                properties.ServiceBusRuleId = ServiceBusRuleId;
            }
        }

        private void SetEventHubRule(ServiceDiagnosticSettingsResource properties)
        {
            if (isEventHubRuleParamPresent)
            {
                properties.EventHubAuthorizationRuleId = EventHubAuthorizationRuleId;
            }
        }


        private void SetStorage(ServiceDiagnosticSettingsResource properties)
        {
            if (isStorageParamPresent)
            {
                properties.StorageAccountId = StorageAccountId;
            }
        }
    }
}
