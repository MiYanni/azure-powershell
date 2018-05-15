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
using Microsoft.Azure.Commands.ResourceManager.Common.ArgumentCompleters;
using Microsoft.Azure.Commands.ResourceManager.Common.Tags;
using Microsoft.Azure.Management.Network;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using MNM = Microsoft.Azure.Management.Network.Models;

namespace Microsoft.Azure.Commands.Network
{
    [Cmdlet(VerbsCommon.New, "AzureRmLocalNetworkGateway", SupportsShouldProcess = true),
        OutputType(typeof(PSLocalNetworkGateway))]
    public class NewAzureLocalNetworkGatewayCommand : LocalNetworkGatewayBaseCmdlet
    {
        [Alias("ResourceName")]
        [Parameter(
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The resource name.")]
        [ValidateNotNullOrEmpty]
        public virtual string Name { get; set; }

        [Parameter(
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The resource group name.")]
        [ResourceGroupCompleter]
        [ValidateNotNullOrEmpty]
        public virtual string ResourceGroupName { get; set; }

        [Parameter(
         Mandatory = true,
         ValueFromPipelineByPropertyName = true,
         HelpMessage = "location.")]
        [LocationCompleter("Microsoft.Network/localNetworkGateways")]
        [ValidateNotNullOrEmpty]
        public virtual string Location { get; set; }

        [Parameter(
       Mandatory = false,
       ValueFromPipelineByPropertyName = true,
       HelpMessage = "IP address of local network gateway.")]
        public string GatewayIpAddress { get; set; }

        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The address prefixes of the virtual network")]
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

        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "A hashtable which represents resource tags.")]
        public Hashtable Tag { get; set; }

        [Parameter(
            Mandatory = false,
            HelpMessage = "Do not ask for confirmation if you want to overrite a resource")]
        public SwitchParameter Force { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Run cmdlet in the background")]
        public SwitchParameter AsJob { get; set; }

        public override void Execute()
        {
            base.Execute();
            WriteWarning("The output object type of this cmdlet will be modified in a future release.");
            var present = IsLocalNetworkGatewayPresent(ResourceGroupName, Name);
            ConfirmAction(
                Force.IsPresent,
                string.Format(Properties.Resources.OverwritingResource, Name),
                Properties.Resources.CreatingResourceMessage,
                Name,
                () =>
                {
                    var localNetworkGateway = CreateLocalNetworkGateway();
                    WriteObject(localNetworkGateway);
                },
                () => present);
        }

        private PSLocalNetworkGateway CreateLocalNetworkGateway()
        {
            var localnetGateway = new PSLocalNetworkGateway();
            localnetGateway.Name = Name;
            localnetGateway.ResourceGroupName = ResourceGroupName;
            localnetGateway.Location = Location;
            localnetGateway.LocalNetworkAddressSpace = new PSAddressSpace();
            localnetGateway.LocalNetworkAddressSpace.AddressPrefixes = AddressPrefix;
            localnetGateway.GatewayIpAddress = GatewayIpAddress;

            if (PeerWeight < 0)
            {
                throw new PSArgumentException("PeerWeight cannot be negative");
            }

            if (Asn > 0 && !string.IsNullOrEmpty(BgpPeeringAddress))
            {
                localnetGateway.BgpSettings = new PSBgpSettings
                {
                    Asn = Asn,
                    BgpPeeringAddress = BgpPeeringAddress,
                    PeerWeight = PeerWeight
                };
            }
            else if (!string.IsNullOrEmpty(BgpPeeringAddress) && Asn == 0 ||
               string.IsNullOrEmpty(BgpPeeringAddress) && Asn > 0)
            {
                throw new PSArgumentException("For a BGP session to be established over IPsec, the local network gateway's ASN and BgpPeeringAddress must both be specified.");
            }

            // Map to the sdk object
            var localnetGatewayModel = NetworkResourceManagerProfile.Mapper.Map<MNM.LocalNetworkGateway>(localnetGateway);
            localnetGatewayModel.Tags = TagsConversionHelper.CreateTagDictionary(Tag, validate: true);

            // Execute the Create Local Network Gateway call
            LocalNetworkGatewayClient.CreateOrUpdate(ResourceGroupName, Name, localnetGatewayModel);

            var getLocalNetworkGateway = GetLocalNetworkGateway(ResourceGroupName, Name);

            return getLocalNetworkGateway;
        }
    }
}
