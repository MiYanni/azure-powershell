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


namespace Microsoft.Azure.Commands.ResourceManager.Cmdlets.Implementation
{
    using Common.ArgumentCompleters;
    using Components;
    using Entities.Application;
    using Entities.Resources;
    using Extensions;
    using WindowsAzure.Commands.Utilities.Common;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections;
    using System.IO;
    using System.Management.Automation;
    using WindowsAzure.Commands.Common;

    /// <summary>
    /// Creates the managed application.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "AzureRmManagedApplication", DefaultParameterSetName = ManagedApplicationNameParameterSet, SupportsShouldProcess = true), OutputType(typeof(PSObject))]
    public class SetAzureManagedApplicationCmdlet : ManagedApplicationCmdletBase
    {
        /// <summary>
        /// The managed application Id parameter set.
        /// </summary>
        internal const string ManagedApplicationIdParameterSet = "SetById";

        /// <summary>
        /// The managed application name parameter set.
        /// </summary>
        internal const string ManagedApplicationNameParameterSet = "SetByNameAndResourceGroup";

        /// <summary>
        /// Gets or sets the managed application name parameter.
        /// </summary>
        [Parameter(ParameterSetName = ManagedApplicationNameParameterSet, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The managed application name.")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the managed application resource group parameter
        /// </summary>
        [Parameter(ParameterSetName = ManagedApplicationNameParameterSet, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The resource group name.")]
        [ResourceGroupCompleter]
        [ValidateNotNullOrEmpty]
        public string ResourceGroupName { get; set; }

        /// <summary>
        /// Gets or sets the managed application id parameter
        /// </summary>
        [Alias("ResourceId")]
        [Parameter(ParameterSetName = ManagedApplicationIdParameterSet, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The fully qualified managed application Id, including the subscription. e.g. /subscriptions/{subscriptionId}/resourcegroups/{resourceGroupName}")]
        [ValidateNotNullOrEmpty]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the managed application managed resource group parameter
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The managed resource group name.")]
        [ValidateNotNullOrEmpty]
        public string ManagedResourceGroupName { get; set; }

        /// <summary>
        /// Gets or sets the managed application managed application definition id parameter
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The managed resource group name.")]
        [ValidateNotNullOrEmpty]
        public string ManagedApplicationDefinitionId { get; set; }

        /// <summary>
        /// Gets or sets the managed application parameters parameter
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The JSON formatted string of parameters for managed application. This can either be a path to a file name or uri containing the parameters, or the parameters as string.")]
        [ValidateNotNullOrEmpty]
        public string Parameter { get; set; }

        /// <summary>
        /// Gets or sets the kind.
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "The managed application kind. One of marketplace or servicecatalog")]
        [ValidateNotNullOrEmpty]
        public string Kind { get; set; }

        /// <summary>
        /// Gets or sets the plan object.
        /// </summary>
        [Alias("PlanObject")]
        [Parameter(Mandatory = false, HelpMessage = "A hash table which represents managed application plan properties.")]
        [ValidateNotNullOrEmpty]
        public Hashtable Plan { get; set; }

        /// <summary>
        /// Gets or sets the tags.
        /// </summary>
        [Alias("Tags")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "A hash table which represents resource tags.")]
        public Hashtable Tag { get; set; }

        /// <summary>
        /// Executes the cmdlet.
        /// </summary>
        protected override void OnProcessRecord()
        {
            base.OnProcessRecord();
            if (ShouldProcess(Name, "Update Managed Application"))
            {
                string resourceId = Id ?? GetResourceId();

                var apiVersion = string.IsNullOrWhiteSpace(ApiVersion) ? Constants.ApplicationApiVersion : ApiVersion;
                var resourceBody = GetResourceBody(resourceId, apiVersion);

                var operationResult = ShouldUsePatchSemantics()
                    ? GetResourcesClient()
                        .PatchResource(
                            resourceId,
                            apiVersion,
                            resourceBody,
                            CancellationToken.Value,
                            null)
                        .Result
                    : GetResourcesClient()
                        .PutResource(
                            resourceId,
                            apiVersion,
                            resourceBody,
                            CancellationToken.Value,
                            null)
                        .Result;

                var managementUri = GetResourcesClient()
                  .GetResourceManagementRequestUri(
                      resourceId,
                      apiVersion,
                      odataQuery: null);

                var activity = string.Format("PUT {0}", managementUri.PathAndQuery);
                var result = GetLongRunningOperationTracker(activity, true)
                    .WaitOnOperation(operationResult);
                WriteObject(GetOutputObjects("ManagedApplicationId", JObject.Parse(result)), true);
            }
        }

        /// <summary>
        /// Gets the resource Id
        /// </summary>
        private string GetResourceId()
        {
            var subscriptionId = DefaultContext.Subscription.Id;
            return string.Format("/subscriptions/{0}/resourcegroups/{1}/providers/{2}/{3}",
                subscriptionId.ToString(),
                ResourceGroupName,
                Constants.MicrosoftApplicationType,
                Name);
        }

        /// <summary>
        /// Gets the resource body.
        /// </summary>
        private JToken GetResourceBody(string resourceId, string apiVersion)
        {
            if (ShouldUsePatchSemantics())
            {
                var resourceBody = GetPatchResourceBody();
                return resourceBody == null ? null : resourceBody.ToJToken();
            }
            return GetResource(resourceId, apiVersion);
        }

        /// <summary>
        /// Gets the resource body for PATCH calls
        /// </summary>
        private Resource<JToken> GetPatchResourceBody()
        {
            if(Tag == null)
            {
                return null;
            }
            Resource<JToken> resourceBody = new Resource<JToken>();
            resourceBody.Tags = TagsHelper.GetTagsDictionary(Tag);
            return resourceBody;
        }

        /// <summary>
        /// Determines if the cmdlet should use <c>PATCH</c> semantics.
        /// </summary>
        private bool ShouldUsePatchSemantics()
        {
            return Tag != null && Plan == null && Kind == null 
                   && ManagedApplicationDefinitionId == null && ManagedResourceGroupName ==null
                   && Parameter == null;
        }


        /// <summary>
        /// Constructs the resource
        /// </summary>
        private JToken GetResource(string resourceId, string apiVersion)
        {
            var resource = GetExistingResource(resourceId, apiVersion).Result.ToResource();

            var applicationObject = new Application
            {
                Name = Name,
                Location = resource.Location,
                Plan = Plan == null
                    ? resource.Plan
                    : Plan.ToDictionary(false).ToJson().FromJson<ResourcePlan>(),
                Properties = new ApplicationProperties
                {
                    ManagedResourceGroupId = string.IsNullOrEmpty(ManagedResourceGroupName)
                        ? resource.Properties["managedResourceGroupId"].ToString()
                        : string.Format("/subscriptions/{0}/resourcegroups/{1}", Guid.Parse(DefaultContext.Subscription.Id), ManagedResourceGroupName),
                    ApplicationDefinitionId =ManagedApplicationDefinitionId ?? resource.Properties["applicationDefinitionId"].ToString(),
                    Parameters = Parameter == null 
                    ? (resource.Properties["parameters"] != null ? JObject.Parse(resource.Properties["parameters"].ToString()) : null)
                    : JObject.Parse(GetObjectFromParameter(Parameter).ToString())
                },
                Tags = TagsHelper.GetTagsDictionary(Tag) ?? resource.Tags
            };

            return applicationObject.ToJToken();
        }
    }
}
