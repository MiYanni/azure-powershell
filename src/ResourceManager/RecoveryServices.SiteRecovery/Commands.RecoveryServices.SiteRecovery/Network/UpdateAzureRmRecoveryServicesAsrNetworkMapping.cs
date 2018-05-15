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

using System.Management.Automation;
using Microsoft.Azure.Management.RecoveryServices.SiteRecovery.Models;

namespace Microsoft.Azure.Commands.RecoveryServices.SiteRecovery
{
    /// <summary>
    ///     Updates Azure Site Recovery Network mapping.
    /// </summary>
    [Cmdlet(
        VerbsData.Update,
        "AzureRmRecoveryServicesAsrNetworkMapping",
        DefaultParameterSetName = ASRParameterSets.ByNetworkObject,
        SupportsShouldProcess = true)]
    [OutputType(typeof(ASRJob))]
    public class UpdateAzureRmRecoveryServicesAsrNetworkMapping : SiteRecoveryCmdletBase
    {
        /// <summary>
        ///     Gets or sets the ASR network mapping object corresponding to the ASR network mapping to be updated 
        /// </summary>
        [Parameter(
            ParameterSetName = ASRParameterSets.ById,
            Mandatory = true,
            ValueFromPipeline = true)]
        [Parameter(
            ParameterSetName = ASRParameterSets.ByNetworkObject,
            Mandatory = true,
            ValueFromPipeline = true)]
        [ValidateNotNullOrEmpty]
        [Alias("NetworkMapping")]
        public ASRNetworkMapping InputObject { get; set; }

        /// <summary>
        ///     Gets or sets the recovery network object for the network mapping.
        /// </summary>
        [Parameter(
            ParameterSetName = ASRParameterSets.ByNetworkObject,
            Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public ASRNetwork RecoveryNetwork { get; set; }

        /// <summary>
        ///     Gets or sets the recovery azure network ID for the network mapping.
        /// </summary>
        [Parameter(
            ParameterSetName = ASRParameterSets.ById,
            Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string RecoveryAzureNetworkId { get; set; }

        /// <summary>
        ///     ProcessRecord of the command.
        /// </summary>
        public override void ExecuteSiteRecoveryCmdlet()
        {
            base.ExecuteSiteRecoveryCmdlet();

            if (ShouldProcess(
                InputObject.FriendlyName,
                VerbsData.Update))
            {
                switch (ParameterSetName)
                {
                    case ASRParameterSets.ByNetworkObject:
                        UpdateEnterpriseToEnterpriseNetworkMapping();
                        break;
                    case ASRParameterSets.ById:
                        if (InputObject.ID.Contains(ARMResourceTypeConstants.AzureNetwork))
                        {
                            UpdateAzureToAzureNetworkMapping();
                        }
                        else
                        {
                            UpdateEnterpriseToAzureNetworkMapping();
                        }
                        break;
                }
            }
        }

        /// <summary>
        ///     Update Azure to Azure network mapping.
        /// </summary>
        private void UpdateAzureToAzureNetworkMapping()
        {
            // Add following checks if needed:
            // Verify whether the subscription is associated with the account or not.
            // Check if the Azure VM Network is associated with the Subscription or not.

            var input = new UpdateNetworkMappingInput
            {
                Properties = new UpdateNetworkMappingInputProperties
                {
                    RecoveryFabricName = InputObject.RecoveryFabricFriendlyName,
                    RecoveryNetworkId = RecoveryAzureNetworkId,
                    FabricSpecificDetails = new AzureToAzureUpdateNetworkMappingInput
                    {
                        PrimaryNetworkId = InputObject.PrimaryNetworkId
                    }
                }
            };
            var response = RecoveryServicesClient.UpdateAzureSiteRecoveryNetworkMapping(
                InputObject.PrimaryFabricFriendlyName,
                ARMResourceTypeConstants.AzureNetwork,
                InputObject.Name,
                input);

            var jobResponse = RecoveryServicesClient.GetAzureSiteRecoveryJobDetails(
                PSRecoveryServicesClient.GetJobIdFromReponseLocation(response.Location));

            WriteObject(new ASRJob(jobResponse));
        }

        /// <summary>
        ///     Enterprise to Azure network mapping.
        /// </summary>
        private void UpdateEnterpriseToAzureNetworkMapping()
        {
            // Add following checks if needed:
            // Verify whether the subscription is associated with the account or not.
            // Check if the Azure VM Network is associated with the Subscription or not.

            var input = new UpdateNetworkMappingInput
            {
                Properties = new UpdateNetworkMappingInputProperties
                {
                    RecoveryFabricName = "Microsoft Azure",
                    RecoveryNetworkId = RecoveryAzureNetworkId,
                    FabricSpecificDetails = new VmmToAzureUpdateNetworkMappingInput()
                }
            };
            var response = RecoveryServicesClient.UpdateAzureSiteRecoveryNetworkMapping(
                Utilities.GetValueFromArmId(
                    InputObject.PrimaryNetworkId,
                    ARMResourceTypeConstants.ReplicationFabrics),
                Utilities.GetValueFromArmId(
                    InputObject.PrimaryNetworkId,
                    ARMResourceTypeConstants.ReplicationNetworks),
                InputObject.Name,
                input);

            var jobResponse = RecoveryServicesClient.GetAzureSiteRecoveryJobDetails(
                PSRecoveryServicesClient.GetJobIdFromReponseLocation(response.Location));

            WriteObject(new ASRJob(jobResponse));
        }

        /// <summary>
        ///     Enterprise to enterprise network mapping.
        /// </summary>
        private void UpdateEnterpriseToEnterpriseNetworkMapping()
        {
            var input = new UpdateNetworkMappingInput
            {
                Properties = new UpdateNetworkMappingInputProperties
                {
                    RecoveryFabricName = Utilities.GetValueFromArmId(
                        RecoveryNetwork.ID,
                        ARMResourceTypeConstants.ReplicationFabrics),
                    RecoveryNetworkId = RecoveryNetwork.ID,
                    FabricSpecificDetails = new VmmToVmmUpdateNetworkMappingInput()
                }
            };
            var response = RecoveryServicesClient.UpdateAzureSiteRecoveryNetworkMapping(
                Utilities.GetValueFromArmId(
                    InputObject.PrimaryNetworkId,
                    ARMResourceTypeConstants.ReplicationFabrics),
                Utilities.GetValueFromArmId(
                    InputObject.PrimaryNetworkId,
                    ARMResourceTypeConstants.ReplicationNetworks),
                InputObject.Name,
                input);

            var jobResponse = RecoveryServicesClient.GetAzureSiteRecoveryJobDetails(
                PSRecoveryServicesClient.GetJobIdFromReponseLocation(response.Location));

            WriteObject(new ASRJob(jobResponse));
        }
    }
}