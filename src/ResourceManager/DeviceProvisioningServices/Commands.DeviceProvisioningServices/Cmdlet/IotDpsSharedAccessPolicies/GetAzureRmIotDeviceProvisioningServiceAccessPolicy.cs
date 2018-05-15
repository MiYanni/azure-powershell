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

    [Cmdlet(VerbsCommon.Get, "AzureRmIoTDeviceProvisioningServiceAccessPolicy", DefaultParameterSetName = ResourceParameterSet)]
    [Alias("Get-AzureRmIoTDpsAccessPolicy")]
    [OutputType(typeof(PSSharedAccessSignatureAuthorizationRuleAccessRightsDescription), typeof(List<PSSharedAccessSignatureAuthorizationRuleAccessRights>))]
    public class GetAzureRmIoTDeviceProvisioningServiceAccessPolicy : IotDpsBaseCmdlet
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
            HelpMessage = "IoT Device Provisioning Service access policy key name")]
        [ValidateNotNullOrEmpty]
        public string KeyName { get; set; }

        public override void ExecuteCmdlet()
        {
            switch (ParameterSetName)
            {
                case InputObjectParameterSet:
                    ResourceGroupName = DpsObject.ResourceGroupName;
                    Name = DpsObject.Name;
                    GetIotDpsAccessPolicy();
                    break;

                case ResourceIdParameterSet:
                    ResourceGroupName = IotDpsUtils.GetResourceGroupName(ResourceId);
                    Name = IotDpsUtils.GetIotDpsName(ResourceId);
                    GetIotDpsAccessPolicy();
                    break;

                case ResourceParameterSet:
                    GetIotDpsAccessPolicy();
                    break;

                default:
                    throw new ArgumentException("BadParameterSetName");
            }
        }

        private void WritePSObject(SharedAccessSignatureAuthorizationRuleAccessRightsDescription iotDpsAccessPolicy)
        {
            WriteObject(IotDpsUtils.ToPSSharedAccessSignatureAuthorizationRuleAccessRightsDescription(iotDpsAccessPolicy, ResourceGroupName, Name), false);
        }

        private void WritePSObjects(IList<SharedAccessSignatureAuthorizationRuleAccessRightsDescription> iotDpsAccessPolicies)
        {
            WriteObject(IotDpsUtils.ToPSSharedAccessSignatureAuthorizationRuleAccessRightsCollection(iotDpsAccessPolicies), true);
        }

        private void GetIotDpsAccessPolicy()
        {
            if (!string.IsNullOrEmpty(KeyName))
            {
                WritePSObject(GetIotDpsAccessPolicy(ResourceGroupName, Name, KeyName));
            }
            else
            {
                IList<SharedAccessSignatureAuthorizationRuleAccessRightsDescription> iotDpsAccessPolicyList = GetIotDpsAccessPolicy(ResourceGroupName, Name);
                if (iotDpsAccessPolicyList.Count == 1)
                {
                    WritePSObject(iotDpsAccessPolicyList[0]);
                }
                else
                {
                    WritePSObjects(iotDpsAccessPolicyList);
                }
            }
        }
    }
}

