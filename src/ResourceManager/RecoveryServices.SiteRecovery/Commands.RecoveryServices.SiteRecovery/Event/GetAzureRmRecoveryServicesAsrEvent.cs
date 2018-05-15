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

namespace Microsoft.Azure.Commands.RecoveryServices.SiteRecovery
{
    /// <summary>
    ///     Gets details of Azure Site Recovery events in the vault.
    /// </summary>
    [Cmdlet(
        VerbsCommon.Get,
        "AzureRmRecoveryServicesAsrEvent",
        DefaultParameterSetName = ASRParameterSets.ByParam)]
    [Alias("Get-ASREvent")]
    [OutputType(typeof(IEnumerable<ASREvent>))]
    public class GetAzureRmRecoveryServicesAsrEvents : SiteRecoveryCmdletBase
    {
        /// <summary>
        ///     Gets or sets Resource Id.
        /// </summary>
        [Parameter(
            ParameterSetName = ASRParameterSets.ByResourceId,
            Mandatory = true,
            ValueFromPipelineByPropertyName = true)]
        public string ResourceId { get; set; }

        /// <summary>
        ///     Gets or sets the fabricId to filter.
        /// </summary>
        [Parameter(ParameterSetName = ASRParameterSets.ByFabricId, Mandatory = true)]
        public string FabricId { get; set; }

        /// <summary>
        ///     Gets or sets name of the event for search.
        /// </summary>
        [Parameter(ParameterSetName = ASRParameterSets.ByName, Mandatory = true)]
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets affected object friendly name for the search.
        /// </summary>
        [Parameter(ParameterSetName = ASRParameterSets.ByParam)]
        [Parameter(ParameterSetName = ASRParameterSets.ByFabricId)]
        [ValidateNotNullOrEmpty]
        public string AffectedObjectFriendlyName { get; set; }

        /// <summary>
        ///     Gets or sets the fabric  to filter the events by.
        /// </summary>
        [Parameter(ParameterSetName = ASRParameterSets.ByParam)]
        [ValidateNotNullOrEmpty]
        public ASRFabric Fabric { get; set; }

        /// <summary>
        ///     Gets or sets the severity to filter on..
        /// </summary>
        [Parameter(ParameterSetName = ASRParameterSets.ByParam)]
        [Parameter(ParameterSetName = ASRParameterSets.ByFabricId)]
        [ValidateSet(
            ASRHealthEventServerity.Critical,
            ASRHealthEventServerity.Warning,
            ASRHealthEventServerity.OK,
            ASRHealthEventServerity.Unknown)]
        public string Severity { get; set; }

        /// <summary>
        ///     Gets or sets the start time of the search window.
        ///     Use this parameter to get only those events that have occurred after the specified time.
        /// </summary>
        [Parameter(ParameterSetName = ASRParameterSets.ByParam)]
        [Parameter(ParameterSetName = ASRParameterSets.ByFabricId)]
        [ValidateNotNullOrEmpty]
        public DateTime StartTime { get; set; }

        /// <summary>
        ///     Gets or sets the end time of the search window.
        ///     Use this parameter to get only those events that have occurred before the specified time.
        /// </summary>
        [Parameter(ParameterSetName = ASRParameterSets.ByParam)]
        [Parameter(ParameterSetName = ASRParameterSets.ByFabricId)]
        [ValidateNotNullOrEmpty]
        public DateTime EndTime { get; set; }

        /// <summary>
        ///     Gets or sets the event type to filter on.
        /// </summary>
        [Parameter(ParameterSetName = ASRParameterSets.ByParam)]
        [Parameter(ParameterSetName = ASRParameterSets.ByFabricId)]
        [ValidateSet(
            Constants.VmHealth,
            Constants.ServerHealth,
            Constants.AgentHealth)]
        public string EventType { get; set; }

        /// <summary>
        ///     ProcessRecord of the command.
        /// </summary>
        public override void ExecuteSiteRecoveryCmdlet()
        {
            base.ExecuteSiteRecoveryCmdlet();
            switch (ParameterSetName)
            {
                case ASRParameterSets.ByName:
                    GetByName();
                    break;
                case ASRParameterSets.ByResourceId:
                    GetByResourceId();
                    break;
                case ASRParameterSets.ByFabricId:
                case ASRParameterSets.ByParam:
                default:
                    GetEvents();
                    break;
            }
        }

        /// <summary>
        ///     Gets the Event by ResourceId.
        /// </summary>
        private void GetByResourceId()
        {
            if (!string.IsNullOrWhiteSpace(ResourceId))
            {
                Name = Utilities.GetValueFromArmId(
                    ResourceId,
                    ARMResourceTypeConstants.Events);

                GetByName();
            }
        }

        /// <summary>
        ///     Gets the Event by Name.
        /// </summary>
        private void GetByName()
        {
            var eventsResponse =
                RecoveryServicesClient.GetAzureRmSiteRecoveryEvent(Name);
            WriteObject(new ASREvent(eventsResponse));
        }

        /// <summary>
        ///     Gets the alert and notification settings.
        /// </summary>
        private void GetEvents()
        {
            var parameters = new EventQueryParameter();

            if (!string.IsNullOrEmpty(EventType))
            {
                parameters.EventType = EventType;
            }

            if (!string.IsNullOrEmpty(Severity))
            {
                parameters.Severity = Severity;
            }

            if (Fabric != null)
            {
                parameters.FabricName = Fabric.Name;
            }

            if (FabricId != null)
            {
                parameters.FabricName = Utilities.GetValueFromArmId(
                    FabricId,
                    ARMResourceTypeConstants.ReplicationFabrics);
            }

            if (!string.IsNullOrEmpty(AffectedObjectFriendlyName))
            {
                parameters.AffectedObjectFriendlyName = AffectedObjectFriendlyName;
            }

            if (StartTime != DateTime.MinValue)
            {
                parameters.StartTime = StartTime;
            }

            if (EndTime != DateTime.MinValue)
            {
                parameters.EndTime = EndTime;
            }

            var eventsResponse =
                RecoveryServicesClient.ListAzureRmSiteRecoveryEvents(parameters);

            WriteObject(eventsResponse.Select(p => new ASREvent(p)), true);
        }
    }
}