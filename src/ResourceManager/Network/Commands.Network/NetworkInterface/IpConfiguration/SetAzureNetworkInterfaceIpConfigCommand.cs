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

using Microsoft.Azure.Commands.Network.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace Microsoft.Azure.Commands.Network
{
    [Cmdlet(VerbsCommon.Set, "AzureRmNetworkInterfaceIpConfig", DefaultParameterSetName = "SetByResource"), OutputType(typeof(PSNetworkInterface))]
    public class SetAzureNetworkInterfaceIpConfigCommand : AzureNetworkInterfaceIpConfigBase
    {
        [Parameter(
            Mandatory = true,
            HelpMessage = "The name of the IpConfiguration")]
        [ValidateNotNullOrEmpty]
        public override string Name { get; set; }

        [Parameter(
            Mandatory = true,
            ValueFromPipeline = true,
            HelpMessage = "The Network Interface")]
        public PSNetworkInterface NetworkInterface { get; set; }

        public override void Execute()
        {
            base.Execute();

            var ipconfig = NetworkInterface.IpConfigurations.SingleOrDefault(resource => string.Equals(resource.Name, Name, StringComparison.CurrentCultureIgnoreCase));

            if (ipconfig == null)
            {
                throw new ArgumentException("IpConfiguration with the specified name does not exist");
            }


            if (string.Equals(ParameterSetName, Properties.Resources.SetByResource))
            {
                if (Subnet != null)
                {
                    SubnetId = Subnet.Id;
                }

                if (PublicIpAddress != null)
                {
                    PublicIpAddressId = PublicIpAddress.Id;
                }

                if (LoadBalancerBackendAddressPool != null)
                {
                    LoadBalancerBackendAddressPoolId = new List<string>();
                    foreach (var bepool in LoadBalancerBackendAddressPool)
                    {
                        LoadBalancerBackendAddressPoolId.Add(bepool.Id);
                    }
                }

                if (LoadBalancerInboundNatRule != null)
                {
                    LoadBalancerInboundNatRuleId = new List<string>();
                    foreach (var natRule in LoadBalancerInboundNatRule)
                    {
                        LoadBalancerInboundNatRuleId.Add(natRule.Id);
                    }
                }

                if (ApplicationGatewayBackendAddressPool != null)
                {
                    ApplicationGatewayBackendAddressPoolId = new List<string>();
                    foreach (var appgwBepool in ApplicationGatewayBackendAddressPool)
                    {
                        ApplicationGatewayBackendAddressPoolId.Add(appgwBepool.Id);
                    }
                }

                if (ApplicationSecurityGroup != null)
                {
                    ApplicationSecurityGroupId = new List<string>();
                    foreach (var asg in ApplicationSecurityGroup)
                    {
                        ApplicationSecurityGroupId.Add(asg.Id);
                    }
                }
            }

            ipconfig.Subnet = null;
            ipconfig.PublicIpAddress = null;
            ipconfig.LoadBalancerBackendAddressPools = null;
            ipconfig.LoadBalancerInboundNatRules = null;
            ipconfig.ApplicationGatewayBackendAddressPools = null;

            if (!string.IsNullOrEmpty(SubnetId))
            {
                ipconfig.Subnet = new PSSubnet();
                ipconfig.Subnet.Id = SubnetId;

                if (!string.IsNullOrEmpty(PrivateIpAddress))
                {
                    ipconfig.PrivateIpAddress = PrivateIpAddress;
                    ipconfig.PrivateIpAllocationMethod = Management.Network.Models.IPAllocationMethod.Static;
                }
                else
                {
                    ipconfig.PrivateIpAllocationMethod = Management.Network.Models.IPAllocationMethod.Dynamic;
                }
            }

            if (!string.IsNullOrEmpty(PublicIpAddressId))
            {
                ipconfig.PublicIpAddress = new PSPublicIpAddress();
                ipconfig.PublicIpAddress.Id = PublicIpAddressId;
            }

            if (LoadBalancerBackendAddressPoolId != null)
            {
                ipconfig.LoadBalancerBackendAddressPools = new List<PSBackendAddressPool>();
                foreach (var bepoolId in LoadBalancerBackendAddressPoolId)
                {
                    ipconfig.LoadBalancerBackendAddressPools.Add(new PSBackendAddressPool { Id = bepoolId });
                }
            }

            if (LoadBalancerInboundNatRuleId != null)
            {
                ipconfig.LoadBalancerInboundNatRules = new List<PSInboundNatRule>();
                foreach (var natruleId in LoadBalancerInboundNatRuleId)
                {
                    ipconfig.LoadBalancerInboundNatRules.Add(new PSInboundNatRule { Id = natruleId });
                }
            }

            if (ApplicationGatewayBackendAddressPoolId != null)
            {
                ipconfig.ApplicationGatewayBackendAddressPools = new List<PSApplicationGatewayBackendAddressPool>();
                foreach (var appgwBepoolId in ApplicationGatewayBackendAddressPoolId)
                {
                    ipconfig.ApplicationGatewayBackendAddressPools.Add(new PSApplicationGatewayBackendAddressPool { Id = appgwBepoolId });
                }
            }

            if (ApplicationSecurityGroupId != null)
            {
                ipconfig.ApplicationSecurityGroups = new List<PSApplicationSecurityGroup>();
                foreach (var asgId in ApplicationSecurityGroupId)
                {
                    ipconfig.ApplicationSecurityGroups.Add(new PSApplicationSecurityGroup { Id = asgId });
                }
            }

            ipconfig.PrivateIpAddressVersion = PrivateIpAddressVersion;
            ipconfig.Primary = Primary.IsPresent;
            WriteObject(NetworkInterface);
        }
    }
}
