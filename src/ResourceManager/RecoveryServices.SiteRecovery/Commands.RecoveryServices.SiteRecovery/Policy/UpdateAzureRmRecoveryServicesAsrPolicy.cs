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
using System.ComponentModel;
using System.Management.Automation;
using Microsoft.Azure.Commands.RecoveryServices.SiteRecovery.Properties;
using Microsoft.Azure.Management.RecoveryServices.SiteRecovery.Models;

namespace Microsoft.Azure.Commands.RecoveryServices.SiteRecovery
{
    /// <summary>
    ///     Updates Azure Site Recovery Policy.
    /// </summary>
    [Cmdlet(
        VerbsData.Update,
        "AzureRmRecoveryServicesAsrPolicy",
        SupportsShouldProcess = true,
        DefaultParameterSetName = ASRParameterSets.Default)]
    [Alias("Update-ASRPolicy")]
    [OutputType(typeof(ASRJob))]
    public class UpdateAzureRmRecoveryServicesAsrPolicy : SiteRecoveryCmdletBase
    {
        /// <summary>
        ///    Switch parameter indicating that the specfied policy is used to replicate VMware virtual machines to Azure.
        /// </summary>
        [Parameter(
            Position = 0,
            ParameterSetName = ASRParameterSets.VMwareToAzure,
            Mandatory = true)]
        public SwitchParameter VMwareToAzure { get; set; }

        /// <summary>
        ///    Switch parameter specifying that the replication policy used to replicate Azure virtual machines between 
        ///    two Azure regions will be updated.
        /// </summary>
        [Parameter(
            ParameterSetName = ASRParameterSets.AzureToAzure,
            Mandatory = true)]
        public SwitchParameter AzureToAzure { get; set; }

        /// <summary>
        ///    Switch parameter indicating that the specfied policy is used to replicate failed over virtual machines 
        ///    running in Azure back to an on-premises VMware site.
        /// </summary>
        [Parameter(Position = 0,
            ParameterSetName = ASRParameterSets.AzureToVMware,
            Mandatory = true)]
        public SwitchParameter AzureToVMware { get; set; }

        /// <summary>
        ///    Switch parameter indicating that the specfied policy is used to replicate Hyper-V virtual machines to Azure.
        /// </summary>
        [Parameter(
            Position = 0,
            ParameterSetName = ASRParameterSets.HyperVToAzure,
            Mandatory = true)]
        public SwitchParameter HyperVToAzure { get; set; }

        /// <summary>
        ///    Switch parameter indicating that the specfied policy is used to replicate VMM managed Hyper-V virtual machines
        ///    between two Hyper-V sites.
        /// </summary>
        [Parameter(
            Position = 0,
            ParameterSetName = ASRParameterSets.EnterpriseToEnterprise,
            Mandatory = true)]
        public SwitchParameter VmmToVmm { get; set; }

        /// <summary>
        ///     Gets or sets ASR replication policy object corresponding to the replication policy to be updated.
        /// </summary>
        [Parameter(
            Mandatory = true,
            ValueFromPipeline = true)]
        [Alias("Policy")]
        public ASRPolicy InputObject { get; set; }

        /// <summary>
        ///     Gets or sets a value for Replication Method of the Policy.
        /// </summary>
        [Parameter(ParameterSetName = ASRParameterSets.EnterpriseToEnterprise)]
        [Parameter(ParameterSetName = ASRParameterSets.Default)]
        [ValidateNotNullOrEmpty]
        [ValidateSet(
            Constants.OnlineReplicationMethod,
            Constants.OfflineReplicationMethod)]
        public string ReplicationMethod { get; set; }

        /// <summary>
        ///     Gets or sets the replication frequency interval in seconds
        /// </summary>
        [Parameter(ParameterSetName = ASRParameterSets.EnterpriseToEnterprise)]
        [Parameter(ParameterSetName = ASRParameterSets.HyperVToAzure)]
        [Parameter(ParameterSetName = ASRParameterSets.Default)]
        [ValidateNotNullOrEmpty]
        [ValidateSet(
            Constants.Thirty,
            Constants.ThreeHundred,
            Constants.NineHundred)]
        public string ReplicationFrequencyInSeconds { get; set; }

