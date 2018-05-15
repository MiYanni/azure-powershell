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
    using Components;
    using Entities.Resources;
    using Extensions;
    using WindowsAzure.Commands.Common;
    using Newtonsoft.Json.Linq;
    using System.Collections;
    using System.Linq;
    using System.Management.Automation;
    using System.Threading.Tasks;

    /// <summary>
    /// A cmdlet that creates a new azure resource.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "AzureRmResource", SupportsShouldProcess = true, DefaultParameterSetName = ResourceIdParameterSet), OutputType(typeof(PSObject))]
    public sealed class SetAzureResourceCmdlet : ResourceManipulationCmdletBase
    {
        /// <summary>
        /// Gets or sets the kind.
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The resource kind.")]
        [ValidateNotNullOrEmpty]
        public string Kind { get; set; }

        /// <summary>
        /// Gets or sets the property object.
        /// </summary>
        [Alias("PropertyObject")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "A hash table which represents resource properties.")]
        [ValidateNotNullOrEmpty]
        public PSObject Properties { get; set; }

        /// <summary>
        /// Gets or sets the plan object.
        /// </summary>
        [Alias("PlanObject")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "A hash table which represents resource plan properties.")]
        [ValidateNotNullOrEmpty]
        public Hashtable Plan { get; set; }

        /// <summary>  
        /// Gets or sets the Sku object.  
        /// </summary>  
        [Alias("SkuObject")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "A hash table which represents sku properties.")]
        [ValidateNotNullOrEmpty]
        public Hashtable Sku { get; set; }


        /// <summary>
        /// Gets or sets the tags.
        /// </summary>
        [Alias("Tags")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "A hash table which represents resource tags.")]
        public Hashtable Tag { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates if an HTTP PATCH request needs to be made instead of PUT.
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "When set indicates if an HTTP PATCH should be used to update the object instead of PUT.")]
        public SwitchParameter UsePatchSemantics { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Run cmdlet in the background")]
        public SwitchParameter AsJob { get; set; }

        /// <summary>
        /// Executes the cmdlet.
        /// </summary>
        protected override void OnProcessRecord()
        {
            base.OnProcessRecord();

            var resourceId = GetResourceId();
            ConfirmAction(
                Force,
                "Are you sure you want to update the following resource: " + resourceId,
                "Updating the resource...",
                resourceId,
                () =>
                {
                    var apiVersion = DetermineApiVersion(resourceId).Result;
                    var resourceBody = GetResourceBody();

                    var operationResult = ShouldUsePatchSemantics()
                        ? GetResourcesClient()
                            .PatchResource(
                                resourceId,
                                apiVersion,
                                resourceBody,
                                CancellationToken.Value,
                                ODataQuery)
                            .Result
                        : GetResourcesClient()
                            .PutResource(
                                resourceId,
                                apiVersion,
                                resourceBody,
                                CancellationToken.Value,
                                ODataQuery)
                            .Result;

                    var managementUri = GetResourcesClient()
                        .GetResourceManagementRequestUri(
                            resourceId,
                            apiVersion,
                            odataQuery: ODataQuery);

                    var activity = string.Format("{0} {1}", ShouldUsePatchSemantics() ? "PATCH" : "PUT", managementUri.PathAndQuery);
                    var result = GetLongRunningOperationTracker(activity, true)
                        .WaitOnOperation(operationResult);

                    TryConvertToResourceAndWriteObject(result);
                });
        }

        /// <summary>
        /// Gets the resource body.
        /// </summary>
        private JToken GetResourceBody()
        {
            if (ShouldUsePatchSemantics())
            {
                var resourceBody = GetPatchResourceBody();

                return resourceBody == null ? null : resourceBody.ToJToken();
            }
            var getResult = GetResource().Result;

            if (getResult.CanConvertTo<Resource<JToken>>())
            {
                var resource = getResult.ToResource();
                return new Resource<JToken>
                {
                    Kind = Kind ?? resource.Kind,
                    Plan = Plan.ToDictionary(false).ToJson().FromJson<ResourcePlan>() ?? resource.Plan,
                    Sku = Sku.ToDictionary(false).ToJson().FromJson<ResourceSku>() ?? resource.Sku,
                    Tags = TagsHelper.GetTagsDictionary(Tag) ?? resource.Tags,
                    Location = resource.Location,
                    Properties = Properties == null ? resource.Properties : Properties.ToResourcePropertiesBody()
                }.ToJToken();
            }
            return Properties.ToJToken();
        }

        /// <summary>
        /// Gets the resource body for PATCH calls
        /// </summary>
        private Resource<JToken> GetPatchResourceBody()
        {
            if (Properties == null && Plan == null && Kind == null && Sku == null && Tag == null)
            {
                return null;
            }

            Resource<JToken> resourceBody = new Resource<JToken>();

            if (Properties != null)
            {
                resourceBody.Properties = Properties.ToResourcePropertiesBody();
            }

            if (Plan != null)
            {
                resourceBody.Plan = Plan.ToDictionary(false).ToJson().FromJson<ResourcePlan>();
            }

            if (Kind != null)
            {
                resourceBody.Kind = Kind;
            }

            if (Sku != null)
            {
                resourceBody.Sku = Sku.ToDictionary(false).ToJson().FromJson<ResourceSku>();
            }

            if (Tag != null)
            {
                resourceBody.Tags = TagsHelper.GetTagsDictionary(Tag);
            }

            return resourceBody;
        }

        /// <summary>
        /// Determines if the cmdlet should use <c>PATCH</c> semantics.
        /// </summary>
        private bool ShouldUsePatchSemantics()
        {
            return UsePatchSemantics || (Tag != null || Sku != null) && Plan == null && Properties == null && Kind == null;
        }

        /// <summary>
        /// Gets a resource.
        /// </summary>
        private async Task<JObject> GetResource()
        {
            var resourceId = GetResourceId();
            var apiVersion = await DetermineApiVersion(resourceId)
                .ConfigureAwait(false);

            return await GetResourcesClient()
                .GetResource<JObject>(
                    resourceId,
                    apiVersion,
                    CancellationToken.Value)
                .ConfigureAwait(false);
        }
    }
}
