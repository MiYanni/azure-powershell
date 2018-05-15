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
    using System.Globalization;
    using System.Linq;
    using ResourceManager.Common.ArgumentCompleters;

    /// <summary>
    /// Updates the integration account agreement.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "AzureRmIntegrationAccountAgreement", SupportsShouldProcess = true)]
    [OutputType(typeof(IntegrationAccountAgreement))]
    public class UpdateAzureIntegrationAccountAgreementCommand : LogicAppBaseCmdlet
    {

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

        [Parameter(Mandatory = true, HelpMessage = "The integration account agreement name.",
            ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string AgreementName { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "The integration account agreement type.",
            ValueFromPipelineByPropertyName = false)]
        [ValidateNotNullOrEmpty]
        [ValidateSet("X12", "AS2", "Edifact", IgnoreCase = false)]
        public string AgreementType { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "The integration account agreement guest partner.",
            ValueFromPipelineByPropertyName = false)]
        [ValidateNotNullOrEmpty]
        public string GuestPartner { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "The integration account agreement host partner.",
            ValueFromPipelineByPropertyName = false)]
        [ValidateNotNullOrEmpty]
        public string HostPartner { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "The integration account agreement guest identity qualifier.",
            ValueFromPipelineByPropertyName = false)]
        [ValidateNotNullOrEmpty]
        public string GuestIdentityQualifier { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "The integration account agreement guest identity qualifier value.",
            ValueFromPipelineByPropertyName = false)]
        [ValidateNotNullOrEmpty]
        public string GuestIdentityQualifierValue { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "The integration account agreement host identity qualifier.",
            ValueFromPipelineByPropertyName = false)]
        [ValidateNotNullOrEmpty]
        public string HostIdentityQualifier { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "The integration account agreement host identity qualifier value.",
            ValueFromPipelineByPropertyName = false)]
        [ValidateNotNullOrEmpty]
        public string HostIdentityQualifierValue { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "The integration account agreement content.",
            ValueFromPipelineByPropertyName = false)]
        [ValidateNotNullOrEmpty]
        public string AgreementContent { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "The integration account agreement content.",
            ValueFromPipelineByPropertyName = false)]
        [ValidateNotNullOrEmpty]
        public string AgreementContentFilePath { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "The integration account agreement metadata.",
            ValueFromPipelineByPropertyName = false)]
        [ValidateNotNullOrEmpty]
        public object Metadata { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Do not ask for confirmation.")]
        public SwitchParameter Force { get; set; }

        #endregion Input Parameters

        /// <summary>
        /// Executes the integration account agreement update command.
        /// </summary>
        public override void ExecuteCmdlet()
        {
            base.ExecuteCmdlet();

            var integrationAccountAgreement =
                IntegrationAccountClient.GetIntegrationAccountAgreement(ResourceGroupName,
                    Name, AgreementName);

            if (Metadata != null)
            {
                integrationAccountAgreement.Metadata = CmdletHelper.ConvertToMetadataJObject(Metadata);
            }

            var hostPartner = IntegrationAccountClient.GetIntegrationAccountPartner(ResourceGroupName, Name,
                string.IsNullOrEmpty(HostPartner)
                    ? integrationAccountAgreement.HostPartner
                    : HostPartner);
            integrationAccountAgreement.HostPartner = hostPartner.Name;

            var guestPartner = IntegrationAccountClient.GetIntegrationAccountPartner(ResourceGroupName, Name,
                string.IsNullOrEmpty(GuestPartner)
                    ? integrationAccountAgreement.GuestPartner
                    : GuestPartner);
            integrationAccountAgreement.GuestPartner = guestPartner.Name;

            if (!string.IsNullOrEmpty(HostIdentityQualifier) && !string.IsNullOrEmpty(HostIdentityQualifierValue))
            {
                var hostIdentity =
                    hostPartner.Content.B2b.BusinessIdentities.FirstOrDefault(
                        s => s.Qualifier == HostIdentityQualifier && s.Value == HostIdentityQualifierValue);

                if (hostIdentity == null)
                {
                    throw new PSArgumentException(string.Format(CultureInfo.InvariantCulture,
                        Properties.Resource.InvalidQualifierSpecified, HostIdentityQualifier, hostPartner.Name));
                }

                integrationAccountAgreement.HostIdentity = hostIdentity;
            }
            else  if (string.IsNullOrEmpty(HostIdentityQualifier) ^ string.IsNullOrEmpty(HostIdentityQualifierValue))
            {
                throw new PSArgumentException(string.Format(CultureInfo.InvariantCulture,Properties.Resource.QualifierWithValueNotSpecified, "Host"));
            }

            if (!string.IsNullOrEmpty(GuestIdentityQualifier) && !string.IsNullOrEmpty(GuestIdentityQualifierValue))
            {
                var guestIdentity =
                    guestPartner.Content.B2b.BusinessIdentities.FirstOrDefault(
                        s => s.Qualifier == GuestIdentityQualifier && s.Value == GuestIdentityQualifierValue);

                if (guestIdentity == null)
                {
                    throw new PSArgumentException(string.Format(CultureInfo.InvariantCulture,
                        Properties.Resource.InvalidQualifierSpecified, GuestIdentityQualifier, guestPartner.Name));
                }

                integrationAccountAgreement.GuestIdentity = guestIdentity;
            }
            else if (string.IsNullOrEmpty(GuestIdentityQualifier) ^ string.IsNullOrEmpty(GuestIdentityQualifierValue))
            {
                throw new PSArgumentException(string.Format(CultureInfo.InvariantCulture, Properties.Resource.QualifierWithValueNotSpecified, "Guest"));
            }

            if (!string.IsNullOrEmpty(AgreementType))
            {
                integrationAccountAgreement.AgreementType =
                    (AgreementType) Enum.Parse(typeof(AgreementType), AgreementType);
            }

            if (!string.IsNullOrEmpty(AgreementContentFilePath))
            {
                AgreementContent =
                    CmdletHelper.GetContentFromFile(this.TryResolvePath(AgreementContentFilePath));
            }

            if (!string.IsNullOrEmpty(AgreementContent))
            {
                integrationAccountAgreement.Content = CmdletHelper.ConvertToAgreementContent(AgreementContent);
            }

            ConfirmAction(Force.IsPresent,
                string.Format(CultureInfo.InvariantCulture, Properties.Resource.UpdateResourceWarning,
                    "Microsoft.Logic/integrationAccounts/agreements", Name),
                string.Format(CultureInfo.InvariantCulture, Properties.Resource.UpdateResourceMessage,
                    "Microsoft.Logic/integrationAccounts/agreements", Name),
                Name,
                () =>
                {
                    WriteObject(
                        IntegrationAccountClient.UpdateIntegrationAccountAgreement(ResourceGroupName, Name,
                            AgreementName,
                            integrationAccountAgreement), true);

                },
                null);
        }
    }
}