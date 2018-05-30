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

using AutoMapper;
using Microsoft.Azure.Commands.Network.Models;
using Microsoft.Azure.Commands.ResourceManager.Common.ArgumentCompleters;
using Microsoft.Azure.Commands.ResourceManager.Common.Tags;
using Microsoft.Azure.Management.Network;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using MNM = Microsoft.Azure.Management.Network.Models;

namespace Microsoft.Azure.Commands.Network
{
    [Cmdlet(VerbsCommon.Set, "AzureRmNetworkWatcherConnectionMonitor", SupportsShouldProcess = true, DefaultParameterSetName = "SetByName"),
        OutputType(typeof(PSConnectionMonitorResult))]
    public class SetAzureNetworkWatcherConnectionMonitorCommand : ConnectionMonitorBaseCmdlet
    {
        [Parameter(
             Mandatory = true,
             ValueFromPipeline = true,
             HelpMessage = "The network watcher resource.",
             ParameterSetName = "SetByResource")]
        [ValidateNotNull]
        public PSNetworkWatcher NetworkWatcher { get; set; }

        [Parameter(
            Mandatory = true,
            HelpMessage = "The name of network watcher.",
            ParameterSetName = "SetByName")]
        [ValidateNotNullOrEmpty]
        public string NetworkWatcherName { get; set; }

        [Parameter(
            Mandatory = true,
            HelpMessage = "The name of the network watcher resource group.",
            ParameterSetName = "SetByName")]
        [ResourceGroupCompleter]
        [ValidateNotNullOrEmpty]
        public string ResourceGroupName { get; set; }

        [Parameter(
            Mandatory = true,
            HelpMessage = "Location of the network watcher.",
            ParameterSetName = "SetByLocation")]
        [LocationCompleter("Microsoft.Network/networkWatchers/connectionMonitors")]
        [ValidateNotNull]
        public string Location { get; set; }

        [Parameter(
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Resource ID.",
            ParameterSetName = "SetByResourceId")]
        [ValidateNotNull]
        public string ResourceId { get; set; }

        [Parameter(
            Mandatory = true,
            ValueFromPipeline = true,
            HelpMessage = "Connection monitor object.",
            ParameterSetName = "SetByInputObject")]
        [ValidateNotNull]
        public PSConnectionMonitorResult InputObject { get; set; }