        /// <summary>
        ///     Gets or sets the number recovery points to retain.
        /// </summary>
        [Parameter(ParameterSetName = ASRParameterSets.EnterpriseToEnterprise)]
        [Parameter(ParameterSetName = ASRParameterSets.HyperVToAzure)]
        [Parameter(ParameterSetName = ASRParameterSets.Default)]
        [ValidateNotNullOrEmpty]
        [Alias("RecoveryPoints")]
        public int NumberOfRecoveryPointsToRetain { get; set; }

        /// <summary>
        ///     Gets or sets time in hours to retain recovery points after creation.
        /// </summary>
        [Parameter(ParameterSetName = ASRParameterSets.AzureToVMware)]
        [Parameter(ParameterSetName = ASRParameterSets.VMwareToAzure)]
        [Parameter(ParameterSetName = ASRParameterSets.AzureToAzure)]
        [ValidateNotNullOrEmpty]
        public int RecoveryPointRetentionInHours { get; set; }

        /// <summary>
        ///     Gets or sets the frequency(in hours) at which to create application consistent recovery points.
        /// </summary>
        [Parameter]
        [ValidateNotNullOrEmpty]
        public int ApplicationConsistentSnapshotFrequencyInHours { get; set; }

        /// <summary>
        ///     Gets or sets compression should be enabled/ disabled.
        /// </summary>
        [Parameter(ParameterSetName = ASRParameterSets.EnterpriseToEnterprise)]
        [Parameter(ParameterSetName = ASRParameterSets.Default)]
        [ValidateNotNullOrEmpty]
        [ValidateSet(
            Constants.Enable,
            Constants.Disable)]
        public string Compression { get; set; }

        /// <summary>
        ///     Gets or sets the port used for replication.
        /// </summary>
        [Parameter(ParameterSetName = ASRParameterSets.EnterpriseToEnterprise)]
        [Parameter(ParameterSetName = ASRParameterSets.Default)]
        [ValidateNotNullOrEmpty]
        public ushort ReplicationPort { get; set; }

        /// <summary>
        ///     Gets or sets the type of authentication used.
        /// </summary>
        [Parameter(ParameterSetName = ASRParameterSets.EnterpriseToEnterprise)]
        [Parameter(ParameterSetName = ASRParameterSets.Default)]
        [ValidateNotNullOrEmpty]
        [ValidateSet(
            Constants.AuthenticationTypeCertificate,
            Constants.AuthenticationTypeKerberos)]
        public string Authentication { get; set; }

        /// <summary>
        ///     Gets or sets the replication start time.
        /// </summary>
        [Parameter(ParameterSetName = ASRParameterSets.EnterpriseToEnterprise)]
        [Parameter(ParameterSetName = ASRParameterSets.HyperVToAzure)]
        [Parameter(ParameterSetName = ASRParameterSets.Default)]
        [ValidateNotNullOrEmpty]
        public TimeSpan? ReplicationStartTime { get; set; }

        /// <summary>
        ///     Gets or sets if the replica virtual machine should be deleted on disabling replication 
        ///     from a VMM managed site to another.
        /// </summary>
        [Parameter(ParameterSetName = ASRParameterSets.EnterpriseToEnterprise)]
        [Parameter(ParameterSetName = ASRParameterSets.Default)]
        [ValidateNotNullOrEmpty]
        [ValidateSet(
            Constants.Required,
            Constants.NotRequired)]
        public string ReplicaDeletion { get; set; }

        /// <summary>
        ///     Gets or sets Specifies the Azure storage account ID of the replication target.
        ///     Used as the target storage account for replication if an alternate is not provided.
        /// </summary>
        [Parameter(ParameterSetName = ASRParameterSets.HyperVToAzure)]
        [Parameter(ParameterSetName = ASRParameterSets.Default)]
        [ValidateNotNullOrEmpty]
        public string RecoveryAzureStorageAccountId { get; set; }

        /// <summary>
        ///     Gets or sets encryption should be enabled or disabled.
        /// </summary>
        [Parameter(ParameterSetName = ASRParameterSets.HyperVToAzure)]
        [Parameter(ParameterSetName = ASRParameterSets.Default)]
        [ValidateNotNullOrEmpty]
        [ValidateSet(
            Constants.Enable,
            Constants.Disable)]
        public string Encryption { get; set; }

