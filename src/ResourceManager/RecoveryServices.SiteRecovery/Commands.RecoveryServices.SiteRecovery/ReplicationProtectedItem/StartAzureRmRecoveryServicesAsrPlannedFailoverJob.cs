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
    ///    Starts a planned failover operation.
    /// </summary>
    [Cmdlet(
        VerbsLifecycle.Start,
        "AzureRmRecoveryServicesAsrPlannedFailoverJob",
        DefaultParameterSetName = ASRParameterSets.ByRPIObject,
        SupportsShouldProcess = true)]
    [Alias(
        "Start-ASRPFO",
        "Start-ASRPlannedFailoverJob")]
    [OutputType(typeof(ASRJob))]
    public class StartAzureRmRecoveryServicesAsrPlannedFailoverJob : SiteRecoveryCmdletBase
    {
        /// <summary>
        ///     Gets or sets the recovery plan object to be failed over.
        /// </summary>
        [Parameter(
            ParameterSetName = ASRParameterSets.ByRPObject,
            Mandatory = true,
            ValueFromPipeline = true)]
        [ValidateNotNullOrEmpty]
        public ASRRecoveryPlan RecoveryPlan { get; set; }

        /// <summary>
        ///     Gets or sets replication protected item object to be failed over.
        /// </summary>
        [Parameter(
            ParameterSetName = ASRParameterSets.ByRPIObject,
            Mandatory = true,
            ValueFromPipeline = true)]
        [ValidateNotNullOrEmpty]
        public ASRReplicationProtectedItem ReplicationProtectedItem { get; set; }

        /// <summary>
        ///     Gets or sets the direction of the failover.
        /// </summary>
        [Parameter(Mandatory = true)]
        [ValidateSet(
            Constants.PrimaryToRecovery,
            Constants.RecoveryToPrimary)]
        public string Direction { get; set; }

        /// <summary>
        ///     Gets or sets what to optimize for.
        /// </summary>
        [Parameter(
            Mandatory = false,
            ValueFromPipeline = false)]
        [ValidateSet(
            Constants.ForDownTime,
            Constants.ForSynchronization)]
        public string Optimize { get; set; }

        /// <summary>
        ///     Gets or sets create the virtual machine if not found while failing back to the primary region
        ///     (used in alternate location recovery.)
        /// </summary>
        [Parameter(
            Mandatory = false,
            ValueFromPipeline = false)]
        [ValidateSet(
            Constants.Yes,
            Constants.No)]
        public string CreateVmIfNotFound { get; set; }

        /// <summary>
        ///     Gets or sets identifies the host to on which to create the virtual machine while failing 
        ///     over to an alternate location by specifying the ASR services provider object corresponding 
        ///     to the ASR services provider running on the host.
        /// </summary>
        [Parameter(
            ParameterSetName = ASRParameterSets.ByRPIObject,
            Mandatory = false,
            ValueFromPipeline = false)]
        public ASRRecoveryServicesProvider ServicesProvider { get; set; }

        /// <summary>
        ///     Gets or sets data encryption primary certificate file path for failover of Protected Item.
        /// </summary>
        [Parameter(
            Mandatory = false,
            ValueFromPipeline = false)]
        [ValidateNotNullOrEmpty]
        public string DataEncryptionPrimaryCertFile { get; set; }

        /// <summary>
        ///     Gets or sets Data encryption secondary certificate file path for failover of Protected Item.
        /// </summary>
        [Parameter(
            Mandatory = false,
            ValueFromPipeline = false)]
        [ValidateNotNullOrEmpty]
        public string DataEncryptionSecondaryCertFile { get; set; }

        /// <summary>
        ///     ProcessRecord of the command.
        /// </summary>
        public override void ExecuteSiteRecoveryCmdlet()
        {
            base.ExecuteSiteRecoveryCmdlet();

            if (ShouldProcess(
                "Protected item or Recovery plan",
                "Start planned failover"))
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
                        protectionContainerName = Utilities.GetValueFromArmId(
                            ReplicationProtectedItem.ID,
                            ARMResourceTypeConstants.ReplicationProtectionContainers);
                        fabricName = Utilities.GetValueFromArmId(
                            ReplicationProtectedItem.ID,
                            ARMResourceTypeConstants.ReplicationFabrics);
                        StartRPIPlannedFailover();
                        break;
                    case ASRParameterSets.ByRPObject:
                        StartRpPlannedFailover();
                        break;
                }
            }
        }

        /// <summary>
        ///     Starts RPI Planned failover.
        /// </summary>
        private void StartRPIPlannedFailover()
        {
            var plannedFailoverInputProperties = new PlannedFailoverInputProperties
            {
                FailoverDirection = Direction,
                ProviderSpecificDetails = new ProviderSpecificFailoverInput()
            };

            var input = new PlannedFailoverInput { Properties = plannedFailoverInputProperties };

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
                        VaultLocation = "dummy"
                    };
                    input.Properties.ProviderSpecificDetails = failoverInput;
                }
                else
                {
                    var failbackInput = new HyperVReplicaAzureFailbackProviderInput
                    {
                        DataSyncOption =
                            Optimize == Constants.ForDownTime ? Constants.ForDownTime
                                : Constants.ForSynchronization,
                        RecoveryVmCreationOption = string.Compare(
                                                       CreateVmIfNotFound,
                                                       Constants.Yes,
                                                       StringComparison.OrdinalIgnoreCase) ==
                                                   0 ? Constants.CreateVmIfNotFound
                            : Constants.NoAction
                    };

                    if (string.Compare(
                            CreateVmIfNotFound,
                            Constants.Yes,
                            StringComparison.OrdinalIgnoreCase) ==
                        0 &&
                        RecoveryServicesClient.GetAzureSiteRecoveryFabric(fabricName)
                            .Properties.CustomDetails is HyperVSiteDetails)
                    {
                        if (ServicesProvider == null ||
                            string.Compare(
                                ServicesProvider.FabricType,
                                Constants.HyperVSite) !=
                            0)
                        {
                            throw new InvalidOperationException(
                                Resources.ImproperServerObjectPassedForHyperVFailback);
                        }

                        failbackInput.ProviderIdForAlternateRecovery = ServicesProvider.ID;
                    }

                    input.Properties.ProviderSpecificDetails = failbackInput;
                }
            }
            else if (string.Compare(
                    ReplicationProtectedItem.ReplicationProvider,
                    Constants.InMageAzureV2,
                    StringComparison.OrdinalIgnoreCase) ==
                0 ||
                string.Compare(
                    ReplicationProtectedItem.ReplicationProvider,
                    Constants.InMage,
                    StringComparison.OrdinalIgnoreCase) ==
                0)
            {
                throw new InvalidOperationException(
                    string.Format(
                        Resources.UnsupportedReplicationProviderForPlannedFailover,
                        ReplicationProtectedItem.ReplicationProvider));
            }

            var response = RecoveryServicesClient.StartAzureSiteRecoveryPlannedFailover(
                fabricName,
                protectionContainerName,
                ReplicationProtectedItem.Name,
                input);

            var jobResponse = RecoveryServicesClient.GetAzureSiteRecoveryJobDetails(
                PSRecoveryServicesClient.GetJobIdFromReponseLocation(response.Location));

            WriteObject(new ASRJob(jobResponse));
        }

        /// <summary>
        ///     Starts RP Planned failover.
        /// </summary>
        private void StartRpPlannedFailover()
        {
            // Refresh RP Object
            var rp = RecoveryServicesClient.GetAzureSiteRecoveryRecoveryPlan(
                RecoveryPlan.Name);

            var recoveryPlanPlannedFailoverInputProperties =
                new RecoveryPlanPlannedFailoverInputProperties
                {
                    FailoverDirection =
                        Direction == PossibleOperationsDirections.PrimaryToRecovery.ToString()
                            ? PossibleOperationsDirections.PrimaryToRecovery
                            : PossibleOperationsDirections.RecoveryToPrimary,
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
                        recoveryPlanPlannedFailoverInputProperties.ProviderSpecificDetails.Add(
                            recoveryPlanHyperVReplicaAzureFailoverInput);
                    }
                    else
                    {
                        var recoveryPlanHyperVReplicaAzureFailbackInput =
                            new RecoveryPlanHyperVReplicaAzureFailbackInput
                            {
                                DataSyncOption =
                                    Optimize == Constants.ForDownTime
                                        ? DataSyncStatus.ForDownTime
                                        : DataSyncStatus.ForSynchronization,
                                RecoveryVmCreationOption = string.Compare(
                                                               CreateVmIfNotFound,
                                                               Constants.Yes,
                                                               StringComparison
                                                                   .OrdinalIgnoreCase) ==
                                                           0 ? AlternateLocationRecoveryOption
                                    .CreateVmIfNotFound : AlternateLocationRecoveryOption.NoAction
                            };
                        recoveryPlanPlannedFailoverInputProperties.ProviderSpecificDetails.Add(
                            recoveryPlanHyperVReplicaAzureFailbackInput);
                    }
                }
                else if (string.Compare(
                        replicationProvider,
                        Constants.InMageAzureV2,
                        StringComparison.OrdinalIgnoreCase) ==
                    0 ||
                    string.Compare(
                        replicationProvider,
                        Constants.InMage,
                        StringComparison.OrdinalIgnoreCase) ==
                    0)
                {
                    throw new InvalidOperationException(
                        string.Format(
                            Resources.UnsupportedReplicationProviderForPlannedFailover,
                            replicationProvider));
                }
            }

            var recoveryPlanPlannedFailoverInput = new RecoveryPlanPlannedFailoverInput
            {
                Properties = recoveryPlanPlannedFailoverInputProperties
            };

            var response = RecoveryServicesClient.StartAzureSiteRecoveryPlannedFailover(
                RecoveryPlan.Name,
                recoveryPlanPlannedFailoverInput);

            var jobResponse = RecoveryServicesClient.GetAzureSiteRecoveryJobDetails(
                PSRecoveryServicesClient.GetJobIdFromReponseLocation(response.Location));

            WriteObject(new ASRJob(jobResponse));
        }

        #region local parameters

        /// <summary>
        ///     Gets or sets Name of the PE.
        /// </summary>
        private string protectionEntityName;

        /// <summary>
        ///     Gets or sets Name of the Protection Container.
        /// </summary>
        private string protectionContainerName;

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
