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

namespace Microsoft.Azure.Commands.Management.IotHub
{
    using System.Management.Automation;
    using Common;
    using Models;
    using Azure.Management.IotHub;
    using Azure.Management.IotHub.Models;
    using ResourceProperties = Properties;
    using ResourceManager.Common.ArgumentCompleters;

    [Cmdlet(VerbsCommon.New, "AzureRmIotHubImportDevices", SupportsShouldProcess = true)]
    [OutputType(typeof(PSIotHubJobResponse))]
    public class NewAzureRmIotHubImportDevices : IotHubBaseCmdlet
    {
        [Parameter(
            Position = 0,
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Name of the Resource Group")]
        [ResourceGroupCompleter]
        [ValidateNotNullOrEmpty]
        public string ResourceGroupName { get; set; }

        [Parameter(
            Position = 1,
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Name of the Iot Hub")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        [Parameter(
            Position = 2,
            Mandatory = true,
            HelpMessage = "The BlobContainerUri to import from")]
        [ValidateNotNullOrEmpty]
        public string InputBlobContainerUri { get; set; }

        [Parameter(
            Position = 3,
            Mandatory = true,
            HelpMessage = "The BlobContainerUri to write output to")]
        [ValidateNotNullOrEmpty]
        public string OutputBlobContainerUri { get; set; }

        public override void ExecuteCmdlet()
        {
            if (ShouldProcess(Name, ResourceProperties.Resources.NewAzureRmIotHubImportDevices))
            {
                var importDevicesRequest = new PSImportDevicesRequest
                {
                    InputBlobContainerUri = InputBlobContainerUri,
                    OutputBlobContainerUri = OutputBlobContainerUri
                };

                JobResponse jobResponse = IotHubClient.IotHubResource.ImportDevices(ResourceGroupName, Name, IotHubUtils.ToImportDevicesRequest(importDevicesRequest));
                WriteObject(IotHubUtils.ToPSIotHubJobResponse(jobResponse), false);
            }
        }
    }
}
