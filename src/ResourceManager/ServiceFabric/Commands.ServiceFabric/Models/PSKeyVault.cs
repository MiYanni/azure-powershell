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

using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Microsoft.Azure.Commands.ServiceFabric.Models
{
    public class PSKeyVault
    {
        public string KeyVaultId { get; set; }

        public string KeyVaultName { get; set; }

        public string KeyVaultCertificateId { get; set; }

        public string KeyVaultCertificateName { get; set; }

        public string SecretIdentifier { get; set; }

        public X509Certificate2 Certificate { get; set; }

        public string CertificateThumbprint { get; set; }

        public string CertificateSavedLocalPath { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();

            const string spaces = "    ";
            sb.AppendLine(string.Format("{0} {1} : {2}", "", "KeyVaultId", KeyVaultId));
            sb.AppendLine(string.Format("{0} {1} : {2}", "", "KeyVaultName", KeyVaultName));
            sb.AppendLine(string.Format("{0} {1} : {2}", "", "KeyVaultCertificateId", KeyVaultCertificateId));
            sb.AppendLine(string.Format("{0} {1} : {2}", "", "SecretIdentifier", SecretIdentifier));

            sb.AppendLine(string.Format("{0} {1} :", "", "Certificate:"));
            if (Certificate != null)
            {
                sb.AppendLine(string.Format("{0} {1} : {2}", spaces, "SubjectName", Certificate.SubjectName.Name));
                sb.AppendLine(string.Format("{0} {1} : {2}", spaces, "IssuerName", Certificate.IssuerName.Name));
                sb.AppendLine(string.Format("{0} {1} : {2}", spaces, "NotBefore", Certificate.NotBefore));
                sb.AppendLine(string.Format("{0} {1} : {2}", spaces, "NotAfter", Certificate.NotAfter));
            }

            sb.AppendLine(string.Format("{0} {1} : {2}", "", "CertificateThumbprint", CertificateThumbprint));
            sb.AppendLine(string.Format("{0} {1} : {2}", "", "CertificateSavedLocalPath", string.IsNullOrWhiteSpace(
                CertificateSavedLocalPath) ? "Not saved" : CertificateSavedLocalPath));

            return sb.ToString();
        }
    }
}