        /// <summary>
        ///     Gets or sets multiVm sync status for the policy.
        /// </summary>
        [Parameter(DontShow = true, ParameterSetName = ASRParameterSets.VMwareToAzure)]
        [Parameter(DontShow = true, ParameterSetName = ASRParameterSets.AzureToVMware)]
        [Parameter(DontShow = true, ParameterSetName = ASRParameterSets.AzureToAzure)]
        [ValidateNotNullOrEmpty]
        [DefaultValue(Constants.Enable)]
        [ValidateSet(Constants.Enable, Constants.Disable)]
        public string MultiVmSyncStatus { get; set; }

        /// <summary>
        ///     Gets or sets RPO threshold value in minutes to warn on.
        /// </summary>
        [Parameter(ParameterSetName = ASRParameterSets.VMwareToAzure)]
        [Parameter(ParameterSetName = ASRParameterSets.AzureToVMware)]
        [ValidateNotNullOrEmpty]
        public int RPOWarningThresholdInMinutes { get; set; }

        /// <summary>
        ///     ProcessRecord of the command.
        /// </summary>
        public override void ExecuteSiteRecoveryCmdlet()
        {
            base.ExecuteSiteRecoveryCmdlet();

            var policy = new ASRPolicy(RecoveryServicesClient.GetAzureSiteRecoveryPolicy(InputObject.Name));
            if (!policy.ReplicationProvider.Equals(InputObject.ReplicationProvider, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    string.Format(
                        Resources.IncorrectReplicationProvider,
                        InputObject.ReplicationProvider));
            }

            if (ShouldProcess(
                InputObject.FriendlyName,
                VerbsData.Update))
            {
                switch (ParameterSetName)
                {
                    case ASRParameterSets.EnterpriseToEnterprise:
                        EnterpriseToEnterprisePolicyObject();
                        break;
                    case ASRParameterSets.HyperVToAzure:
                        HyperVToAzurePolicyObject();
                        break;
                    case ASRParameterSets.VMwareToAzure:
                        UpdateV2APolicyObject();
                        break;
                    case ASRParameterSets.AzureToVMware:
                        UpdateV2VPolicyObject();
                        break;
                    case ASRParameterSets.AzureToAzure:
                        UpdateA2APolicy();
                        break;
                    case ASRParameterSets.Default:
                        DefaultUpdatePolicy();
                        break;
                }
            }
        }

        private void DefaultUpdatePolicy()
        {
            if (string.Compare(
                    InputObject.ReplicationProvider,
                    Constants.HyperVReplica2012,
                    StringComparison.OrdinalIgnoreCase) ==
                0 ||
                    string.Compare(
                        InputObject.ReplicationProvider,
                        Constants.HyperVReplica2012R2,
                        StringComparison.OrdinalIgnoreCase) ==
                    0)
            {
                EnterpriseToEnterprisePolicyObject();
            }
            else if (string.Compare(
                         InputObject.ReplicationProvider,
                         Constants.HyperVReplicaAzure,
                         StringComparison.OrdinalIgnoreCase) ==
                     0)
            {
                HyperVToAzurePolicyObject();
            }

            // Not  HyperVReplicaAzure, HyperVReplica2012, HyperVReplica2012R2 policy
            throw new InvalidOperationException(
                     string.Format(
                         Resources.IncorrectReplicationProvider,
                         InputObject.ReplicationProvider));
        }

