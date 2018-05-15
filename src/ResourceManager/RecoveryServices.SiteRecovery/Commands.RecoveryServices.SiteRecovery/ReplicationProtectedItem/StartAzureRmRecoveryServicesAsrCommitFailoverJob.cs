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

namespace Microsoft.Azure.Commands.RecoveryServices.SiteRecovery
{
    /// <summary>
    ///     Starts the commit failover action for a site recovery object.
    /// </summary>
    [Cmdlet(
        VerbsLifecycle.Start,
        "AzureRmRecoveryServicesAsrCommitFailoverJob",
        DefaultParameterSetName = ASRParameterSets.ByRPIObject,
        SupportsShouldProcess = true)]
    [Alias(
        "Start-ASRCommitFailover",
        "Start-ASRCommitFailoverJob")]
    [OutputType(typeof(ASRJob))]
    public class StartAzureRmRecoveryServicesAsrCommitFailoverJob : SiteRecoveryCmdletBase
    {
        /// <summary>
        ///     Gets or sets recovery plan object corresponding to recovery plan to be failovered .
        /// </summary>
        [Parameter(
            ParameterSetName = ASRParameterSets.ByRPObject,
            Mandatory = true,
            ValueFromPipeline = true)]
        [ValidateNotNullOrEmpty]
        public ASRRecoveryPlan RecoveryPlan { get; set; }

        /// <summary>
        ///     Gets or sets replication protected item object corresponding to replication protected item  to be failovered.
        /// </summary>
        [Parameter(
            ParameterSetName = ASRParameterSets.ByRPIObject,
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
                "Protected item or Recovery plan",
                "Commit failover"))
            {
                switch (ParameterSetName)
                {
                    case ASRParameterSets.ByRPIObject:
                        protectionContainerName = Utilities.GetValueFromArmId(
                            ReplicationProtectedItem.ID,
                            ARMResourceTypeConstants.ReplicationProtectionContainers);
                        fabricName = Utilities.GetValueFromArmId(
                            ReplicationProtectedItem.ID,
                            ARMResourceTypeConstants.ReplicationFabrics);
                        SetRPICommit();
                        break;
                    case ASRParameterSets.ByRPObject:
                        StartRpCommit();
                        break;
                }
            }
        }

        /// <summary>
        ///     Start RPI Commit.
        /// </summary>
        private void SetRPICommit()
        {
            // Check if the Replication Provider is InMageAzureV2.
            if (string.Compare(
                    ReplicationProtectedItem.ReplicationProvider,
                    Constants.InMageAzureV2,
                    StringComparison.OrdinalIgnoreCase) ==
                0)
            {
                // Validate if the Replication Protection Item is part of any Replication Group.
                Guid guidResult;
                var parseFlag = Guid.TryParse(
                    ((ASRInMageAzureV2SpecificRPIDetails)ReplicationProtectedItem
                        .ProviderSpecificDetails).MultiVmGroupName,
                    out guidResult);
                if (parseFlag == false ||
                    guidResult == Guid.Empty ||
                    string.Compare(
                        ((ASRInMageAzureV2SpecificRPIDetails)ReplicationProtectedItem
                            .ProviderSpecificDetails).MultiVmGroupName,
                        ((ASRInMageAzureV2SpecificRPIDetails)ReplicationProtectedItem
                            .ProviderSpecificDetails).MultiVmGroupId) !=
                    0)
                {
                    // Replication Group was created at the time of Protection.
                    throw new InvalidOperationException(
                        string.Format(
                            Resources
                                .UnsupportedReplicationProtectionActionForCommit,
                            ReplicationProtectedItem
                                .ReplicationProvider));
                }
            }
            else if (string.Compare(
                    ReplicationProtectedItem.ReplicationProvider,
                    Constants.InMage,
                    StringComparison.OrdinalIgnoreCase) ==
                0)
            {
                // Validate if the Replication Protection Item is part of any Replication Group.
                Guid guidResult;
                var parseFlag = Guid.TryParse(
                    ((ASRInMageSpecificRPIDetails)ReplicationProtectedItem
                        .ProviderSpecificDetails).MultiVmGroupName,
                    out guidResult);
                if (parseFlag == false ||
                    guidResult == Guid.Empty ||
                    string.Compare(
                        ((ASRInMageSpecificRPIDetails)ReplicationProtectedItem
                            .ProviderSpecificDetails)
                        .MultiVmGroupName,
                        ((ASRInMageSpecificRPIDetails)ReplicationProtectedItem
                            .ProviderSpecificDetails)
                        .MultiVmGroupId) !=
                    0)
                {
                    // Replication Group was created at the time of Protection.
                    throw new InvalidOperationException(
                        string.Format(
                            Resources
                                .UnsupportedReplicationProtectionActionForCommit,
                            ReplicationProtectedItem
                                .ReplicationProvider));
                }
            }

            var response = RecoveryServicesClient.StartAzureSiteRecoveryCommitFailover(
                fabricName,
                protectionContainerName,
                ReplicationProtectedItem.Name);

            var jobResponse = RecoveryServicesClient.GetAzureSiteRecoveryJobDetails(
                PSRecoveryServicesClient.GetJobIdFromReponseLocation(response.Location));

            WriteObject(new ASRJob(jobResponse));
        }

        /// <summary>
        ///     Starts RP Commit.
        /// </summary>
        private void StartRpCommit()
        {
            var response =
                RecoveryServicesClient.StartAzureSiteRecoveryCommitFailover(
                    RecoveryPlan.Name);

            var jobResponse = RecoveryServicesClient.GetAzureSiteRecoveryJobDetails(
                PSRecoveryServicesClient.GetJobIdFromReponseLocation(response.Location));

            WriteObject(new ASRJob(jobResponse));
        }

        #region local Variable

        /// <summary>
        ///     Gets or sets Name of the Fabric.
        /// </summary>
        private string fabricName;

        /// <summary>
        ///     Gets or sets ID of the Protection Container.
        /// </summary>
        private string protectionContainerName;

        /// <summary>
        ///     Gets or sets ID of the PE.
        /// </summary>
        private string protectionEntityName;

        #endregion
    }
}