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
using ApiAgreementTerms = Microsoft.Azure.Management.MarketplaceOrdering.Models.AgreementTerms;

namespace Microsoft.Azure.Commands.MarketplaceOrdering.Models
{
    public class PSAgreementTerms
    {
        private string Id { get; set; }

        private string Name { get; set; }

        private string Type { get; set; }

        public string Publisher { get; private set; }

        public string Product { get; private set; }

        public string Plan { get; set; }

        public string LicenseTextLink { get; private set; }

        public string PrivacyPolicyLink { get; private set; }

        public string Signature { get; private set; }

        public bool? Accepted { get; set; }

        public DateTime? RetrieveDatetime { get; private set; }
        
        public PSAgreementTerms()
        {
        }

        public PSAgreementTerms(ApiAgreementTerms agreementTerms)
        {
            if (agreementTerms != null)
            {
                Id = agreementTerms.Id;
                Type = agreementTerms.Type;
                Name = agreementTerms.Name;
                Accepted = agreementTerms.Accepted;
                LicenseTextLink = agreementTerms.LicenseTextLink;
                PrivacyPolicyLink = agreementTerms.PrivacyPolicyLink;
                Publisher = agreementTerms.Publisher;
                Product = agreementTerms.Product;
                Plan = agreementTerms.Plan;
                RetrieveDatetime = agreementTerms.RetrieveDatetime;
                Signature = agreementTerms.Signature;
            }
        }

        public ApiAgreementTerms ToAgreementTerms()
        {
            return new ApiAgreementTerms(
                Id,
                Name,
                Type,
                Publisher,
                Product,
                Plan,
                LicenseTextLink,
                PrivacyPolicyLink,
                RetrieveDatetime,
                Signature,
                Accepted);
        }
    }
}
