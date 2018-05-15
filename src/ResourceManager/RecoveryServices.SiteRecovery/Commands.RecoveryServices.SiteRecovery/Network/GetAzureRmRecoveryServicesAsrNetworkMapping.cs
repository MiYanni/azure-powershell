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
using System.Linq;
using System.Management.Automation;
using Microsoft.Azure.Management.RecoveryServices.SiteRecovery.Models;
using Microsoft.Azure.Commands.RecoveryServices.SiteRecovery.Properties;

namespace Microsoft.Azure.Commands.RecoveryServices.SiteRecovery
{
    /// <summary>
    ///     Gets information about Site Recovery network mappings for the current vault.
    /// </summary>
    [Cmdlet(
        VerbsCommon.Get,
        "AzureRmRecoveryServicesAsrNetworkMapping",
        DefaultParameterSetName = ASRParameterSets.ByObject)]
    [Alias("Get-ASRNetworkMapping")]
    [OutputType(typeof(IEnumerable<ASRNetworkMapping>))]
    public class GetAzureRmRecoveryServicesAsrNetworkMapping : SiteRecoveryCmdletBase
    {
        /// <summary>
        ///     Gets or sets name of the ASR network mapping object to get.
        /// </summary>
        [Parameter(ParameterSetName = ASRParameterSets.ByObject)]
        [Parameter(ParameterSetName = ASRParameterSets.ByFabricObject)]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the ASR network mappings corresponding to the specified network ASR object.
        /// </summary>
        [Parameter(ParameterSetName = ASRParameterSets.ByObject, Mandatory = true, ValueFromPipeline = true)]
        [ValidateNotNullOrEmpty]
        public ASRNetwork Network { get; set; }

        /// <summary>
        ///     Gets or sets the ASR network mappings corresponding to the specified primary fabric object.
        /// </summary>
        [Parameter(ParameterSetName = ASRParameterSets.ByFabricObject, Mandatory = true, ValueFromPipeline = true)]
        [ValidateNotNullOrEmpty]
        public ASRFabric PrimaryFabric { get; set; }

        /// <summary>
        ///     ProcessRecord of the command.
        /// </summary>
        public override void ExecuteSiteRecoveryCmdlet()
        {
            base.ExecuteSiteRecoveryCmdlet();

            switch (ParameterSetName)
            {
                case ASRParameterSets.ByObject:
                    ByObject();
                    break;

                case ASRParameterSets.ByFabricObject:
                    GetNetworkMappingByFabric();
                    break;
            }
        }

        /// <summary>
        ///     Get network mapping by network object.
        /// </summary>
        private void ByObject()
        {
            var tokens =
                    Network.ID.UnFormatArmId(ARMResourceIdPaths.NetworkResourceIdPath);

            if (Network.FabricType.Equals(Constants.Azure))
            {
                PrimaryFabric = new ASRFabric(RecoveryServicesClient.GetAzureSiteRecoveryFabric(tokens[0]));
                GetNetworkMappingByFabric();
            }
            else
            {
                if (Name == null)
                {
                    var networkMappingList =
                    RecoveryServicesClient.GetAzureSiteRecoveryNetworkMappings(
                        tokens[0],
                        Network.Name);

                    WriteNetworkMappings(networkMappingList);
                }
                else
                {
                    var networkMapping = RecoveryServicesClient.GetAzureSiteRecoveryNetworkMappings(
                            tokens[0],
                            Network.Name,
                            Name);

                    WriteNetworkMapping(networkMapping);
                }
            }
        }

        /// <summary>
        ///     Write Network mapping.
        /// </summary>
        /// <param name="networkMapping">Network mapping</param>
        private void WriteNetworkMapping(
            NetworkMapping networkMapping)
        {
            WriteObject(new ASRNetworkMapping(networkMapping));
        }

        /// <summary>
        ///     Write Network mappings.
        /// </summary>
        /// <param name="networkMappings">List of Network mappings</param>
        private void WriteNetworkMappings(
            IList<NetworkMapping> networkMappings)
        {
            WriteObject(
                networkMappings.Select(nm => new ASRNetworkMapping(nm)),
                true);
        }

        /// <summary>
        /// Get Azure to Azure Network mapping.
        /// </summary>
        private void GetNetworkMappingByFabric()
        {
            if (string.Equals(
                PrimaryFabric.FabricType,
                Constants.Azure,
                StringComparison.InvariantCultureIgnoreCase))
            {
                if (string.IsNullOrEmpty(Name))
                {
                    var networkMappingList = RecoveryServicesClient.GetAzureSiteRecoveryNetworkMappings(
                        PrimaryFabric.Name,
                        ARMResourceTypeConstants.AzureNetwork);
                    WriteNetworkMappings(networkMappingList);
                }
                else
                {
                    var networkMapping =
                    RecoveryServicesClient.GetAzureSiteRecoveryNetworkMappings(
                        PrimaryFabric.Name,
                        ARMResourceTypeConstants.AzureNetwork,
                        Name);
                    WriteNetworkMapping(networkMapping);
                }
            }
            else
            {
                var networks = RecoveryServicesClient.GetAzureSiteRecoveryNetworks(PrimaryFabric.Name);
                foreach (var network in networks)
                {
                    Network = new ASRNetwork(network);
                    ByObject();
                }
            }
        }
    }
}
