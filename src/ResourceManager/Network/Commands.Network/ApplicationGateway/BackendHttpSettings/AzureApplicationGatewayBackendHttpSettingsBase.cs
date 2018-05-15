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

using System;
using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.Azure.Commands.Network.Models;

namespace Microsoft.Azure.Commands.Network
{
    public class AzureApplicationGatewayBackendHttpSettingsBase : NetworkBaseCmdlet
    {
        [Parameter(
               Mandatory = true,
               HelpMessage = "The name of the backend http settings")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        [Parameter(
               Mandatory = true,
               HelpMessage = "Port")]
        [ValidateNotNullOrEmpty]
        public int Port { get; set; }

        [Parameter(
               Mandatory = true,
               HelpMessage = "Protocol")]
        [ValidateSet("Http", "Https", IgnoreCase = true)]
        [ValidateNotNullOrEmpty]
        public string Protocol { get; set; }

        [Parameter(
               Mandatory = true,
               HelpMessage = "Cookie Based Affinity")]
        [ValidateSet("Enabled", "Disabled", IgnoreCase = true)]
        [ValidateNotNullOrEmpty]
        public string CookieBasedAffinity { get; set; }

        [Parameter(
            Mandatory = false,
            HelpMessage = "Request Timeout. Default value 30 seconds.")]
        [ValidateNotNullOrEmpty]
        public int RequestTimeout { get; set; }


        [Parameter(
            Mandatory = false,
            HelpMessage = "Connection draining of the backend http settings resource.")]
        [ValidateNotNullOrEmpty]
        public PSApplicationGatewayConnectionDraining ConnectionDraining { get; set; }

        [Parameter(
            Mandatory = false,
            HelpMessage = "ID of the application gateway Probe")]
        [ValidateNotNullOrEmpty]
        public string ProbeId { get; set; }

        [Parameter(
            Mandatory = false,
            HelpMessage = "Application gateway Probe")]
        [ValidateNotNullOrEmpty]
        public PSApplicationGatewayProbe Probe { get; set; }

        [Parameter(
            Mandatory = false,
            HelpMessage = "Application gateway Authentication Certificates")]
        [ValidateNotNullOrEmpty]
        public List<PSApplicationGatewayAuthenticationCertificate> AuthenticationCertificates { get; set; }

        [Parameter(
            Mandatory = false,
            HelpMessage = "Flag if host header should be picked from the host name of the backend server.")]
        public SwitchParameter PickHostNameFromBackendAddress { get; set; }

        [Parameter(
            Mandatory = false,
            HelpMessage = "Sets host header to be sent to the backend servers.")]
        public string HostName { get; set; }

        [Parameter(
            Mandatory = false,
            HelpMessage = "Cookie name to use for the affinity cookie")]
        [ValidateNotNullOrEmpty]
        public string AffinityCookieName { get; set; }

        [Parameter(
            Mandatory = false,
            HelpMessage = "Path which should be used as a prefix for all HTTP requests. If no value is provided for this parameter, then no path will be prefixed.")]
        [ValidateNotNullOrEmpty]
        public string Path { get; set; }

        public override void ExecuteCmdlet()
        {
            base.ExecuteCmdlet();

            if (Probe != null)
            {
                ProbeId = Probe.Id;
            }
        }
        public PSApplicationGatewayBackendHttpSettings NewObject()
        {
            var backendHttpSettings = new PSApplicationGatewayBackendHttpSettings();
            backendHttpSettings.Name = Name;
            backendHttpSettings.Port = Port;
            backendHttpSettings.Protocol = Protocol;
            backendHttpSettings.CookieBasedAffinity = CookieBasedAffinity;
            if (0 == RequestTimeout)
            {
                backendHttpSettings.RequestTimeout = 30;
            }
            else
            {
                backendHttpSettings.RequestTimeout = RequestTimeout;
            }
            if(ConnectionDraining != null)
            {
                backendHttpSettings.ConnectionDraining = ConnectionDraining;
            }
            if (!string.IsNullOrEmpty(ProbeId))
            {
                backendHttpSettings.Probe = new PSResourceId();
                backendHttpSettings.Probe.Id = ProbeId;
            }
            if (AuthenticationCertificates != null && AuthenticationCertificates.Count > 0)
            {
                backendHttpSettings.AuthenticationCertificates = new List<PSResourceId>();
                foreach (var authcert in AuthenticationCertificates)
                {
                    backendHttpSettings.AuthenticationCertificates.Add(
                        new PSResourceId
                        {
                            Id = authcert.Id
                        });
                }
            }
            if(PickHostNameFromBackendAddress.IsPresent)
            {
                backendHttpSettings.PickHostNameFromBackendAddress = true;
            }
            if(HostName != null)
            {
                backendHttpSettings.HostName = HostName;
            }
            if (AffinityCookieName != null)
            {
                backendHttpSettings.AffinityCookieName = AffinityCookieName;
            }
            if (Path != null)
            {
                backendHttpSettings.Path = Path;
            }

            backendHttpSettings.Id = ApplicationGatewayChildResourceHelper.GetResourceNotSetId(
                                    NetworkClient.NetworkManagementClient.SubscriptionId,
                                    Properties.Resources.ApplicationGatewaybackendHttpSettingsName,
                                    Name);

            return backendHttpSettings;
        }
    }
}
