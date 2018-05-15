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
using Microsoft.Azure.Commands.RecoveryServices.SiteRecovery.Properties;
using Microsoft.Azure.Management.RecoveryServices.SiteRecovery.Models;

namespace Microsoft.Azure.Commands.RecoveryServices.SiteRecovery
{
    /// <summary>
    ///     Edits a ASR Recovery plan in memory.
    /// </summary>
    [Cmdlet(
        VerbsData.Edit,
        "AzureRmRecoveryServicesAsrRecoveryPlan",
        DefaultParameterSetName = ASRParameterSets.AppendGroup,
        SupportsShouldProcess = true)]
    [Alias(
        "Edit-ASRRP",
        "Edit-ASRRecoveryPlan")]
    [OutputType(typeof(ASRRecoveryPlan))]
    public class EditAzureRmRecoveryServicesAsrRecoveryPlan : SiteRecoveryCmdletBase
    {
        /// <summary>
        ///     Gets or sets the ASR recovery plan object to be edited
        /// </summary>
        [Parameter(
            Mandatory = true,
            ValueFromPipeline = true)]
        [ValidateNotNullOrEmpty]
        [Alias("RecoveryPlan")]
        public ASRRecoveryPlan InputObject { get; set; }

        /// <summary>
        ///     Switch parameter to append a recovery plan group to the recovery plan object.
        /// </summary>
        [Parameter(
            ParameterSetName = ASRParameterSets.AppendGroup,
            Mandatory = true)]
        public SwitchParameter AppendGroup { get; set; }

        /// <summary>
        ///     Gets or sets the specified group from the recovery plan object to be removed.
        /// </summary>
        [Parameter(
            ParameterSetName = ASRParameterSets.RemoveGroup,
            Mandatory = true)]
        public ASRRecoveryPlanGroup RemoveGroup { get; set; }

        /// <summary>
        ///     Gets or sets a recovery plan group.
        /// </summary>
        [Parameter(
            ParameterSetName = ASRParameterSets.AddReplicationProtectedItems,
            Mandatory = true)]
        [Parameter(
            ParameterSetName = ASRParameterSets.RemoveReplicationProtectedItems,
            Mandatory = true)]
        public ASRRecoveryPlanGroup Group { get; set; }

        /// <summary>
        ///     Gets or sets the list of ASR replication protected items to be added to the recovery plan group 
        ///     in the recovery plan object.
        /// </summary>
        [Parameter(
            ParameterSetName = ASRParameterSets.AddReplicationProtectedItems,
            Mandatory = true)]
        [Alias("AddProtectedItems")]
        public ASRReplicationProtectedItem[] AddProtectedItem { get; set; }

        /// <summary>
        ///     Gets or sets switch parameter
        /// </summary>
        [Parameter(
            ParameterSetName = ASRParameterSets.RemoveReplicationProtectedItems,
            Mandatory = true)]
        [Alias("RemoveProtectedItems")]
        public ASRReplicationProtectedItem[] RemoveProtectedItem { get; set; }

