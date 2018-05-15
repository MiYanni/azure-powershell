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

using System;
using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.Azure.Commands.EventGrid.Models;
using Microsoft.Azure.Commands.EventGrid.Utilities;
using Microsoft.Azure.Management.EventGrid.Models;
using Microsoft.Azure.Commands.ResourceManager.Common.ArgumentCompleters;

namespace Microsoft.Azure.Commands.EventGrid
{
    [Cmdlet(
        VerbsCommon.Get,
        EventGridEventSubscriptionVerb,
        DefaultParameterSetName = EventSubscriptionTopicNameParameterSet),
     OutputType(typeof(PSEventSubscription), typeof(List<PSEventSubscriptionListInstance>))]
    public class GetAzureRmEventGridSubscription : AzureEventGridCmdletBase
    {
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            Position = 0,
            HelpMessage = EventGridConstants.EventSubscriptionName,
            ParameterSetName = EventSubscriptionTopicNameParameterSet)]
        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            Position = 0,
            HelpMessage = EventGridConstants.EventSubscriptionName,
            ParameterSetName = ResourceIdEventSubscriptionParameterSet)]
        [ValidateNotNullOrEmpty]
        public string EventSubscriptionName { get; set; }

        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            Position = 0,
            HelpMessage = EventGridConstants.ResourceGroupName,
            ParameterSetName = EventSubscriptionTopicTypeNameParameterSet)]
        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            Position = 1,
            HelpMessage = EventGridConstants.ResourceGroupName,
            ParameterSetName = EventSubscriptionTopicNameParameterSet)]
        [ResourceGroupCompleter]
        [Alias(AliasResourceGroup)]
        public string ResourceGroupName { get; set; }

        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            Position = 1,
            HelpMessage = "Identifier of the resource to which event subscriptions have been created.",
            ParameterSetName = ResourceIdEventSubscriptionParameterSet)]
        [ValidateNotNullOrEmpty]
        public string ResourceId { get; set; }

        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            Position = 2,
            HelpMessage = EventGridConstants.TopicTypeName,
            ParameterSetName = EventSubscriptionTopicNameParameterSet)]
        public string TopicName { get; set; }

        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            Position = 1,
            HelpMessage = EventGridConstants.TopicTypeName,
            ParameterSetName = EventSubscriptionTopicTypeNameParameterSet)]
        [ValidateNotNullOrEmpty]
        public string TopicTypeName { get; set; }

        [Parameter(Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            Position = 2,
            HelpMessage = "Location",
            ParameterSetName = EventSubscriptionTopicTypeNameParameterSet)]
        [LocationCompleter("Microsoft.EventGrid/eventSubscriptions")]
        [ValidateNotNullOrEmpty]
        public string Location { get; set; }

        [Parameter(Mandatory = true,
            ValueFromPipeline = true,
            Position = 0,
            HelpMessage = "EventGrid Topic object.",
            ParameterSetName = EventSubscriptionInputObjectParameterSet)]
        [ValidateNotNullOrEmpty]
        public PSTopic InputObject { get; set; }

        [Parameter(Mandatory = false,
            HelpMessage = "Include the full endpoint URL of the event subscription destination.",
            ParameterSetName = EventSubscriptionTopicNameParameterSet)]
        [Parameter(Mandatory = false,
            HelpMessage = "Include the full endpoint URL of the event subscription destination.",
            ParameterSetName = EventSubscriptionTopicTypeNameParameterSet)]
        [Parameter(Mandatory = false,
            HelpMessage = "If specified, include the full endpoint URL of the event subscription destination in the response.",
            ParameterSetName = ResourceIdEventSubscriptionParameterSet)]
        public SwitchParameter IncludeFullEndpointUrl { get; set; }

        public override void ExecuteCmdlet()
        {
            string scope;
            bool includeFullEndpointUrl = IncludeFullEndpointUrl.IsPresent;

            if (!string.IsNullOrEmpty(EventSubscriptionName))
            {
                // Since an EventSubscription name is specified, we need to retrieve 
                // only the particular event subscription corresponding to this name.
                if (InputObject != null)
                {
                    // Retrieve the event subscription for the specified topic
                    scope = InputObject.Id;
                }
                else if (string.IsNullOrEmpty(ResourceId))
                {
                    // ResourceID not specified, retrieve the event subscription for either the 
                    // subscription, or resource group, or custom topic depending on which of the parameters are provided.
                    scope = EventGridUtils.GetScope(DefaultContext.Subscription.Id, ResourceGroupName, TopicName);
                }
                else
                {
                    // Since both a ResourceId and EventSubscriptionName are specified, we need to retrieve 
                    // only this particular event subscription corresponding to this resource ID.
                    scope = ResourceId;
                }

                RetrieveAndWriteEventSubscription(scope, EventSubscriptionName, includeFullEndpointUrl);
            }
            else
            {
                // EventSubscription name was not specified, we need to retrieve a list of 
                // event subscriptions based on the provided parameters.
                IEnumerable<EventSubscription> eventSubscriptionsList = null;

                if (InputObject != null)
                {
                    // Retrieve all the event subscriptions based on the ID of the specified topic object
                    eventSubscriptionsList = Client.ListByResourceId(DefaultContext.Subscription.Id, InputObject.Id);
                }
                else if (!string.IsNullOrEmpty(ResourceId))
                {
                    eventSubscriptionsList = Client.ListByResourceId(DefaultContext.Subscription.Id, ResourceId);
                }
                else if (!string.IsNullOrEmpty(TopicName))
                {
                    // Get all event subscriptions for this topic
                    if (string.IsNullOrEmpty(ResourceGroupName))
                    {
                        throw new ArgumentNullException(ResourceGroupName,
                            "Resource Group Name should be specified to retrieve event subscriptions for a topic");
                    }

                    eventSubscriptionsList = Client.ListByResource(ResourceGroupName, "Microsoft.EventGrid", "topics", TopicName);
                }
                else if (!string.IsNullOrEmpty(ResourceGroupName))
                {
                    if (string.IsNullOrEmpty(Location) && string.IsNullOrEmpty(TopicTypeName))
                    {
                        // List all global Event Grid subscriptions in the given resource group
                        eventSubscriptionsList = Client.ListGlobalEventSubscriptionsByResourceGroup(ResourceGroupName);
                    }
                    else if (string.IsNullOrEmpty(Location) && !string.IsNullOrEmpty(TopicTypeName))
                    {
                        eventSubscriptionsList = Client.ListGlobalEventSubscriptionsByResourceGroupForTopicType(ResourceGroupName, TopicTypeName);
                    }
                    else if (!string.IsNullOrEmpty(Location) && string.IsNullOrEmpty(TopicTypeName))
                    {
                        // List all regional Event Grid subscriptions in the given resource group
                        eventSubscriptionsList = Client.ListRegionalEventSubscriptionsByResourceGroup(ResourceGroupName, Location);
                    }
                    else if (!string.IsNullOrEmpty(Location) && !string.IsNullOrEmpty(TopicTypeName))
                    {
                        eventSubscriptionsList = Client.ListRegionalEventSubscriptionsByResourceGroupForTopicType(ResourceGroupName, Location, TopicTypeName);
                    }
                }
                else
                {
                    // List all Event Grid event subscriptions in the given subscription
                    if (string.IsNullOrEmpty(Location) && string.IsNullOrEmpty(TopicTypeName))
                    {
                        // List all global Event Grid subscriptions in the given resource group
                        eventSubscriptionsList = Client.ListGlobalEventSubscriptionsBySubscription();
                    }
                    else if (string.IsNullOrEmpty(Location) && !string.IsNullOrEmpty(TopicTypeName))
                    {
                        eventSubscriptionsList = Client.ListGlobalEventSubscriptionsBySubscriptionForTopicType(TopicTypeName);
                    }
                    else if (!string.IsNullOrEmpty(Location) && string.IsNullOrEmpty(TopicTypeName))
                    {
                        // List all regional Event Grid subscriptions in the given resource group
                        eventSubscriptionsList = Client.ListRegionalEventSubscriptionsBySubscription(Location);
                    }
                    else if (!string.IsNullOrEmpty(Location) && !string.IsNullOrEmpty(TopicTypeName))
                    {
                        eventSubscriptionsList = Client.ListRegionalEventSubscriptionsBySubscriptionForTopicType(Location, TopicTypeName);
                    }
                }

                WritePSEventSubscriptionsList(eventSubscriptionsList, includeFullEndpointUrl);
            }
        }

        void RetrieveAndWriteEventSubscription(string scope, string eventSubscriptionName, bool includeFullEndpointUrl)
        {
            EventSubscription eventSubscription = Client.GetEventSubscription(scope, eventSubscriptionName);
            PSEventSubscription psEventSubscription;

            if (includeFullEndpointUrl &&
                eventSubscription.Destination is WebHookEventSubscriptionDestination)
            {
                EventSubscriptionFullUrl fullUrl = Client.GetEventSubscriptionFullUrl(scope, eventSubscriptionName);
                psEventSubscription = new PSEventSubscription(eventSubscription, fullUrl.EndpointUrl);
            }
            else
            {
                psEventSubscription = new PSEventSubscription(eventSubscription);
            }

            WriteObject(psEventSubscription);
        }

        void WritePSEventSubscriptionsList(IEnumerable<EventSubscription> eventSubscriptionsList, bool includeFullEndpointUrl)
        {
            var psEventSubscriptionsList = new List<PSEventSubscription>();

            if (eventSubscriptionsList == null)
            {
                return;
            }

            foreach (EventSubscription eventSubscription in eventSubscriptionsList)
            {
                PSEventSubscriptionListInstance psEventSubscription;

                if (includeFullEndpointUrl &&
                    eventSubscription.Destination is WebHookEventSubscriptionDestination)
                {
                    EventSubscriptionFullUrl fullUrl = Client.GetEventSubscriptionFullUrl(eventSubscription.Topic, eventSubscription.Name);
                    psEventSubscription = new PSEventSubscriptionListInstance(eventSubscription, fullUrl.EndpointUrl);
                }
                else
                {
                    psEventSubscription = new PSEventSubscriptionListInstance(eventSubscription);
                }

                psEventSubscriptionsList.Add(psEventSubscription);
            }

            WriteObject(psEventSubscriptionsList, true);
        }
    }
}