        /// <summary>
        ///     Creates an E2A Policy Object
        /// </summary>
        private void HyperVToAzurePolicyObject()
        {
            if (string.Compare(
                    InputObject.ReplicationProvider,
                    Constants.HyperVReplicaAzure,
                    StringComparison.OrdinalIgnoreCase) !=
                0)
            {
                throw new InvalidOperationException(
                    string.Format(
                        Resources.IncorrectReplicationProvider,
                        InputObject.ReplicationProvider));
            }

            var replicationProviderSettings =
                InputObject.ReplicationProviderSettings as ASRHyperVReplicaAzurePolicyDetails;

            replicationFrequencyInSeconds =
                MyInvocation.BoundParameters.ContainsKey(
                    Utilities.GetMemberName(() => ReplicationFrequencyInSeconds))
                    ? PSRecoveryServicesClient.ConvertReplicationFrequencyToUshort(
                        ReplicationFrequencyInSeconds) : (ushort)replicationProviderSettings
                        .ReplicationFrequencyInSeconds;
            recoveryPoints =
                MyInvocation.BoundParameters.ContainsKey(
                    Utilities.GetMemberName(() => NumberOfRecoveryPointsToRetain)) ? NumberOfRecoveryPointsToRetain
                    : replicationProviderSettings.RecoveryPoints;
            applicationConsistentSnapshotFrequencyInHours =
                MyInvocation.BoundParameters.ContainsKey(
                    Utilities.GetMemberName(
                        () => ApplicationConsistentSnapshotFrequencyInHours))
                    ? ApplicationConsistentSnapshotFrequencyInHours
                    : replicationProviderSettings.ApplicationConsistentSnapshotFrequencyInHours;
            replicationStartTime =
                MyInvocation.BoundParameters.ContainsKey(
                    Utilities.GetMemberName(() => ReplicationStartTime))
                    ? replicationStartTime
                    : replicationProviderSettings.OnlineReplicationStartTime;
            recoveryAzureStorageAccountId =
                MyInvocation.BoundParameters.ContainsKey(
                    Utilities.GetMemberName(() => RecoveryAzureStorageAccountId))
                    ? RecoveryAzureStorageAccountId
                    : replicationProviderSettings.ActiveStorageAccountId;
            encryption =
                MyInvocation.BoundParameters.ContainsKey(
                    Utilities.GetMemberName(() => Encryption)) ? Encryption
                    : (string.Compare(
                           replicationProviderSettings.Encryption,
                           "Disabled",
                           StringComparison.OrdinalIgnoreCase) ==
                       0 ? Constants.Disable : Constants.Enable);

            var hyperVReplicaAzurePolicyInput = new HyperVReplicaAzurePolicyInput
            {
                ApplicationConsistentSnapshotFrequencyInHours =
                    applicationConsistentSnapshotFrequencyInHours,
                Encryption = encryption,
                OnlineReplicationStartTime = replicationStartTime.ToString(),
                RecoveryPointHistoryDuration = recoveryPoints,
                ReplicationInterval = replicationFrequencyInSeconds
            };

            hyperVReplicaAzurePolicyInput.StorageAccounts = new List<string>();

            if (RecoveryAzureStorageAccountId != null)
            {
                var storageAccount = recoveryAzureStorageAccountId;
                hyperVReplicaAzurePolicyInput.StorageAccounts.Add(storageAccount);
            }

            var updatePolicyProperties = new UpdatePolicyInputProperties
            {
                ReplicationProviderSettings = hyperVReplicaAzurePolicyInput
            };

            var updatePolicyInput = new UpdatePolicyInput { Properties = updatePolicyProperties };

            var response = RecoveryServicesClient.UpdatePolicy(
                InputObject.Name,
                updatePolicyInput);

            var jobResponse = RecoveryServicesClient.GetAzureSiteRecoveryJobDetails(
                PSRecoveryServicesClient.GetJobIdFromReponseLocation(response.Location));

            WriteObject(new ASRJob(jobResponse));
        }