        [Alias("ConnectionMonitorName")]
        [Parameter(
            Mandatory = true,
            HelpMessage = "The connection monitor name.",
            ParameterSetName = "SetByName")]
        [Parameter(
            Mandatory = true,
            HelpMessage = "The connection monitor name.",
            ParameterSetName = "SetByResource")]
        [Parameter(
            Mandatory = true,
            HelpMessage = "The connection monitor name.",
            ParameterSetName = "SetByLocation")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        [Parameter(
            Mandatory = true,
            HelpMessage = "The ID of the connection monitor source.")]
        [ValidateNotNullOrEmpty]
        public string SourceResourceId { get; set; }

        [Parameter(
            Mandatory = false,
            HelpMessage = "Monitoring interval in seconds. Default value is 60 seconds.")]
        [ValidateNotNullOrEmpty]
        public int? MonitoringIntervalInSeconds { get; set; }

        [Parameter(
            Mandatory = false,
            HelpMessage = "Source port.")]
        [ValidateNotNull]
        [ValidateRange(1, int.MaxValue)]
        public int SourcePort { get; set; }

        [Parameter(
             Mandatory = false,
             HelpMessage = "The ID of the connection monitor destination.")]
        [ValidateNotNullOrEmpty]
        public string DestinationResourceId { get; set; }

        [Parameter(
            Mandatory = false,
            HelpMessage = "The Ip address of the connection monitor destination.")]
        [ValidateNotNullOrEmpty]
        public string DestinationAddress { get; set; }

        [Parameter(
            Mandatory = false,
            HelpMessage = "Destination port.")]
        [ValidateNotNull]
        [ValidateRange(1, int.MaxValue)]
        public int DestinationPort { get; set; }

        [Parameter(
            Mandatory = false,
            HelpMessage = "Configure connection monitor, but do not start it")]
        [ValidateNotNullOrEmpty]
        public SwitchParameter ConfigureOnly { get; set; }

        [Parameter(
            Mandatory = false,
            HelpMessage = "A hashtable which represents resource tags.")]
        public Hashtable Tag { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Run cmdlet in the background")]
        public SwitchParameter AsJob { get; set; }

        public string connectionMonitorName;

        public override void Execute()
        {
            base.Execute();

            connectionMonitorName = Name;
            string resourceGroupName = ResourceGroupName;
            string networkWatcherName = NetworkWatcherName;

            if (ParameterSetName.Contains("SetByResourceId"))
            {
                ConnectionMonitorDetails connectionMonitorDetails = new ConnectionMonitorDetails();
                connectionMonitorDetails = GetConnectionMonitorDetails(ResourceId);

                connectionMonitorName = connectionMonitorDetails.ConnectionMonitorName;
                resourceGroupName = connectionMonitorDetails.ResourceGroupName;
                networkWatcherName = connectionMonitorDetails.NetworkWatcherName;
            }
            else if (ParameterSetName.Contains("SetByResource"))
            {
                resourceGroupName = NetworkWatcher.ResourceGroupName;
                networkWatcherName = NetworkWatcher.Name;
            }
            else if (ParameterSetName.Contains("SetByInputObject"))
            {
                ConnectionMonitorDetails connectionMonitorDetails = new ConnectionMonitorDetails();
                connectionMonitorDetails = GetConnectionMonitorDetails(InputObject.Id);

                connectionMonitorName = connectionMonitorDetails.ConnectionMonitorName;
                resourceGroupName = connectionMonitorDetails.ResourceGroupName;
                networkWatcherName = connectionMonitorDetails.NetworkWatcherName;
            }
            else if (ParameterSetName.Contains("SetByLocation"))
            {
                var networkWatcher = GetNetworkWatcherByLocation(Location);

                if (networkWatcher == null)
                {
                    throw new ArgumentException("There is no network watcher in location {0}", Location);
                }

                resourceGroupName = GetResourceGroup(networkWatcher.Id);
                networkWatcherName = networkWatcher.Name;
            }

            var present = IsConnectionMonitorPresent(resourceGroupName, networkWatcherName, connectionMonitorName);

            if (!present)
            {
                throw new ArgumentException(Properties.Resources.ResourceNotFound);
            }

            var connectionMonitor = UpdateConnectionMonitor(resourceGroupName, networkWatcherName);
            WriteObject(connectionMonitor);
        }

        private PSConnectionMonitorResult UpdateConnectionMonitor(string resourceGroupName, string networkWatcherName)
        {
            ConnectionMonitor parameters = new ConnectionMonitor
            {
                Source = new ConnectionMonitorSource
                {
                    ResourceId = SourceResourceId,
                    Port = SourcePort
                },
                Destination = new ConnectionMonitorDestination
                {
                    ResourceId = DestinationResourceId,
                    Address = DestinationAddress,
                    Port = DestinationPort
                },
                Tags = TagsConversionHelper.CreateTagDictionary(Tag, validate: true)
            };

            if (ConfigureOnly)
            {
                parameters.AutoStart = false;
            }

            if (MonitoringIntervalInSeconds != null)
            {
                parameters.MonitoringIntervalInSeconds = MonitoringIntervalInSeconds;
            }

            PSConnectionMonitorResult getConnectionMonitor = new PSConnectionMonitorResult();

            // Execute the CreateOrUpdate Connection monitor call
            if (ParameterSetName.Contains("SetByResource") && !ParameterSetName.Contains("SetByResourceId"))
            {
                parameters.Location = NetworkWatcher.Location;
            }
            else if (ParameterSetName.Contains("SetByLocation"))
            {
                parameters.Location = Location;
            }
            else
            {
                MNM.NetworkWatcher networkWatcher = NetworkClient.NetworkManagementClient.NetworkWatchers.Get(resourceGroupName, networkWatcherName);
                parameters.Location = networkWatcher.Location;
            }

            ConnectionMonitors.CreateOrUpdate(resourceGroupName, networkWatcherName, connectionMonitorName, parameters);
            getConnectionMonitor = GetConnectionMonitor(resourceGroupName, networkWatcherName, connectionMonitorName);

            return getConnectionMonitor;
        }
    }
}
