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

namespace Microsoft.Azure.Commands.PolicyInsights.Models
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Policy event record.
    /// </summary>
    public class PolicyEvent
    {
        /// <summary>
        /// Gets the additional properties (i.e. query time generated properties like aggregations).
        /// </summary>
        public IDictionary<string, object> AdditionalProperties { get; }

        /// <summary>
        /// Gets timestamp for the policy event record.
        /// </summary>
        public DateTime? Timestamp { get; }

        /// <summary>
        /// Gets resource ID.
        /// </summary>
        public string ResourceId { get; }

        /// <summary>
        /// Gets policy assignment ID.
        /// </summary>
        public string PolicyAssignmentId { get; }

        /// <summary>
        /// Gets policy definition ID.
        /// </summary>
        public string PolicyDefinitionId { get; }

        /// <summary>
        /// Gets effective parameters for the policy assignment.
        /// </summary>
        public string EffectiveParameters { get; }

        /// <summary>
        /// Gets flag which states whether the resource is compliant
        /// against the policy assignment it was evaluated against.
        /// </summary>
        public bool? IsCompliant { get; }

        /// <summary>
        /// Gets subscription ID.
        /// </summary>
        public string SubscriptionId { get; }

        /// <summary>
        /// Gets resource type.
        /// </summary>
        public string ResourceType { get; }

        /// <summary>
        /// Gets resource location.
        /// </summary>
        public string ResourceLocation { get; }

        /// <summary>
        /// Gets resource group name.
        /// </summary>
        public string ResourceGroup { get; }

        /// <summary>
        /// Gets list of resource tags.
        /// </summary>
        public string ResourceTags { get; }

        /// <summary>
        /// Gets policy assignment name.
        /// </summary>
        public string PolicyAssignmentName { get; }

        /// <summary>
        /// Gets policy assignment owner.
        /// </summary>
        public string PolicyAssignmentOwner { get; }

        /// <summary>
        /// Gets policy assignment parameters.
        /// </summary>
        public string PolicyAssignmentParameters { get; }

        /// <summary>
        /// Gets policy assignment scope.
        /// </summary>
        public string PolicyAssignmentScope { get; }

        /// <summary>
        /// Gets policy definition name.
        /// </summary>
        public string PolicyDefinitionName { get; }

        /// <summary>
        /// Gets policy definition action, i.e. effect.
        /// </summary>
        public string PolicyDefinitionAction { get; }

        /// <summary>
        /// Gets policy definition category.
        /// </summary>
        public string PolicyDefinitionCategory { get; }

        /// <summary>
        /// Gets policy set definition ID, if the policy assignment is
        /// for a policy set.
        /// </summary>
        public string PolicySetDefinitionId { get; }

        /// <summary>
        /// Gets policy set definition name, if the policy assignment
        /// is for a policy set.
        /// </summary>
        public string PolicySetDefinitionName { get; }

        /// <summary>
        /// Gets policy set definition owner, if the policy assignment
        /// is for a policy set.
        /// </summary>
        public string PolicySetDefinitionOwner { get; }

        /// <summary>
        /// Gets policy set definition category, if the policy
        /// assignment is for a policy set.
        /// </summary>
        public string PolicySetDefinitionCategory { get; }

        /// <summary>
        /// Gets policy set definition parameters, if the policy
        /// assignment is for a policy set.
        /// </summary>
        public string PolicySetDefinitionParameters { get; }

        /// <summary>
        /// Gets comma seperated list of management group IDs, which
        /// represent the hierarchy of the management groups the resource is
        /// under.
        /// </summary>
        public string ManagementGroupIds { get; }

        /// <summary>
        /// Gets reference ID for the policy definition inside the
        /// policy set, if the policy assignment is for a policy set.
        /// </summary>
        public string PolicyDefinitionReferenceId { get; }

        /// <summary>
        /// Gets tenant ID for the policy event record.
        /// </summary>
        public string TenantId { get; }

        /// <summary>
        /// Gets principal object ID for the user who initiated the
        /// resource operation that triggered the policy event.
        /// </summary>
        public string PrincipalOid { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyEvent" /> class.
        /// </summary>
        /// <param name="policyEvent">Policy event.</param>
        public PolicyEvent(Management.PolicyInsights.Models.PolicyEvent policyEvent)
        {
            if (null == policyEvent)
            {
                return;
            }

            AdditionalProperties = policyEvent.AdditionalProperties;
            AdditionalProperties.Remove("@odata.id");

            Timestamp = policyEvent.Timestamp;
            ResourceId = policyEvent.ResourceId;
            PolicyAssignmentId = policyEvent.PolicyAssignmentId;
            PolicyDefinitionId = policyEvent.PolicyDefinitionId;
            EffectiveParameters = policyEvent.EffectiveParameters;
            IsCompliant = policyEvent.IsCompliant;
            SubscriptionId = policyEvent.SubscriptionId;
            ResourceType = policyEvent.ResourceType;
            ResourceLocation = policyEvent.ResourceLocation;
            ResourceGroup = policyEvent.ResourceGroup;
            ResourceTags = policyEvent.ResourceTags;
            PolicyAssignmentName = policyEvent.PolicyAssignmentName;
            PolicyAssignmentOwner = policyEvent.PolicyAssignmentOwner;
            PolicyAssignmentParameters = policyEvent.PolicyAssignmentParameters;
            PolicyAssignmentScope = policyEvent.PolicyAssignmentScope;
            PolicyDefinitionName = policyEvent.PolicyDefinitionName;
            PolicyDefinitionAction = policyEvent.PolicyDefinitionAction;
            PolicyDefinitionCategory = policyEvent.PolicyDefinitionCategory;
            PolicySetDefinitionId = policyEvent.PolicySetDefinitionId;
            PolicySetDefinitionName = policyEvent.PolicySetDefinitionName;
            PolicySetDefinitionOwner = policyEvent.PolicySetDefinitionOwner;
            PolicySetDefinitionCategory = policyEvent.PolicySetDefinitionCategory;
            PolicySetDefinitionParameters = policyEvent.PolicySetDefinitionParameters;
            ManagementGroupIds = policyEvent.ManagementGroupIds;
            PolicyDefinitionReferenceId = policyEvent.PolicyDefinitionReferenceId;
            TenantId = policyEvent.TenantId;
            PrincipalOid = policyEvent.PrincipalOid;
        }
    }
}