        /// <summary>
        ///     Creates an E2E Policy object
        /// </summary>
        private void EnterpriseToEnterprisePolicyObject()
        {
            if (string.Compare(
                    InputObject.ReplicationProvider,
                    Constants.HyperVReplica2012,
                    StringComparison.OrdinalIgnoreCase) !=
                0 &&
                string.Compare(
                    InputObject.ReplicationProvider,
                    Constants.HyperVReplica2012R2,
                    StringComparison.OrdinalIgnoreCase) !=
                0)
            {
                throw new InvalidOperationException(
                    string.Format(
                        Resources.IncorrectReplicationProvider,
                        InputObject.ReplicationProvider));
            }

            var replicationProviderSettings =
                InputObject.ReplicationProviderSettings as ASRHyperVReplicaPolicyDetails;

            replicationMethod =
                MyInvocation.BoundParameters.ContainsKey(
                    Utilities.GetMemberName(() => ReplicationMethod)) ? (string.Compare(
                                                                                  ReplicationMethod,
                                                                                  Constants
                                                                                      .OnlineReplicationMethod,
                                                                                  StringComparison
                                                                                      .OrdinalIgnoreCase) ==
                                                                              0 ? "OverNetwork"
                    : "Offline") : (string.Compare(
                                        replicationProviderSettings.InitialReplicationMethod,
                                        Constants.OnlineReplicationMethod,
                                        StringComparison.OrdinalIgnoreCase) ==
                                    0 ? "OverNetwork" : "Offline");
            replicationFrequencyInSeconds =
                MyInvocation.BoundParameters.ContainsKey(
                    Utilities.GetMemberName(() => ReplicationFrequencyInSeconds))
                    ? PSRecoveryServicesClient.ConvertReplicationFrequencyToUshort(
                        ReplicationFrequencyInSeconds) : replicationProviderSettings
                        .ReplicationFrequencyInSeconds;
            recoveryPoints =
                MyInvocation.BoundParameters.ContainsKey(
                    Utilities.GetMemberName(() => NumberOfRecoveryPointsToRetain)) ? NumberOfRecoveryPointsToRetain
                    : replicationProviderSettings.RecoveryPoints;
            applicationConsistentSnapshotFrequencyInHours =
                MyInvocation.BoundParameters.ContainsKey(
                    Utilities.GetMemberName(
                        () => ApplicationConsistentSnapshotFrequencyInHours))
                    ? ApplicationConsistentSnapshotFrequencyInHours
                    : replicationProviderSettings.ApplicationConsistentSnapshotFrequencyInHours;
            compression =
                MyInvocation.BoundParameters.ContainsKey(
                    Utilities.GetMemberName(() => Compression)) ? Compression
                    : replicationProviderSettings.Compression;
            replicationPort =
                MyInvocation.BoundParameters.ContainsKey(
                    Utilities.GetMemberName(() => ReplicationPort)) ? ReplicationPort
                    : replicationProviderSettings.ReplicationPort;
            authentication =
                MyInvocation.BoundParameters.ContainsKey(
                    Utilities.GetMemberName(() => Authentication)) ? (ushort)(string.Compare(
                                                                                       Authentication,
                                                                                       Constants
                                                                                           .AuthenticationTypeKerberos,
                                                                                       StringComparison
                                                                                           .OrdinalIgnoreCase) ==
                                                                                   0 ? 1 : 2)
                    : (ushort)(string.Compare(
                                   replicationProviderSettings.AllowedAuthenticationType,
                                   Constants.AuthenticationTypeKerberos,
                                   StringComparison.OrdinalIgnoreCase) ==
                               0 ? 1 : 2);
            replicationStartTime =
                MyInvocation.BoundParameters.ContainsKey(
                    Utilities.GetMemberName(() => ReplicationStartTime))
                    ? replicationStartTime
                    : replicationProviderSettings.OnlineReplicationStartTime;
            replicaDeletion =
                MyInvocation.BoundParameters.ContainsKey(
                    Utilities.GetMemberName(() => ReplicaDeletion)) ? ReplicaDeletion
                    : replicationProviderSettings.ReplicaDeletionOption;

            var updatePolicyProperties = new UpdatePolicyInputProperties();

            if (string.Compare(
                    InputObject.ReplicationProvider,
                    Constants.HyperVReplica2012,
                    StringComparison.OrdinalIgnoreCase) ==
                0)
            {
                updatePolicyProperties.ReplicationProviderSettings = new HyperVReplicaPolicyInput
                {
                    AllowedAuthenticationType = authentication,
                    ApplicationConsistentSnapshotFrequencyInHours =
                        applicationConsistentSnapshotFrequencyInHours,
                    Compression = compression,
                    InitialReplicationMethod = replicationMethod,
                    OnlineReplicationStartTime = replicationStartTime.ToString(),
                    RecoveryPoints = recoveryPoints,
                    ReplicaDeletion = replicaDeletion,
                    ReplicationPort = replicationPort
                };
            }
            else
            {
                updatePolicyProperties.ReplicationProviderSettings =
                    new HyperVReplicaBluePolicyInput
                    {
                        AllowedAuthenticationType = authentication,
                        ApplicationConsistentSnapshotFrequencyInHours =
                            applicationConsistentSnapshotFrequencyInHours,
                        Compression = compression,
                        InitialReplicationMethod = replicationMethod,
                        OnlineReplicationStartTime = replicationStartTime.ToString(),
                        RecoveryPoints = recoveryPoints,
                        ReplicaDeletion = replicaDeletion,
                        ReplicationPort = replicationPort,
                        ReplicationFrequencyInSeconds = replicationFrequencyInSeconds
                    };
            }

            var updatePolicyInput = new UpdatePolicyInput { Properties = updatePolicyProperties };

            var responseBlue = RecoveryServicesClient.UpdatePolicy(
                InputObject.Name,
                updatePolicyInput);

            var jobResponseBlue = RecoveryServicesClient.GetAzureSiteRecoveryJobDetails(
                PSRecoveryServicesClient.GetJobIdFromReponseLocation(responseBlue.Location));

            WriteObject(new ASRJob(jobResponseBlue));
        }

