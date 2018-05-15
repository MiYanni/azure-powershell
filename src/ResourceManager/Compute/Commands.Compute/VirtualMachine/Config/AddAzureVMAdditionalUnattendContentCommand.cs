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
using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace Microsoft.Azure.Commands.Compute
{
    /// <summary>
    /// Add an Additional Unattend Content Object to VM
    /// </summary>
    [Cmdlet(
        VerbsCommon.Add,
        ProfileNouns.AdditionalUnattendContent),
    OutputType(
        typeof(PSVirtualMachine))]
    public class NewAzureAdditionalUnattendContentCommand : ResourceManager.Common.AzureRMCmdlet
    {
        private const ComponentNames defaultComponentName = ComponentNames.MicrosoftWindowsShellSetup;
        private const PassNames defaultPassName = PassNames.OobeSystem;

        [Alias("VMProfile")]
        [Parameter(
            Mandatory = true,
            Position = 0,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = HelpMessages.VMProfile)]
        [ValidateNotNullOrEmpty]
        public PSVirtualMachine VM { get; set; }

        [Parameter(
            Position = 1,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "XML Formatted Content.")]
        [ValidateNotNullOrEmpty]
        public string Content { get; set; }

        [Parameter(
            Position = 2,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Setting Name.")]
        public SettingNames? SettingName { get; set; }

        public override void ExecuteCmdlet()
        {
            if (VM.OSProfile == null)
            {
                VM.OSProfile = new OSProfile();
            }

            if (VM.OSProfile.WindowsConfiguration == null && VM.OSProfile.LinuxConfiguration == null)
            {
                VM.OSProfile.WindowsConfiguration = new WindowsConfiguration();
            }
            else if (VM.OSProfile.WindowsConfiguration == null && VM.OSProfile.LinuxConfiguration != null)
            {
                throw new ArgumentException(Properties.Resources.BothWindowsAndLinuxConfigurationsSpecified);
            }

            if (VM.OSProfile.WindowsConfiguration.AdditionalUnattendContent == null)
            {
                VM.OSProfile.WindowsConfiguration.AdditionalUnattendContent = new List<AdditionalUnattendContent>();
            }

            VM.OSProfile.WindowsConfiguration.AdditionalUnattendContent.Add(
                new AdditionalUnattendContent
                {
                    ComponentName = defaultComponentName,
                    Content = Content,
                    PassName = defaultPassName,
                    SettingName = SettingName,
                });

            WriteObject(VM);
        }
    }
}
