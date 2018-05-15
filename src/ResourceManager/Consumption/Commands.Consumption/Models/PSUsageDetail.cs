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
using Microsoft.Azure.Commands.Consumption.Common;
using ApiUsageDetail = Microsoft.Azure.Management.Consumption.Models.UsageDetail;
using MeterDetails = Microsoft.Azure.Management.Consumption.Models.MeterDetails;

namespace Microsoft.Azure.Commands.Consumption.Models
{
    public class PSUsageDetail
    {
        public string AccountName { get; set; }
        public string AdditionalInfo { get; set; }
        public IDictionary<string, string> AdditionalProperties { get; set; }
        public decimal? BillableQuantity { get; set; }
        public string BillingPeriodId { get; set; }
        public string BillingPeriodName { get; set; }
        public string ConsumedService { get; set; }
        public string CostCenter { get; set; }
        public string Currency { get; set; }
        public string DepartmentName { get; set; }
        public string Id { get; private set; }
        public string InstanceId { get; set; }
        public string InstanceLocation { get; set; }
        public string InstanceName { get; set; }
        public string InvoiceId { get; set; }
        public string InvoiceName { get; set; }
        public bool? IsEstimated { get; set; }
        public MeterDetails MeterDetails { get; set; }
        public string MeterId { get; set; }
        public string Name { get; private set; }
        public decimal? PretaxCost { get; set; }
        public string Product { get; set; }
        public string SubscriptionGuid { get; set; }
        public string SubscriptionName { get; set; }
        public IDictionary<string, string> Tags { get; private set; }
        public string Type { get; private set; }
        public DateTime? UsageEnd { get; private set; }
        public decimal? UsageQuantity { get; set; }
        public DateTime? UsageStart { get; private set; }       

        public PSUsageDetail()
        {
        }

        public PSUsageDetail(ApiUsageDetail usageDetail)
        {
            if (usageDetail != null)
            {
                AccountName = usageDetail.AccountName;
                AdditionalInfo = usageDetail.AdditionalProperties;
                BillableQuantity = usageDetail.BillableQuantity;
                BillingPeriodId = usageDetail.BillingPeriodId;
                BillingPeriodName = Utilities.GetResourceNameFromId(usageDetail.BillingPeriodId);
                ConsumedService = usageDetail.ConsumedService;
                CostCenter = usageDetail.CostCenter;
                Currency = usageDetail.Currency;
                DepartmentName = usageDetail.DepartmentName;
                Id = usageDetail.Id;
                InstanceId = usageDetail.InstanceId;
                InstanceLocation = usageDetail.InstanceLocation;
                InstanceName = usageDetail.InstanceName;
                InvoiceId = usageDetail.InvoiceId;
                InvoiceName = Utilities.GetResourceNameFromId(usageDetail.InvoiceId);
                IsEstimated = usageDetail.IsEstimated;
                MeterDetails = usageDetail.MeterDetails;
                MeterId = usageDetail.MeterId;
                Name = usageDetail.Name;
                PretaxCost = usageDetail.PretaxCost;
                Product = usageDetail.Product;
                SubscriptionGuid = usageDetail.SubscriptionGuid;
                SubscriptionName = usageDetail.SubscriptionName;
                Tags = usageDetail.Tags;
                Type = usageDetail.Type;
                UsageEnd = usageDetail.UsageEnd;
                UsageQuantity = usageDetail.UsageQuantity;
                UsageStart = usageDetail.UsageStart;              
            }
        }
    }
}
