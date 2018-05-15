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
using System.Management.Automation;
using Microsoft.Azure.Commands.RecoveryServices.SiteRecovery.Properties;
using Microsoft.Azure.Management.RecoveryServices.SiteRecovery.Models;

namespace Microsoft.Azure.Commands.RecoveryServices.SiteRecovery
{
    /// <summary>
    ///     Push mobility service agent updates to protected machines.
    /// </summary>
    [Cmdlet(
        VerbsData.Update,
        "AzureRmRecoveryServicesAsrMobilityService",
        DefaultParameterSetName = ASRParameterSets.Default,
        SupportsShouldProcess = true)]
    [Alias("Update-ASRMobilityService")]
    [OutputType(typeof(ASRJob))]
    public class UpdateAzureRmRecoveryServicesAsrMobilityService : SiteRecoveryCmdletBase
    {
        /// <summary>
        ///     Gets or sets te run as account ID to be used to push the update.
        ///     Must be one from the list of run as accounts in the ASR fabric corresponding to machine being updated.
        /// </summary>
        [Parameter(ParameterSetName = ASRParameterSets.Default, Mandatory = false)]
        [ValidateNotNullOrEmpty]
        public ASRRunAsAccount Account { get; set; }

        /// <summary>
        ///     Gets or sets replication protected item to be updated.
        /// </summary>
        [Parameter(
            ParameterSetName = ASRParameterSets.Default,
            Mandatory = true,
            ValueFromPipeline = true)]
        [ValidateNotNullOrEmpty]
        public ASRReplicationProtectedItem ReplicationProtectedItem { get; set; }

        /// <summary>
        ///     ProcessRecord of the command.
        /// </summary>
        public override void ExecuteSiteRecoveryCmdlet()
        {
            base.ExecuteSiteRecoveryCmdlet();

            if (ShouldProcess(
                "Update Mobility Service",
                ReplicationProtectedItem.FriendlyName))
            {
                // Validate the Replication Provider for InMageAzureV2 / InMage.
                if (string.Compare(
                    ReplicationProtectedItem.ReplicationProvider,
                    Constants.InMageAzureV2,
                    StringComparison.OrdinalIgnoreCase) !=
                0 &&
                string.Compare(
                    ReplicationProtectedItem.ReplicationProvider,
                    Constants.InMage,
                    StringComparison.OrdinalIgnoreCase) !=
                0 &&
                string.Compare(
                    ReplicationProtectedItem.ReplicationProvider,
                    Constants.A2A,
                    StringComparison.OrdinalIgnoreCase) !=
                0)
                {
                    throw new InvalidOperationException(
                        string.Format(
                            Resources.UnsupportedReplicationProviderForUpdateMobilityService,
                            ReplicationProtectedItem.ReplicationProvider));
                }

                // Set the Fabric Name and Protection Container Name.
                fabricName =
                    Utilities.GetValueFromArmId(
                        ReplicationProtectedItem.ID,
                        ARMResourceTypeConstants.ReplicationFabrics);
                protectionContainerName =
                    Utilities.GetValueFromArmId(
                        ReplicationProtectedItem.ID,
                        ARMResourceTypeConstants.ReplicationProtectionContainers);
                protectableItemName =
                    Utilities.GetValueFromArmId(
                        ReplicationProtectedItem.ID,
                        ARMResourceTypeConstants.ReplicationProtectedItems);

                // Create the Update Mobility Service input request.
                var input = new UpdateMobilityServiceRequest();
                if (Account != null)
                {
                    input.Properties = new UpdateMobilityServiceRequestProperties
                    {
                        RunAsAccountId = Account.AccountId
                    };
                }
                else
                {
                    input.Properties = new UpdateMobilityServiceRequestProperties();
                }

                // Update the Mobility Service.
                var response = RecoveryServicesClient.UpdateAzureSiteRecoveryMobilityService(
                    fabricName,
                    protectionContainerName,
                    protectableItemName,
                    input);

                var jobResponse = RecoveryServicesClient.GetAzureSiteRecoveryJobDetails(
                    PSRecoveryServicesClient.GetJobIdFromReponseLocation(response.Location));

                WriteObject(new ASRJob(jobResponse));
            }
        }

        #region Private parameters

        /// <summary>
        ///     Gets or sets Name of the Fabric.
        /// </summary>
        private string fabricName;

        /// <summary>
        ///     Gets or sets Name of the Protection Container.
        /// </summary>
        private string protectionContainerName;

        /// <summary>
        ///     Gets or sets Name of the Protectable Item.
        /// </summary>
        private string protectableItemName;

        #endregion
    }
}