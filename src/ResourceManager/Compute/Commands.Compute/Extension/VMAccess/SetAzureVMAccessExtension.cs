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
using Microsoft.WindowsAzure.Commands.Common;
using System.Collections;
using System.Management.Automation;

namespace Microsoft.Azure.Commands.Compute
{
    [Cmdlet(
        VerbsCommon.Set,
        ProfileNouns.VirtualMachineAccessExtension,
        SupportsShouldProcess = true)]
    [OutputType(typeof(PSAzureOperationResponse))]
    public class SetAzureVMAccessExtensionCommand : SetAzureVMExtensionBaseCmdlet
    {
        private const string userNameKey = "UserName";
        private const string passwordKey = "Password";

        [Parameter(
           Mandatory = false,
           ValueFromPipelineByPropertyName = true,
           HelpMessage = "Credential")]
        [ValidateNotNullOrEmpty]
        public PSCredential Credential { get; set; }

        public override void ExecuteCmdlet()
        {
            base.ExecuteCmdlet();

            if (ShouldProcess(VirtualMachineAccessExtensionContext.ExtensionDefaultName, VerbsCommon.Set))
            {
                ExecuteClientAction(() =>
                {
                    Hashtable publicSettings = new Hashtable();
                    Hashtable privateSettings = new Hashtable();

                    if (Credential != null)
                    {
                        publicSettings.Add(userNameKey, Credential.UserName ?? "");
                        privateSettings.Add(passwordKey, ConversionUtilities.SecureStringToString(Credential.Password));
                    }

                    if (string.IsNullOrEmpty(Location))
                    {
                        Location = GetLocationFromVm(ResourceGroupName, VMName);
                    }

                    var parameters = new VirtualMachineExtension
                    {
                        Location = Location,
                        VirtualMachineExtensionType = VirtualMachineAccessExtensionContext.ExtensionDefaultName,
                        Publisher = VirtualMachineAccessExtensionContext.ExtensionDefaultPublisher,
                        TypeHandlerVersion = TypeHandlerVersion ?? VirtualMachineAccessExtensionContext.ExtensionDefaultVersion,
                        Settings = publicSettings,
                        ProtectedSettings = privateSettings,
                        AutoUpgradeMinorVersion = !DisableAutoUpgradeMinorVersion.IsPresent,
                        ForceUpdateTag = ForceRerun
                    };

                    var op = VirtualMachineExtensionClient.CreateOrUpdateWithHttpMessagesAsync(
                        ResourceGroupName,
                        VMName,
                        Name,
                        parameters).GetAwaiter().GetResult();
                    var result = ComputeAutoMapperProfile.Mapper.Map<PSAzureOperationResponse>(op);
                    WriteObject(result);
                });
            }
        }
    }
}
