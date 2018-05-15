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
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using Common;
    using Models;
    using Azure.Management.IotHub;
    using Azure.Management.IotHub.Models;
    using ResourceManager.Common.ArgumentCompleters;

    [Cmdlet(VerbsCommon.Get, "AzureRmIotHubConnectionString")]
    [OutputType(typeof(PSSharedAccessSignatureAuthorizationRule), typeof(List<PSSharedAccessSignatureAuthorizationRule>))]
    public class GetAzureRmIotHubConnectionString : IotHubBaseCmdlet
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
            Mandatory = false,
            HelpMessage = "KeyName")]
        public string KeyName { get; set; }

        public override void ExecuteCmdlet()
        {
            // Fetch the hostname
            IotHubDescription iotHubDescription = IotHubClient.IotHubResource.Get(ResourceGroupName, Name);
            var hostName = iotHubDescription.Properties.HostName;

            if (KeyName != null)
            {
                SharedAccessSignatureAuthorizationRule authPolicy = IotHubClient.IotHubResource.GetKeysForKeyName(ResourceGroupName, Name, KeyName);
                WriteObject(authPolicy.ToPSIotHubConnectionString(hostName), false);
            }
            else
            { 
                IEnumerable<SharedAccessSignatureAuthorizationRule> authPolicies = IotHubClient.IotHubResource.ListKeys(ResourceGroupName, Name);
                WriteObject(IotHubUtils.ToPSIotHubConnectionStrings(authPolicies, hostName), true);
            }
        }
    }
}
