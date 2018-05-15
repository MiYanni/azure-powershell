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

using Microsoft.Azure.Commands.Dns.Models;
using Microsoft.Azure.Commands.ResourceManager.Common.ArgumentCompleters;
using System.Management.Automation;
using ProjectResources = Microsoft.Azure.Commands.Dns.Properties.Resources;

namespace Microsoft.Azure.Commands.Dns
{
    /// <summary>
    /// Gets one or more existing zones.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureRmDnsZone", DefaultParameterSetName = "Default"), OutputType(typeof(DnsZone))]
    public class GetAzureDnsZone : DnsBaseCmdlet
    {
        private const string ParameterSetResourceGroup = "ResourceGroup";
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, ParameterSetName = ParameterSetResourceGroup, HelpMessage = "The full name of the zone (without a terminating dot).")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = ParameterSetResourceGroup, HelpMessage = "The resource group in which the zone exists.")]
        [ResourceGroupCompleter]
        [ValidateNotNullOrEmpty]
        public string ResourceGroupName { get; set; }

        public override void ExecuteCmdlet()
        {
            if (Name != null)
            {
                if (Name.EndsWith("."))
                {
                    Name = Name.TrimEnd('.');
                    WriteWarning(string.Format("Modifying zone name to remove terminating '.'.  Zone name used is \"{0}\".", Name));
                }

                WriteObject(DnsClient.GetDnsZone(Name, ResourceGroupName));
            }
            else if (!string.IsNullOrEmpty(ResourceGroupName))
            {
                WriteObject(DnsClient.ListDnsZonesInResourceGroup(ResourceGroupName), true);
            }
            else
            {
                WriteObject(DnsClient.ListDnsZonesInSubscription(), true);
            }
        }
    }
}
