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
    using Azure.Management.DeviceProvisioningServices;
    using Azure.Management.DeviceProvisioningServices.Models;

    [Cmdlet(VerbsCommon.Get, "AzureRmIoTDeviceProvisioningService", DefaultParameterSetName = ListIotDpsByRGParameterSet)]
    [Alias("Get-AzureRmIoTDps")]
    [OutputType(typeof(PSProvisioningServiceDescription), typeof(List<PSProvisioningServicesDescription>))]
    public class GetAzureRmIoTDeviceProvisioningService : IotDpsBaseCmdlet
    {
        private const string GetIotDpsParameterSet = "GetIotDpsByName";
        private const string ListIotDpsByRGParameterSet = "ListIotDpsByResourceGroup";

        [Parameter(
            Mandatory = false,
            ParameterSetName = ListIotDpsByRGParameterSet,
            HelpMessage = "Name of the Resource Group")]
        [Parameter(
            Position = 0,
            Mandatory = true,
            ParameterSetName = GetIotDpsParameterSet,
            HelpMessage = "Name of the Resource Group")]
        [ResourceGroupCompleter]
        [ValidateNotNullOrEmpty]
        public string ResourceGroupName { get; set; }

        [Parameter(
            Position = 1,
            Mandatory = true,
            ParameterSetName = GetIotDpsParameterSet,
            HelpMessage = "Name of the IoT Device Provisioning Service")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        public override void ExecuteCmdlet()
        {
            switch(ParameterSetName)
            {
                case GetIotDpsParameterSet:
                    GetIotDps();
                    break;

                case ListIotDpsByRGParameterSet:
                    if (string.IsNullOrEmpty(ResourceGroupName))
                    {
                        IEnumerable<ProvisioningServiceDescription> iotprovisioningServiceDescriptionsBySubscription = IotDpsClient.IotDpsResource.ListBySubscription();
                        GetIotDpsCollection(iotprovisioningServiceDescriptionsBySubscription);
                    }
                    else
                    {
                        IEnumerable<ProvisioningServiceDescription> provisioningServiceDescriptions = IotDpsClient.IotDpsResource.ListByResourceGroup(ResourceGroupName);
                        GetIotDpsCollection(provisioningServiceDescriptions);
                    }
                    break;
                
                default:
                    throw new ArgumentException("BadParameterSetName");
            }
        }

        private void WritePSObject(ProvisioningServiceDescription provisioningServiceDescription)
        {
            WriteObject(IotDpsUtils.ToPSProvisioningServiceDescription(provisioningServiceDescription), false);
        }

        private void WritePSObjects(IEnumerable<ProvisioningServiceDescription> provisioningServicesDescription)
        {
            WriteObject(IotDpsUtils.ToPSProvisioningServicesDescription(provisioningServicesDescription), true);
        }

        private void GetIotDps()
        {
            WritePSObject(GetIotDpsResource(ResourceGroupName, Name));
        }

        private void GetIotDpsCollection(IEnumerable<ProvisioningServiceDescription> provisioningServicesDescription)
        {
            List<ProvisioningServiceDescription> iotDpsList = new List<ProvisioningServiceDescription>(provisioningServicesDescription);
            if (iotDpsList.Count == 1)
            {
                WritePSObject(iotDpsList[0]);
            }
            else
            {
                WritePSObjects(provisioningServicesDescription);
            }
        }
    }
}
