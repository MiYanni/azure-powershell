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

using Microsoft.Azure.Management.ApplicationInsights.Management.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Azure.Commands.ApplicationInsights.Models
{
    public class PSApplicationInsightsComponent
    {
        public PSApplicationInsightsComponent(ApplicationInsightsComponent component)
        {
            ResourceGroupName = ParseResourceGroupFromId(component.Id);
            Name = component.Name;
            Id = component.Id;
            Location = component.Location;
            Tags = component.Tags;
            Kind = component.Kind;
            Type = component.Type;
            AppId = component.AppId;
            ApplicationType = component.ApplicationType;
            CreationDate = component.CreationDate;
            FlowType = component.FlowType;
            HockeyAppId = component.HockeyAppId;
            HockeyAppToken = component.HockeyAppToken;
            InstrumentationKey = component.InstrumentationKey;
            ProvisioningState = component.ProvisioningState;
            RequestSource = component.RequestSource;
            SamplingPercentage = component.SamplingPercentage;
            TenantId = component.TenantId;
        }

        public string Id { get; set; }

        public string ResourceGroupName { get; set; }

        public string Name { get; set; }

        public string Kind { get; set; }

        public string Location { get; set; }

        public string Type { get; set; }

        public string AppId { get; set; }

        public string ApplicationType { get; set; }

        public IDictionary<string, string> Tags { get; set; }

        public DateTime? CreationDate { get; set; }

        public string FlowType { get; set; }

        public string HockeyAppId { get; set; }

        public string HockeyAppToken { get; set; }

        public string InstrumentationKey { get; set; }

        public string ProvisioningState { get; set; }

        public string RequestSource { get; set; }

        public double? SamplingPercentage { get; set; }

        public string TenantId { get; set; }

        public static PSApplicationInsightsComponent Create(ApplicationInsightsComponent component)
        {
            var result = new PSApplicationInsightsComponent(component);

            return result;
        }

        private static string ParseResourceGroupFromId(string idFromServer)
        {
            if (!string.IsNullOrEmpty(idFromServer))
            {
                string[] tokens = idFromServer.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                return tokens[3];
            }

            return null;
        }
    }

    public class PSApplicationInsightsComponentTableView : PSApplicationInsightsComponent
    {
        public PSApplicationInsightsComponentTableView(ApplicationInsightsComponent component) 
            : base(component)
        {
        }
    }

    public class PSApplicationInsightsComponentWithPricingPlan : PSApplicationInsightsComponent
    {
        public string PricingPlan;

        public double? Cap { get; set; }

        public int? ResetTime { get; set; }

        public bool StopSendNotificationWhenHitCap { get; set; }

        public string CapExpirationTime { get; }

        public bool IsCapped { get; }

        public PSApplicationInsightsComponentWithPricingPlan(ApplicationInsightsComponent component, 
                                                             ApplicationInsightsComponentBillingFeatures billing, 
                                                             ApplicationInsightsComponentQuotaStatus status) 
            : base(component)
        {
            if (billing.CurrentBillingFeatures.Any(f => f.Contains("Enterprise")))
            {
                PricingPlan = "Application Insights Enterprise";
            }
            else
            {
                PricingPlan = billing.CurrentBillingFeatures.FirstOrDefault();
            }

            Cap = billing.DataVolumeCap.Cap;
            ResetTime = billing.DataVolumeCap.ResetTime;
            StopSendNotificationWhenHitCap = billing.DataVolumeCap.StopSendNotificationWhenHitCap.Value;
            CapExpirationTime = status.ExpirationTime;
            IsCapped = status.ShouldBeThrottled != null ? status.ShouldBeThrottled.Value : false;
        }
    }
}