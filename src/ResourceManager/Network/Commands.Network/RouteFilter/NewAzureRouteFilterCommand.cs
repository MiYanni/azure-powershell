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

namespace Microsoft.Azure.Commands.Network
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Management.Automation;

    using AutoMapper;

    using Models;

    using MNM = Management.Network.Models;
    using ResourceManager.Common.Tags;
    using Management.Network;
    using ResourceManager.Common.ArgumentCompleters;

    [Cmdlet(VerbsCommon.New, "AzureRmRouteFilter", SupportsShouldProcess = true),OutputType(typeof(PSRouteFilter))]
    public class NewAzureRouteFilterCommand : RouteFilterBaseCmdlet
    {
        [Alias("ResourceName")]
        [Parameter(
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The resource name.")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        [Parameter(
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The resource group name.")]
        [ResourceGroupCompleter]
        [ValidateNotNullOrEmpty]
        public string ResourceGroupName { get; set; }

        [Parameter(
         Mandatory = true,
         ValueFromPipelineByPropertyName = true,
         HelpMessage = "location.")]
        [LocationCompleter("Microsoft.Network/routeFilters")]
        [ValidateNotNullOrEmpty]
        public string Location { get; set; }

        [Parameter(
             Mandatory = false,
             ValueFromPipelineByPropertyName = true,
             HelpMessage = "The list of Routes")]
        public List<PSRouteFilterRule> Rule { get; set; }

        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "A hashtable which represents resource tags.")]
        public Hashtable Tag { get; set; }

        [Parameter(
            Mandatory = false,
            HelpMessage = "Do not ask for confirmation if you want to overrite a resource")]
        public SwitchParameter Force { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Run cmdlet in the background")]
        public SwitchParameter AsJob { get; set; }

        public override void Execute()
        {
            base.Execute();
            WriteWarning("The output object type of this cmdlet will be modified in a future release.");
            var present = IsRouteFilterPresent(ResourceGroupName, Name);
            ConfirmAction(
                Force.IsPresent,
                string.Format(Properties.Resources.OverwritingResource, Name),
                Properties.Resources.CreatingResourceMessage,
                Name,
                () =>
                {
                    var routeTable = CreateRouteFilter();
                    WriteObject(routeTable);
                },
                () => present);
        }

        private PSRouteFilter CreateRouteFilter()
        {
            var psRouteFilter = new PSRouteFilter();
            psRouteFilter.Name = Name;
            psRouteFilter.ResourceGroupName = ResourceGroupName;
            psRouteFilter.Location = Location;
            psRouteFilter.Rules = Rule;

            // Map to the sdk object
            var routeFilterModel = NetworkResourceManagerProfile.Mapper.Map<MNM.RouteFilter>(psRouteFilter);
            routeFilterModel.Tags = TagsConversionHelper.CreateTagDictionary(Tag, validate: true);

            // Execute the Create RouteTable call
            RouteFilterClient.CreateOrUpdate(ResourceGroupName, Name, routeFilterModel);

            var getRouteFilter = GetRouteFilter(ResourceGroupName, Name);

            return getRouteFilter;
        }
    }
}
