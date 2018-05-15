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
using Microsoft.Azure.Commands.ResourceManager.Common.ArgumentCompleters;
using Microsoft.Azure.Management.Compute.Models;
using System;
using System.Management.Automation;

namespace Microsoft.Azure.Commands.Compute
{
    [Cmdlet(
        VerbsCommon.Set,
        ProfileNouns.OSDisk,
        DefaultParameterSetName = DefaultParamSet),
    OutputType(
        typeof(PSVirtualMachine))]
    public class SetAzureVMOSDiskCommand : ResourceManager.Common.AzureRMCmdlet
    {
        protected const string DefaultParamSet = "DefaultParamSet";
        protected const string WindowsParamSet = "WindowsParamSet";
        protected const string LinuxParamSet = "LinuxParamSet";
        protected const string WindowsAndDiskEncryptionParameterSet = "WindowsDiskEncryptionParameterSet";
        protected const string LinuxAndDiskEncryptionParameterSet = "LinuxDiskEncryptionParameterSet";

        [Alias("VMProfile")]
        [Parameter(
            Mandatory = true,
            Position = 0,
            ValueFromPipeline = true,
            HelpMessage = HelpMessages.VMProfile)]
        [ValidateNotNullOrEmpty]
        public PSVirtualMachine VM { get; set; }

        [Alias("OSDiskName", "DiskName")]
        [Parameter(
            Mandatory = false,
            Position = 1,
            HelpMessage = HelpMessages.VMOSDiskName)]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        [Alias("OSDiskVhdUri", "DiskVhdUri")]
        [Parameter(
            Mandatory = false,
            Position = 2,
            HelpMessage = HelpMessages.VMOSDiskVhdUri)]
        [ValidateNotNullOrEmpty]
        public string VhdUri { get; set; }

        [Parameter(
            Position = 3,
            HelpMessage = HelpMessages.VMOSDiskCaching)]
        public CachingTypes? Caching { get; set; }

        [Alias("SourceImage")]
        [Parameter(
            Position = 4,
            HelpMessage = HelpMessages.VMSourceImageUri)]
        [ValidateNotNullOrEmpty]
        public string SourceImageUri { get; set; }

        [Parameter(
            Mandatory = false,
            Position = 5,
            HelpMessage = HelpMessages.VMDataDiskCreateOption)]
        public string CreateOption { get; set; }

        [Parameter(
            ParameterSetName = WindowsParamSet,
            Position = 6,
            HelpMessage = HelpMessages.VMOSDiskWindowsOSType)]
        [Parameter(
            ParameterSetName = WindowsAndDiskEncryptionParameterSet,
            Position = 6,
            HelpMessage = HelpMessages.VMOSDiskWindowsOSType)]
        public SwitchParameter Windows { get; set; }

        [Parameter(
            ParameterSetName = LinuxParamSet,
            Position = 6,
            HelpMessage = HelpMessages.VMOSDiskLinuxOSType)]
        [Parameter(
            ParameterSetName = LinuxAndDiskEncryptionParameterSet,
            Position = 6,
            HelpMessage = HelpMessages.VMOSDiskLinuxOSType)]
        public SwitchParameter Linux { get; set; }

        [Parameter(
            ParameterSetName = WindowsAndDiskEncryptionParameterSet,
            Mandatory = true,
            Position = 7,
            HelpMessage = HelpMessages.VMOSDiskDiskEncryptionKeyUrl)]
        [Parameter(
            ParameterSetName = LinuxAndDiskEncryptionParameterSet,
            Mandatory = true,
            Position = 7,
            HelpMessage = HelpMessages.VMOSDiskDiskEncryptionKeyUrl)]
        public string DiskEncryptionKeyUrl { get; set; }

        [Parameter(
            ParameterSetName = WindowsAndDiskEncryptionParameterSet,
            Mandatory = true,
            Position = 8,
            HelpMessage = HelpMessages.VMOSDiskDiskEncryptionKeyVaultId)]
        [Parameter(
            ParameterSetName = LinuxAndDiskEncryptionParameterSet,
            Mandatory = true,
            Position = 8,
            HelpMessage = HelpMessages.VMOSDiskDiskEncryptionKeyVaultId)]
        public string DiskEncryptionKeyVaultId { get; set; }

        [Parameter(
            ParameterSetName = WindowsAndDiskEncryptionParameterSet,
            Mandatory = false,
            Position = 9,
            HelpMessage = HelpMessages.VMOSDiskKeyEncryptionKeyUrl)]
        [Parameter(
            ParameterSetName = LinuxAndDiskEncryptionParameterSet,
            Mandatory = false,
            Position = 9,
            HelpMessage = HelpMessages.VMOSDiskKeyEncryptionKeyUrl)]
        public string KeyEncryptionKeyUrl { get; set; }

