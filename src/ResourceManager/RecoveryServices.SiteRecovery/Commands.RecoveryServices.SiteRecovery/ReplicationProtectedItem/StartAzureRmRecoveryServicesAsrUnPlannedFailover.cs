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
using System.IO;
using System.Management.Automation;
using Microsoft.Azure.Commands.RecoveryServices.SiteRecovery.Properties;
using Microsoft.Azure.Management.RecoveryServices.SiteRecovery.Models;

namespace Microsoft.Azure.Commands.RecoveryServices.SiteRecovery
{
    /// <summary>
    ///    Starts a unplanned failover operation.
    /// </summary>
    [Cmdlet(
        VerbsLifecycle.Start,
        "AzureRmRecoveryServicesAsrUnplannedFailoverJob",
        DefaultParameterSetName = ASRParameterSets.ByRPIObject,
        SupportsShouldProcess = true)]
    [Alias(
        "Start-ASRFO",
        "Start-ASRUnplannedFailoverJob")]
    [OutputType(typeof(ASRJob))]
    public class StartAzureRmRecoveryServicesAsrUnplannedFailoverJob : SiteRecoveryCmdletBase
    {
        /// <summary>
        ///     Gets or sets an ASR recovery plan object.
        /// </summary>
        [Parameter(
            ParameterSetName = ASRParameterSets.ByRPObject,
            Mandatory = true,
            ValueFromPipeline = true)]
        [ValidateNotNullOrEmpty]
        public ASRRecoveryPlan RecoveryPlan { get; set; }

        /// <summary>
        ///     Gets or sets an ASR replication protected item.
        /// </summary>
        [Parameter(
            ParameterSetName = ASRParameterSets.ByRPIObject,
            Mandatory = true,
            ValueFromPipeline = true)]
        [Parameter(
           ParameterSetName = ASRParameterSets.ByRPIObjectWithRecoveryTag,
           Mandatory = true,
           ValueFromPipeline = true)]
        [ValidateNotNullOrEmpty]
        public ASRReplicationProtectedItem ReplicationProtectedItem { get; set; }

        /// <summary>
        ///     Gets or sets the failover direction.
        /// </summary>
        [Parameter(Mandatory = true)]
        [ValidateSet(
            Constants.PrimaryToRecovery,
            Constants.RecoveryToPrimary)]
        public string Direction { get; set; }

        /// <summary>
        ///     Switch parameter to perform operation in source side before starting unplanned failover.
        /// </summary>
        [Parameter]
        [Alias("PerformSourceSideActions")]
        public SwitchParameter PerformSourceSideAction { get; set; }

        /// <summary>
        ///     Gets or sets data encryption certificate file path for failover of protected item.
        /// </summary>
        [Parameter]
        [ValidateNotNullOrEmpty]
        public string DataEncryptionPrimaryCertFile { get; set; }

        /// <summary>
        ///     Gets or sets data encryption certificate file path for failover of protected item.
        /// </summary>
        [Parameter]
        [ValidateNotNullOrEmpty]
        public string DataEncryptionSecondaryCertFile { get; set; }

        /// <summary>
        ///     Gets or sets a custom recovery point to test failover the protected machine to.
        /// </summary>
        [Parameter(
           ParameterSetName = ASRParameterSets.ByRPIObject,
           Mandatory = false)]
        [ValidateNotNullOrEmpty]
        public ASRRecoveryPoint RecoveryPoint { get; set; }

        /// <summary>
        ///     Gets or sets recovery tag to failover to.
        /// </summary>
        [Parameter(
            ParameterSetName = ASRParameterSets.ByRPObject,
           Mandatory = false)]
        [Parameter(
            ParameterSetName = ASRParameterSets.ByRPIObjectWithRecoveryTag,
           Mandatory = true)]
        [ValidateNotNullOrEmpty]
        [ValidateSet(
            Constants.RecoveryTagLatest,
            Constants.RecoveryTagLatestAvailable,
            Constants.RecoveryTagLatestAvailableApplicationConsistent,
            Constants.RecoveryTagLatestAvailableCrashConsistent)]
        public string RecoveryTag { get; set; }

