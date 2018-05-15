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
    using ResourceManager.Common.ArgumentCompleters;

    /// <summary>
    /// Update the integration account partner.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "AzureRmIntegrationAccountPartner", SupportsShouldProcess = true)]
    [OutputType(typeof(IntegrationAccountPartner))]
    public class UpdateAzureIntegrationAccountPartnerCommand : LogicAppBaseCmdlet
    {

        #region Defaults

        /// <summary>
        /// Default partner type.
        /// </summary>
        private string partnerType = "B2B";

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

        [Parameter(Mandatory = true, HelpMessage = "The integration account partner name.",
            ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string PartnerName { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "The integration account partner type.",
            ValueFromPipelineByPropertyName = false)]
        [ValidateSet("B2B", IgnoreCase = false)]
        [ValidateNotNullOrEmpty]
        public string PartnerType
        {
            get { return partnerType; }
            set { value = partnerType; }
        }

        [Parameter(Mandatory = false, HelpMessage = "The integration account partner business identities.",
            ValueFromPipelineByPropertyName = false)]
        [ValidateNotNullOrEmpty]
        public object BusinessIdentities { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "The integration account partner metadata.",
            ValueFromPipelineByPropertyName = false)]
        [ValidateNotNullOrEmpty]
        public object Metadata { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Do not ask for confirmation.")]
        public SwitchParameter Force { get; set; }

        #endregion Input Parameters

        /// <summary>
        /// Executes the integration account partner update command.
        /// </summary>
        public override void ExecuteCmdlet()
        {
            base.ExecuteCmdlet();

            var integrationAccount = IntegrationAccountClient.GetIntegrationAccount(ResourceGroupName, Name);

            var integrationAccountPartner = IntegrationAccountClient.GetIntegrationAccountPartner(
                ResourceGroupName,
                Name, PartnerName);

            if (!string.IsNullOrEmpty(PartnerType))
            {
                integrationAccountPartner.PartnerType = (PartnerType) Enum.Parse(typeof(PartnerType), PartnerType);
            }

            if (BusinessIdentities != null)
            {
                integrationAccountPartner.Content.B2b.BusinessIdentities =
                    CmdletHelper.ConvertToBusinessIdentityList(BusinessIdentities);
            }

            if (Metadata != null)
            {
                integrationAccountPartner.Metadata = CmdletHelper.ConvertToMetadataJObject(Metadata);
            }

            ConfirmAction(Force.IsPresent,
                string.Format(CultureInfo.InvariantCulture, Properties.Resource.UpdateResourceWarning,
                    "Microsoft.Logic/integrationAccounts/partners", Name),
                string.Format(CultureInfo.InvariantCulture, Properties.Resource.UpdateResourceMessage,
                    "Microsoft.Logic/integrationAccounts/partners", Name),
                Name,
                () =>
                {
                    WriteObject(
                        IntegrationAccountClient.UpdateIntegrationAccountPartner(ResourceGroupName,
                            integrationAccount.Name,
                            PartnerName,
                            integrationAccountPartner), true);
                },
                null);
        }
    }
}