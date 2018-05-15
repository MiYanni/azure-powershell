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
using System.Linq;
using System.Management.Automation;

namespace Microsoft.Azure.Commands.Network
{
    [Cmdlet(VerbsCommon.Add, "AzureRmLoadBalancerRuleConfig"), OutputType(typeof(PSLoadBalancer))]
    public class AddAzureLoadBalancerRuleConfigCommand : AzureLoadBalancerRuleConfigBase
    {
        [Parameter(
            Mandatory = true,
            HelpMessage = "The name of the load balancer rule")]
        [ValidateNotNullOrEmpty]
        public override string Name { get; set; }

        [Parameter(
            Mandatory = true,
            ValueFromPipeline = true,
            HelpMessage = "The load balancer")]
        public PSLoadBalancer LoadBalancer { get; set; }

        public override void Execute()
        {

            base.Execute();
            var existingLoadBalancingRule = LoadBalancer.LoadBalancingRules.SingleOrDefault(resource => string.Equals(resource.Name, Name, StringComparison.CurrentCultureIgnoreCase));

            if (existingLoadBalancingRule != null)
            {
                throw new ArgumentException("LoadBalancingRule with the specified name already exists");
            }

            var loadBalancingRule = new PSLoadBalancingRule();
            loadBalancingRule.Name = Name;
            loadBalancingRule.Protocol = Protocol;
            loadBalancingRule.FrontendPort = FrontendPort;
            loadBalancingRule.BackendPort = BackendPort;

            if (IdleTimeoutInMinutes > 0)
            {
                loadBalancingRule.IdleTimeoutInMinutes = IdleTimeoutInMinutes;
            }

            if (!string.IsNullOrEmpty(LoadDistribution))
            {
                loadBalancingRule.LoadDistribution = LoadDistribution;
            }

            loadBalancingRule.EnableFloatingIP = EnableFloatingIP.IsPresent;
            loadBalancingRule.DisableOutboundSNAT = DisableOutboundSNAT.IsPresent;

            if (!string.IsNullOrEmpty(BackendAddressPoolId))
            {
                loadBalancingRule.BackendAddressPool = new PSResourceId();
                loadBalancingRule.BackendAddressPool.Id = BackendAddressPoolId;
            }

            if (!string.IsNullOrEmpty(ProbeId))
            {
                loadBalancingRule.Probe = new PSResourceId();
                loadBalancingRule.Probe.Id = ProbeId;
            }

            if (!string.IsNullOrEmpty(FrontendIpConfigurationId))
            {
                loadBalancingRule.FrontendIPConfiguration = new PSResourceId { Id = FrontendIpConfigurationId };
            }

            loadBalancingRule.Id =
                ChildResourceHelper.GetResourceId(
                    NetworkClient.NetworkManagementClient.SubscriptionId,
                    LoadBalancer.ResourceGroupName,
                    LoadBalancer.Name,
                    Properties.Resources.LoadBalancerRuleName,
                    Name);

            LoadBalancer.LoadBalancingRules.Add(loadBalancingRule);

            WriteObject(LoadBalancer);
        }
    }
}
