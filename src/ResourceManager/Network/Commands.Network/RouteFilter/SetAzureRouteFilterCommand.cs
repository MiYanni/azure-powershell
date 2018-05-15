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
    using System;
    using System.Management.Automation;

    using AutoMapper;

    using Models;
    using MNM = Management.Network.Models;
    using ResourceManager.Common.Tags;
    using Management.Network;

    [Cmdlet(VerbsCommon.Set, "AzureRmRouteFilter", SupportsShouldProcess = true), OutputType(typeof(PSRouteFilter))]
    public class SetAzureRouteFilterCommand : RouteFilterBaseCmdlet
    {
        [Parameter(
            Mandatory = true,
            ValueFromPipeline = true,
            HelpMessage = "The RouteFilter")]
        public PSRouteFilter RouteFilter { get; set; }

        [Parameter(
            Mandatory = false,
            HelpMessage = "Do not ask for confirmation if you want to overrite a resource")]
        public SwitchParameter Force { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Run cmdlet in the background")]
        public SwitchParameter AsJob { get; set; }

        public override void Execute()
        {

            base.Execute();

            ConfirmAction(
                Force.IsPresent,
                string.Format(Properties.Resources.OverwritingResource, RouteFilter.Name),
                Properties.Resources.CreatingResourceMessage,
                RouteFilter.Name,
                () =>
                {
                    // Map to the sdk object
                    var routeFilterModel = NetworkResourceManagerProfile.Mapper.Map<MNM.RouteFilter>(RouteFilter);
                    routeFilterModel.Tags = TagsConversionHelper.CreateTagDictionary(RouteFilter.Tag, validate: true);

                    // Execute the PUT RouteTable call
                    RouteFilterClient.CreateOrUpdate(RouteFilter.ResourceGroupName, RouteFilter.Name, routeFilterModel);

                    var getRouteTable = GetRouteFilter(RouteFilter.ResourceGroupName, RouteFilter.Name);
                    WriteObject(getRouteTable);
                });
        }
    }
}
