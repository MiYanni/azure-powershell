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
using System.Management.Automation;

namespace Microsoft.Azure.Commands.Network
{
    public class AzureApplicationGatewayHttpListenerBase : NetworkBaseCmdlet
    {
        [Parameter(
                Mandatory = true,
                HelpMessage = "The name of the HTTP listener")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        [Parameter(
                ParameterSetName = "SetByResourceId",
                HelpMessage = "ID of the application gateway FrontendIPConfiguration")]
        [ValidateNotNullOrEmpty]
        public string FrontendIPConfigurationId { get; set; }

        [Parameter(
                ParameterSetName = "SetByResource",
                HelpMessage = "Application gateway FrontendIPConfiguration")]
        [ValidateNotNullOrEmpty]
        public PSApplicationGatewayFrontendIPConfiguration FrontendIPConfiguration { get; set; }

        [Parameter(
                ParameterSetName = "SetByResourceId",
                HelpMessage = "ID of the application gateway FrontendPort")]
        [ValidateNotNullOrEmpty]
        public string FrontendPortId { get; set; }

        [Parameter(
                ParameterSetName = "SetByResource",
                HelpMessage = "Application gateway FrontendPort")]
        [ValidateNotNullOrEmpty]
        public PSApplicationGatewayFrontendPort FrontendPort { get; set; }

        [Parameter(
                ParameterSetName = "SetByResourceId",
                HelpMessage = "ID of the application gateway SslCertificate")]
        [ValidateNotNullOrEmpty]
        public string SslCertificateId { get; set; }

        [Parameter(
                ParameterSetName = "SetByResource",
                HelpMessage = "Application gateway SslCertificate")]
        [ValidateNotNullOrEmpty]
        public PSApplicationGatewaySslCertificate SslCertificate { get; set; }

        [Parameter(
               HelpMessage = "Host name")]
        [ValidateNotNullOrEmpty]
        public string HostName { get; set; }

        [Parameter(
               HelpMessage = "RequireServerNameIndication")]
        [ValidateSet("true", "false", IgnoreCase = true)]
        [ValidateNotNullOrEmpty]
        public string RequireServerNameIndication { get; set; }

        [Parameter(
               Mandatory = true,
               HelpMessage = "Protocol")]
        [ValidateSet("Http", "Https", IgnoreCase = true)]
        [ValidateNotNullOrEmpty]
        public string Protocol { get; set; }

        public override void ExecuteCmdlet()
        {
            base.ExecuteCmdlet();

            if (string.Equals(ParameterSetName, Properties.Resources.SetByResource))
            {
                if (FrontendIPConfiguration != null)
                {
                    FrontendIPConfigurationId = FrontendIPConfiguration.Id;
                }
                if (FrontendPort != null)
                {
                    FrontendPortId = FrontendPort.Id;
                }
                if (SslCertificate != null)
                {
                    SslCertificateId = SslCertificate.Id;
                }
            }
        }

        public PSApplicationGatewayHttpListener NewObject()
        {
            var httpListener = new PSApplicationGatewayHttpListener();
            httpListener.Name = Name;
            httpListener.Protocol = Protocol;
            httpListener.HostName = HostName;

            if(string.Equals(RequireServerNameIndication,"true", StringComparison.OrdinalIgnoreCase))
            {
                httpListener.RequireServerNameIndication = true;
            }
            else if(string.Equals(RequireServerNameIndication, "false", StringComparison.OrdinalIgnoreCase))
            {
                httpListener.RequireServerNameIndication = false;
            }

            if (!string.IsNullOrEmpty(FrontendIPConfigurationId))
            {
                httpListener.FrontendIpConfiguration = new PSResourceId();
                httpListener.FrontendIpConfiguration.Id = FrontendIPConfigurationId;
            }

            if (!string.IsNullOrEmpty(FrontendPortId))
            {
                httpListener.FrontendPort = new PSResourceId();
                httpListener.FrontendPort.Id = FrontendPortId;
            }
            if (!string.IsNullOrEmpty(SslCertificateId))
            {
                httpListener.SslCertificate = new PSResourceId();
                httpListener.SslCertificate.Id = SslCertificateId;
            }

            httpListener.Id = ApplicationGatewayChildResourceHelper.GetResourceNotSetId(
                                NetworkClient.NetworkManagementClient.SubscriptionId,
                                Properties.Resources.ApplicationGatewayHttpListenerName,
                                Name);

            return httpListener;
        }
    }
}
