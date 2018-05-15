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
    ///     Starts the test failover cleanup operation.
    /// </summary>
    [Cmdlet(
        VerbsLifecycle.Start,
        "AzureRmRecoveryServicesAsrTestFailoverCleanupJob",
        DefaultParameterSetName = ASRParameterSets.ByRPIObject,
        SupportsShouldProcess = true)]
    [Alias("Start-ASRTestFailoverCleanupJob")]
    [OutputType(typeof(ASRJob))]
    public class StartAzureRmRecoveryServicesAsrTestFailoverCleanupJob : SiteRecoveryCmdletBase
    {
        /// <summary>
        ///     Gets or sets resource Id of replication protected item / recovery plan for cleaningup test failover.
        /// </summary>
        [Parameter(
            ParameterSetName = ASRParameterSets.ByResourceId,
            Mandatory = true,
            ValueFromPipelineByPropertyName = true)]
        public string ResourceId { get; set; }

        /// <summary>
        ///     Gets or sets recovery plan to perform the test failover cleanup on.
        /// </summary>
        [Parameter(
            ParameterSetName = ASRParameterSets.ByRPObject,
            Mandatory = true,
            ValueFromPipeline = true)]
        [ValidateNotNullOrEmpty]
        public ASRRecoveryPlan RecoveryPlan { get; set; }

        /// <summary>
        ///     Gets or sets replication protected item to perform the test failover cleanup on.
        /// </summary>
        [Parameter(
            ParameterSetName = ASRParameterSets.ByRPIObject,
            Mandatory = true,
            ValueFromPipeline = true)]
        [ValidateNotNullOrEmpty]
        public ASRReplicationProtectedItem ReplicationProtectedItem { get; set; }

        /// <summary>
        ///     Gets or sets user Comment for Test Failover.
        /// </summary>
        [Parameter(ParameterSetName = ASRParameterSets.ByRPObject, Mandatory = false)]
        [Parameter(ParameterSetName = ASRParameterSets.ByRPIObject, Mandatory = false)]
        [Parameter(ParameterSetName = ASRParameterSets.ByResourceId, Mandatory = false)]
        public string Comment { get; set; }

        /// <summary>
        ///     ProcessRecord of the command.
        /// </summary>
        public override void ExecuteSiteRecoveryCmdlet()
        {
            base.ExecuteSiteRecoveryCmdlet();
            if (ShouldProcess(
                "Protected item or Recovery plan",
                "Cleanup Test Failover"))
            {
                switch (ParameterSetName)
                {
                    case ASRParameterSets.ByRPObject:
                        // Refresh RP Object
                        var rp = RecoveryServicesClient.GetAzureSiteRecoveryRecoveryPlan(
                            RecoveryPlan.Name);
                        recoveryPlanName = RecoveryPlan.Name;
                        StartRpTestFailoverCleanup();
                        break;
                    case ASRParameterSets.ByRPIObject:
                        rpiId = ReplicationProtectedItem.ID;
                        StartRPITestFailoverCleanup();
                        break;
                    case ASRParameterSets.ByResourceId:
                        StartTFOCleanupByResourceId();
                        break;
                }
            }
        }

        /// <summary>
        ///     Starts PE Test failover cleanup.
        /// </summary>
        private void StartRPITestFailoverCleanup()
        {
            protectionContainerName =
                            Utilities.GetValueFromArmId(
                                ReplicationProtectedItem.ID,
                                ARMResourceTypeConstants.ReplicationProtectionContainers);
            fabricName = Utilities.GetValueFromArmId(
                ReplicationProtectedItem.ID,
                ARMResourceTypeConstants.ReplicationFabrics);

            var rpiName = Utilities.GetValueFromArmId(
                ReplicationProtectedItem.ID,
                ARMResourceTypeConstants.ReplicationProtectedItems);

            var testFailoverCleanupInputProperties = new TestFailoverCleanupInputProperties
            {
                Comments = Comment == null ? "" : Comment
            };

            var input = new TestFailoverCleanupInput
            {
                Properties = testFailoverCleanupInputProperties
            };

            var response = RecoveryServicesClient.StartAzureSiteRecoveryTestFailoverCleanup(
                fabricName,
                protectionContainerName,
                rpiName,
                input);

            var jobResponse = RecoveryServicesClient
                .GetAzureSiteRecoveryJobDetails(
                    PSRecoveryServicesClient
                        .GetJobIdFromReponseLocation(response.Location));

            WriteObject(new ASRJob(jobResponse));
        }

        /// <summary>
        ///     Starts RP Test failover cleanup.
        /// </summary>
        private void StartRpTestFailoverCleanup()
        {
           var recoveryPlanTestFailoverCleanupInputProperties =
                new RecoveryPlanTestFailoverCleanupInputProperties
                {
                    Comments = Comment
                };

            var recoveryPlanTestFailoverCleanupInput = new RecoveryPlanTestFailoverCleanupInput
            {
                Properties = recoveryPlanTestFailoverCleanupInputProperties
            };

            var response = RecoveryServicesClient.StartAzureSiteRecoveryTestFailoverCleanup(
                RecoveryPlan.Name,
                recoveryPlanTestFailoverCleanupInput);

            var jobResponse = RecoveryServicesClient
                .GetAzureSiteRecoveryJobDetails(
                    PSRecoveryServicesClient
                        .GetJobIdFromReponseLocation(response.Location));

            WriteObject(new ASRJob(jobResponse));
        }


        /// <summary>
        ///     Starts Test failover cleanup by resource ID.
        /// </summary>
        private void StartTFOCleanupByResourceId()
        {
            if (ResourceId.ToLower().Contains("/" + ARMResourceTypeConstants.RecoveryPlans.ToLower() + "/"))
            {
                recoveryPlanName = Utilities.GetValueFromArmId(
                    ResourceId,
                    ARMResourceTypeConstants.RecoveryPlans);

                StartRpTestFailoverCleanup();
            }
            else
            {
                rpiId = ResourceId;
                StartRPITestFailoverCleanup();
            }
           
        }

        #region Private Parameters

        /// <summary>
        ///     Gets or sets Name of the Protection Container.
        /// </summary>
        private string protectionContainerName;

        /// <summary>
        ///     Gets or sets Name of the Fabric.
        /// </summary>
        private string fabricName;

        /// <summary>
        ///     Gets or sets Name of the RecoveryPlan.
        /// </summary>
        private string recoveryPlanName;

        /// <summary>
        ///     Gets or sets Name of the RPI Id.
        /// </summary>
        private string rpiId;
        #endregion
    }
}