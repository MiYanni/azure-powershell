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
    [Cmdlet(VerbsCommon.Set, "AzureRmLoadBalancerRuleConfig"), OutputType(typeof(PSLoadBalancer))]
    public class SetAzureLoadBalancerRuleConfigCommand : AzureLoadBalancerRuleConfigBase
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

            var loadBalancingRule = LoadBalancer.LoadBalancingRules.SingleOrDefault(resource => string.Equals(resource.Name, Name, StringComparison.CurrentCultureIgnoreCase));

            if (loadBalancingRule == null)
            {
                throw new ArgumentException("LoadBalancingRule with the specified name does not exist");
            }

            loadBalancingRule.Name = Name;
            loadBalancingRule.Protocol = Protocol;
            loadBalancingRule.FrontendPort = FrontendPort;
            loadBalancingRule.BackendPort = BackendPort;
            if (IdleTimeoutInMinutes > 0)
            {
                loadBalancingRule.IdleTimeoutInMinutes = IdleTimeoutInMinutes;
            }

            loadBalancingRule.LoadDistribution = string.IsNullOrEmpty(LoadDistribution) ? "Default" : LoadDistribution;

            loadBalancingRule.EnableFloatingIP = EnableFloatingIP.IsPresent;
            loadBalancingRule.DisableOutboundSNAT = DisableOutboundSNAT.IsPresent;

            loadBalancingRule.BackendAddressPool = null;
            if (!string.IsNullOrEmpty(BackendAddressPoolId))
            {
                loadBalancingRule.BackendAddressPool = new PSResourceId();
                loadBalancingRule.BackendAddressPool.Id = BackendAddressPoolId;
            }

            loadBalancingRule.Probe = null;
            if (!string.IsNullOrEmpty(ProbeId))
            {
                loadBalancingRule.Probe = new PSResourceId();
                loadBalancingRule.Probe.Id = ProbeId;
            }

            loadBalancingRule.FrontendIPConfiguration = null;
            if (!string.IsNullOrEmpty(FrontendIpConfigurationId))
            {
                loadBalancingRule.FrontendIPConfiguration = new PSResourceId { Id = FrontendIpConfigurationId };
            }

            WriteObject(LoadBalancer);
        }
    }
}
