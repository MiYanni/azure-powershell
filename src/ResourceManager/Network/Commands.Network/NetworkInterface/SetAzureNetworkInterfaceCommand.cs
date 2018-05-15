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

using AutoMapper;
using Microsoft.Azure.Commands.Network.Models;
using Microsoft.Azure.Commands.ResourceManager.Common.Tags;
using Microsoft.Azure.Management.Network;
using System;
using System.Management.Automation;
using MNM = Microsoft.Azure.Management.Network.Models;

namespace Microsoft.Azure.Commands.Network
{
    [Cmdlet(VerbsCommon.Set, "AzureRmNetworkInterface"), OutputType(typeof(PSNetworkInterface))]
    public class SetAzureNetworkInterfaceCommand : NetworkInterfaceBaseCmdlet
    {
        [Parameter(
            Mandatory = true,
            ValueFromPipeline = true,
            HelpMessage = "The NetworkInterface")]
        public PSNetworkInterface NetworkInterface { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Run cmdlet in the background")]
        public SwitchParameter AsJob { get; set; }

        public override void Execute()
        {
            base.Execute();

            if (!IsNetworkInterfacePresent(NetworkInterface.ResourceGroupName, NetworkInterface.Name))
            {
                throw new ArgumentException(Properties.Resources.ResourceNotFound);
            }

            // Verify if PublicIpAddress is empty
            foreach (var ipconfig in NetworkInterface.IpConfigurations)
            {
                if (ipconfig.PublicIpAddress != null &&
                    string.IsNullOrEmpty(ipconfig.PublicIpAddress.Id))
                {
                    ipconfig.PublicIpAddress = null;
                }
            }

            // Map to the sdk object
            var networkInterfaceModel = NetworkResourceManagerProfile.Mapper.Map<MNM.NetworkInterface>(NetworkInterface);

			NullifyApplicationSecurityGroupIfAbsent(networkInterfaceModel);

			networkInterfaceModel.Tags = TagsConversionHelper.CreateTagDictionary(NetworkInterface.Tag, validate: true);

            NetworkInterfaceClient.CreateOrUpdate(NetworkInterface.ResourceGroupName, NetworkInterface.Name, networkInterfaceModel);

            var getNetworkInterface = GetNetworkInterface(NetworkInterface.ResourceGroupName, NetworkInterface.Name);
            WriteObject(getNetworkInterface);
        }
    }
}