        /// <summary>
        ///     ProcessRecord of the command.
        /// </summary>
        public override void ExecuteSiteRecoveryCmdlet()
        {
            base.ExecuteSiteRecoveryCmdlet();

            if (ShouldProcess(
                "Protected item or Recovery plan",
                "Start failover"))
            {
                if (!string.IsNullOrEmpty(DataEncryptionPrimaryCertFile))
                {
                    var certBytesPrimary = File.ReadAllBytes(DataEncryptionPrimaryCertFile);
                    primaryKekCertpfx = Convert.ToBase64String(certBytesPrimary);
                }

                if (!string.IsNullOrEmpty(DataEncryptionSecondaryCertFile))
                {
                    var certBytesSecondary =
                        File.ReadAllBytes(DataEncryptionSecondaryCertFile);
                    secondaryKekCertpfx = Convert.ToBase64String(certBytesSecondary);
                }

                switch (ParameterSetName)
                {
                    case ASRParameterSets.ByRPIObject:
                    case ASRParameterSets.ByRPIObjectWithRecoveryTag:
                        protectionContainerName = Utilities.GetValueFromArmId(
                            ReplicationProtectedItem.ID,
                            ARMResourceTypeConstants.ReplicationProtectionContainers);
                        fabricName = Utilities.GetValueFromArmId(
                            ReplicationProtectedItem.ID,
                            ARMResourceTypeConstants.ReplicationFabrics);
                        StartRPIUnplannedFailover();
                        break;
                    case ASRParameterSets.ByRPObject:
                        StartRpUnplannedFailover();
                        break;
                }
            }
        }

        /// <summary>
        ///     Starts replication protected item unplanned failover.
        /// </summary>
        private void StartRPIUnplannedFailover()
        {
            var unplannedFailoverInputProperties = new UnplannedFailoverInputProperties
            {
                FailoverDirection = Direction,
                SourceSiteOperations = PerformSourceSideAction ? "Required" : "NotRequired",
                ProviderSpecificDetails = new ProviderSpecificFailoverInput()
            };

            var input =
                new UnplannedFailoverInput { Properties = unplannedFailoverInputProperties };

            if (0 ==
                string.Compare(
                    ReplicationProtectedItem.ReplicationProvider,
                    Constants.HyperVReplicaAzure,
                    StringComparison.OrdinalIgnoreCase))
            {
                if (Direction == Constants.PrimaryToRecovery)
                {
                    var failoverInput = new HyperVReplicaAzureFailoverProviderInput
                    {
                        PrimaryKekCertificatePfx = primaryKekCertpfx,
                        SecondaryKekCertificatePfx = secondaryKekCertpfx,
                        VaultLocation = "dummy",
                        RecoveryPointId = RecoveryPoint == null ? null : RecoveryPoint.ID
                    };
                    input.Properties.ProviderSpecificDetails = failoverInput;
                }
            }
            else if (string.Compare(
                    ReplicationProtectedItem.ReplicationProvider,
                    Constants.InMageAzureV2,
                    StringComparison.OrdinalIgnoreCase) ==
                0)
            {
                InMageAzureV2UnplannedFailover(input);
            }
            else if (string.Compare(
                    ReplicationProtectedItem.ReplicationProvider,
                    Constants.InMage,
                    StringComparison.OrdinalIgnoreCase) ==
                0)
            {
                InMageUnplannedFailover(input);
            }
            else if (0 == string.Compare(
               ReplicationProtectedItem.ReplicationProvider,
               Constants.A2A,
               StringComparison.OrdinalIgnoreCase))
            {
                var failoverInput = new A2AFailoverProviderInput()
                {
                    RecoveryPointId = RecoveryPoint == null ? null : RecoveryPoint.ID
                };

                input.Properties.ProviderSpecificDetails = failoverInput;
            }

            var response = RecoveryServicesClient.StartAzureSiteRecoveryUnplannedFailover(
                fabricName,
                protectionContainerName,
                ReplicationProtectedItem.Name,
                input);

            var jobResponse = RecoveryServicesClient.GetAzureSiteRecoveryJobDetails(
                PSRecoveryServicesClient.GetJobIdFromReponseLocation(response.Location));

            WriteObject(new ASRJob(jobResponse));
        }

