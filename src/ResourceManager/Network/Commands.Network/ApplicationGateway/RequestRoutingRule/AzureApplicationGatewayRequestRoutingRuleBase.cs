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
using System.Management.Automation;

namespace Microsoft.Azure.Commands.Network
{
    public class AzureApplicationGatewayRequestRoutingRuleBase : NetworkBaseCmdlet
    {
        [Parameter(
                Mandatory = true,
                HelpMessage = "The name of the Request Routing Rule")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        [Parameter(
        Mandatory = true,
        HelpMessage = "The type of rule")]
        [ValidateSet("Basic", "PathBasedRouting", IgnoreCase = true)]
        [ValidateNotNullOrEmpty]
        public string RuleType { get; set; }

        [Parameter(
                ParameterSetName = "SetByResourceId",
                HelpMessage = "ID of the application gateway BackendHttpSettings")]
        [ValidateNotNullOrEmpty]
        public string BackendHttpSettingsId { get; set; }

        [Parameter(
                ParameterSetName = "SetByResource",
                HelpMessage = "Application gateway BackendHttpSettings")]
        [ValidateNotNullOrEmpty]
        public PSApplicationGatewayBackendHttpSettings BackendHttpSettings { get; set; }

        [Parameter(
                ParameterSetName = "SetByResourceId",
                HelpMessage = "ID of the application gateway HttpListener")]
        [ValidateNotNullOrEmpty]
        public string HttpListenerId { get; set; }

        [Parameter(
                ParameterSetName = "SetByResource",
                HelpMessage = "Application gateway HttpListener")]
        [ValidateNotNullOrEmpty]
        public PSApplicationGatewayHttpListener HttpListener { get; set; }

        [Parameter(
                ParameterSetName = "SetByResourceId",
                HelpMessage = "ID of the application gateway BackendAddressPool")]
        [ValidateNotNullOrEmpty]
        public string BackendAddressPoolId { get; set; }

        [Parameter(
                ParameterSetName = "SetByResource",
                HelpMessage = "Application gateway BackendAddressPool")]
        [ValidateNotNullOrEmpty]
        public PSApplicationGatewayBackendAddressPool BackendAddressPool { get; set; }

        [Parameter(
                ParameterSetName = "SetByResourceId",
                HelpMessage = "ID of the application gateway UrlPathMap")]
        [ValidateNotNullOrEmpty]
        public string UrlPathMapId { get; set; }

        [Parameter(
                ParameterSetName = "SetByResource",
                HelpMessage = "Application gateway UrlPathMap")]
        [ValidateNotNullOrEmpty]
        public PSApplicationGatewayUrlPathMap UrlPathMap { get; set; }

        [Parameter(
                ParameterSetName = "SetByResourceId",
                HelpMessage = "ID of the application gateway RedirectConfiguration")]
        [ValidateNotNullOrEmpty]
        public string RedirectConfigurationId { get; set; }

        [Parameter(
                ParameterSetName = "SetByResource",
                HelpMessage = "Application gateway RedirectConfiguration")]
        [ValidateNotNullOrEmpty]
        public PSApplicationGatewayRedirectConfiguration RedirectConfiguration { get; set; }

        public override void ExecuteCmdlet()
        {
            base.ExecuteCmdlet();

            if (string.Equals(ParameterSetName, Properties.Resources.SetByResource))
            {
                if (BackendHttpSettings != null)
                {
                    BackendHttpSettingsId = BackendHttpSettings.Id;
                }
                if (BackendAddressPool != null)
                {
                    BackendAddressPoolId = BackendAddressPool.Id;
                }
                if (HttpListener != null)
                {
                    HttpListenerId = HttpListener.Id;
                }
                if (UrlPathMap != null)
                {
                    UrlPathMapId = UrlPathMap.Id;
                }
                if (RedirectConfiguration != null)
                {
                    RedirectConfigurationId = RedirectConfiguration.Id;
                }
            }
        }

        public PSApplicationGatewayRequestRoutingRule NewObject()
        {
            var requestRoutingRule = new PSApplicationGatewayRequestRoutingRule();
            requestRoutingRule.Name = Name;
            requestRoutingRule.RuleType = RuleType;

            if (!string.IsNullOrEmpty(BackendHttpSettingsId))
            {
                requestRoutingRule.BackendHttpSettings = new PSResourceId();
                requestRoutingRule.BackendHttpSettings.Id = BackendHttpSettingsId;
            }

            if (!string.IsNullOrEmpty(HttpListenerId))
            {
                requestRoutingRule.HttpListener = new PSResourceId();
                requestRoutingRule.HttpListener.Id = HttpListenerId;
            }
            if (!string.IsNullOrEmpty(BackendAddressPoolId))
            {
                requestRoutingRule.BackendAddressPool = new PSResourceId();
                requestRoutingRule.BackendAddressPool.Id = BackendAddressPoolId;
            }
            if (!string.IsNullOrEmpty(UrlPathMapId))
            {
                requestRoutingRule.UrlPathMap = new PSResourceId();
                requestRoutingRule.UrlPathMap.Id = UrlPathMapId;
            }
            if (!string.IsNullOrEmpty(RedirectConfigurationId))
            {
                requestRoutingRule.RedirectConfiguration = new PSResourceId();
                requestRoutingRule.RedirectConfiguration.Id = RedirectConfigurationId;
            }

            requestRoutingRule.Id = ApplicationGatewayChildResourceHelper.GetResourceNotSetId(
                                NetworkClient.NetworkManagementClient.SubscriptionId,
                                Properties.Resources.ApplicationGatewayRequestRoutingRuleName,
                                Name);

            return requestRoutingRule;
        }
    }
}
