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
    using System.Management.Automation;
    using Models;
    using ResourceManager.Common.ArgumentCompleters;
    using Azure.Management.DeviceProvisioningServices.Models;

    [Cmdlet(VerbsCommon.Get, "AzureRmIoTDeviceProvisioningServiceLinkedHub", DefaultParameterSetName = ResourceParameterSet)]
    [Alias("Get-AzureRmIoTDpsHub")]
    [OutputType(typeof(PSIotHubDefinitionDescription), typeof(List<PSIotHubDefinitions>))]
    public class GetAzureRmIoTDeviceProvisioningServiceLinkedHub : IotDpsBaseCmdlet
    {
        private const string ResourceIdParameterSet = "ResourceIdSet";
        private const string ResourceParameterSet = "ResourceSet";
        private const string InputObjectParameterSet = "InputObjectSet";

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
            Mandatory = false,
            HelpMessage = "Host name of linked IoT Hub")]
        [ValidateNotNullOrEmpty]
        public string LinkedHubName { get; set; }

        public override void ExecuteCmdlet()
        {
            switch (ParameterSetName)
            {
                case InputObjectParameterSet:
                    ResourceGroupName = DpsObject.ResourceGroupName;
                    Name = DpsObject.Name;
                    GetIotDpsHubs();
                    break;

                case ResourceIdParameterSet:
                    ResourceGroupName = IotDpsUtils.GetResourceGroupName(ResourceId);
                    Name = IotDpsUtils.GetIotDpsName(ResourceId);
                    GetIotDpsHubs();
                    break;

                case ResourceParameterSet:
                    GetIotDpsHubs();
                    break;

                default:
                    throw new ArgumentException("BadParameterSetName");
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

        private void GetIotDpsHubs()
        {
            if (!string.IsNullOrEmpty(LinkedHubName))
            {
                WritePSObject(GetIotDpsHubs(ResourceGroupName, Name, LinkedHubName));
            }
            else
            {
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
}

