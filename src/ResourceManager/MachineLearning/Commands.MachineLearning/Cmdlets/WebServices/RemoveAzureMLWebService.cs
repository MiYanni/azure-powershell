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
using Microsoft.WindowsAzure.Commands.Common;
using Microsoft.Azure.Commands.Common.Authentication.Abstractions;
using Microsoft.Azure.Commands.ResourceManager.Common.ArgumentCompleters;

namespace Microsoft.Azure.Commands.MachineLearning
{
    [Cmdlet(
        VerbsCommon.Remove, 
        CommandletSuffix,
        SupportsShouldProcess = true)]
    [OutputType(typeof(void))]
    public class RemoveAzureMLWebService : WebServicesCmdletBase
    {
        protected const string RemoveByNameGroupParameterSet = "RemoveByNameAndResourceGroup";
        protected const string RemoveByObjectParameterSet = "RemoveByObject";

        [Parameter(
            ParameterSetName = RemoveByNameGroupParameterSet, 
            Mandatory = true, 
            HelpMessage = "The name of the resource group for the Azure ML web service.")]
        [ResourceGroupCompleter]
        [ValidateNotNullOrEmpty]
        public string ResourceGroupName { get; set; }

        [Parameter(
            ParameterSetName = RemoveByNameGroupParameterSet, 
            Mandatory = true, 
            HelpMessage = "The name of the web service.")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        [Parameter(
            ParameterSetName = RemoveByObjectParameterSet, 
            Mandatory = true, 
            HelpMessage = "The machine learning web service object.", 
            ValueFromPipeline = true)]
        [ValidateNotNullOrEmpty]
        public WebService MlWebService { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Do not ask for confirmation.")]
        public SwitchParameter Force { get; set; }

        protected override void RunCmdlet()
        {
            if (ShouldProcess(Name, @"Deleting machine learning web service.."))
            {
                if (string.Equals(
                                ParameterSetName,
                                RemoveByObjectParameterSet,
                                StringComparison.OrdinalIgnoreCase))
                {
                    string subscriptionId, resourceGroup, webServiceName;
                    if (!CmdletHelpers.TryParseMlResourceMetadataFromResourceId(
                                        MlWebService.Id,
                                        out subscriptionId,
                                        out resourceGroup,
                                        out webServiceName))
                    {
                        throw new ValidationMetadataException(Resources.InvalidWebServiceIdOnObject);
                    }

                    ResourceGroupName = resourceGroup;
                    Name = webServiceName;
                }

                if (Force.IsPresent || 
                    ShouldContinue(
                        Resources.RemoveMlServiceWarning.FormatInvariant(Name), 
                        string.Empty))
                {
                    WebServicesClient.DeleteAzureMlWebService(
                                                                ResourceGroupName,
                                                                Name);
                }
            }
        }
    }
}
