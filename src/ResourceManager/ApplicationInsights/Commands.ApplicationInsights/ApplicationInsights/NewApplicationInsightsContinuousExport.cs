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

using Microsoft.Azure.Commands.ApplicationInsights.Models;
using Microsoft.Azure.Commands.ResourceManager.Common.ArgumentCompleters;
using Microsoft.Azure.Management.ApplicationInsights.Management.Models;
using Microsoft.Azure.Management.Internal.Resources.Utilities.Models;
using System.Linq;
using System.Management.Automation;

namespace Microsoft.Azure.Commands.ApplicationInsights
{
    [Cmdlet(VerbsCommon.New, ApplicationInsightsContinuousExportNounStr, DefaultParameterSetName = ComponentNameParameterSet, SupportsShouldProcess = true), OutputType(typeof(PSExportConfiguration))]
    public class NewAzureApplicationInsightsContinuousExportCommand : ApplicationInsightsBaseCmdlet
    {
        [Parameter(
            Position = 0,
            Mandatory = true,
            ParameterSetName = ComponentObjectParameterSet,
            ValueFromPipeline = true,
            HelpMessage = "Application Insights Component Object.")]
        [ValidateNotNull]
        public PSApplicationInsightsComponent ApplicationInsightsComponent { get; set; }

        [Parameter(
            Position = 0,
            Mandatory = true,
            ParameterSetName = ResourceIdParameterSet,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Application Insights Component Resource Id.")]
        [ValidateNotNullOrEmpty]
        public string ResourceId { get; set; }

        [Parameter(
            Position = 0,
            Mandatory = true,
            ParameterSetName = ComponentNameParameterSet,
            HelpMessage = "Resource Group Name.")]
        [ResourceGroupCompleter]
        [ValidateNotNullOrEmpty]
        public string ResourceGroupName { get; set; }

        [Parameter(
            Position = 1,
            Mandatory = true,
            ParameterSetName = ComponentNameParameterSet,
            HelpMessage = "Component Name.")]
        [Alias(ApplicationInsightsComponentNameAlias, ComponentNameAlias)]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        [Parameter(
            Mandatory = true,
            HelpMessage = "Document types that need exported.")]        
        [ValidateSet(DocumentTypes.Requests,
            DocumentTypes.Exceptions,
            DocumentTypes.Event,
            DocumentTypes.Messages,
            DocumentTypes.Metrics,
            DocumentTypes.PageViewPerformance,
            DocumentTypes.PageViews,
            DocumentTypes.RemoteDependency,
            DocumentTypes.Availability,
            DocumentTypes.PerformanceCounters,
            IgnoreCase = true)]
        public string[] DocumentType { get; set; }

        [Parameter(
            Mandatory = true,
            HelpMessage = "Destination Storage Account Id.")]
        [ValidateNotNullOrEmpty]
        public string StorageAccountId { get; set; }

        [Parameter(
            Mandatory = true,
            HelpMessage = "Destination Storage Location Id.")]
        [ValidateNotNullOrEmpty]
        public string StorageLocation { get; set; }

        [Parameter(
            Mandatory = true,
            HelpMessage = "Destination Storage SAS Uri.")]
        [ValidateNotNullOrEmpty]
        public string StorageSASUri { get; set; }

        public override void ExecuteCmdlet()
        {
            base.ExecuteCmdlet();

            if (ApplicationInsightsComponent != null)
            {
                ResourceGroupName = ApplicationInsightsComponent.ResourceGroupName;
                Name = ApplicationInsightsComponent.Name;
            }

            if (!string.IsNullOrEmpty(ResourceId))
            {
                ResourceIdentifier identifier = new ResourceIdentifier(ResourceId);
                ResourceGroupName = identifier.ResourceGroupName;
                Name = identifier.ResourceName;
            }

            ApplicationInsightsComponentExportRequest exportRequest = new ApplicationInsightsComponentExportRequest();
            exportRequest.IsEnabled = "true";
            exportRequest.DestinationAccountId = StorageAccountId;
            exportRequest.DestinationStorageSubscriptionId = ParseSubscriptionFromId(StorageAccountId);
            exportRequest.DestinationAddress = StorageSASUri;
            exportRequest.DestinationStorageLocationId = StorageLocation;
            exportRequest.DestinationType = "Blob";
            exportRequest.RecordTypes = string.Join(",", ConvertToRecordType(DocumentType));

            if (ShouldProcess(Name, "Create Application Insights Continuous Export"))
            {
                try
                {
                    var exportConfigurationsResponse = AppInsightsManagementClient
                                                            .ExportConfigurations
                                                            .CreateWithHttpMessagesAsync(
                                                                ResourceGroupName,
                                                                Name,
                                                                exportRequest)
                                                            .GetAwaiter()
                                                            .GetResult();

                    WriteComponentExportConfiguration(exportConfigurationsResponse.Body.FirstOrDefault());
                }
                catch (Rest.Azure.CloudException exception)
                {
                    if (exception.Response != null && exception.Response.StatusCode == System.Net.HttpStatusCode.Conflict)
                    {
                        throw new System.Exception("There is already an export defined for this destination.");
                    }

                    throw exception;
                }
            }
        }
    }
}