        /// <summary>
        ///     InMage unplanned failover.
        /// </summary>
        private void InMageUnplannedFailover(UnplannedFailoverInput input)
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
                        .ProviderSpecificDetails).MultiVmGroupName,
                    ((ASRInMageSpecificRPIDetails)ReplicationProtectedItem
                        .ProviderSpecificDetails).MultiVmGroupId) !=
                0)
            {
                // Replication Group was created at the time of Protection.
                throw new InvalidOperationException(
                    string.Format(
                        Resources.UnsupportedReplicationProtectionActionForUnplannedFailover,
                        ReplicationProtectedItem.ReplicationProvider));
            }

            // Validate the Direction as PrimaryToRecovery.
            if (Direction == Constants.PrimaryToRecovery)
            {
                // Set the Recovery Point Types for InMage.
                var recoveryPointType =
                    RecoveryTag ==
                    Constants.RecoveryTagLatestAvailableApplicationConsistent
                        ? RecoveryPointType.LatestTag
                        : RecoveryPointType.LatestTime;

                // Set the InMage Provider specific input in the Unplanned Failover Input.
                var failoverInput = new InMageFailoverProviderInput
                {
                    RecoveryPointType = recoveryPointType
                };
                input.Properties.ProviderSpecificDetails = failoverInput;
            }
            else
            {
                // RecoveryToPrimary Direction is Invalid for InMage.
                new ArgumentException(Resources.InvalidDirectionForAzureToVMWare);
            }
        }

        /// <summary>
        ///     InMageAzureV2 unplanned failover.
        /// </summary>
        private void InMageAzureV2UnplannedFailover(UnplannedFailoverInput input)
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
                        Resources.UnsupportedReplicationProtectionActionForUnplannedFailover,
                        ReplicationProtectedItem.ReplicationProvider));
            }

            // Validate the Direction as PrimaryToRecovery.
            if (Direction == Constants.PrimaryToRecovery)
            {
                // Set the InMageAzureV2 Provider specific input in the Unplanned Failover Input.
                var failoverInput = new InMageAzureV2FailoverProviderInput
                {
                    RecoveryPointId = RecoveryPoint != null ? RecoveryPoint.ID : null
                };
                input.Properties.ProviderSpecificDetails = failoverInput;
            }
            else
            {
                // RecoveryToPrimary Direction is Invalid for InMageAzureV2.
                new ArgumentException(Resources.InvalidDirectionForVMWareToAzure);
            }
        }

        /// <summary>
        ///     Starts recovery plan unplanned failover.
        /// </summary>
        private void StartRpUnplannedFailover()
        {
            // Refresh RP Object
            var rp = RecoveryServicesClient.GetAzureSiteRecoveryRecoveryPlan(
                RecoveryPlan.Name);

            var recoveryPlanUnplannedFailoverInputProperties =
                new RecoveryPlanUnplannedFailoverInputProperties
                {
                    FailoverDirection =
                        Direction == PossibleOperationsDirections.PrimaryToRecovery.ToString()
                            ? PossibleOperationsDirections.PrimaryToRecovery
                            : PossibleOperationsDirections.RecoveryToPrimary,
                    SourceSiteOperations = PerformSourceSideAction
                        ? SourceSiteOperations.Required
                        : SourceSiteOperations.NotRequired, //Required|NotRequired
                    ProviderSpecificDetails = new List<RecoveryPlanProviderSpecificFailoverInput>()
                };

            foreach (var replicationProvider in rp.Properties.ReplicationProviders)
            {
                if (0 ==
                    string.Compare(
                        replicationProvider,
                        Constants.HyperVReplicaAzure,
                        StringComparison.OrdinalIgnoreCase))
                {
                    if (Direction == Constants.PrimaryToRecovery)
                    {
                        var recoveryPlanHyperVReplicaAzureFailoverInput =
                            new RecoveryPlanHyperVReplicaAzureFailoverInput
                            {
                                PrimaryKekCertificatePfx = primaryKekCertpfx,
                                SecondaryKekCertificatePfx = secondaryKekCertpfx,
                                VaultLocation = "dummy"
                            };
                        if (RecoveryTag != null)
                        {
                            var recoveryPointType =
                           RecoveryTag ==
                           Constants.RecoveryTagLatestAvailableApplicationConsistent
                               ? HyperVReplicaAzureRpRecoveryPointType.LatestApplicationConsistent
                               : RecoveryTag == Constants.RecoveryTagLatestAvailable
                                   ? HyperVReplicaAzureRpRecoveryPointType.LatestProcessed
                                   : HyperVReplicaAzureRpRecoveryPointType.Latest;

                            recoveryPlanHyperVReplicaAzureFailoverInput.RecoveryPointType = recoveryPointType;
                        }
                        recoveryPlanUnplannedFailoverInputProperties.ProviderSpecificDetails.Add(
                            recoveryPlanHyperVReplicaAzureFailoverInput);
                    }
                }
                else if (string.Compare(
                        replicationProvider,
                        Constants.InMageAzureV2,
                        StringComparison.OrdinalIgnoreCase) ==
                    0)
                {
                    // Check if the Direction is PrimaryToRecovery.
                    if (Direction == Constants.PrimaryToRecovery)
                    {
                        // Set the Recovery Point Types for InMage.
                        var recoveryPointType =
                            RecoveryTag ==
                            Constants.RecoveryTagLatestAvailableApplicationConsistent
                                ? InMageV2RpRecoveryPointType.LatestApplicationConsistent
                                : RecoveryTag == Constants.RecoveryTagLatestAvailable
                                    ? InMageV2RpRecoveryPointType.LatestProcessed
                                    : RecoveryTag == Constants.RecoveryTagLatestAvailableCrashConsistent
                                        ? InMageV2RpRecoveryPointType.LatestCrashConsistent
                                        : InMageV2RpRecoveryPointType.Latest;

                        // Create the InMageAzureV2 Provider specific input.
                        var recoveryPlanInMageAzureV2FailoverInput =
                            new RecoveryPlanInMageAzureV2FailoverInput
                            {
                                RecoveryPointType = recoveryPointType,
                                VaultLocation = "dummy"
                            };

                        // Add the InMageAzureV2 Provider specific input in the Planned Failover Input.
                        recoveryPlanUnplannedFailoverInputProperties.ProviderSpecificDetails.Add(
                            recoveryPlanInMageAzureV2FailoverInput);
                    }
                }
                else if (string.Compare(
                        replicationProvider,
                        Constants.InMage,
                        StringComparison.OrdinalIgnoreCase) ==
                    0)
                {
                    // Check if the Direction is RecoveryToPrimary.
                    if (Direction == Constants.RecoveryToPrimary)
                    {
                        // Set the Recovery Point Types for InMage.
                        var recoveryPointType =
                            RecoveryTag ==
                            Constants.RecoveryTagLatestAvailableApplicationConsistent
                                ? RpInMageRecoveryPointType.LatestTag
                                : RpInMageRecoveryPointType.LatestTime;

                        // Create the InMage Provider specific input.
                        var recoveryPlanInMageFailoverInput = new RecoveryPlanInMageFailoverInput
                        {
                            RecoveryPointType = recoveryPointType
                        };

                        // Add the InMage Provider specific input in the Planned Failover Input.
                        recoveryPlanUnplannedFailoverInputProperties.ProviderSpecificDetails.Add(
                            recoveryPlanInMageFailoverInput);
                    }
                }
                else if (0 == string.Compare(
                   replicationProvider,
                   Constants.A2A,
                   StringComparison.OrdinalIgnoreCase))
                {
                    A2ARpRecoveryPointType recoveryPointType = A2ARpRecoveryPointType.Latest;

                    switch (RecoveryTag)
                    {
                        case Constants.RecoveryTagLatestAvailableCrashConsistent:
                            recoveryPointType = A2ARpRecoveryPointType.LatestCrashConsistent;
                            break;
                        case Constants.RecoveryTagLatestAvailableApplicationConsistent:
                            recoveryPointType = A2ARpRecoveryPointType.LatestApplicationConsistent;
                            break;
                        case Constants.RecoveryTagLatestAvailable:
                            recoveryPointType = A2ARpRecoveryPointType.LatestProcessed;
                            break;
                        case Constants.RecoveryTagLatest:
                            recoveryPointType = A2ARpRecoveryPointType.Latest;
                            break;
                    }

                    var recoveryPlanA2AFailoverInput = new RecoveryPlanA2AFailoverInput()
                    {
                        RecoveryPointType = recoveryPointType
                    };
                    recoveryPlanUnplannedFailoverInputProperties.ProviderSpecificDetails.Add(recoveryPlanA2AFailoverInput);
                }
            }

            var recoveryPlanUnplannedFailoverInput = new RecoveryPlanUnplannedFailoverInput
            {
                Properties = recoveryPlanUnplannedFailoverInputProperties
            };

            var response = RecoveryServicesClient.StartAzureSiteRecoveryUnplannedFailover(
                RecoveryPlan.Name,
                recoveryPlanUnplannedFailoverInput);

            var jobResponse = RecoveryServicesClient.GetAzureSiteRecoveryJobDetails(
                PSRecoveryServicesClient.GetJobIdFromReponseLocation(response.Location));

            WriteObject(new ASRJob(jobResponse));
        }

        #region local parameters

        /// <summary>
        ///     Gets or sets Name of the PE.
        /// </summary>
        public string protectionEntityName;

        /// <summary>
        ///     Gets or sets Name of the Protection Container.
        /// </summary>
        public string protectionContainerName;

        /// <summary>
        ///     Gets or sets Name of the Fabric.
        /// </summary>
        public string fabricName;

        /// <summary>
        ///     Primary Kek Cert pfx file.
        /// </summary>
        private string primaryKekCertpfx;

        /// <summary>
        ///     Secondary Kek Cert pfx file.
        /// </summary>
        private string secondaryKekCertpfx;

        #endregion local parameters
    }

}
