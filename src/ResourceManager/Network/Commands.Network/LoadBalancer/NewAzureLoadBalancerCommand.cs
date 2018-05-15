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
    [Cmdlet(VerbsCommon.New, "AzureRmLoadBalancer", SupportsShouldProcess = true),
        OutputType(typeof(PSLoadBalancer))]
    public class NewAzureLoadBalancerCommand : LoadBalancerBaseCmdlet
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
        [LocationCompleter("Microsoft.Network/loadBalancers")]
        [ValidateNotNullOrEmpty]
        public virtual string Location { get; set; }

        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The load balancer Sku name.")]
        [ValidateNotNullOrEmpty]
        [ValidateSet(
            MNM.LoadBalancerSkuName.Basic,
            MNM.LoadBalancerSkuName.Standard,
            IgnoreCase = true)]
        public string Sku { get; set; }

        [Parameter(
             Mandatory = false,
             ValueFromPipelineByPropertyName = true,
             HelpMessage = "The list of frontend Ip config")]
        [ValidateNotNullOrEmpty]
        public List<PSFrontendIPConfiguration> FrontendIpConfiguration { get; set; }

        [Parameter(
             Mandatory = false,
             ValueFromPipelineByPropertyName = true,
             HelpMessage = "The list of backend address pool")]
        public List<PSBackendAddressPool> BackendAddressPool { get; set; }

        [Parameter(
             Mandatory = false,
             ValueFromPipelineByPropertyName = true,
             HelpMessage = "The list of probe")]
        public List<PSProbe> Probe { get; set; }

        [Parameter(
             Mandatory = false,
             ValueFromPipelineByPropertyName = true,
             HelpMessage = "The list of inbound NAT rule")]
        public List<PSInboundNatRule> InboundNatRule { get; set; }

        [Parameter(
             Mandatory = false,
             ValueFromPipelineByPropertyName = true,
             HelpMessage = "The list of load balancing rule")]
        public List<PSLoadBalancingRule> LoadBalancingRule { get; set; }

        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "A hashtable which represents resource tags.")]
        public Hashtable Tag { get; set; }

        [Parameter(
             Mandatory = false,
             ValueFromPipelineByPropertyName = true,
             HelpMessage = "The list of inbound NAT pools")]
        public List<PSInboundNatPool> InboundNatPool { get; set; }

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
            var present = IsLoadBalancerPresent(ResourceGroupName, Name);
            ConfirmAction(
                Force.IsPresent,
                string.Format(Properties.Resources.OverwritingResource, Name),
                Properties.Resources.CreatingResourceMessage,
                Name,
                () =>
                {
                    var loadBalancer = CreateLoadBalancer();
                    WriteObject(loadBalancer);
                },
                () => present);
        }

        private PSLoadBalancer CreateLoadBalancer()
        {
            var loadBalancer = new PSLoadBalancer();
            loadBalancer.Name = Name;
            loadBalancer.ResourceGroupName = ResourceGroupName;
            loadBalancer.Location = Location;

            if (!string.IsNullOrEmpty(Sku))
            {
                loadBalancer.Sku = new PSLoadBalancerSku();
                loadBalancer.Sku.Name = Sku;
            }

            if (FrontendIpConfiguration != null)
            {
                loadBalancer.FrontendIpConfigurations = FrontendIpConfiguration;
            }

            if (BackendAddressPool != null)
            {
                loadBalancer.BackendAddressPools = BackendAddressPool;
            }

            if (Probe != null)
            {
                loadBalancer.Probes = Probe;
            }

            if (InboundNatRule != null)
            {
                loadBalancer.InboundNatRules = InboundNatRule;
            }

            if (LoadBalancingRule != null)
            {
                loadBalancer.LoadBalancingRules = LoadBalancingRule;
            }

            if (InboundNatPool != null)
            {
                loadBalancer.InboundNatPools = InboundNatPool;
            }

            loadBalancer.ResourceGroupName = ResourceGroupName;
            loadBalancer.Name = Name;

            // Normalize the IDs
            ChildResourceHelper.NormalizeChildResourcesId(loadBalancer, NetworkClient.NetworkManagementClient.SubscriptionId);

            // Map to the sdk object
            var lbModel = NetworkResourceManagerProfile.Mapper.Map<MNM.LoadBalancer>(loadBalancer);
            lbModel.Tags = TagsConversionHelper.CreateTagDictionary(Tag, validate: true);

            // Execute the Create VirtualNetwork call
            LoadBalancerClient.CreateOrUpdate(ResourceGroupName, Name, lbModel);

            var getLoadBalancer = GetLoadBalancer(ResourceGroupName, Name);

            return getLoadBalancer;
        }
    }
}
