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
    using System;
    using System.Collections.Concurrent;
    using System.Management.Automation;
    using System.Threading.Tasks;
    using Extensions;
    using Common;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Gets the resource lock.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureRmResourceLock"), OutputType(typeof(PSObject))]
    public class GetAzureResourceLockCmdlet : ResourceLockManagementCmdletBase
    {
        /// <summary>
        /// Contains the errors that encountered while satifying the request.
        /// </summary>
        private readonly ConcurrentBag<ErrorRecord> errors = new ConcurrentBag<ErrorRecord>();

        /// <summary>
        /// Gets or sets the extension resource name parameter.
        /// </summary>
        [Alias("ExtensionResourceName", "Name")]
        [Parameter(ParameterSetName = ResourceGroupLevelLock, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The name of the lock.")]
        [Parameter(ParameterSetName = ResourceGroupResourceLevelLock, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The name of the lock.")]
        [Parameter(ParameterSetName = ScopeLevelLock, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The name of the lock.")]
        [Parameter(ParameterSetName = SubscriptionLevelLock, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The name of the lock.")]
        [Parameter(ParameterSetName = SubscriptionResourceLevelLock, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The name of the lock.")]
        [Parameter(ParameterSetName = TenantResourceLevelLock, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The name of the lock.")]
        [ValidateNotNullOrEmpty]
        public string LockName { get; set; }

        /// <summary>
        /// Gets or sets the at-scope filter.
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "When specified returns all locks at or above the specified scope, otherwise returns all locks at, above or below the scope.")]
        [ValidateNotNullOrEmpty]
        public SwitchParameter AtScope { get; set; }

        /// <summary>
        /// Executes the cmdlet.
        /// </summary>
        protected override void OnProcessRecord()
        {
            base.OnProcessRecord();
            PaginatedResponseHelper.ForEach(
                () => GetResources(),
                nextLink => GetNextLink<JObject>(nextLink),
                CancellationToken,
                resources => WriteObject(GetOutputObjects(resources), true));

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
            var response = new ResponseWithContinuation<JObject[]> { Value = new JObject[0] };

            try
            {
                if (!string.IsNullOrWhiteSpace(LockName))
                {
                    var resource = await GetResource().ConfigureAwait(false);

                    response.Value = resource.AsArray();
                }
                else
                {
                    response = await ListResourcesTypeCollection().ConfigureAwait(false);
                }
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

                errors.Add(new ErrorRecord(ex, "ErrorGettingResourceLock", ErrorCategory.CloseError, null));
            }

            return response;
        }

        /// <summary>
        /// Gets a resource.
        /// </summary>
        private async Task<JObject> GetResource()
        {
            var resourceId = GetResourceId(LockName);

            return await GetResourcesClient()
                .GetResource<JObject>(
                    resourceId,
                    LockApiVersion,
                    CancellationToken.Value)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Lists resources in a type collection.
        /// </summary>
        private async Task<ResponseWithContinuation<JObject[]>> ListResourcesTypeCollection()
        {
            var resourceCollectionId = GetResourceId(LockName);

            var filter = AtScope
                ? "$filter=atScope()"
                : null;

            return await GetResourcesClient()
                .ListObjectColleciton<JObject>(
                    resourceCollectionId,
                    LockApiVersion,
                    CancellationToken.Value,
                    filter)
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
    }
}
