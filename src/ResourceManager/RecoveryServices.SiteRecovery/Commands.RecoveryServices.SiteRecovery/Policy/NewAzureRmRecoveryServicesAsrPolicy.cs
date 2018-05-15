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
    ///     Creates an Azure Site Recovery replication policy.
    /// </summary>
    [Cmdlet(
        VerbsCommon.New,
        "AzureRmRecoveryServicesAsrPolicy",
        DefaultParameterSetName = ASRParameterSets.HyperVToAzure,
        SupportsShouldProcess = true)]
    [Alias("New-ASRPolicy")]
    [OutputType(typeof(ASRJob))]
    public class NewAzureRmRecoveryServicesAsrPolicy : SiteRecoveryCmdletBase
    {
        /// <summary>
        ///    Switch parameter specifying that the replication policy being created will be used 
        ///    to replicate VMware virtual machines and/or Physical servers to Azure.
        /// </summary>
        [Parameter(
            ParameterSetName = ASRParameterSets.VMwareToAzure,
            Position = 0,
            Mandatory = true)]
        public SwitchParameter VMwareToAzure { get; set; }

        /// <summary>
        ///    Switch parameter specifying that the replication policy being created will be used to reverse replicate failed over 
        ///    VMware virtual machines and Physical servers running in Azure back to an on-premises VMware site.
        /// </summary>
        [Parameter(
            ParameterSetName = ASRParameterSets.AzureToVMware,
            Position = 0,
            Mandatory = true)]
        public SwitchParameter AzureToVMware { get; set; }

        /// <summary>
        ///    Switch parameter specifying that the replication policy being created will be used 
        ///    to replicated Azure virtual machines between two Azure regions.
        /// </summary>
        [Parameter(
            ParameterSetName = ASRParameterSets.AzureToAzure,
            Mandatory = true)]
        public SwitchParameter AzureToAzure { get; set; }

        /// <summary>
        ///    Switch parameter to specify policy is to be used to replicate Hyper-V virtual machines to Azure
        /// </summary>
        [Parameter(Position = 0,
            ParameterSetName = ASRParameterSets.HyperVToAzure,
            Mandatory = false)]
        public SwitchParameter HyperVToAzure { get; set; }

        /// <summary>
        ///    Switch parameter to specify policy is to be used to replicate between Hyper-V sites managed by a VMM server.
        /// </summary>
        [Parameter(Position = 0,
            ParameterSetName = ASRParameterSets.EnterpriseToEnterprise,
            Mandatory = false)]
        public SwitchParameter VmmToVmm { get; set; }

        /// <summary>
        ///     Gets or sets the name of the ASR replication policy.
        /// </summary>
        [Parameter(Mandatory = true)]
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets Replication Provider of the policy.
        /// </summary>
        [Parameter(
            ParameterSetName = ASRParameterSets.EnterpriseToEnterprise,
            Mandatory = true)]
        [Parameter(
            ParameterSetName = ASRParameterSets.HyperVToAzure,
            Mandatory = true)]
        [ValidateNotNullOrEmpty]
        [ValidateSet(
            Constants.HyperVReplica2012R2,
            Constants.HyperVReplica2012,
            Constants.HyperVReplicaAzure)]
        public string ReplicationProvider { get; set; }

        /// <summary>
        ///     Gets or sets a value for replication method of the policy.
        /// </summary>
        [Parameter(ParameterSetName = ASRParameterSets.EnterpriseToEnterprise)]
        [ValidateNotNullOrEmpty]
        [ValidateSet(
            Constants.OnlineReplicationMethod,
            Constants.OfflineReplicationMethod)]
        public string ReplicationMethod { get; set; }

        /// <summary>
        ///     Gets or sets the replication frequency interval in seconds.
        /// </summary>
        [Parameter(
            ParameterSetName = ASRParameterSets.EnterpriseToEnterprise,
            Mandatory = true)]
        [Parameter(
            ParameterSetName = ASRParameterSets.HyperVToAzure,
            Mandatory = true)]
        [ValidateNotNullOrEmpty]
        [ValidateSet(
            Constants.Thirty,
            Constants.ThreeHundred,
            Constants.NineHundred)]
        public string ReplicationFrequencyInSeconds { get; set; }

        /// <summary>
        ///     Gets or sets the number recovery points to retain.
        /// </summary>
        [Parameter(
            ParameterSetName = ASRParameterSets.EnterpriseToEnterprise)]
        [Parameter(
            ParameterSetName = ASRParameterSets.HyperVToAzure)]
        [ValidateNotNullOrEmpty]
        [DefaultValue(0)]
        [Alias("RecoveryPoints")]
        public int NumberOfRecoveryPointsToRetain { get; set; }

        /// <summary>
        ///     Gets or sets the recovery points for given time in hours.
        /// </summary>
        [Parameter(ParameterSetName = ASRParameterSets.VMwareToAzure, Mandatory = true)]
        [Parameter(ParameterSetName = ASRParameterSets.AzureToVMware, Mandatory = true)]
        [Parameter(ParameterSetName = ASRParameterSets.AzureToAzure, Mandatory = true)]
        [ValidateNotNullOrEmpty]
        [DefaultValue(0)]
        public int RecoveryPointRetentionInHours { get; set; }

        /// <summary>
        ///     Gets or sets the frequency(in hours) at which to create application consistent recovery points.
        /// </summary>
        [Parameter]
        [ValidateNotNullOrEmpty]
        [DefaultValue(0)]
        public int ApplicationConsistentSnapshotFrequencyInHours { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether Compression needs to be Enabled on the Policy.
        /// </summary>
        [Parameter(
            ParameterSetName = ASRParameterSets.EnterpriseToEnterprise)]
        [DefaultValue(Constants.Disable)]
        [ValidateSet(
            Constants.Enable,
            Constants.Disable)]
        public string Compression { get; set; }

        /// <summary>
        ///     Gets or sets the port used for replication.
        /// </summary>
        [Parameter(
            ParameterSetName = ASRParameterSets.EnterpriseToEnterprise, Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public ushort ReplicationPort { get; set; }

        /// <summary>
        ///     Gets or sets the Replication Port of the Policy.
        /// </summary>
        [Parameter(
            ParameterSetName = ASRParameterSets.EnterpriseToEnterprise)]
        [ValidateNotNullOrEmpty]
        [ValidateSet(
            Constants.AuthenticationTypeCertificate,
            Constants.AuthenticationTypeKerberos)]
        public string Authentication { get; set; }

        /// <summary>
        ///     Gets or sets the replication start time.
        /// </summary>
        [Parameter(
            ParameterSetName = ASRParameterSets.EnterpriseToEnterprise)]
        [Parameter(
            ParameterSetName = ASRParameterSets.HyperVToAzure)]
        [ValidateNotNullOrEmpty]
        public TimeSpan? ReplicationStartTime { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether Replica should be Deleted on
        ///     disabling protection of a protection entity protected by the Policy.
        /// </summary>
        [Parameter(
            ParameterSetName = ASRParameterSets.EnterpriseToEnterprise)]
        [DefaultValue(Constants.NotRequired)]
        [ValidateSet(
            Constants.Required,
            Constants.NotRequired)]
        public string ReplicaDeletion { get; set; }

        /// <summary>
        ///     Gets or sets the Azure storage account ID of the replication target.
        /// </summary>
        [Parameter(ParameterSetName = ASRParameterSets.HyperVToAzure)]
        [ValidateNotNullOrEmpty]
        public string RecoveryAzureStorageAccountId { get; set; }

        /// <summary>
        ///     Gets or sets if encryption should be enabled or disabled.
        /// </summary>
        [Parameter(ParameterSetName = ASRParameterSets.HyperVToAzure)]
        [DefaultValue(Constants.Disable)]
        [ValidateSet(
            Constants.Enable,
            Constants.Disable)]
        public string Encryption { get; set; }

        /// <summary>
        ///     Gets or sets the multiVm sync status for the policy.
        /// </summary>
        [Parameter(DontShow = true, ParameterSetName = ASRParameterSets.VMwareToAzure)]
        [Parameter(DontShow = true, ParameterSetName = ASRParameterSets.AzureToVMware)]
        [Parameter(DontShow = true, ParameterSetName = ASRParameterSets.AzureToAzure)]
        [ValidateNotNullOrEmpty]
        [DefaultValue(Constants.Enable)]
        [ValidateSet(Constants.Enable, Constants.Disable)]
        public string MultiVmSyncStatus { get; set; }

        /// <summary>
        ///     Gets or sets the RPO threshold value in minutes to warn on.
        /// </summary>
        [Parameter(ParameterSetName = ASRParameterSets.VMwareToAzure, Mandatory = true)]
        [Parameter(ParameterSetName = ASRParameterSets.AzureToVMware, Mandatory = true)]
        public int RPOWarningThresholdInMinutes { get; set; }

        /// <summary>
        ///     ProcessRecord of the command.
        /// </summary>
        public override void ExecuteSiteRecoveryCmdlet()
        {
            base.ExecuteSiteRecoveryCmdlet();

            if (ShouldProcess(
                Name,
                VerbsCommon.New))
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
                    case ASRParameterSets.AzureToVMware:
                        ReplicationProvider = ParameterSetName == ASRParameterSets.VMwareToAzure ?
                            Constants.InMageAzureV2
                            : Constants.InMage;
                        V2AandV2VPolicyObject();
                        break;
                    case ASRParameterSets.AzureToAzure:
                        ReplicationProvider = Constants.A2A;
                        CreateA2APolicy();
                        break;
                }
            }
        }

        /// <summary>
        ///     Creates an E2A Policy Object
        /// </summary>
        private void HyperVToAzurePolicyObject()
        {
            if (string.Compare(
                    ReplicationProvider,
                    Constants.HyperVReplicaAzure,
                    StringComparison.OrdinalIgnoreCase) !=
                0)
            {
                throw new InvalidOperationException(
                    string.Format(
                        Resources.IncorrectReplicationProvider,
                        ReplicationProvider));
            }

            PSRecoveryServicesClient.ValidateReplicationStartTime(ReplicationStartTime);

            var replicationFrequencyInSeconds =
                PSRecoveryServicesClient.ConvertReplicationFrequencyToUshort(
                    ReplicationFrequencyInSeconds);

            var hyperVReplicaAzurePolicyInput = new HyperVReplicaAzurePolicyInput
            {
                ApplicationConsistentSnapshotFrequencyInHours =
                    ApplicationConsistentSnapshotFrequencyInHours,
                Encryption =
                    MyInvocation.BoundParameters.ContainsKey(
                        Utilities.GetMemberName(() => Encryption)) ? Encryption
                        : Constants.Disable,
                OnlineReplicationStartTime =
                    ReplicationStartTime == null ? null : ReplicationStartTime.ToString(),
                RecoveryPointHistoryDuration = NumberOfRecoveryPointsToRetain,
                ReplicationInterval = replicationFrequencyInSeconds
            };

            hyperVReplicaAzurePolicyInput.StorageAccounts = new List<string>();

            if (RecoveryAzureStorageAccountId != null)
            {
                var storageAccount = RecoveryAzureStorageAccountId;
                hyperVReplicaAzurePolicyInput.StorageAccounts.Add(storageAccount);
            }

            var createPolicyInputProperties =
                new CreatePolicyInputProperties
                {
                    ProviderSpecificInput = hyperVReplicaAzurePolicyInput
                };

            var createPolicyInput =
                new CreatePolicyInput { Properties = createPolicyInputProperties };

            var response = RecoveryServicesClient.CreatePolicy(
                Name,
                createPolicyInput);

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
                    ReplicationProvider,
                    Constants.HyperVReplica2012,
                    StringComparison.OrdinalIgnoreCase) !=
                0 &&
                string.Compare(
                    ReplicationProvider,
                    Constants.HyperVReplica2012R2,
                    StringComparison.OrdinalIgnoreCase) !=
                0)
            {
                throw new InvalidOperationException(
                    string.Format(
                        Resources.IncorrectReplicationProvider,
                        ReplicationProvider));
            }

            PSRecoveryServicesClient.ValidateReplicationStartTime(ReplicationStartTime);

            var replicationFrequencyInSeconds =
                PSRecoveryServicesClient.ConvertReplicationFrequencyToUshort(
                    ReplicationFrequencyInSeconds);

            var createPolicyInputProperties = new CreatePolicyInputProperties();

            if (string.Compare(
                    ReplicationProvider,
                    Constants.HyperVReplica2012,
                    StringComparison.OrdinalIgnoreCase) ==
                0)
            {
                createPolicyInputProperties.ProviderSpecificInput = new HyperVReplicaPolicyInput
                {
                    AllowedAuthenticationType = (ushort)(string.Compare(
                                                             Authentication,
                                                             Constants.AuthenticationTypeKerberos,
                                                             StringComparison.OrdinalIgnoreCase) ==
                                                         0 ? 1 : 2),
                    ApplicationConsistentSnapshotFrequencyInHours =
                        ApplicationConsistentSnapshotFrequencyInHours,
                    Compression =
                        MyInvocation.BoundParameters.ContainsKey(
                            Utilities.GetMemberName(() => Compression)) ? Compression
                            : Constants.Disable,
                    InitialReplicationMethod = string.Compare(
                                                   ReplicationMethod,
                                                   Constants.OnlineReplicationMethod,
                                                   StringComparison.OrdinalIgnoreCase) ==
                                               0 ? "OverNetwork" : "Offline",
                    OnlineReplicationStartTime = ReplicationStartTime.ToString(),
                    RecoveryPoints = NumberOfRecoveryPointsToRetain,
                    ReplicaDeletion =
                        MyInvocation.BoundParameters.ContainsKey(
                            Utilities.GetMemberName(() => ReplicaDeletion))
                            ? ReplicaDeletion : Constants.NotRequired,
                    ReplicationPort = ReplicationPort
                };
            }
            else
            {
                createPolicyInputProperties.ProviderSpecificInput =
                    new HyperVReplicaBluePolicyInput
                    {
                        AllowedAuthenticationType = (ushort)(string.Compare(
                                                                 Authentication,
                                                                 Constants
                                                                     .AuthenticationTypeKerberos,
                                                                 StringComparison
                                                                     .OrdinalIgnoreCase) ==
                                                             0 ? 1 : 2),
                        ApplicationConsistentSnapshotFrequencyInHours =
                            ApplicationConsistentSnapshotFrequencyInHours,
                        Compression =
                            MyInvocation.BoundParameters.ContainsKey(
                                Utilities.GetMemberName(() => Compression)) ? Compression
                                : Constants.Disable,
                        InitialReplicationMethod = string.Compare(
                                                       ReplicationMethod,
                                                       Constants.OnlineReplicationMethod,
                                                       StringComparison.OrdinalIgnoreCase) ==
                                                   0 ? "OverNetwork" : "Offline",
                        OnlineReplicationStartTime = ReplicationStartTime.ToString(),
                        RecoveryPoints = NumberOfRecoveryPointsToRetain,
                        ReplicaDeletion =
                            MyInvocation.BoundParameters.ContainsKey(
                                Utilities.GetMemberName(() => ReplicaDeletion))
                                ? ReplicaDeletion : Constants.NotRequired,
                        ReplicationFrequencyInSeconds = replicationFrequencyInSeconds,
                        ReplicationPort = ReplicationPort
                    };
            }

            var createPolicyInput =
                new CreatePolicyInput { Properties = createPolicyInputProperties };

            var responseBlue = RecoveryServicesClient.CreatePolicy(
                Name,
                createPolicyInput);

            var jobResponseBlue = RecoveryServicesClient.GetAzureSiteRecoveryJobDetails(
                PSRecoveryServicesClient.GetJobIdFromReponseLocation(responseBlue.Location));

            WriteObject(new ASRJob(jobResponseBlue));
        }

        /// <summary>
        ///     Creates an InMageAzureV2 / InMage Policy Object.
        /// </summary>
        private void V2AandV2VPolicyObject()
        {
            // Validate the Replication Provider.
            if (string.Compare(
                    ReplicationProvider,
                    Constants.InMageAzureV2,
                    StringComparison.OrdinalIgnoreCase) !=
                0 &&
                string.Compare(
                    ReplicationProvider,
                    Constants.InMage,
                    StringComparison.OrdinalIgnoreCase) !=
                0)
            {
                throw new InvalidOperationException(
                    string.Format(
                        Resources.IncorrectReplicationProvider,
                        ReplicationProvider));
            }

            // Set the Default Parameters.
            ApplicationConsistentSnapshotFrequencyInHours = ApplicationConsistentSnapshotFrequencyInHours;
            RecoveryPointRetentionInHours =
                MyInvocation.BoundParameters.ContainsKey(
                    Utilities.GetMemberName(() => RecoveryPointRetentionInHours))
                    ? RecoveryPointRetentionInHours
                    : 24;
            RPOWarningThresholdInMinutes =
                MyInvocation.BoundParameters.ContainsKey(
                    Utilities.GetMemberName(() => RPOWarningThresholdInMinutes))
                    ? RPOWarningThresholdInMinutes
                    : 15;
            MultiVmSyncStatus =
                MyInvocation.BoundParameters.ContainsKey(
                    Utilities.GetMemberName(() => MultiVmSyncStatus))
                    ? MultiVmSyncStatus
                    : Constants.Enable;
            var crashConsistentFrequencyInMinutes = 5;

            // Create the Create Policy Input.
            var createPolicyInput = new CreatePolicyInput
            {
                Properties = new CreatePolicyInputProperties()
            };

            // Check the Replication Provider Type.
            if (string.Compare(
                    ReplicationProvider,
                    Constants.InMageAzureV2,
                    StringComparison.OrdinalIgnoreCase) ==
                0)
            {
                // Set the Provider Specific Input for InMageAzureV2.
                createPolicyInput.Properties.ProviderSpecificInput = new InMageAzureV2PolicyInput
                {
                    AppConsistentFrequencyInMinutes =
                        ApplicationConsistentSnapshotFrequencyInHours * 60,
                    RecoveryPointHistory =
                        RecoveryPointRetentionInHours * 60, // Convert from hours to minutes.
                    RecoveryPointThresholdInMinutes = RPOWarningThresholdInMinutes,
                    MultiVmSyncStatus = (SetMultiVmSyncStatus)Enum.Parse(
                                            typeof(SetMultiVmSyncStatus),
                                            MultiVmSyncStatus),
                    CrashConsistentFrequencyInMinutes = crashConsistentFrequencyInMinutes
                };
            }
            else
            {
                // Set the Provider Specific Input for InMage.
                createPolicyInput.Properties.ProviderSpecificInput = new InMagePolicyInput
                {
                    AppConsistentFrequencyInMinutes =
                        ApplicationConsistentSnapshotFrequencyInHours * 60,
                    RecoveryPointHistory =
                        RecoveryPointRetentionInHours * 60, // Convert from hours to minutes.
                    RecoveryPointThresholdInMinutes = RPOWarningThresholdInMinutes,
                    MultiVmSyncStatus = (SetMultiVmSyncStatus)Enum.Parse(
                                            typeof(SetMultiVmSyncStatus),
                                            MultiVmSyncStatus)
                };
            }

            var response = RecoveryServicesClient.CreatePolicy(Name, createPolicyInput);

            var jobId = PSRecoveryServicesClient.GetJobIdFromReponseLocation(response.Location);
            var jobResponse = RecoveryServicesClient
                .GetAzureSiteRecoveryJobDetails(jobId);

            WriteObject(new ASRJob(jobResponse));
        }

        /// <summary>
        /// Creates an A2A Policy.
        /// </summary>
        private void CreateA2APolicy()
        {
            MultiVmSyncStatus =
                MyInvocation.BoundParameters.ContainsKey(
                    Utilities.GetMemberName(() => MultiVmSyncStatus))
                    ? MultiVmSyncStatus
                    : Constants.Enable;
            var crashConsistentFrequencyInMinutes = 5;
            var a2aPolicyCreationInput = new A2APolicyCreationInput()
            {
                AppConsistentFrequencyInMinutes = ApplicationConsistentSnapshotFrequencyInHours * 60,
                CrashConsistentFrequencyInMinutes = crashConsistentFrequencyInMinutes,
                MultiVmSyncStatus = (SetMultiVmSyncStatus)Enum.Parse(
                                            typeof(SetMultiVmSyncStatus),
                                            MultiVmSyncStatus),
                RecoveryPointHistory = RecoveryPointRetentionInHours * 60
            };

            var createPolicyInputProperties = new CreatePolicyInputProperties()
            {
                ProviderSpecificInput = a2aPolicyCreationInput
            };

            var createPolicyInput = new CreatePolicyInput()
            {
                Properties = createPolicyInputProperties
            };

            var response =
                RecoveryServicesClient.CreatePolicy(Name, createPolicyInput);

            string jobId = PSRecoveryServicesClient.GetJobIdFromReponseLocation(response.Location);

            var jobResponse =
                RecoveryServicesClient
                .GetAzureSiteRecoveryJobDetails(jobId);

            WriteObject(new ASRJob(jobResponse));
        }
    }
}
