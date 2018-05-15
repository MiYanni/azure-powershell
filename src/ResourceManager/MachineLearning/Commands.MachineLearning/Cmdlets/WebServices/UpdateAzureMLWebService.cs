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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Microsoft.Azure.Management.MachineLearning.WebServices.Models;
using Microsoft.WindowsAzure.Commands.Common;
using Microsoft.Azure.Commands.Common.Authentication.Abstractions;
using Microsoft.Azure.Commands.ResourceManager.Common.ArgumentCompleters;

namespace Microsoft.Azure.Commands.MachineLearning
{
    [Cmdlet(
        VerbsData.Update, 
        CommandletSuffix, 
        SupportsShouldProcess = true)]
    [OutputType(typeof(WebService))]
    public class UpdateAzureMLWebService : WebServicesCmdletBase
    {
        protected const string UpdateFromArgumentsParameterSet = "UpdateFromParameters";
        protected const string UpdateFromObjectParameterSet = "UpdateFromObject";

        [Parameter(Mandatory = true, HelpMessage = "The name of the resource group for the Azure ML web service.")]
        [ResourceGroupCompleter]
        [ValidateNotNullOrEmpty]
        public string ResourceGroupName { get; set; }

        [Parameter(Mandatory = true, HelpMessage = "The name of the web service.")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        [Parameter(
            ParameterSetName = UpdateFromArgumentsParameterSet, 
            Mandatory = false, 
            HelpMessage = "New title for the web service.")]
        public string Title { get; set; }

        [Parameter(
            ParameterSetName = UpdateFromArgumentsParameterSet, 
            Mandatory = false, 
            HelpMessage = "New description for the web service.")]
        public string Description { get; set; }

        [Parameter(
            ParameterSetName = UpdateFromArgumentsParameterSet, 
            Mandatory = false, 
            HelpMessage = "Mark the service as readonly.")]
        public SwitchParameter IsReadOnly { get; set; }

        [Parameter(
            ParameterSetName = UpdateFromArgumentsParameterSet, 
            Mandatory = false, 
            HelpMessage = "New access keys for the web service.")]
        public WebServiceKeys Keys { get; set; }

        [Parameter(
            ParameterSetName = UpdateFromArgumentsParameterSet, 
            Mandatory = false, 
            HelpMessage = "New access key for the storage account associated with the web service. This allows for key rotation if needed.")]
        public string StorageAccountKey { get; set; }

        [Parameter(
            ParameterSetName = UpdateFromArgumentsParameterSet, 
            Mandatory = false, 
            HelpMessage = "New diagnostics settings for the web service.")]
        public DiagnosticsConfiguration Diagnostics { get; set; }

        [Parameter(
            ParameterSetName = UpdateFromArgumentsParameterSet, 
            Mandatory = false, 
            HelpMessage = "New realtime endpoint runtime settings for the web service.")]
        public RealtimeConfiguration RealtimeConfiguration { get; set; }

        [Parameter(
            ParameterSetName = UpdateFromArgumentsParameterSet, 
            Mandatory = false, 
            HelpMessage = "Updated assets for the web service.")]
        public Hashtable Assets { get; set; }

        [Parameter(
            ParameterSetName = UpdateFromArgumentsParameterSet, 
            Mandatory = false, 
            HelpMessage = "Updated input schema for the web service.")]
        public ServiceInputOutputSpecification Input { get; set; }

        [Parameter(
            ParameterSetName = UpdateFromArgumentsParameterSet, 
            Mandatory = false, 
            HelpMessage = "Updated output schema for the web service.")]
        public ServiceInputOutputSpecification Output { get; set; }

        [Parameter(
            ParameterSetName = UpdateFromArgumentsParameterSet, 
            Mandatory = false, 
            HelpMessage = "Updated global parameter values definition for the web service.")]
        public Hashtable Parameters { get; set; }

        [Parameter(
            ParameterSetName = UpdateFromArgumentsParameterSet, 
            Mandatory = false, 
            HelpMessage = "Updated graph package for the web service.")]
        public GraphPackage Package { get; set; }

        [Parameter(
            ParameterSetName = UpdateFromObjectParameterSet, 
            Mandatory = true, 
            HelpMessage = "An updated definition object to update the referenced web service with.", 
            ValueFromPipeline = true)]
        public WebService ServiceUpdates { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Do not ask for confirmation.")]
        public SwitchParameter Force { get; set; }

        protected override void RunCmdlet()
        {
            if (ShouldProcess(Name, @"Updating machine learning web service.."))
            {
                bool isUpdateToReadonly = IsReadOnly.IsPresent;
                if (string.Equals(
                            ParameterSetName,
                            UpdateFromObjectParameterSet,
                            StringComparison.OrdinalIgnoreCase))
                {
                    isUpdateToReadonly = ServiceUpdates.Properties != null &&
                                         ServiceUpdates.Properties.ReadOnlyProperty.HasValue &&
                                         ServiceUpdates.Properties.ReadOnlyProperty.Value;
                }

                var warningMessage = Resources.UpdateServiceWarning.FormatInvariant(Name);
                if (isUpdateToReadonly)
                {
                    warningMessage = Resources.UpdateServiceToReadonly.FormatInvariant(Name);
                }

                if (Force.IsPresent || ShouldContinue(warningMessage, string.Empty))
                {
                    UpdateWebServiceResource();
                }
            }
        }

        private void UpdateWebServiceResource()
        {
            WebService serviceDefinitionUpdate = ServiceUpdates;
            if (string.Equals(
                ParameterSetName, 
                UpdateFromArgumentsParameterSet, 
                StringComparison.OrdinalIgnoreCase))
            {
                serviceDefinitionUpdate = new WebService
                {
                    Properties = new WebServicePropertiesForGraph
                    {
                        Title = Title,
                        Description = Description,
                        Diagnostics = Diagnostics,
                        Keys = Keys,
                        Assets = Assets != null ? 
                                        Assets.Cast<DictionaryEntry>()
                                                .ToDictionary(
                                                    kvp => kvp.Key as string, 
                                                    kvp => kvp.Value as AssetItem)
                                        : null,
                        Input = Input,
                        Output = Output,
                        ReadOnlyProperty = IsReadOnly.IsPresent,
                        RealtimeConfiguration = RealtimeConfiguration,
                        Parameters = Parameters != null ? 
                                        Parameters.Cast<DictionaryEntry>()
                                                .ToDictionary(
                                                    kvp => kvp.Key as string,
                                                    kvp => kvp.Value as WebServiceParameter)
                                        : null,
                        Package = Package
                    }
                };

                if (!string.IsNullOrWhiteSpace(StorageAccountKey))
                {
                    serviceDefinitionUpdate.Properties.StorageAccount = 
                        new StorageAccount(null, StorageAccountKey);
                }
            }

            WebService updatedService = 
                WebServicesClient.UpdateAzureMlWebService(
                                        ResourceGroupName, 
                                        Name, 
                                        serviceDefinitionUpdate);
            WriteObject(updatedService);
        }
    }
}
