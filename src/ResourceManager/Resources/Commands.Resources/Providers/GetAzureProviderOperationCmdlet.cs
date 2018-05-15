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

namespace Microsoft.Azure.Commands.Resources
{
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using ProjectResources = Properties.Resources;
    using Management.ResourceManager;
    using Management.ResourceManager.Models;
    using Management.Authorization.Models;

    /// <summary>
    /// Get an existing resource.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureRmProviderOperation"), OutputType(typeof(PSResourceProviderOperation))]
    [Alias("Get-AzureRmResourceProviderAction")]
    public class GetAzureProviderOperationCommand : ResourcesBaseCmdlet
    {
        private const string WildCardCharacter = "*";
        private static readonly char Separator = '/';

        /// <summary>
        /// Gets or sets the provider namespace
        /// </summary>
        [Parameter(Position = 0, Mandatory = false, ValueFromPipelineByPropertyName = false, ValueFromPipeline = true, HelpMessage = "The action string.")]
        [Alias("Name")]
        [ValidateNotNullOrEmpty]
        public string OperationSearchString { get; set; } = "*";

        /// <summary>
        /// Executes the cmdlet
        /// </summary>
        public override void ExecuteCmdlet()
        {
            // remove leading and trailing whitespaces
            OperationSearchString = OperationSearchString.Trim();

            ValidateActionSearchString(OperationSearchString);

            List<PSResourceProviderOperation> operationsToDisplay;

            if (OperationSearchString.Contains(WildCardCharacter))
            {
                operationsToDisplay = ProcessProviderOperationsWithWildCard(OperationSearchString);
            }
            else
            {
                operationsToDisplay = ProcessProviderOperationsWithoutWildCard(OperationSearchString);
            }

            WriteObject(operationsToDisplay, true);
        }

        private static void ValidateActionSearchString(string actionSearchString)
        {
            if (actionSearchString.Contains("?"))
            {
                throw new ArgumentException(ProjectResources.ProviderOperationUnsupportedWildcard);
            }

            string[] parts = actionSearchString.Split(Separator);
            if (parts.Any(p => p.Contains(WildCardCharacter) && p.Length != 1))
            {
                throw new ArgumentException(ProjectResources.OperationSearchStringInvalidWildcard);
            }

            if (parts.Length == 1 && parts[0] != WildCardCharacter)
            {
                throw new ArgumentException(string.Format(ProjectResources.OperationSearchStringInvalidProviderName, parts[0]));
            }
        }

        /// <summary>
        /// Get a list of Provider operations in the case that the Actionstring input contains a wildcard
        /// </summary>
        private List<PSResourceProviderOperation> ProcessProviderOperationsWithWildCard(string actionSearchString)
        {
            // Filter the list of all operation names to what matches the wildcard
            WildcardPattern wildcard = new WildcardPattern(actionSearchString, WildcardOptions.IgnoreCase | WildcardOptions.Compiled);

            var providers = new List<ProviderOperationsMetadata>();
            string provider = OperationSearchString.Split(Separator).First();
            if (provider.Equals(WildCardCharacter))
            {
                // 'Get-AzureRmProviderOperation *' or 'Get-AzureRmProviderOperation */virtualmachines/*'
                // get operations for all providers
                providers.AddRange(ResourcesClient.ListProviderOperationsMetadata());
            }
            else
            {
                // 'Get-AzureRmProviderOperation Microsoft.Compute/virtualmachines/*' or 'Get-AzureRmProviderOperation Microsoft.Sql/*'
                providers.Add(ResourcesClient.GetProviderOperationsMetadata(provider));
            }

            return providers.SelectMany(p => GetPSOperationsFromProviderOperationsMetadata(p)).Where(operation => wildcard.IsMatch(operation.Operation)).ToList();
        }

        /// <summary>
        /// Gets a list of Provider operations in the case that the Actionstring input does not contain a wildcard
        /// </summary>
        private List<PSResourceProviderOperation> ProcessProviderOperationsWithoutWildCard(string operationString)
        {
            string providerFullName = operationString.Split(Separator).First();

            var providerOperations = ResourcesClient.GetProviderOperationsMetadata(providerFullName);
            IEnumerable<PSResourceProviderOperation> flattenedProviderOperations = GetPSOperationsFromProviderOperationsMetadata(providerOperations);
            return flattenedProviderOperations.Where(op => string.Equals(op.Operation, operationString, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        private static IEnumerable<PSResourceProviderOperation> GetPSOperationsFromProviderOperationsMetadata(ProviderOperationsMetadata providerOperationsMetadata)
        {
            IEnumerable<PSResourceProviderOperation> operations = providerOperationsMetadata.Operations.Where(op => IsUserOperation(op))
                        .Select(op => ToPSResourceProviderOperation(op, providerOperationsMetadata.DisplayName));
            if (providerOperationsMetadata.ResourceTypes != null)
            {
                operations = operations.Concat(providerOperationsMetadata.ResourceTypes.SelectMany(rt => rt.Operations.Where(op => IsUserOperation(op))
                    .Select(op => ToPSResourceProviderOperation(op, providerOperationsMetadata.DisplayName, rt.DisplayName))));
            }

            return operations;
        }

        private static bool IsUserOperation(ProviderOperation operation)
        {
            return operation.Origin == null || operation.Origin.Contains("user");
        }
        private static PSResourceProviderOperation ToPSResourceProviderOperation(ProviderOperation operation, string provider, string resource = null)
        {
            PSResourceProviderOperation psOperation = new PSResourceProviderOperation();
            psOperation.Operation = operation.Name;
            psOperation.OperationName = operation.DisplayName;
            psOperation.Description = operation.Description;
            psOperation.ProviderNamespace = provider;
            psOperation.ResourceName = resource ?? string.Empty;
            psOperation.IsDataAction = operation.IsDataAction.HasValue ? operation.IsDataAction.Value : false;

            return psOperation;
        }

        /// <summary>
        /// Extracts the resource provider's full name - i.e portion of the non-wildcard prefix before the '/'
        /// Returns null if the nonWildCardPrefix does not contain a '/'
        /// </summary>
        private static string GetResourceProviderFullName(string nonWildCardPrefix)
        {
            int index = nonWildCardPrefix.IndexOf(Separator.ToString(), 0);
            return index > 0 ? nonWildCardPrefix.Substring(0, index) : string.Empty;
        }

        /// <summary>
        /// Extracts the portion of the actionstring before the first wildcard character (*)
        /// </summary>
        private static string GetNonWildcardPrefix(string actionString)
        {
            int index = actionString.IndexOf(WildCardCharacter);
            return index >= 0 ? actionString.Substring(0, index) : actionString;
        }
    }
}