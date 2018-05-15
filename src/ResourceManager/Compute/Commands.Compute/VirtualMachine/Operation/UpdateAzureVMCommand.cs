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
using System.Collections;
using System.Management.Automation;

namespace Microsoft.Azure.Commands.Compute
{
    [Cmdlet(VerbsData.Update,
        ProfileNouns.VirtualMachine,
        SupportsShouldProcess = true,
        DefaultParameterSetName = ResourceGroupNameParameterSet)]
    [OutputType(typeof(PSAzureOperationResponse))]
    public class UpdateAzureVMCommand : VirtualMachineBaseCmdlet
    {
        private const string ResourceGroupNameParameterSet = "ResourceGroupNameParameterSetName";
        private const string IdParameterSet = "IdParameterSetName";
        private const string AssignIdentityParameterSet = "AssignIdentityParameterSet";
        private const string ExplicitIdentityParameterSet = "ExplicitIdentityParameterSet";

        [Parameter(
           Mandatory = true,
           Position = 0,
           ParameterSetName = ResourceGroupNameParameterSet,
           ValueFromPipelineByPropertyName = true,
           HelpMessage = "The resource group name.")]
        [Parameter(
           Mandatory = true,
           Position = 0,
           ParameterSetName = AssignIdentityParameterSet,
           ValueFromPipelineByPropertyName = true,
           HelpMessage = "The resource group name.")]
        [Parameter(
           Mandatory = true,
           Position = 0,
           ParameterSetName = ExplicitIdentityParameterSet,
           ValueFromPipelineByPropertyName = true,
           HelpMessage = "The resource group name.")]
        [ResourceGroupCompleter]
        [ValidateNotNullOrEmpty]
        public string ResourceGroupName { get; set; }

        [Parameter(
           Mandatory = true,
           Position = 0,
           ParameterSetName = IdParameterSet,
           ValueFromPipelineByPropertyName = true,
           HelpMessage = "The resource group name.")]
        public string Id { get; set; }

        [Alias("VMProfile")]
        [Parameter(Mandatory = true, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public PSVirtualMachine VM { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = false)]
        public Hashtable Tag { get; set; }

        [Parameter(
            Mandatory = true,
            ParameterSetName = ExplicitIdentityParameterSet,
            ValueFromPipelineByPropertyName = false)]
        [ValidateNotNullOrEmpty]
        public ResourceIdentityType? IdentityType { get; set; }

        [Parameter(
            Mandatory = false,
            ParameterSetName = ExplicitIdentityParameterSet,
            ValueFromPipelineByPropertyName = false)]
        public string[] IdentityId { get; set; }

        [Parameter(
            Mandatory = true,
            ParameterSetName = AssignIdentityParameterSet,
            ValueFromPipelineByPropertyName = false)]
        [ValidateNotNullOrEmpty]
        public SwitchParameter AssignIdentity { get; set; }

        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = false)]
        public bool OsDiskWriteAccelerator { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Run cmdlet in the background")]
        public SwitchParameter AsJob { get; set; }

        public override void ExecuteCmdlet()
        {
            base.ExecuteCmdlet();

            if (ParameterSetName.Equals(IdParameterSet))
            {
                ResourceGroupName = GetResourceGroupNameFromId(Id);
            }

            if (ShouldProcess(VM.Name, VerbsData.Update))
            {
                ExecuteClientAction(() =>
                {
                    var parameters = new VirtualMachine
                    {
                        DiagnosticsProfile = VM.DiagnosticsProfile,
                        HardwareProfile = VM.HardwareProfile,
                        StorageProfile = VM.StorageProfile,
                        NetworkProfile = VM.NetworkProfile,
                        OsProfile = VM.OSProfile,
                        Plan = VM.Plan,
                        AvailabilitySet = VM.AvailabilitySetReference,
                        Location = VM.Location,
                        LicenseType = VM.LicenseType,
                        Tags = Tag != null ? Tag.ToDictionary() : VM.Tags,
                        Identity = AssignIdentity.IsPresent ? new VirtualMachineIdentity(null, null, ResourceIdentityType.SystemAssigned) : VM.Identity,
                        Zones = VM.Zones != null && VM.Zones.Count > 0 ? VM.Zones : null
                    };

                    if (IdentityType != null)
                    {
                        parameters.Identity = new VirtualMachineIdentity(null, null, IdentityType);
                    }

                    if (MyInvocation.BoundParameters.ContainsKey("OsDiskWriteAccelerator"))
                    {
                        if (parameters.StorageProfile == null)
                        {
                            parameters.StorageProfile = new StorageProfile();
                        }
                        if (parameters.StorageProfile.OsDisk == null)
                        {
                            parameters.StorageProfile.OsDisk = new OSDisk();
                        }
                        parameters.StorageProfile.OsDisk.WriteAcceleratorEnabled = OsDiskWriteAccelerator;
                    }

                    if (IdentityId != null)
                    {
                        if (parameters.Identity != null)
                        {
                            parameters.Identity.IdentityIds = IdentityId;
                        }
                    }

                    var op = VirtualMachineClient.CreateOrUpdateWithHttpMessagesAsync(
                        ResourceGroupName,
                        VM.Name,
                        parameters).GetAwaiter().GetResult();
                    var result = ComputeAutoMapperProfile.Mapper.Map<PSAzureOperationResponse>(op);
                    WriteObject(result);
                });
            }
        }
    }
}
