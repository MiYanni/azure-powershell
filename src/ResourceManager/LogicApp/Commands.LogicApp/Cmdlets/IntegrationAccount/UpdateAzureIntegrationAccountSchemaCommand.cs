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
    using System.Globalization;
    using System.Management.Automation;
    using Utilities;
    using Management.Logic.Models;
    using WindowsAzure.Commands.Utilities.Common;
    using ResourceManager.Common.ArgumentCompleters;

    /// <summary>
    /// Updates the integration account schema.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "AzureRmIntegrationAccountSchema", SupportsShouldProcess = true)]
    [OutputType(typeof(IntegrationAccountSchema))]
    public class UpdateAzureIntegrationAccountSchemaCommand : LogicAppBaseCmdlet
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

        #region Input Paramters

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

        [Parameter(Mandatory = false, HelpMessage = "Do not ask for confirmation.")]
        public SwitchParameter Force { get; set; }

        #endregion Input Parameters

        /// <summary>
        /// Executes the integration account schema create command.
        /// </summary>
        public override void ExecuteCmdlet()
        {
            base.ExecuteCmdlet();

            var integrationAccount = IntegrationAccountClient.GetIntegrationAccount(ResourceGroupName, Name);

            var integrationAccountSchema = IntegrationAccountClient.GetIntegrationAccountSchema(ResourceGroupName,
                Name,
                SchemaName);

            var integrationAccountSchemaCopy = new IntegrationAccountSchema(integrationAccountSchema.SchemaType,
                integrationAccountSchema.Id,
                integrationAccountSchema.Name,
                integrationAccountSchema.Type,
                integrationAccountSchema.Location,
                integrationAccountSchema.Tags,
                integrationAccountSchema.TargetNamespace,
                integrationAccountSchema.DocumentName,
                integrationAccountSchema.FileName,
                integrationAccountSchema.CreatedTime,
                integrationAccountSchema.ChangedTime,
                integrationAccountSchema.Metadata,
                integrationAccountSchema.Content,
                integrationAccountSchema.ContentType,
                null);

            if (!string.IsNullOrEmpty(SchemaFilePath))
            {
                integrationAccountSchemaCopy.Content =
                    CmdletHelper.GetContentFromFile(this.TryResolvePath(SchemaFilePath));
            }

            if (!string.IsNullOrEmpty(SchemaDefinition))
            {
                integrationAccountSchemaCopy.Content = SchemaDefinition;
            }

            if (!string.IsNullOrEmpty(schemaType))
            {
                integrationAccountSchemaCopy.SchemaType = (SchemaType)Enum.Parse(typeof(SchemaType), SchemaType);
            }

            if (!string.IsNullOrEmpty(ContentType))
            {
                integrationAccountSchemaCopy.ContentType = ContentType;
            }

            if (Metadata != null)
            {
                integrationAccountSchemaCopy.Metadata = CmdletHelper.ConvertToMetadataJObject(Metadata);
            }

            ConfirmAction(Force.IsPresent,
                string.Format(CultureInfo.InvariantCulture, Properties.Resource.UpdateResourceWarning,
                    "Microsoft.Logic/integrationAccounts/schemas", Name),
                string.Format(CultureInfo.InvariantCulture, Properties.Resource.UpdateResourceMessage,
                    "Microsoft.Logic/integrationAccounts/schemas", Name),
                Name,
                () =>
                {
                    WriteObject(
                        IntegrationAccountClient.UpdateIntegrationAccountSchema(ResourceGroupName,
                            integrationAccount.Name,
                            SchemaName, integrationAccountSchemaCopy), true);
                },
                null);
        }
    }
}