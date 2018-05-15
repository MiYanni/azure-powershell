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

namespace Microsoft.Azure.Commands.Management.DeviceProvisioningServices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using Models;
    using ResourceManager.Common.ArgumentCompleters;
    using Azure.Management.DeviceProvisioningServices;
    using Azure.Management.DeviceProvisioningServices.Models;
    using DPSResources = Properties.Resources;

    [Cmdlet(VerbsCommon.Add, "AzureRmIoTDeviceProvisioningServiceLinkedHub", DefaultParameterSetName = ResourceParameterSet, SupportsShouldProcess = true)]
    [Alias("Add-AzureRmIoTDpsHub")]
    [OutputType(typeof(PSIotHubDefinitionDescription), typeof(List<PSIotHubDefinitions>))]
    public class AddAzureRmIotDeviceProvisioningServiceLinkedHub : IotDpsBaseCmdlet
    {
        private const string ResourceParameterSet = "ResourceSet";
        private const string InputObjectParameterSet = "InputObjectSet";
        private const string ResourceIdParameterSet = "ResourceIdSet";

        [Parameter(
            Position = 0,
            Mandatory = true,
            ParameterSetName = InputObjectParameterSet,
            ValueFromPipeline = true,
            HelpMessage = "IoT Device Provisioning Service Object")]
        [ValidateNotNullOrEmpty]
        public PSProvisioningServiceDescription DpsObject { get; set; }

        [Parameter(
            Position = 0,
            Mandatory = true,
            ParameterSetName = ResourceIdParameterSet,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "IoT Device Provisioning Service Resource Id")]
        [ValidateNotNullOrEmpty]
        public string ResourceId { get; set; }

        [Parameter(
            Position = 0,
            Mandatory = true,
            ParameterSetName = ResourceParameterSet,
            HelpMessage = "Name of the Resource Group")]
        [ResourceGroupCompleter]
        [ValidateNotNullOrEmpty]
        public string ResourceGroupName { get; set; }

        [Parameter(
            Position = 1,
            Mandatory = true,
            ParameterSetName = ResourceParameterSet,
            HelpMessage = "Name of the IoT Device Provisioning Service")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        [Parameter(
            Position = 1,
            Mandatory = true,
            ParameterSetName = InputObjectParameterSet,
            HelpMessage = "Connection String of the Iot Hub resource.")]
        [Parameter(
            Position = 1,
            Mandatory = true,
            ParameterSetName = ResourceIdParameterSet,
            HelpMessage = "Connection String of the Iot Hub resource.")]
        [Parameter(
            Position = 2,
            Mandatory = true,
            ParameterSetName = ResourceParameterSet,
            HelpMessage = "Connection String of the Iot Hub resource.")]
        [ValidateNotNullOrEmpty]
        public string IotHubConnectionString { get; set; }

        [Parameter(
            Position = 2,
            Mandatory = true,
            ParameterSetName = InputObjectParameterSet,
            HelpMessage = "Location of the Iot Hub")]
        [Parameter(
            Position = 2,
            Mandatory = true,
            ParameterSetName = ResourceIdParameterSet,
            HelpMessage = "Location of the Iot Hub")]
        [Parameter(
            Position = 3,
            Mandatory = true,
            ParameterSetName = ResourceParameterSet,
            HelpMessage = "Location of the Iot Hub")]
        [ValidateNotNullOrEmpty]
        [LocationCompleter("Microsoft.Devices/ProvisioningServices")]
        public string IotHubLocation { get; set; }

        [Parameter(
            Mandatory = false,
            HelpMessage = "Allocation weight of the IoT Hub")]
        [ValidateNotNullOrEmpty]
        public int? AllocationWeight { get; set; }

        [Parameter(
            Mandatory = false,
            HelpMessage = "Apply allocation policy to the IoT Hub")]
        public SwitchParameter ApplyAllocationPolicy { get; set; }

        public override void ExecuteCmdlet()
        {
            if (ShouldProcess(Name, DPSResources.AddLinkedHub))
            {
                switch (ParameterSetName)
                {
                    case InputObjectParameterSet:
                        ResourceGroupName = DpsObject.ResourceGroupName;
                        Name = DpsObject.Name;
                        AddIotDpsLinkedHub();
                        break;

                    case ResourceIdParameterSet:
                        ResourceGroupName = IotDpsUtils.GetResourceGroupName(ResourceId);
                        Name = IotDpsUtils.GetIotDpsName(ResourceId);
                        AddIotDpsLinkedHub();
                        break;

                    case ResourceParameterSet:
                        AddIotDpsLinkedHub();
                        break;

                    default:
                        throw new ArgumentException("BadParameterSetName");
                }
            }
        }

        private void WritePSObject(IotHubDefinitionDescription iotDpsHub)
        {
            WriteObject(IotDpsUtils.ToPSIotHubDefinitionDescription(iotDpsHub, ResourceGroupName, Name), false);
        }

        private void WritePSObjects(IList<IotHubDefinitionDescription> iotDpsHubs)
        {
            WriteObject(IotDpsUtils.ToPSIotHubDefinitionDescription(iotDpsHubs), true);
        }

        private void AddIotDpsLinkedHub()
        {
            IotHubDefinitionDescription iotDpsHub = new IotHubDefinitionDescription()
            {
                ConnectionString = IotHubConnectionString,
                Location = IotHubLocation,
                AllocationWeight = AllocationWeight,
                ApplyAllocationPolicy = ApplyAllocationPolicy.IsPresent
            };

            ProvisioningServiceDescription provisioningServiceDescription = GetIotDpsResource(ResourceGroupName, Name);
            provisioningServiceDescription.Properties.IotHubs.Add(iotDpsHub);
            IotDpsCreateOrUpdate(ResourceGroupName, Name, provisioningServiceDescription);

            IList<IotHubDefinitionDescription> iotDpsHubs = GetIotDpsHubs(ResourceGroupName, Name);
            if (iotDpsHubs.Count == 1)
            {
                WritePSObject(iotDpsHubs[0]);
            }
            else
            {
                WritePSObjects(iotDpsHubs);
            }
        }
    }
}

