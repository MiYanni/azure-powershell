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
using System.Collections.Generic;
using System.Management.Automation;
using MNM = Microsoft.Azure.Management.Network.Models;

namespace Microsoft.Azure.Commands.Network
{
    [Cmdlet(VerbsCommon.Set, "AzureRmLocalNetworkGateway"), OutputType(typeof(PSLocalNetworkGateway))]
    public class SetAzureLocalNetworkGatewayCommand : LocalNetworkGatewayBaseCmdlet
    {
        [Parameter(
            Mandatory = true,
            ValueFromPipeline = true,
            HelpMessage = "The LocalNetworkGateway")]
        public PSLocalNetworkGateway LocalNetworkGateway { get; set; }

        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The address prefixes of the local network to be changed.")]
        [ValidateNotNullOrEmpty]
        public List<string> AddressPrefix { get; set; }

        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The local network gateway's ASN")]
        public uint Asn { get; set; }

        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The IP address of the local network gateway's BGP speaker")]
        public string BgpPeeringAddress { get; set; }

        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Weight added to BGP routes learned from this local network gateway")]
        public int PeerWeight { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Run cmdlet in the background")]
        public SwitchParameter AsJob { get; set; }

        public override void Execute()
        {

            base.Execute();
            if (!IsLocalNetworkGatewayPresent(LocalNetworkGateway.ResourceGroupName, LocalNetworkGateway.Name))
            {
                throw new ArgumentException(Properties.Resources.ResourceNotFound);
            }

            if (AddressPrefix != null && AddressPrefix.Count > 0)
            {
                LocalNetworkGateway.LocalNetworkAddressSpace.AddressPrefixes = AddressPrefix;
            }

            if ((Asn > 0 || !string.IsNullOrEmpty(BgpPeeringAddress) || PeerWeight > 0) && LocalNetworkGateway.BgpSettings == null)
            {
                LocalNetworkGateway.BgpSettings = new PSBgpSettings();
            }

            if (Asn > 0)
            {
                LocalNetworkGateway.BgpSettings.Asn = Asn;
            }

            if (!string.IsNullOrEmpty(BgpPeeringAddress))
            {
                LocalNetworkGateway.BgpSettings.BgpPeeringAddress = BgpPeeringAddress;
            }

            if (PeerWeight > 0)
            {
                LocalNetworkGateway.BgpSettings.PeerWeight = PeerWeight;
            }
            else if (PeerWeight < 0)
            {
                throw new PSArgumentException("PeerWeight cannot be negative");
            }

            // Map to the sdk object
            var localnetGatewayModel = NetworkResourceManagerProfile.Mapper.Map<MNM.LocalNetworkGateway>(LocalNetworkGateway);
            localnetGatewayModel.Tags = TagsConversionHelper.CreateTagDictionary(LocalNetworkGateway.Tag, validate: true);

            LocalNetworkGatewayClient.CreateOrUpdate(LocalNetworkGateway.ResourceGroupName, LocalNetworkGateway.Name, localnetGatewayModel);

            var getlocalnetGateway = GetLocalNetworkGateway(LocalNetworkGateway.ResourceGroupName, LocalNetworkGateway.Name);

            WriteObject(getlocalnetGateway);
        }
    }
}
