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

using Microsoft.Azure.Management.ApplicationInsights.Management.Models;

namespace Microsoft.Azure.Commands.ApplicationInsights.Models
{
    public class PSExportConfiguration
    {
        public string ExportId { get; set; }
        public string StorageName { get; set; }
        public string ContainerName { get; set; }
        public string DocumentTypes { get; set; }
        public string DestinationStorageSubscriptionId { get; set; }
        public string DestinationStorageLocationId { get; set; }
        public string DestinationStorageAccountId { get; set; }
        public string IsEnabled { get; set; }
        public string ExportStatus { get; set; }
        public string LastSuccessTime { get; set; }

        public PSExportConfiguration(ApplicationInsightsComponentExportConfiguration response)
        {
            ExportId = response.ExportId;

            DocumentTypes = string.Join(", ", ApplicationInsightsBaseCmdlet.ConvertToDocumentType(response.RecordTypes.Split(',')));
            DestinationStorageSubscriptionId = response.DestinationStorageSubscriptionId;
            DestinationStorageLocationId = response.DestinationStorageLocationId;
            DestinationStorageAccountId = response.DestinationAccountId;
            IsEnabled = response.IsUserEnabled;
            ExportStatus = response.ExportStatus;
            StorageName = response.StorageName;
            ContainerName = response.ContainerName;
            LastSuccessTime = response.LastSuccessTime;
        }
    }

    public class PSExportConfigurationTableView : PSExportConfiguration
    {
        public PSExportConfigurationTableView(ApplicationInsightsComponentExportConfiguration response)
            : base(response)
        { }
    }
}