        [Parameter(
            ParameterSetName = WindowsAndDiskEncryptionParameterSet,
            Mandatory = false,
            Position = 10,
            HelpMessage = HelpMessages.VMOSDiskKeyEncryptionKeyVaultId)]
        [Parameter(
            ParameterSetName = LinuxAndDiskEncryptionParameterSet,
            Mandatory = false,
            Position = 10,
            HelpMessage = HelpMessages.VMOSDiskKeyEncryptionKeyVaultId)]
        public string KeyEncryptionKeyVaultId { get; set; }

        [Parameter(
            Mandatory = false,
            HelpMessage = HelpMessages.VMOSDiskSizeInGB)]
        [AllowNull]
        public int? DiskSizeInGB { get; set; }

        [Parameter(
            Mandatory = false,
            HelpMessage = HelpMessages.VMManagedDiskId)]
        [ValidateNotNullOrEmpty]
        public string ManagedDiskId { get; set; }

        [Parameter(
            Mandatory = false,
            HelpMessage = HelpMessages.VMManagedDiskAccountType)]
        [ValidateNotNullOrEmpty]
        [PSArgumentCompleter("Standard_LRS", "Premium_LRS")]
        public string StorageAccountType { get; set; }

        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = false)]
        public SwitchParameter WriteAccelerator { get; set; }

        public override void ExecuteCmdlet()
        {
            if (VM.StorageProfile == null)
            {
                VM.StorageProfile = new StorageProfile();
            }

            if (string.IsNullOrEmpty(KeyEncryptionKeyVaultId) && !string.IsNullOrEmpty(KeyEncryptionKeyUrl)
                || !string.IsNullOrEmpty(KeyEncryptionKeyVaultId) && string.IsNullOrEmpty(KeyEncryptionKeyUrl))
            {
                WriteError(new ErrorRecord(
                        new Exception(Properties.Resources.VMOSDiskDiskEncryptionBothKekVaultIdAndKekUrlRequired),
                        string.Empty, ErrorCategory.InvalidArgument, null));
            }

            if (VM.StorageProfile.OsDisk == null)
            {
                VM.StorageProfile.OsDisk = new OSDisk();
            }

            VM.StorageProfile.OsDisk.Name = Name ?? VM.StorageProfile.OsDisk.Name;
            VM.StorageProfile.OsDisk.Caching = Caching ?? VM.StorageProfile.OsDisk.Caching;
            VM.StorageProfile.OsDisk.DiskSizeGB = DiskSizeInGB ?? VM.StorageProfile.OsDisk.DiskSizeGB;

            if (Windows.IsPresent)
            {
                VM.StorageProfile.OsDisk.OsType = OperatingSystemTypes.Windows;
            }
            else if (Linux.IsPresent)
            {
                VM.StorageProfile.OsDisk.OsType = OperatingSystemTypes.Linux;
            }

            if (!string.IsNullOrEmpty(VhdUri))
            {
                VM.StorageProfile.OsDisk.Vhd = new VirtualHardDisk
                {
                    Uri = VhdUri
                };
            }

            if (!string.IsNullOrEmpty(SourceImageUri))
            {
                VM.StorageProfile.OsDisk.Image = new VirtualHardDisk
                {
                    Uri = SourceImageUri
                };
            }

            if (MyInvocation.BoundParameters.ContainsKey("CreateOption"))
            {
                VM.StorageProfile.OsDisk.CreateOption = CreateOption;
            }

            if (ParameterSetName.Equals(WindowsAndDiskEncryptionParameterSet) || ParameterSetName.Equals(LinuxAndDiskEncryptionParameterSet))
            {
                VM.StorageProfile.OsDisk.EncryptionSettings = new DiskEncryptionSettings
                {
                    DiskEncryptionKey = new KeyVaultSecretReference
                    {
                        SourceVault = new SubResource
                        {
                            Id = DiskEncryptionKeyVaultId
                        },
                        SecretUrl = DiskEncryptionKeyUrl
                    },
                    KeyEncryptionKey = KeyEncryptionKeyVaultId == null || KeyEncryptionKeyUrl == null
                    ? null
                    : new KeyVaultKeyReference
                    {
                        KeyUrl = KeyEncryptionKeyUrl,
                        SourceVault = new SubResource
                        {
                            Id = KeyEncryptionKeyVaultId
                        },
                    }
                };
            }

            if (!string.IsNullOrEmpty(ManagedDiskId) || StorageAccountType != null)
            {
                if (VM.StorageProfile.OsDisk.ManagedDisk == null)
                {
                    VM.StorageProfile.OsDisk.ManagedDisk = new ManagedDiskParameters();
                }

                VM.StorageProfile.OsDisk.ManagedDisk.Id = ManagedDiskId ?? VM.StorageProfile.OsDisk.ManagedDisk.Id;
                VM.StorageProfile.OsDisk.ManagedDisk.StorageAccountType = StorageAccountType ?? VM.StorageProfile.OsDisk.ManagedDisk.StorageAccountType;
            }

            VM.StorageProfile.OsDisk.WriteAcceleratorEnabled = WriteAccelerator.IsPresent;

            WriteObject(VM);
        }
    }
}
