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
    using Commands.Common.Authentication.Abstractions;
    using Common.ArgumentCompleters;
    using Components;
    using Entities.Resources;
    using Extensions;
    using SdkModels;
    using Common;
    using Management.ResourceManager.Models;
    using WindowsAzure.Commands.Utilities.Common;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using System.Threading.Tasks;

    /// <summary>
    /// Cmdlet to get existing resources.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureRmResource", DefaultParameterSetName = ByTagNameValueParameterSet), OutputType(typeof(PSResource))]
    public sealed class GetAzureResourceCmdlet : ResourceManagerCmdletBase
    {
        public const string ByResourceIdParameterSet = "ByResourceId";
        public const string ByTagObjectParameterSet = "ByTagObjectParameterSet";
        public const string ByTagNameValueParameterSet = "ByTagNameValueParameterSet";

        /// <summary>
        /// Contains the errors that encountered while satifying the request.
        /// </summary>
        private readonly ConcurrentBag<ErrorRecord> errors = new ConcurrentBag<ErrorRecord>();

        /// <summary>
        /// Gets or sets the resource name parameter.
        /// </summary>
        [Alias("Id")]
        [Parameter(ParameterSetName = ByResourceIdParameterSet, Mandatory = true, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string ResourceId { get; set; }

        /// <summary>
        /// Gets or sets the resource name parameter.
        /// </summary>
        [Alias("ResourceName")]
        [Parameter(ParameterSetName = ByTagObjectParameterSet, Mandatory = false)]
        [Parameter(ParameterSetName = ByTagNameValueParameterSet, Mandatory = false)]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the resource type parameter.
        /// </summary>
        [Parameter(ParameterSetName = ByTagObjectParameterSet, Mandatory = false)]
        [Parameter(ParameterSetName = ByTagNameValueParameterSet, Mandatory = false)]
        [ResourceTypeCompleter]
        [ValidateNotNullOrEmpty]
        public string ResourceType { get; set; }

        /// <summary>
        /// Gets or sets the OData query parameter.
        /// </summary>
        [Parameter(ParameterSetName = ByTagObjectParameterSet, Mandatory = false)]
        [Parameter(ParameterSetName = ByTagNameValueParameterSet, Mandatory = false)]
        [ValidateNotNullOrEmpty]
        public string ODataQuery { get; set; }

        /// <summary>
        /// Gets or sets the resource group name.
        /// </summary>
        [Parameter(ParameterSetName = ByTagObjectParameterSet, Mandatory = false)]
        [Parameter(ParameterSetName = ByTagNameValueParameterSet, Mandatory = false)]
        [ResourceGroupCompleter]
        [ValidateNotNullOrEmpty]
        public string ResourceGroupName { get; set; }

        [Parameter(ParameterSetName = ByTagObjectParameterSet, Mandatory = true)]
        public Hashtable Tag { get; set; }

        [Parameter(ParameterSetName = ByTagNameValueParameterSet, Mandatory = false)]
        public string TagName { get; set; }

        [Parameter(ParameterSetName = ByTagNameValueParameterSet, Mandatory = false)]
        public string TagValue { get; set; }

        /// <summary>
        /// Gets or sets the expand properties property.
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "When specified, expands the properties of the resource.")]
        public SwitchParameter ExpandProperties { get; set; }

        /// <summary>
        /// Gets or sets the subscription id.
        /// </summary>
        public Guid? SubscriptionId { get; set; }

        /// <summary>
        /// Gets or sets the default api-version to use.
        /// </summary>
        public string DefaultApiVersion { get; set; }

        /// <summary>
        /// Collects subscription ids from the pipeline.
        /// </summary>
        protected override void OnProcessRecord()
        {
            base.OnProcessRecord();
            DefaultApiVersion = string.IsNullOrWhiteSpace(ApiVersion) ? Constants.ResourcesApiVersion : ApiVersion;
            var resourceId = string.Empty;
            if (ShouldConstructResourceId(out resourceId))
            {
                ResourceId = resourceId;
            }

            if (!string.IsNullOrEmpty(ResourceId))
            {
                var resource = ResourceManagerSdkClient.GetById(ResourceId, DefaultApiVersion);
                WriteObject(resource);
            }
            else if (this.IsParameterBound(c => c.ApiVersion) || this.IsParameterBound(c => c.ExpandProperties))
            {
                RunCmdlet();
            }
            else
            {
                RunSimpleCmdlet();
            }
        }

        /// <summary>
        /// Finishes the pipeline execution and runs the cmdlet.
        /// </summary>
        protected override void OnEndProcessing()
        {
            base.OnEndProcessing();
        }

        private void RunSimpleCmdlet()
        {
            if (this.IsParameterBound(c => c.Tag))
            {
                TagName = TagsHelper.GetTagNameFromParameters(Tag, null);
                TagValue = TagsHelper.GetTagValueFromParameters(Tag, null);
            }

            var expression = QueryFilterBuilder.CreateFilter(
                null,
                null,
                ResourceType,
                null,
                null,
                null,
                ODataQuery);

            var odataQuery = new Rest.Azure.OData.ODataQuery<GenericResourceFilter>(expression);
            var result = Enumerable.Empty<PSResource>();
            if (!string.IsNullOrEmpty(ResourceGroupName) && !ResourceGroupName.Contains('*'))
            {
                result = ResourceManagerSdkClient.ListByResourceGroup(ResourceGroupName, odataQuery);
            }
            else
            {
                result = ResourceManagerSdkClient.ListResources(odataQuery);
                if (!string.IsNullOrEmpty(ResourceGroupName))
                {
                    result = FilterResourceGroupByWildcard(result);
                }
            }

            if (!string.IsNullOrEmpty(Name))
            {
                result = FilterResourceByWildcard(result);
            }

            if (!string.IsNullOrEmpty(TagName))
            {
                result = result.Where(r => r.Tags != null && r.Tags.Keys != null && r.Tags.Keys.Where(k => string.Equals(k, TagName, StringComparison.OrdinalIgnoreCase)).Any());
            }

            if (!string.IsNullOrEmpty(TagValue))
            {
                result = result.Where(r => r.Tags != null && r.Tags.Values != null && r.Tags.Values.Where(v => string.Equals(v, TagValue, StringComparison.OrdinalIgnoreCase)).Any());
            }

            WriteObject(result, true);
        }

        private bool ShouldConstructResourceId(out string resourceId)
        {
            resourceId = null;
            if (this.IsParameterBound(c => c.Name) && !ContainsWildcard(Name) &&
                this.IsParameterBound(c => c.ResourceGroupName) && !ContainsWildcard(ResourceGroupName) &&
                this.IsParameterBound(c => c.ResourceType) && ResourceType.Split('/').Count() == 2)
            {
                resourceId = string.Format("/subscriptions/{0}/resourceGroups/{1}/providers/{2}/{3}",
                                            DefaultContext.Subscription.Id,
                                            ResourceGroupName,
                                            ResourceType,
                                            Name);
                return true;
            }

            return false;
        }

        private bool ContainsWildcard(string parameter)
        {
            return !string.IsNullOrEmpty(parameter) && (parameter.StartsWith("*") || parameter.EndsWith("*"));
        }

        private IEnumerable<PSResource> FilterResourceGroupByWildcard(IEnumerable<PSResource> result)
        {
            if (ResourceGroupName.StartsWith("*"))
            {
                ResourceGroupName = ResourceGroupName.TrimStart('*');
                if (ResourceGroupName.EndsWith("*"))
                {
                    ResourceGroupName = ResourceGroupName.TrimEnd('*');
                    result = result.Where(r => r.ResourceGroupName.IndexOf(ResourceGroupName, StringComparison.OrdinalIgnoreCase) >= 0);
                }
                else
                {
                    result = result.Where(r => r.ResourceGroupName.EndsWith(ResourceGroupName, StringComparison.OrdinalIgnoreCase));
                }
            }
            else if (ResourceGroupName.EndsWith("*"))
            {
                ResourceGroupName = ResourceGroupName.TrimEnd('*');
                result = result.Where(r => r.ResourceGroupName.StartsWith(ResourceGroupName, StringComparison.OrdinalIgnoreCase));
            }

            return result;
        }

        private IEnumerable<PSResource> FilterResourceByWildcard(IEnumerable<PSResource> result)
        {
            if (Name.StartsWith("*"))
            {
                Name = Name.TrimStart('*');
                if (Name.EndsWith("*"))
                {
                    Name = Name.TrimEnd('*');
                    result = result.Where(r => r.Name.IndexOf(Name, StringComparison.OrdinalIgnoreCase) >= 0);
                }
                else
                {
                    result = result.Where(r => r.Name.EndsWith(Name, StringComparison.OrdinalIgnoreCase));
                }
            }
            else if (Name.EndsWith("*"))
            {
                Name = Name.TrimEnd('*');
                result = result.Where(r => r.Name.StartsWith(Name, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                result = result.Where(r => string.Equals(r.Name, Name, StringComparison.OrdinalIgnoreCase));
            }

            return result;
        }

        /// <summary>
        /// Contains the cmdlet's execution logic.
        /// </summary>
        private void RunCmdlet()
        {
            if (string.IsNullOrEmpty(ResourceId))
            {
                SubscriptionId = DefaultContext.Subscription.GetId();
            }

            PaginatedResponseHelper.ForEach(
                () => GetResources(),
                nextLink => GetNextLink<JObject>(nextLink),
                CancellationToken,
                resources =>
                {
                    Resource<JToken> resource;
                    if (resources.CoalesceEnumerable().FirstOrDefault().TryConvertTo(out resource))
                    {
                        var genericResources = resources.CoalesceEnumerable().Where(res => res != null).SelectArray(res => res.ToResource());

                        foreach (var batch in genericResources.Batch())
                        {
                            var items = batch;
                            if (ExpandProperties)
                            {
                                items = GetPopulatedResource(batch).Result;
                            }

                            var powerShellObjects = items.SelectArray(genericResource => genericResource.ToPsObject());

                            WriteObject(powerShellObjects, true);
                        }
                    }
                    else
                    {
                        WriteObject(resources.CoalesceEnumerable().SelectArray(res => res.ToPsObject()), true);
                    }
                });

            if (errors.Count != 0)
            {
                foreach (var error in errors)
                {
                    WriteError(error);
                }
            }
        }

        /// <summary>
        /// Queries the ARM cache and returns the cached resource that match the query specified.
        /// </summary>
        private async Task<ResponseWithContinuation<JObject[]>> GetResources()
        {
            if (IsResourceGet())
            {
                var resource = await GetResource().ConfigureAwait(false);
                ResponseWithContinuation<JObject[]> retVal;
                return resource.TryConvertTo(out retVal) && retVal.Value != null
                    ? retVal
                    : new ResponseWithContinuation<JObject[]> { Value = resource.AsArray() };
            }

            if (IsResourceGroupLevelQuery())
            {
                return await ListResourcesInResourceGroup().ConfigureAwait(false);
            }

            if (IsSubscriptionLevelQuery())
            {
                return await ListResourcesInSubscription().ConfigureAwait(false);
            }

            return await ListResourcesInTenant().ConfigureAwait(false);
        }

        /// <summary>
        /// Gets a resource.
        /// </summary>
        private async Task<JObject> GetResource()
        {
#pragma warning disable 618

            var resourceId = GetResourceId();

#pragma warning restore 618

            var apiVersion = await DetermineApiVersion(resourceId)
                .ConfigureAwait(false);

            var odataQuery = QueryFilterBuilder.CreateFilter(
                null,
                null,
                null,
                null,
                null,
                null,
                ODataQuery);

            return await GetResourcesClient()
                .GetResource<JObject>(
                    resourceId,
                    apiVersion,
                    CancellationToken.Value,
                    odataQuery)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Lists resources in a type collection.
        /// </summary>
        private async Task<ResponseWithContinuation<JObject[]>> ListResourcesTypeCollection()
        {
            var resourceCollectionId = ResourceIdUtility.GetResourceId(
                SubscriptionId.AsArray().CoalesceEnumerable().Cast<Guid?>().FirstOrDefault(),
                ResourceGroupName,
                ResourceType,
                Name);

            var odataQuery = QueryFilterBuilder.CreateFilter(
                null,
                null,
                null,
                null,
                null,
                null,
                ODataQuery);

            return await GetResourcesClient()
                .ListObjectColleciton<JObject>(
                    resourceCollectionId,
                    DefaultApiVersion,
                    CancellationToken.Value,
                    odataQuery)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Lists the resources in the tenant.
        /// </summary>
        private async Task<ResponseWithContinuation<JObject[]>> ListResourcesInTenant()
        {
            var filterQuery = QueryFilterBuilder
                .CreateFilter(
                    null,
                    ResourceGroupName,
                    ResourceType,
                    Name,
                    null,
                    null,
                    ODataQuery);

            return await GetResourcesClient()
                .ListResources<JObject>(
                    DefaultApiVersion,
                    filter: filterQuery,
                    cancellationToken: CancellationToken.Value)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Lists the resources in a resource group.
        /// </summary>
        private async Task<ResponseWithContinuation<JObject[]>> ListResourcesInResourceGroup()
        {
            var filterQuery = QueryFilterBuilder
                .CreateFilter(
                    null,
                    null,
                    ResourceType,
                    Name,
                    null,
                    null,
                    ODataQuery);

            return await GetResourcesClient()
                .ListResources<JObject>(
                    SubscriptionId.Value,
                    ResourceGroupName,
                    DefaultApiVersion,
                    filter: filterQuery,
                    cancellationToken: CancellationToken.Value)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the resources in a subscription.
        /// </summary>
        private async Task<ResponseWithContinuation<JObject[]>> ListResourcesInSubscription()
        {
            var filterQuery = QueryFilterBuilder
                .CreateFilter(
                    null,
                    null,
                    ResourceType,
                    Name,
                    null,
                    null,
                    ODataQuery);

            return await GetResourcesClient()
                .ListResources<JObject>(
                    SubscriptionId.Value,
                    DefaultApiVersion,
                    filter: filterQuery,
                    cancellationToken: CancellationToken.Value)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the next set of resources using the <paramref name="nextLink"/>
        /// </summary>
        /// <param name="nextLink">The next link.</param>
        private Task<ResponseWithContinuation<TType[]>> GetNextLink<TType>(string nextLink)
        {
            return GetResourcesClient()
                .ListNextBatch<TType>(nextLink, CancellationToken.Value);
        }

        /// <summary>
        /// Populates the properties on an array of resources.
        /// </summary>
        /// <param name="resources">The resource.</param>
        private async Task<Resource<JToken>[]> GetPopulatedResource(IEnumerable<Resource<JToken>> resources)
        {
            return await resources
                .Select(resource => GetPopulatedResource(resource))
                .WhenAllForAwait()
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Populates the properties of a single resource.
        /// </summary>
        /// <param name="resource">The resource.</param>
        private async Task<Resource<JToken>> GetPopulatedResource(Resource<JToken> resource)
        {
            try
            {
                var apiVersion = await DetermineApiVersion(
                    resource.Id,
                    Pre)
                    .ConfigureAwait(false);

                return await GetResourcesClient()
                    .GetResource<Resource<JToken>>(
                        resource.Id,
                        apiVersion,
                        CancellationToken.Value)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (ex.IsFatal())
                {
                    throw;
                }

                ex = ex is AggregateException
                    ? (ex as AggregateException).Flatten()
                    : ex;

                errors.Add(new ErrorRecord(ex, "ErrorExpandingProperties", ErrorCategory.CloseError, resource));
            }

            return resource;
        }

        /// <summary>
        /// Gets the resource Id from the supplied PowerShell parameters.
        /// </summary>
        private string GetResourceId()
        {
            return !string.IsNullOrWhiteSpace(ResourceId)
            ? ResourceId
            : ResourceIdUtility.GetResourceId(
                SubscriptionId.Value,
                ResourceGroupName,
                ResourceType,
                Name);
        }

        /// <summary>
        /// Returns true if this is a resource get at any level.
        /// </summary>
        private bool IsResourceGet()
        {
            return !string.IsNullOrWhiteSpace(ResourceId) ||
                IsResourceGroupLevelResourceGet() ||
                IsSubscriptionLevelResourceGet() ||
                IsTenantLevelResourceGet();
        }

        /// <summary>
        /// Returns true if this is a get on a type collection that is at the subscription level.
        /// </summary>
        private bool IsSubscriptionLevelResourceTypeCollectionGet()
        {
            return SubscriptionId.HasValue &&
                ResourceGroupName == null &&
                Name == null &&
                ResourceType != null;
        }

        /// <summary>
        /// Returns true if this is a get on a type collection that is at the resource group.
        /// </summary>
        private bool IsResourceGroupLevelResourceTypeCollectionGet()
        {
            return SubscriptionId.HasValue &&
                ResourceGroupName != null &&
                Name == null &&
                ResourceType != null;
        }

        /// <summary>
        /// Returns true if this is a tenant level resource type collection get (a get without a subscription or
        /// resource group or resource name but with a resource or extension type.)
        /// </summary>
        private bool IsTenantLevelResourceTypeCollectionGet()
        {
            return SubscriptionId == null &&
                ResourceGroupName == null &&
                Name == null &&
                ResourceType != null;
        }

        /// <summary>
        /// Returns true if this is a subscription level resource get (a get that has a single
        /// subscription and a resource type or name.)
        /// </summary>
        private bool IsSubscriptionLevelResourceGet()
        {
            return SubscriptionId.HasValue &&
                ResourceGroupName == null &&
                Name != null &&
                ResourceType != null;
        }

        /// <summary>
        /// Returns true if this is a resource group level resource get (a get that has a single
        /// subscription and resource group name as well as a resource name and type.)
        /// </summary>
        private bool IsResourceGroupLevelResourceGet()
        {
            return SubscriptionId.HasValue &&
                ResourceGroupName != null &&
                Name != null &&
                ResourceType != null;
        }

        /// <summary>
        /// Returns true if this is a tenant level query.
        /// </summary>
        private bool IsTenantLevelResourceGet()
        {
            return SubscriptionId == null &&
                ResourceGroupName == null &&
                Name != null &&
                ResourceType != null;
        }

        /// <summary>
        /// Returns true if this is a subscription level query.
        /// </summary>
        private bool IsSubscriptionLevelQuery()
        {
            return SubscriptionId.HasValue &&
                ResourceGroupName == null;
        }

        /// <summary>
        /// Returns true if this is a resource group level query.
        /// </summary>
        private bool IsResourceGroupLevelQuery()
        {
            return SubscriptionId.HasValue &&
                ResourceGroupName != null &&
                Name != null ||
                ResourceType != null;
        }
    }
}