        /// <summary>
        ///     Updates an InMageAzureV2 Policy Object.
        /// </summary>
        private void UpdateV2APolicyObject()
        {
            if (string.Compare(
                    InputObject.ReplicationProvider,
                    Constants.InMageAzureV2,
                    StringComparison.OrdinalIgnoreCase) !=
                0)
            {
                throw new InvalidOperationException(
                    string.Format(
                        Resources.IncorrectReplicationProvider,
                        InputObject.ReplicationProvider));
            }

            // Get the InMageAzureV2 Provider specific details from the Policy.
            var replicationProviderSettings =
                InputObject.ReplicationProviderSettings as ASRInMageAzureV2PolicyDetails;

            // Set the Paremeters to be updated.
            applicationConsistentSnapshotFrequencyInMinutes =
                MyInvocation.BoundParameters.ContainsKey(
                    Utilities.GetMemberName(
                        () =>
                            applicationConsistentSnapshotFrequencyInHours))
                    ? ApplicationConsistentSnapshotFrequencyInHours * 60
                    : replicationProviderSettings.AppConsistentFrequencyInMinutes;
            RecoveryPointRetentionInHours =
                MyInvocation.BoundParameters.ContainsKey(
                    Utilities.GetMemberName(
                        () =>
                            RecoveryPointRetentionInHours))
                    ? RecoveryPointRetentionInHours
                    : replicationProviderSettings.RecoveryPointHistory / 60;
            rpoWarningThresholdInMinutes =
                MyInvocation.BoundParameters.ContainsKey(
                    Utilities.GetMemberName(
                        () =>
                            RPOWarningThresholdInMinutes))
                    ? RPOWarningThresholdInMinutes
                    : replicationProviderSettings.RecoveryPointThresholdInMinutes;
            multiVmSyncStatus =
                MyInvocation.BoundParameters.ContainsKey(
                    Utilities.GetMemberName(
                        () =>
                            MultiVmSyncStatus))
                    ? MultiVmSyncStatus
                    : (replicationProviderSettings.MultiVmSyncStatus.Equals("Enabled")
                        ? Constants.Enable
                        : Constants.Disable);
            crashConsistentFrequencyInMinutes =
                replicationProviderSettings.CrashConsistentFrequencyInMinutes;

            // Set the Provider Specific Input for InMageAzureV2.
            var inmageAzureV2PolicyInput = new InMageAzureV2PolicyInput
            {
                AppConsistentFrequencyInMinutes =
                    applicationConsistentSnapshotFrequencyInMinutes,
                RecoveryPointHistory = RecoveryPointRetentionInHours * 60, // Convert from hours to minutes.
                RecoveryPointThresholdInMinutes = rpoWarningThresholdInMinutes,
                MultiVmSyncStatus = (SetMultiVmSyncStatus)Enum.Parse(
                    typeof(SetMultiVmSyncStatus),
                    multiVmSyncStatus),
                CrashConsistentFrequencyInMinutes = crashConsistentFrequencyInMinutes
            };

            var updatePolicyProperties = new UpdatePolicyInputProperties
            {
                ReplicationProviderSettings = inmageAzureV2PolicyInput
            };

            // Create the Update Policy Input.
            var updatePolicyInput = new UpdatePolicyInput
            {
                Properties = updatePolicyProperties
            };

            var response = RecoveryServicesClient.UpdatePolicy(
                InputObject.Name,
                updatePolicyInput);

            var jobResponse = RecoveryServicesClient.GetAzureSiteRecoveryJobDetails(
                PSRecoveryServicesClient.GetJobIdFromReponseLocation(response.Location));

            WriteObject(new ASRJob(jobResponse));
        }

