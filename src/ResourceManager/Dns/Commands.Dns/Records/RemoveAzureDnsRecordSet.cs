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
using Microsoft.Azure.Management.Dns.Models;
using System;
using System.Management.Automation;
using ProjectResources = Microsoft.Azure.Commands.Dns.Properties.Resources;

namespace Microsoft.Azure.Commands.Dns
{
    /// <summary>
    /// Deletes an existing record set.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "AzureRmDnsRecordSet", SupportsShouldProcess = true),
        OutputType(typeof(bool))]
    public class RemoveAzureDnsRecordSet : DnsBaseCmdlet
    {
        [Parameter(Mandatory = true, ParameterSetName = "Fields", HelpMessage = "The name of the records in the record set (relative to the name of the zone and without a terminating dot).")]
        [Parameter(Mandatory = true, ParameterSetName = "Mixed")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The type of DNS records in the record set.", ParameterSetName = "Fields")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The type of DNS records in the record set.", ParameterSetName = "Mixed")]
        [ValidateNotNullOrEmpty]
        public RecordType RecordType { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The zone in which the record set exists (without a terminating dot).", ParameterSetName = "Fields")]
        [ValidateNotNullOrEmpty]
        public string ZoneName { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The resource group to which the zone belongs.", ParameterSetName = "Fields")]
        [ResourceGroupCompleter]
        [ValidateNotNullOrEmpty]
        public string ResourceGroupName { get; set; }

        [Parameter(Mandatory = true, ValueFromPipeline = true, HelpMessage = "The DnsZone object representing the zone in which to create the record set.", ParameterSetName = "Mixed")]
        [ValidateNotNullOrEmpty]
        public DnsZone Zone { get; set; }

        [Parameter(Mandatory = true, ValueFromPipeline = true, HelpMessage = "The record set in which to add the record.", ParameterSetName = "Object")]
        [ValidateNotNullOrEmpty]
        public DnsRecordSet RecordSet { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Do not use the ETag field of the RecordSet parameter for optimistic concurrency checks.", ParameterSetName = "Object")]
        public SwitchParameter Overwrite { get; set; }

        [Parameter(Mandatory = false)]
        public SwitchParameter PassThru { get; set; }

        public override void ExecuteCmdlet()
        {
            bool deleted = false;
            DnsRecordSet recordSetToDelete = null;

            if (ParameterSetName == "Fields")
            {
                if (Name.EndsWith("."))
                {
                    Name = Name.TrimEnd('.');
                    WriteWarning(string.Format("Modifying recordset name to remove terminating '.'.  Recordset name used is \"{0}\".", Name));
                }

                recordSetToDelete = new DnsRecordSet
                {
                    Name = Name,
                    Etag = null,
                    RecordType = RecordType,
                    ResourceGroupName = ResourceGroupName,
                    ZoneName = ZoneName,
                };
            }
            else if (ParameterSetName == "Mixed")
            {
                if (Name.EndsWith("."))
                {
                    Name = Name.TrimEnd('.');
                    WriteWarning(string.Format("Modifying recordset name to remove terminating '.'.  Recordset name used is \"{0}\".", Name));
                }

                recordSetToDelete = new DnsRecordSet
                {
                    Name = Name,
                    Etag = null,
                    RecordType = RecordType,
                    ResourceGroupName = Zone.ResourceGroupName,
                    ZoneName = Zone.Name,
                };
            }
            else if (ParameterSetName == "Object")
            {
                if ((string.IsNullOrWhiteSpace(RecordSet.Etag) || RecordSet.Etag == "*") && !Overwrite.IsPresent)
                {
                    throw new PSArgumentException(string.Format(ProjectResources.Error_EtagNotSpecified, typeof(DnsRecordSet).Name));
                }

                recordSetToDelete = RecordSet;
            }

            if (recordSetToDelete.ZoneName != null && recordSetToDelete.ZoneName.EndsWith("."))
            {
                recordSetToDelete.ZoneName = recordSetToDelete.ZoneName.TrimEnd('.');
                WriteWarning(string.Format("Modifying zone name to remove terminating '.'.  Zone name used is \"{0}\".", recordSetToDelete.ZoneName));
            }

            bool overwrite = Overwrite.IsPresent || ParameterSetName != "Object";

            ConfirmAction(
                ProjectResources.Progress_RemovingRecordSet,
                Name,
                () =>
                {
                    deleted = DnsClient.DeleteDnsRecordSet(recordSetToDelete, overwrite);
                    if (deleted)
                    {
                        WriteVerbose(ProjectResources.Success);
                        WriteVerbose(ProjectResources.Success_RemoveRecordSet);
                    }

                    if (PassThru)
                    {
                        WriteObject(deleted);
                    }
                });
        }
    }
}
