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

namespace Microsoft.Azure.Commands.Management.IotHub
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Management.Automation;
    using System.Text;
    using Common;
    using Models;
    using Azure.Management.IotHub;
    using Azure.Management.IotHub.Models;
    using ResourceManager.Common.ArgumentCompleters;

    [Cmdlet(VerbsCommon.Set, "AzureRmIotHubVerifiedCertificate", DefaultParameterSetName = ResourceParameterSet, SupportsShouldProcess = true)]
    [OutputType(typeof(PSCertificateDescription))]
    [Alias("Set-AzureRmIotHubVC")]
    public class SetAzureRmIotHubVerifiedCertificate : IotHubBaseCmdlet
    {
        private const string ResourceIdParameterSet = "ResourceIdSet";
        private const string ResourceParameterSet = "ResourceSet";
        private const string InputObjectParameterSet = "InputObjectSet";

        [Parameter(
            Position = 0,
            Mandatory = true,
            ParameterSetName = InputObjectParameterSet,
            ValueFromPipeline = true,
            HelpMessage = "Certificate Object")]
        [ValidateNotNullOrEmpty]
        public PSCertificateDescription InputObject { get; set; }

        [Parameter(
            Position = 0,
            Mandatory = true,
            ParameterSetName = ResourceParameterSet,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Name of the Resource Group")]
        [ValidateNotNullOrEmpty]
        [ResourceGroupCompleter]
        public string ResourceGroupName { get; set; }

        [Parameter(
            Position = 0,
            Mandatory = true,
            ParameterSetName = ResourceIdParameterSet,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Certificate Resource Id")]
        [ValidateNotNullOrEmpty]
        public string ResourceId { get; set; }

        [Parameter(
            Position = 1,
            Mandatory = true,
            ParameterSetName = ResourceParameterSet,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Name of the Iot Hub")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        [Parameter(
            Position = 2,
            Mandatory = true,
            ParameterSetName = ResourceParameterSet,
            HelpMessage = "Name of the Certificate")]
        [ValidateNotNullOrEmpty]
        public string CertificateName { get; set; }

        [Parameter(
           Position = 1,
           Mandatory = true,
           ParameterSetName = ResourceIdParameterSet,
           HelpMessage = "Etag of the Certificate")]
        [Parameter(
           Position = 3,
           Mandatory = true,
           ParameterSetName = ResourceParameterSet,
           HelpMessage = "Etag of the Certificate")]
        [ValidateNotNullOrEmpty]
        public string Etag { get; set; }

        [Parameter(
            Position = 1,
            Mandatory = true,
            ParameterSetName = InputObjectParameterSet,
            HelpMessage = "base-64 representation of X509 certificate .cer file or .pem file path.")]
        [Parameter(
            Position = 2,
            Mandatory = true,
            ParameterSetName = ResourceIdParameterSet,
            HelpMessage = "base-64 representation of X509 certificate .cer file or .pem file path.")]
        [Parameter(
            Position = 4,
            Mandatory = true,
            ParameterSetName = ResourceParameterSet,
            HelpMessage = "base-64 representation of X509 certificate .cer file or .pem file path.")]
        [ValidateNotNullOrEmpty]
        public string Path { get; set; }

        public override void ExecuteCmdlet()
        {
            if (ShouldProcess(CertificateName, Properties.Resources.VerifyIotHubCertificate))
            {
                string certificate = string.Empty;
                FileInfo fileInfo = new FileInfo(Path);
                switch (fileInfo.Extension.ToLower(CultureInfo.InvariantCulture))
                {
                    case ".cer":
                        var certificateByteContent = File.ReadAllBytes(Path);
                        certificate = Convert.ToBase64String(certificateByteContent);
                        break;
                    case ".pem":
                        certificate = File.ReadAllText(Path);
                        break;
                    default:
                        certificate = Path;
                        break;
                }

                if (ParameterSetName.Equals(InputObjectParameterSet))
                {
                    ResourceGroupName = InputObject.ResourceGroupName;
                    Name = InputObject.Name;
                    CertificateName = InputObject.CertificateName;
                    Etag = InputObject.Etag;
                }

                if (ParameterSetName.Equals(ResourceIdParameterSet))
                {
                    ResourceGroupName = IotHubUtils.GetResourceGroupName(ResourceId);
                    Name = IotHubUtils.GetIotHubName(ResourceId);
                    CertificateName = IotHubUtils.GetIotHubCertificateName(ResourceId);
                }

                try
                {
                    certificate = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(certificate));

                    CertificateVerificationDescription certificateVerificationDescription = new CertificateVerificationDescription();
                    certificateVerificationDescription.Certificate = certificate;

                    CertificateDescription certificateDescription = IotHubClient.Certificates.Verify(ResourceGroupName, Name, CertificateName, certificateVerificationDescription, Etag);
                    WriteObject(IotHubUtils.ToPSCertificateDescription(certificateDescription));
                }
                catch(Exception e)
                {
                    throw e;
                }
            }
        }
    }
}