        /// <summary>
        ///     ProcessRecord of the command.
        /// </summary>
        public override void ExecuteSiteRecoveryCmdlet()
        {
            base.ExecuteSiteRecoveryCmdlet();

            if (ShouldProcess(
                InputObject.FriendlyName,
                VerbsData.Edit))
            {
                ASRRecoveryPlanGroup tempGroup;

                switch (ParameterSetName)
                {
                    case ASRParameterSets.AppendGroup:
                        var recoveryPlanGroup = new RecoveryPlanGroup
                        {
                            GroupType = RecoveryPlanGroupType.Boot,
                            ReplicationProtectedItems = new List<RecoveryPlanProtectedItem>(),
                            StartGroupActions = new List<RecoveryPlanAction>(),
                            EndGroupActions = new List<RecoveryPlanAction>()
                        };

                        InputObject.Groups.Add(
                            new ASRRecoveryPlanGroup(
                                "Group " + (InputObject.Groups.Count - 1),
                                recoveryPlanGroup));
                        break;
                    case ASRParameterSets.RemoveGroup:
                        tempGroup = InputObject.Groups.FirstOrDefault(
                            g => string.Compare(
                                     g.Name,
                                     RemoveGroup.Name,
                                     StringComparison.OrdinalIgnoreCase) ==
                                 0);

                        if (tempGroup != null)
                        {
                            InputObject.Groups.Remove(tempGroup);
                            InputObject = InputObject.RefreshASRRecoveryPlanGroupNames();
                        }
                        else
                        {
                            throw new PSArgumentException(
                                string.Format(
                                    Resources.GroupNotFoundInRecoveryPlan,
                                    RemoveGroup.Name,
                                    InputObject.FriendlyName));
                        }

                        break;
                    case ASRParameterSets.AddReplicationProtectedItems:
                        foreach (var rpi in AddProtectedItem)
                        {
                            var fabricName = Utilities.GetValueFromArmId(
                                rpi.ID,
                                ARMResourceTypeConstants.ReplicationFabrics);

                            var replicationProtectedItemResponse = RecoveryServicesClient
                                .GetAzureSiteRecoveryReplicationProtectedItem(
                                    fabricName,
                                    Utilities.GetValueFromArmId(
                                        rpi.ID,
                                        ARMResourceTypeConstants.ReplicationProtectionContainers),
                                    rpi.Name);

                            tempGroup = InputObject.Groups.FirstOrDefault(
                                g => string.Compare(
                                         g.Name,
                                         Group.Name,
                                         StringComparison.OrdinalIgnoreCase) ==
                                     0);

                            if (tempGroup != null)
                            {
                                foreach (var gp in InputObject.Groups)
                                {
                                    if (gp.ReplicationProtectedItems == null)
                                        continue;

                                    if (gp.ReplicationProtectedItems.Any(
                                        pi => string.Compare(
                                                  pi.Id,
                                                  replicationProtectedItemResponse.Id,
                                                  StringComparison.OrdinalIgnoreCase) ==
                                              0))
                                    {
                                        throw new PSArgumentException(
                                            string.Format(
                                                Resources.VMAlreadyPartOfGroup,
                                                rpi.FriendlyName,
                                                gp.Name,
                                                InputObject.FriendlyName));
                                    }
                                }

                                InputObject.Groups[InputObject.Groups.IndexOf(tempGroup)]
                                    .ReplicationProtectedItems
                                    .Add(replicationProtectedItemResponse);
                            }
                            else
                            {
                                throw new PSArgumentException(
                                    string.Format(
                                        Resources.GroupNotFoundInRecoveryPlan,
                                        Group.Name,
                                        InputObject.FriendlyName));
                            }
                        }

                        break;
                    case ASRParameterSets.RemoveReplicationProtectedItems:
                        foreach (var rpi in RemoveProtectedItem)
                        {
                            var fabricName = Utilities.GetValueFromArmId(
                                rpi.ID,
                                ARMResourceTypeConstants.ReplicationFabrics);

                            tempGroup = InputObject.Groups.FirstOrDefault(
                                g => string.Compare(
                                         g.Name,
                                         Group.Name,
                                         StringComparison.OrdinalIgnoreCase) ==
                                     0);

                            if (tempGroup != null)
                            {
                                var ReplicationProtectedItem = InputObject
                                    .Groups[InputObject.Groups.IndexOf(tempGroup)]
                                    .ReplicationProtectedItems.FirstOrDefault(
                                        pi => string.Compare(
                                                  pi.Id,
                                                  rpi.ID,
                                                  StringComparison.OrdinalIgnoreCase) ==
                                              0);

                                if (ReplicationProtectedItem != null)
                                {
                                    InputObject
                                        .Groups[InputObject.Groups.IndexOf(tempGroup)]
                                        .ReplicationProtectedItems.Remove(ReplicationProtectedItem);
                                }
                                else
                                {
                                    throw new PSArgumentException(
                                        string.Format(
                                            Resources.VMNotFoundInGroup,
                                            rpi.FriendlyName,
                                            Group.Name,
                                            InputObject.FriendlyName));
                                }
                            }
                            else
                            {
                                throw new PSArgumentException(
                                    string.Format(
                                        Resources.GroupNotFoundInRecoveryPlan,
                                        Group.Name,
                                        InputObject.FriendlyName));
                            }
                        }

                        break;
                }

                ;

                WriteObject(InputObject);
            }
        }
    }
}
