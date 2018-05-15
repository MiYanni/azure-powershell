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

    [Cmdlet(VerbsCommon.New, "AzureRmIotHub", SupportsShouldProcess = true)]
    [OutputType(typeof(PSIotHub))]
    public class NewAzureRmIotHub : IotHubBaseCmdlet
    {
        [Parameter(
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Name of the Resource Group")]
        [ResourceGroupCompleter]
        [ValidateNotNullOrEmpty]
        public string ResourceGroupName { get; set; }

        [Parameter(
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Name of the Iot Hub")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        [Parameter(
            Mandatory = true,
            HelpMessage = "Name of the Sku")]
        [ValidateNotNullOrEmpty]
        public PSIotHubSku SkuName { get; set; }

        [Parameter(
            Mandatory = true,
            HelpMessage = "Number of Units")]
        [ValidateNotNullOrEmpty]
        public long Units { get; set; }

        [Parameter(
            Mandatory = true,
            HelpMessage = "Location")]
        [LocationCompleter("Microsoft.Devices/IotHubs")]
        [ValidateNotNullOrEmpty]
        public string Location { get; set; }

        [Parameter(
            Mandatory = false,
            HelpMessage = "Properties")]
        [ValidateNotNullOrEmpty]
        public PSIotHubInputProperties Properties { get; set; }

        public override void ExecuteCmdlet()
        {
            if (ShouldProcess(Name, ResourceProperties.Resources.AddIotHub))
            {
                var iotHubDescription = new IotHubDescription
                {
                    Location = Location,
                    Sku = new IotHubSkuInfo
                    {
                        Name = SkuName.ToString(),
                        Capacity = Units
                    }
                };

                if (Properties != null)
                {
                    iotHubDescription.Properties = IotHubUtils.ToIotHubProperties(Properties);
                }

                IotHubClient.IotHubResource.CreateOrUpdate(ResourceGroupName, Name, iotHubDescription);
                IotHubDescription updatedIotHubDescription = IotHubClient.IotHubResource.Get(ResourceGroupName, Name);
                WriteObject(IotHubUtils.ToPSIotHub(updatedIotHubDescription), false);
            }
        }
    }
}
