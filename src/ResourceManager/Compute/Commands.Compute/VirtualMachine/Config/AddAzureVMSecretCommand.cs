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

using Microsoft.Azure.Commands.Compute.Common;
using Microsoft.Azure.Commands.Compute.Models;
using Microsoft.Azure.Management.Compute.Models;
using System.Collections.Generic;
using System.Management.Automation;

namespace Microsoft.Azure.Commands.Compute
{
    /// <summary>
    /// Add a Vault Secret Group object to VM
    /// </summary>
    [Cmdlet(
        VerbsCommon.Add,
        ProfileNouns.VaultSecretGroup),
    OutputType(
        typeof(PSVirtualMachine))]
    public class NewAzureVaultSecretGroupCommand : ResourceManager.Common.AzureRMCmdlet
    {
        [Alias("VMProfile")]
        [Parameter(
            Mandatory = true,
            Position = 0,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = HelpMessages.VMProfile)]
        [ValidateNotNullOrEmpty]
        public PSVirtualMachine VM { get; set; }

        [Alias("Id")]
        [Parameter(
            Position = 1,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The ID for Source Vault")]
        [ValidateNotNullOrEmpty]
        public string SourceVaultId { get; set; }

        [Parameter(
            Position = 2,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Certificate store in LocalMachine")]
        [ValidateNotNullOrEmpty]
        public string CertificateStore { get; set; }

        [Parameter(
            Position = 3,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "URL referencing a secret in a Key Vault.")]
        [ValidateNotNullOrEmpty]
        public string CertificateUrl { get; set; }

        public override void ExecuteCmdlet()
        {
            if (VM.OSProfile == null)
            {
                VM.OSProfile = new OSProfile();
            }

            if (VM.OSProfile.Secrets == null)
            {
                VM.OSProfile.Secrets = new List<VaultSecretGroup>();
            }

            int i = 0;

            for (; i <= VM.OSProfile.Secrets.Count; i++)
            {
                if (i == VM.OSProfile.Secrets.Count)
                {
                    var sourceVault = new SubResource
                    {
                        Id = SourceVaultId
                    };

                    var vaultCertificates = new List<VaultCertificate>{
                        new VaultCertificate
                        {
                            CertificateStore = CertificateStore,
                            CertificateUrl = CertificateUrl,
                        }
                    };

                    VM.OSProfile.Secrets.Add(
                        new VaultSecretGroup
                        {
                            SourceVault = sourceVault,
                            VaultCertificates = vaultCertificates,
                        });

                    break;
                }

                if (VM.OSProfile.Secrets[i].SourceVault != null && VM.OSProfile.Secrets[i].SourceVault.Id.Equals(SourceVaultId))
                {
                    if (VM.OSProfile.Secrets[i].VaultCertificates == null)
                    {
                        VM.OSProfile.Secrets[i].VaultCertificates = new List<VaultCertificate>();
                    }

                    VM.OSProfile.Secrets[i].VaultCertificates.Add(
                        new VaultCertificate
                        {
                            CertificateStore = CertificateStore,
                            CertificateUrl = CertificateUrl,
                        });

                    break;
                }
            }

            WriteObject(VM);
        }
    }
}