        /// <summary>
        ///     Updates an InMage Policy Object.
        /// </summary>
        private void UpdateV2VPolicyObject()
        {
            if (string.Compare(
                    InputObject.ReplicationProvider,
                    Constants.InMage,
                    StringComparison.OrdinalIgnoreCase) !=
                0)
            {
                throw new InvalidOperationException(
                    string.Format(
                        Resources.IncorrectReplicationProvider,
                        InputObject.ReplicationProvider));
            }

            // Get the InMage Provider specific details from the Policy.
            var replicationProviderSettings =
                InputObject.ReplicationProviderSettings as ASRInMagePolicyDetails;

            // Set the Paremeters to be updated.
            applicationConsistentSnapshotFrequencyInMinutes =
                MyInvocation.BoundParameters.ContainsKey(
                    Utilities.GetMemberName(
                        () =>
                            ApplicationConsistentSnapshotFrequencyInHours))
                    ? ApplicationConsistentSnapshotFrequencyInHours * 60
                    : replicationProviderSettings.AppConsistentFrequencyInMinutes;
            RecoveryPointRetentionInHours =
                MyInvocation.BoundParameters.ContainsKey(
                    Utilities.GetMemberName(
                        () =>
                            RecoveryPointRetentionInHours))
                    ? RecoveryPointRetentionInHours
                    : replicationProviderSettings.RecoveryPointHistory / 60;
            rpoWarningThresholdInMinutes =
                MyInvocation.BoundParameters.ContainsKey(
                    Utilities.GetMemberName(
                        () =>
                            RPOWarningThresholdInMinutes))
                    ? RPOWarningThresholdInMinutes
                    : replicationProviderSettings.RecoveryPointThresholdInMinutes;
            multiVmSyncStatus =
                MyInvocation.BoundParameters.ContainsKey(
                    Utilities.GetMemberName(
                        () =>
                            MultiVmSyncStatus))
                    ? MultiVmSyncStatus
                    : (replicationProviderSettings.MultiVmSyncStatus.Equals("Enabled")
                        ? Constants.Enable
                        : Constants.Disable);

            // Set the Provider Specific Input for InMage.
            var inmagePolicyInput = new InMagePolicyInput
            {
                AppConsistentFrequencyInMinutes =
                    applicationConsistentSnapshotFrequencyInMinutes,
                RecoveryPointHistory = RecoveryPointRetentionInHours * 60, // Convert from hours to minutes.
                RecoveryPointThresholdInMinutes = rpoWarningThresholdInMinutes,
                MultiVmSyncStatus = (SetMultiVmSyncStatus)Enum.Parse(
                    typeof(SetMultiVmSyncStatus),
                    multiVmSyncStatus)
            };

            var updatePolicyProperties = new UpdatePolicyInputProperties
            {
                ReplicationProviderSettings = inmagePolicyInput
            };

            // Create the Update Policy Input.
            var updatePolicyInput = new UpdatePolicyInput
            {
                Properties = updatePolicyProperties
            };

            var response = RecoveryServicesClient.UpdatePolicy(
                InputObject.Name,
                updatePolicyInput);

            var jobResponse = RecoveryServicesClient.GetAzureSiteRecoveryJobDetails(
                PSRecoveryServicesClient.GetJobIdFromReponseLocation(response.Location));

            WriteObject(new ASRJob(jobResponse));
        }

