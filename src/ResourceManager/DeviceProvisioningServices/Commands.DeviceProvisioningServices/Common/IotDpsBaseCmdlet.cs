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

namespace Microsoft.Azure.Commands.Management.DeviceProvisioningServices
{
    using System.Collections.Generic;
    using System.Linq;
    using Azure.Management.Internal.Resources;
    using Common.Authentication;
    using Common.Authentication.Abstractions;
    using ResourceManager.Common;
    using Azure.Management.DeviceProvisioningServices;
    using Azure.Management.DeviceProvisioningServices.Models;
    using Rest.Azure;

    public class IotDpsBaseCmdlet : AzureRMCmdlet
    {
        private IIotDpsClient iotDpsClient;

        private IResourceManagementClient resourceManagementClient;

        protected IIotDpsClient IotDpsClient
        {
            get
            {
                if (iotDpsClient == null)
                {
                    iotDpsClient = AzureSession.Instance.ClientFactory.CreateArmClient<IotDpsClient>(DefaultProfile.DefaultContext, AzureEnvironment.Endpoint.ResourceManager);
                }

                return iotDpsClient;
            }
        }

        protected IResourceManagementClient ResourceManagementClient
        {
            get
            {
                if (resourceManagementClient == null)
                {
                    resourceManagementClient = AzureSession.Instance.ClientFactory.CreateArmClient<ResourceManagementClient>(DefaultProfile.DefaultContext, AzureEnvironment.Endpoint.ResourceManager);
                }

                return resourceManagementClient;
            }
        }

        public string SubscriptionId
        {
            get { return DefaultProfile.DefaultContext.Subscription.Id; }
        }

        public ProvisioningServiceDescription GetIotDpsResource(string resourceGroupName, string provisioningServiceName)
        {
            return IotDpsClient.IotDpsResource.Get(provisioningServiceName, resourceGroupName);
        }

        public ProvisioningServiceDescription IotDpsCreateOrUpdate(string resourceGroupName, string provisioningServiceName, ProvisioningServiceDescription provisioningServiceDescription)
        {
            return IotDpsClient.IotDpsResource.CreateOrUpdate(resourceGroupName, provisioningServiceName, provisioningServiceDescription);
        }

        public IList<SharedAccessSignatureAuthorizationRuleAccessRightsDescription> GetIotDpsAccessPolicy(string resourceGroupName, string provisioningServiceName)
        {
            IPage<SharedAccessSignatureAuthorizationRuleAccessRightsDescription> iotDpsAccessPolicies = IotDpsClient.IotDpsResource.ListKeys(provisioningServiceName, resourceGroupName);
            return new List<SharedAccessSignatureAuthorizationRuleAccessRightsDescription>(iotDpsAccessPolicies);
        }

        public SharedAccessSignatureAuthorizationRuleAccessRightsDescription GetIotDpsAccessPolicy(string resourceGroupName, string provisioningServiceName, string keyName)
        {
            return IotDpsClient.IotDpsResource.ListKeysForKeyName(provisioningServiceName, keyName, resourceGroupName);
        }

        public IList<IotHubDefinitionDescription> GetIotDpsHubs(string resourceGroupName, string provisioningServiceName)
        {
            ProvisioningServiceDescription provisioningServiceDescription = GetIotDpsResource(resourceGroupName, provisioningServiceName);
            return provisioningServiceDescription.Properties.IotHubs;
        }

        public IotHubDefinitionDescription GetIotDpsHubs(string resourceGroupName, string provisioningServiceName, string linkedHubName)
        {
            IList<IotHubDefinitionDescription> linkedHubs = GetIotDpsHubs(resourceGroupName, provisioningServiceName);
            return linkedHubs.FirstOrDefault(hubs => hubs.Name.Equals(linkedHubName));
        }

        public IList<CertificateResponse> GetIotDpsCertificates(string resourceGroupName, string provisioningServiceName)
        {
            CertificateListDescription certificates = IotDpsClient.DpsCertificates.List(resourceGroupName, provisioningServiceName);
            return certificates.Value;
        }

        public CertificateResponse GetIotDpsCertificates(string resourceGroupName, string provisioningServiceName, string certificateName)
        {
            return IotDpsClient.DpsCertificate.Get(certificateName, resourceGroupName, provisioningServiceName);
        }
    }
}
