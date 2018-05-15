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
using System.Management.Automation;
using Microsoft.Azure.Commands.MachineLearning.Utilities;
using Microsoft.Azure.Management.MachineLearning.WebServices.Models;
using Microsoft.Azure.Management.MachineLearning.WebServices.Util;
using Microsoft.WindowsAzure.Commands.Common;
using Microsoft.Azure.Commands.Common.Authentication.Abstractions;
using Microsoft.Azure.Commands.ResourceManager.Common.ArgumentCompleters;

namespace Microsoft.Azure.Commands.MachineLearning.Cmdlets
{
    [Cmdlet(
        VerbsCommon.New, 
        CommandletSuffix,
        SupportsShouldProcess = true)]
    [OutputType(typeof(WebService))]
    public class NewAzureMLWebService : WebServicesCmdletBase
    {
        protected const string CreateFromFileParameterSet = "CreateFromFile";
        protected const string CreateFromObjectParameterSet = "CreateFromInstance";
       
        [Parameter(
            Mandatory = true, 
            HelpMessage = "The name of the resource group for the Azure ML web service.")]
        [ResourceGroupCompleter]
        [ValidateNotNullOrEmpty]
        public string ResourceGroupName { get; set; }

        [Parameter(Mandatory = true, HelpMessage = "The location of the AzureML.")]
        [LocationCompleter("Microsoft.MachineLearning/webServices")]
        [ValidateNotNullOrEmpty]
        public string Location { get; set; }
        
        [Parameter(Mandatory = true, HelpMessage = "The name of the web service")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        [Parameter(
            ParameterSetName = CreateFromFileParameterSet, 
            Mandatory = true, 
            HelpMessage = "The definition of the new web service.")]
        [ValidateNotNullOrEmpty]
        public string DefinitionFile { get; set; }

        [Parameter(
            ParameterSetName = CreateFromObjectParameterSet, 
            Mandatory = true, 
            HelpMessage = "The definition of the new web service.", 
            ValueFromPipeline = true)]
        [ValidateNotNullOrEmpty]
        public WebService NewWebServiceDefinition { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates if the user should be prompted for confirmation.
        /// </summary>
        [Parameter(
            Mandatory = false,
            HelpMessage = "Do not ask for confirmation.")]
        public SwitchParameter Force { get; set; }

        protected override void RunCmdlet()
        {
            ConfirmAction(
                Force.IsPresent,
                Resources.NewServiceWarning.FormatInvariant(Name), 
                "Creating the new web service", 
                Name, 
                () => {
                    if (string.Equals(
                                ParameterSetName,
                                CreateFromFileParameterSet,
                                StringComparison.OrdinalIgnoreCase))
                    {
                        string jsonDefinition = 
                                CmdletHelpers.GetWebServiceDefinitionFromFile(
                                                SessionState.Path.CurrentFileSystemLocation.Path,
                                                DefinitionFile);
                        var webServiceFromJson = 
                                ModelsSerializationUtil.GetAzureMLWebServiceFromJsonDefinition(jsonDefinition);

                        // The name and location in command line parameters overwrite the content from 
                        // Web Service Definition json file.
                        NewWebServiceDefinition = new WebService(
                                                                Location, 
                                                                webServiceFromJson.Properties, 
                                                                webServiceFromJson.Id, 
                                                                Name, webServiceFromJson.Type, 
                                                                webServiceFromJson.Tags);
                    }

                    WebService newWebService =
                        WebServicesClient.CreateAzureMlWebService(
                                                    ResourceGroupName,
                                                    Name,
                                                    NewWebServiceDefinition);
                    WriteObject(newWebService);
                });
        }
    }
}
