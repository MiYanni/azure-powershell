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
    [Cmdlet(VerbsCommon.New, "AzureRmApplicationGateway", SupportsShouldProcess = true), 
        OutputType(typeof(PSApplicationGateway))]
    public class NewAzureApplicationGatewayCommand : ApplicationGatewayBaseCmdlet
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
        [LocationCompleter("Microsoft.Network/applicationGateways")]
        [ValidateNotNullOrEmpty]
        public virtual string Location { get; set; }

        [Parameter(
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The SKU of application gateway")]
        [ValidateNotNullOrEmpty]
        public virtual PSApplicationGatewaySku Sku { get; set; }

        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The SSL policy of application gateway")]
        public virtual PSApplicationGatewaySslPolicy SslPolicy { get; set; }

        [Parameter(
             Mandatory = true,
             ValueFromPipelineByPropertyName = true,
             HelpMessage = "The list of IPConfiguration (subnet)")]
        [ValidateNotNullOrEmpty]
        public List<PSApplicationGatewayIPConfiguration> GatewayIPConfigurations { get; set; }

        [Parameter(
             Mandatory = false,
             ValueFromPipelineByPropertyName = true,
             HelpMessage = "The list of ssl certificates")]
        public List<PSApplicationGatewaySslCertificate> SslCertificates { get; set; }

        [Parameter(
             Mandatory = false,
             ValueFromPipelineByPropertyName = true,
             HelpMessage = "The list of authentication certificates")]
        public List<PSApplicationGatewayAuthenticationCertificate> AuthenticationCertificates { get; set; }

        [Parameter(
             Mandatory = false,
             ValueFromPipelineByPropertyName = true,
             HelpMessage = "The list of frontend IP config")]
        public List<PSApplicationGatewayFrontendIPConfiguration> FrontendIPConfigurations { get; set; }

        [Parameter(
             Mandatory = true,
             ValueFromPipelineByPropertyName = true,
             HelpMessage = "The list of frontend port")]
        public List<PSApplicationGatewayFrontendPort> FrontendPorts { get; set; }

        [Parameter(
             Mandatory = false,
             ValueFromPipelineByPropertyName = true,
             HelpMessage = "The list of probe")]
        public List<PSApplicationGatewayProbe> Probes { get; set; }

        [Parameter(
             Mandatory = true,
             ValueFromPipelineByPropertyName = true,
             HelpMessage = "The list of backend address pool")]
        public List<PSApplicationGatewayBackendAddressPool> BackendAddressPools { get; set; }

        [Parameter(
             Mandatory = true,
             ValueFromPipelineByPropertyName = true,
             HelpMessage = "The list of backend http settings")]
        public List<PSApplicationGatewayBackendHttpSettings> BackendHttpSettingsCollection { get; set; }

        [Parameter(
             Mandatory = true,
             ValueFromPipelineByPropertyName = true,
             HelpMessage = "The list of http listener")]
        public List<PSApplicationGatewayHttpListener> HttpListeners { get; set; }

        [Parameter(
             Mandatory = false,
             ValueFromPipelineByPropertyName = true,
             HelpMessage = "The list of UrlPathMap")]
        public List<PSApplicationGatewayUrlPathMap> UrlPathMaps { get; set; }

        [Parameter(
             Mandatory = true,
             ValueFromPipelineByPropertyName = true,
             HelpMessage = "The list of request routing rule")]
        public List<PSApplicationGatewayRequestRoutingRule> RequestRoutingRules { get; set; }

        [Parameter(
             Mandatory = false,
             ValueFromPipelineByPropertyName = true,
             HelpMessage = "The list of redirect configuration")]
        public List<PSApplicationGatewayRedirectConfiguration> RedirectConfigurations { get; set; }

        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Firewall configuration")]
        public virtual PSApplicationGatewayWebApplicationFirewallConfiguration WebApplicationFirewallConfiguration { get; set; }

        [Parameter(
            Mandatory = false,
            HelpMessage = " Whether HTTP2 is enabled.")]
        public SwitchParameter EnableHttp2 { get; set; }

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

        public override void ExecuteCmdlet()
        {
            base.ExecuteCmdlet();

            WriteWarning("The output object type of this cmdlet will be modified in a future release.");
            var present = IsApplicationGatewayPresent(ResourceGroupName, Name);
            ConfirmAction(
                Force.IsPresent,
                string.Format(Properties.Resources.OverwritingResource, Name),
                Properties.Resources.OverwritingResourceMessage,
                Name,
                () =>
                {
                    var applicationGateway = CreateApplicationGateway();
                    WriteObject(applicationGateway);
                },
                () => present);
        }

        private PSApplicationGateway CreateApplicationGateway()
        {
            var applicationGateway = new PSApplicationGateway();
            applicationGateway.Name = Name;
            applicationGateway.ResourceGroupName = ResourceGroupName;
            applicationGateway.Location = Location;
            applicationGateway.Sku = Sku;

            if (SslPolicy != null)
            {
                applicationGateway.SslPolicy = new PSApplicationGatewaySslPolicy();
                applicationGateway.SslPolicy = SslPolicy;
            }

            if (GatewayIPConfigurations != null)
            {
                applicationGateway.GatewayIPConfigurations = GatewayIPConfigurations;
            }

            if (SslCertificates != null)
            {
                applicationGateway.SslCertificates = SslCertificates;
            }

            if (AuthenticationCertificates != null)
            {
                applicationGateway.AuthenticationCertificates = AuthenticationCertificates;
            }

            if (FrontendIPConfigurations != null)
            {
                applicationGateway.FrontendIPConfigurations = FrontendIPConfigurations;
            }

            if (FrontendPorts != null)
            {
                applicationGateway.FrontendPorts = FrontendPorts;
            }

            if (Probes != null)
            {
                applicationGateway.Probes = Probes;
            }

            if (BackendAddressPools != null)
            {
                applicationGateway.BackendAddressPools = BackendAddressPools;
            }

            if (BackendHttpSettingsCollection != null)
            {
                applicationGateway.BackendHttpSettingsCollection = BackendHttpSettingsCollection;
            }

            if (HttpListeners != null)
            {
                applicationGateway.HttpListeners = HttpListeners;
            }

            if (UrlPathMaps != null)
            {
                applicationGateway.UrlPathMaps = UrlPathMaps;
            }

            if (RequestRoutingRules != null)
            {
                applicationGateway.RequestRoutingRules = RequestRoutingRules;
            }

            if (RedirectConfigurations != null)
            {
                applicationGateway.RedirectConfigurations = RedirectConfigurations;
            }

            if (WebApplicationFirewallConfiguration != null)
            {
                applicationGateway.WebApplicationFirewallConfiguration = WebApplicationFirewallConfiguration;
            }

            if (EnableHttp2.IsPresent)
            {
                applicationGateway.EnableHttp2 = true;
            }

            // Normalize the IDs
            ApplicationGatewayChildResourceHelper.NormalizeChildResourcesId(applicationGateway);

            // Map to the sdk object
            var appGwModel = NetworkResourceManagerProfile.Mapper.Map<MNM.ApplicationGateway>(applicationGateway);
            appGwModel.Tags = TagsConversionHelper.CreateTagDictionary(Tag, validate: true);

            // Execute the Create ApplicationGateway call
            ApplicationGatewayClient.CreateOrUpdate(ResourceGroupName, Name, appGwModel);

            var getApplicationGateway = GetApplicationGateway(ResourceGroupName, Name);

            return getApplicationGateway;
        }
    }
}