        /// <summary>
        ///     Updates an Azure to Azure Policy.
        /// </summary>
        private void UpdateA2APolicy()
        {
            if (string.Compare(
                    InputObject.ReplicationProvider,
                    Constants.A2A,
                    StringComparison.OrdinalIgnoreCase) !=
                0)
            {
                throw new InvalidOperationException(
                    string.Format(
                        Resources.IncorrectReplicationProvider,
                        InputObject.ReplicationProvider));
            }

            // Get the A2A Provider specific details from the Policy.
            var replicationProviderSettings =
                InputObject.ReplicationProviderSettings as ASRAzureToAzurePolicyDetails;

            // Set the Paremeters to be updated.
            applicationConsistentSnapshotFrequencyInMinutes =
                MyInvocation.BoundParameters.ContainsKey(
                    Utilities.GetMemberName(
                        () =>
                            applicationConsistentSnapshotFrequencyInHours))
                    ? ApplicationConsistentSnapshotFrequencyInHours * 60
                    : replicationProviderSettings.AppConsistentFrequencyInMinutes;

            crashConsistentFrequencyInMinutes = 
                replicationProviderSettings.CrashConsistentFrequencyInMinutes;

            RecoveryPointRetentionInHours =
                MyInvocation.BoundParameters.ContainsKey(
                    Utilities.GetMemberName(
                        () =>
                            RecoveryPointRetentionInHours))
                    ? RecoveryPointRetentionInHours
                    : replicationProviderSettings.RecoveryPointHistory / 60;
            rpoWarningThresholdInMinutes =
                MyInvocation.BoundParameters.ContainsKey(
                    Utilities.GetMemberName(
                        () =>
                            RPOWarningThresholdInMinutes))
                    ? RPOWarningThresholdInMinutes
                    : replicationProviderSettings.RecoveryPointThresholdInMinutes;
            multiVmSyncStatus =
                MyInvocation.BoundParameters.ContainsKey(
                    Utilities.GetMemberName(
                        () =>
                            MultiVmSyncStatus))
                    ? MultiVmSyncStatus
                    : (replicationProviderSettings.MultiVmSyncStatus.Equals("Enabled")
                        ? Constants.Enable
                        : Constants.Disable);

            // Set the Provider Specific Input for AzureToAzure.
            var a2APolicyInput = new A2APolicyCreationInput
            {
                AppConsistentFrequencyInMinutes =
                    applicationConsistentSnapshotFrequencyInMinutes,
                RecoveryPointHistory = RecoveryPointRetentionInHours * 60, // Convert from hours to minutes.
                MultiVmSyncStatus = (SetMultiVmSyncStatus)Enum.Parse(
                    typeof(SetMultiVmSyncStatus),
                    multiVmSyncStatus),
                CrashConsistentFrequencyInMinutes = crashConsistentFrequencyInMinutes
            };

            var updatePolicyProperties = new UpdatePolicyInputProperties
            {
                ReplicationProviderSettings = a2APolicyInput
            };

            // Create the Update Policy Input.
            var updatePolicyInput = new UpdatePolicyInput
            {
                Properties = updatePolicyProperties
            };

            var response = RecoveryServicesClient.UpdatePolicy(
                InputObject.Name,
                updatePolicyInput);

            var jobResponse = RecoveryServicesClient.GetAzureSiteRecoveryJobDetails(
                PSRecoveryServicesClient.GetJobIdFromReponseLocation(response.Location));

            WriteObject(new ASRJob(jobResponse));
        }
        #region Private

        private string replicationMethod;
        private ushort replicationFrequencyInSeconds;
        private int recoveryPoints;
        private int applicationConsistentSnapshotFrequencyInHours;
        private string compression;
        private ushort replicationPort { get; set; }
        private ushort authentication { get; set; }
        private TimeSpan? replicationStartTime { get; set; }
        private string replicaDeletion { get; set; }
        private string recoveryAzureStorageAccountId { get; set; }
        private string encryption { get; set; }
        private int applicationConsistentSnapshotFrequencyInMinutes { get; set; }
        private int rpoWarningThresholdInMinutes { get; set; }
        private int crashConsistentFrequencyInMinutes { get; set; }
        private string multiVmSyncStatus { get; set; }

        #endregion Private
    }
}
