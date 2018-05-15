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
    using System.Security.Cryptography.X509Certificates;
    using Utilities;
    using Management.Logic.Models;
    using WindowsAzure.Commands.Utilities.Common;
    using ResourceManager.Common.ArgumentCompleters;

    /// <summary>
    /// Updates the integration account certificate.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "AzureRmIntegrationAccountCertificate", SupportsShouldProcess = true)]
    [OutputType(typeof(IntegrationAccountCertificate))]
    public class UpdateAzureIntegrationAccountCertificateCommand : LogicAppBaseCmdlet
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

        [Parameter(Mandatory = true, HelpMessage = "The integration account certificate name.",
            ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string CertificateName { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "The integration account certificate key name.",
            ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string KeyName { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "The integration account certificate key version.",
            ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string KeyVersion { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "The integration account certificate key vault ID.",
            ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string KeyVaultId { get; set; }

        [Parameter(Mandatory = false,
            HelpMessage = "The integration account certificate file path.",
            ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string PublicCertificateFilePath { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "The integration account certificate metadata.",
            ValueFromPipelineByPropertyName = false)]
        [ValidateNotNullOrEmpty]
        public object Metadata { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Do not ask for confirmation.")]
        public SwitchParameter Force { get; set; }

        #endregion Input Parameters

        /// <summary>
        /// Executes the integration account certificate update command.
        /// </summary>
        public override void ExecuteCmdlet()
        {
            base.ExecuteCmdlet();

            var integrationAccount = IntegrationAccountClient.GetIntegrationAccount(ResourceGroupName, Name);

            var integrationAccountCertificate =
                IntegrationAccountClient.GetIntegrationAccountCertifcate(ResourceGroupName,
                    Name, CertificateName);

            if (!string.IsNullOrEmpty(KeyName))
            {
                integrationAccountCertificate.Key.KeyName = KeyName;
            }

            if (!string.IsNullOrEmpty(KeyVersion))
            {
                integrationAccountCertificate.Key.KeyVersion = KeyVersion;
            }

            if (!string.IsNullOrEmpty(KeyVaultId))
            {
                integrationAccountCertificate.Key.KeyVault.Id = KeyVaultId;
            }

            string certificate = null;

            if (!string.IsNullOrEmpty(PublicCertificateFilePath))
            {
                var certificateFilePath = this.TryResolvePath(PublicCertificateFilePath);

                if (!string.IsNullOrEmpty(certificateFilePath) && CmdletHelper.FileExists(certificateFilePath))
                {
                    var cert = new X509Certificate2(certificateFilePath);
                    certificate = Convert.ToBase64String(cert.RawData);
                }
            }

            if (!string.IsNullOrEmpty(certificate))
            {
                integrationAccountCertificate.PublicCertificate = certificate;
            }

            if (Metadata != null)
            {
                integrationAccountCertificate.Metadata = CmdletHelper.ConvertToMetadataJObject(Metadata);
            }

            ConfirmAction(Force.IsPresent,
                string.Format(CultureInfo.InvariantCulture, Properties.Resource.UpdateResourceWarning,
                    "Microsoft.Logic/integrationAccounts/certificates", Name),
                string.Format(CultureInfo.InvariantCulture, Properties.Resource.UpdateResourceMessage,
                    "Microsoft.Logic/integrationAccounts/certificates", Name),
                Name,
                () =>
                {
                    WriteObject(
                        IntegrationAccountClient.UpdateIntegrationAccountCertificate(ResourceGroupName,
                            integrationAccount.Name,
                            CertificateName, integrationAccountCertificate), true);
                },
                null);
        }
    }
}