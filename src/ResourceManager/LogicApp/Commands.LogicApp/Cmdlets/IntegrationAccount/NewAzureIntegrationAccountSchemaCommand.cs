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

namespace Microsoft.Azure.Commands.LogicApp.Cmdlets
{
    using System;
    using System.Management.Automation;
    using Utilities;
    using Management.Logic.Models;
    using WindowsAzure.Commands.Utilities.Common;
    using ResourceManager.Common.ArgumentCompleters;

    /// <summary>
    /// Creates a new integration account schema.
    /// </summary>
    [Cmdlet(VerbsCommon.New, "AzureRmIntegrationAccountSchema", SupportsShouldProcess = true)]
    [OutputType(typeof(IntegrationAccountSchema))]
    public class NewAzureIntegrationAccountSchemaCommand : LogicAppBaseCmdlet
    {

        #region Defaults

        /// <summary>
        /// Default content type for schema.
        /// </summary>
        private string contentType = "application/xml";

        /// <summary>
        /// Default schema type.
        /// </summary>
        private string schemaType = "Xml";

        #endregion Defaults

        #region Input Parameters

        [Parameter(Mandatory = true, HelpMessage = "The integration account resource group name.",
            ValueFromPipelineByPropertyName = true)]
        [ResourceGroupCompleter]
        [ValidateNotNullOrEmpty]
        public string ResourceGroupName { get; set; }

        [Parameter(Mandatory = true, HelpMessage = "The integration account name.",
            ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        [Alias("IntegrationAccountName", "ResourceName")]
        public string Name { get; set; }

        [Parameter(Mandatory = true, HelpMessage = "The integration account schema name.",
            ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string SchemaName { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "The integration account schema file path.")]
        [ValidateNotNullOrEmpty]
        public string SchemaFilePath { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "The integration account schema definition.")]
        [ValidateNotNullOrEmpty]
        public string SchemaDefinition { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "The integration account schema type.")]
        [ValidateSet("Xml", IgnoreCase = false)]
        [ValidateNotNullOrEmpty]
        public string SchemaType
        {
            get { return schemaType; }
            set { value = schemaType; }
        }

        [Parameter(Mandatory = false, HelpMessage = "The integration account schema content type.")]
        [ValidateNotNullOrEmpty]
        public string ContentType
        {
            get { return contentType; }
            set { value = contentType; }
        }

        [Parameter(Mandatory = false, HelpMessage = "The integration account schema metadata.",
            ValueFromPipelineByPropertyName = false)]
        [ValidateNotNullOrEmpty]
        public object Metadata { get; set; }

        #endregion Input Parameters

        /// <summary>
        /// Executes the integration account schema create command.
        /// </summary>
        public override void ExecuteCmdlet()
        {
            base.ExecuteCmdlet();

            if (Metadata != null)
            {
                Metadata = CmdletHelper.ConvertToMetadataJObject(Metadata);
            }

            var integrationAccount = IntegrationAccountClient.GetIntegrationAccount(ResourceGroupName, Name);

            if (string.IsNullOrEmpty(SchemaDefinition))
            {
                SchemaDefinition = CmdletHelper.GetContentFromFile(this.TryResolvePath(SchemaFilePath));
            }

            WriteObject(
                IntegrationAccountClient.CreateIntegrationAccountSchema(ResourceGroupName, integrationAccount.Name,
                    SchemaName,
                    new IntegrationAccountSchema
                    {
                        ContentType = contentType,
                        SchemaType = (SchemaType) Enum.Parse(typeof(SchemaType), schemaType),                        
                        Content = SchemaDefinition,
                        Metadata = Metadata
                    }), true);
        }
    }
}