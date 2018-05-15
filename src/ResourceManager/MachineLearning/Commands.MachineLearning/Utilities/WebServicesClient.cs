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

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Commands.Common.Authentication;
using Microsoft.Azure.Commands.Common.Authentication.Models;
using Microsoft.Azure.Management.MachineLearning.WebServices;
using Microsoft.Azure.Management.MachineLearning.WebServices.Models;
using APIClient = Microsoft.Azure.Management.MachineLearning.
                    WebServices.AzureMLWebServicesManagementClient;
using Microsoft.Azure.Commands.ResourceManager.Common;
using Microsoft.Azure.Commands.Common.Authentication.Abstractions;

namespace Microsoft.Azure.Commands.MachineLearning.Utilities
{
    public class WebServicesClient : MachineLearningClientBase
    {
        private const int AsyncOperationPollingIntervalSeconds = 5;

        private readonly APIClient apiClient;

        public WebServicesClient(IAzureContext context)
        {
            apiClient = AzureSession.Instance.ClientFactory.
                                            CreateArmClient<APIClient>(
                                                context,
                                                AzureEnvironment.Endpoint.ResourceManager);
            apiClient.LongRunningOperationRetryTimeout =
                    AsyncOperationPollingIntervalSeconds;
        }

        public WebService CreateAzureMlWebService(
                            string resourceGroupName,
                            string webServiceName,
                            WebService serviceDefinition)
        {
            return apiClient.WebServices.CreateOrUpdateWithRequestId(
                                                serviceDefinition,
                                                resourceGroupName,
                                                webServiceName);
        }

        public WebService UpdateAzureMlWebService(
                            string resourceGroupName,
                            string webServiceName,
                            WebService serviceDefinition)
        {
            return apiClient.WebServices.PatchWithRequestId(
                                                serviceDefinition,
                                                resourceGroupName,
                                                webServiceName);
        }

        public void DeleteAzureMlWebService(
                        string resourceGroupName,
                        string webServiceName)
        {
            apiClient.WebServices.RemoveWithRequestId(resourceGroupName, webServiceName);
        }

        public WebService GetAzureMlWebService(
                            string resourceGroupName,
                            string webServiceName,
                            string region)
        {
            return apiClient.WebServices.Get(resourceGroupName, webServiceName, region);
        }

        public WebServiceKeys GetAzureMlWebServiceKeys(
                                string resourceGroupName, string webServiceName)
        {
            return apiClient.WebServices.ListKeys(resourceGroupName, webServiceName);
        }

        public async Task<ResponseWithContinuation<WebService[]>> 
                        GetAzureMlWebServicesBySubscriptionAndGroupAsync(
                                                    string resourceGroupName,
                                                    string nextLink,
                                                    CancellationToken? cancellationToken)
        {
            string skipToken = GetSkipTokenFromLink(nextLink);
            var cancellationTokenParam = cancellationToken ?? CancellationToken.None;
            var paginatedResponse = await apiClient.WebServices.ListByResourceGroupWithHttpMessagesAsync(
                                                                        resourceGroupName,
                                                                        skipToken,
                                                                        null,
                                                                        cancellationTokenParam).ConfigureAwait(false);

            return new ResponseWithContinuation<WebService[]>
            {
                Value = paginatedResponse.Body.ToArray(),
                NextLink = paginatedResponse.Body.NextPageLink
            };
        }

        public async Task<ResponseWithContinuation<WebService[]>> 
                        GetAzureMlWebServicesBySubscriptionAsync(
                                                    string nextLink,
                                                    CancellationToken? cancellationToken)
        {
            string skipToken = GetSkipTokenFromLink(nextLink);
            var cancellationTokenParam = cancellationToken ?? CancellationToken.None;

            var paginatedResponse =
                    await apiClient.WebServices.ListBySubscriptionIdWithHttpMessagesAsync(
                                                        skipToken,
                                                        null,
                                                        cancellationTokenParam).ConfigureAwait(false);

            return new ResponseWithContinuation<WebService[]>
            {
                Value = paginatedResponse.Body.ToArray(),
                NextLink = paginatedResponse.Body.NextPageLink
            };
        }

        public void CreateRegionalProperties(
                        string resourceGroupName,
                        string webServiceName,
                        string region
                        )
        {
            apiClient.WebServices.CreateRegionalPropertiesWithRequestId(resourceGroupName, webServiceName, region);
        }
    }
}