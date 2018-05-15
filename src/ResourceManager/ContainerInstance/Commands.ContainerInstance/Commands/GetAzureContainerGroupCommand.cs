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

using System.Collections.Generic;
using System.Management.Automation;
using AutoMapper;
using Microsoft.Azure.Commands.ContainerInstance.Models;
using Microsoft.Azure.Management.ContainerInstance;
using Microsoft.Azure.Management.ContainerInstance.Models;
using Microsoft.Azure.Management.Internal.Resources;
using Microsoft.Rest.Azure;
using Microsoft.Azure.Commands.ResourceManager.Common.ArgumentCompleters;

namespace Microsoft.Azure.Commands.ContainerInstance
{
    /// <summary>
    /// Get-AzureRmContainerGroup.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, ContainerGroupNoun, DefaultParameterSetName = ListContainerGroupParamSet)]
    [OutputType(typeof(PSContainerGroup))]
    public class GetAzureContainerGroupCommand : ContainerInstanceCmdletBase
    {
        protected const string GetContainerGroupInResourceGroupParamSet = "GetContainerGroupInResourceGroupParamSet";
        protected const string GetContainerGroupByResourceIdParamSet = "GetContainerGroupByResourceIdParamSet";
        protected const string ListContainerGroupParamSet = "ListContainerGroupParamSet";

        [Parameter(
            Position = 0,
            Mandatory = true,
            ParameterSetName = GetContainerGroupInResourceGroupParamSet,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The resource Group Name.")]
        [Parameter(
            Position = 0,
            Mandatory = false,
            ParameterSetName = ListContainerGroupParamSet,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The resource Group Name.")]
        [ResourceGroupCompleter]
        [ValidateNotNullOrEmpty]
        public string ResourceGroupName { get; set; }

        [Parameter(
            Position = 1,
            Mandatory = true,
            ParameterSetName = GetContainerGroupInResourceGroupParamSet,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The container group Name.")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        [Parameter(
            Mandatory = true,
            ParameterSetName = GetContainerGroupByResourceIdParamSet,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The resource id.")]
        [ValidateNotNullOrEmpty]
        public string ResourceId { get; set; }

        public override void ExecuteCmdlet()
        {
            if (!string.IsNullOrEmpty(ResourceGroupName) && !string.IsNullOrEmpty(Name))
            {
                var psContainerGroup = PSContainerGroup.FromContainerGroup(
                    ContainerClient.ContainerGroups.Get(ResourceGroupName, Name));
                WriteObject(psContainerGroup);
            }
            else if (!string.IsNullOrEmpty(ResourceId))
            {
                var resource = ResourceClient.Resources.GetById(ResourceId, ContainerClient.ApiVersion);
                if (resource != null)
                {
                    var psContainerGroup = PSContainerGroup.FromContainerGroup(
                        ContainerClient.ContainerGroups.Get(ParseResourceGroupFromResourceId(ResourceId), resource.Name));
                    WriteObject(psContainerGroup);
                }
            }
            else
            {
                var psContainerGroups = new List<PSContainerGroupList>();
                var containerGroups = ListContainerGroups();
                foreach (var containerGroup in containerGroups)
                {
                    psContainerGroups.Add(ContainerInstanceAutoMapperProfile.Mapper.Map<PSContainerGroupList>(PSContainerGroup.FromContainerGroup(containerGroup)));
                }

                while (!string.IsNullOrEmpty(containerGroups.NextPageLink))
                {
                    containerGroups = ListContainerGroupsNext(containerGroups.NextPageLink);
                    foreach (var containerGroup in containerGroups)
                    {
                        psContainerGroups.Add(ContainerInstanceAutoMapperProfile.Mapper.Map<PSContainerGroupList>(PSContainerGroup.FromContainerGroup(containerGroup)));
                    }
                }

                WriteObject(psContainerGroups, true);
            }
        }

        private IPage<ContainerGroup> ListContainerGroups()
        {
            return string.IsNullOrEmpty(ResourceGroupName)
                ? ContainerClient.ContainerGroups.List()
                : ContainerClient.ContainerGroups.ListByResourceGroup(ResourceGroupName);
        }

        private IPage<ContainerGroup> ListContainerGroupsNext(string nextPageLink)
        {
            return string.IsNullOrEmpty(ResourceGroupName)
                ? ContainerClient.ContainerGroups.ListNext(nextPageLink)
                : ContainerClient.ContainerGroups.ListByResourceGroupNext(nextPageLink);
        }
    }
}
