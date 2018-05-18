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

namespace Microsoft.Azure.Commands.Resources.Test
{
    using ResourceManager.Cmdlets.Implementation;
    using ResourceManager.Cmdlets.SdkClient;
    using ResourceManager.Cmdlets.SdkModels;
    using Management.ResourceManager;
    using Management.ResourceManager.Models;
    using ServiceManagemenet.Common.Models;
    using WindowsAzure.Commands.Common.Test.Mocks;
    using WindowsAzure.Commands.ScenarioTest;
    using WindowsAzure.Commands.Test.Utilities.Common;
    using Moq;
    using Rest.Azure;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;
    using Xunit.Abstractions;
    /// <summary>
    /// Tests the AzureProvider cmdlets
    /// </summary>
    public class GetAzureProviderCmdletTests : RMTestBase
    {
        /// <summary>
        /// An instance of the cmdlet
        /// </summary>
        private readonly GetAzureProviderCmdletTest cmdlet;

        /// <summary>
        /// A mock of the command runtime
        /// </summary>
        private readonly Mock<ICommandRuntime> commandRuntimeMock;
        private MockCommandRuntime mockRuntime;

        /// <summary>
        /// A mock of the IProvidersOperations
        /// </summary>
        private readonly Mock<IProvidersOperations> providerOperationsMock;

        /// <summary>
        /// A mock of the ISubscriptionsOperations
        /// </summary>
        private readonly Mock<Internal.Subscriptions.ISubscriptionsOperations> subscriptionsOperationsMock;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetAzureProviderCmdletTests"/> class.
        /// </summary>
        public GetAzureProviderCmdletTests(ITestOutputHelper output)
        {
            providerOperationsMock = new Mock<IProvidersOperations>();
            subscriptionsOperationsMock = new Mock<Internal.Subscriptions.ISubscriptionsOperations>();
            XunitTracingInterceptor.AddToContext(new XunitTracingInterceptor(output));
            var resourceManagementClient = new Mock<IResourceManagementClient>();
            var subscriptionClient = new Mock<Internal.Subscriptions.ISubscriptionClient>();

            resourceManagementClient
                .SetupGet(client => client.Providers)
                .Returns(() => providerOperationsMock.Object);

            subscriptionClient
                .SetupGet(client => client.Subscriptions)
                .Returns(() => subscriptionsOperationsMock.Object);

            commandRuntimeMock = new Mock<ICommandRuntime>();
            cmdlet = new GetAzureProviderCmdletTest
            {
                //CommandRuntime = commandRuntimeMock.Object,
                ResourceManagerSdkClient = new ResourceManagerSdkClient(resourceManagementClient.Object),
                SubscriptionSdkClient = new SubscriptionSdkClient(subscriptionClient.Object)
            };
            PSCmdletExtensions.SetCommandRuntimeMock(cmdlet, commandRuntimeMock.Object);
            mockRuntime = new MockCommandRuntime();
            commandRuntimeMock.Setup(f => f.Host).Returns(mockRuntime.Host);
        }

        /// <summary>
        /// Validates all Get-AzureRmResourceProvider parameter combinations
        /// </summary>
        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void GetsResourceProviderTests()
        {
            //setup return values
            const string RegisteredProviderNamespace = "Providers.Test1";
            const string UnregisteredProviderNamespace = "Providers.Test2";

            const string ResourceTypeName = "TestResource1";

            var unregisteredProvider = new Provider(
                namespaceProperty: UnregisteredProviderNamespace,
                registrationState: "Unregistered",
                resourceTypes: new[]
                {
                    new ProviderResourceType
                    {
                        Locations = new[] {"West US", "East US", "South US"},
                        ResourceType = "TestResource2"
                    }
                });

            var listResult = new List<Provider>
            {
                new Provider(
                    namespaceProperty: RegisteredProviderNamespace,
                    registrationState: ResourceManagerSdkClient.RegisteredStateName,
                    resourceTypes: new[]
                    {
                        new ProviderResourceType
                        {
                            Locations = new[] { "West US", "East US" },
                            //Name = ResourceTypeName,
                        }
                    }),
                unregisteredProvider,
            };
            var pagableResult = new Page<Provider>();
            pagableResult.SetItemValue(listResult);
            var result = new AzureOperationResponse<IPage<Provider>>
            {
                Body = pagableResult
            };
            providerOperationsMock
                .Setup(f => f.ListWithHttpMessagesAsync(null, null, null, It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult(result));

            var locationList = new List<Internal.Subscriptions.Models.Location>
            {
                new Internal.Subscriptions.Models.Location(name: "southus", displayName: "South US")
            };
            var pagableLocations = new Page<Internal.Subscriptions.Models.Location>();
            pagableLocations.SetItemValue(locationList);
            var locationsResult = new AzureOperationResponse<IEnumerable<Internal.Subscriptions.Models.Location>>
            {
                Body = pagableLocations
            };
            subscriptionsOperationsMock
                .Setup(f => f.ListLocationsWithHttpMessagesAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult(locationsResult));


            // 1. List only registered providers
            commandRuntimeMock
                .Setup(m => m.WriteObject(It.IsAny<object>()))
                .Callback((object obj) =>
                {
                    Assert.IsType<PSResourceProvider[]>(obj);

                    var providers = (PSResourceProvider[])obj;
                    Assert.Single(providers);

                    var provider = providers.Single();
                    Assert.Equal(RegisteredProviderNamespace, provider.ProviderNamespace);
                    Assert.Equal(ResourceManagerSdkClient.RegisteredStateName, provider.RegistrationState);

                    Assert.Single(provider.ResourceTypes);

                    var resourceType = provider.ResourceTypes.Single();
                    Assert.Equal(ResourceTypeName, resourceType.ResourceTypeName);
                    Assert.Equal(2, resourceType.Locations.Length);
                });

            cmdlet.ParameterSetOverride = GetAzureProviderCmdlet.ListAvailableParameterSet;

            cmdlet.ExecuteCmdlet();

            VerifyListCallPatternAndReset();

            // 2. List all providers
            cmdlet.ListAvailable = true;

            commandRuntimeMock
              .Setup(m => m.WriteObject(It.IsAny<object>()))
              .Callback((object obj) =>
              {
                  Assert.IsType<PSResourceProvider[]>(obj);
                  var providers = (PSResourceProvider[])obj;
                  Assert.Equal(2, providers.Length);
              });

            cmdlet.ExecuteCmdlet();

            VerifyListCallPatternAndReset();

            // 3. List a single provider by name
            cmdlet.ProviderNamespace = new[] { UnregisteredProviderNamespace };
            cmdlet.MyInvocation.BoundParameters.Add("ProviderNamespace", new[] { UnregisteredProviderNamespace });

            providerOperationsMock
              .Setup(f => f.GetWithHttpMessagesAsync(It.IsAny<string>(), null, null, It.IsAny<CancellationToken>()))
              .Returns(Task.FromResult(new AzureOperationResponse<Provider>
                {
                    Body = unregisteredProvider
                }));

            commandRuntimeMock
                .Setup(m => m.WriteObject(It.IsAny<object>()))
                .Callback((object obj) =>
                {
                    Assert.IsType<PSResourceProvider[]>(obj);

                    var providers = (PSResourceProvider[])obj;
                    Assert.Single(providers);

                    var provider = providers.Single();
                    Assert.Equal(UnregisteredProviderNamespace, provider.ProviderNamespace);
                });

            cmdlet.ParameterSetOverride = GetAzureProviderCmdlet.IndividualProviderParameterSet;

            cmdlet.ExecuteCmdlet();

            VerifyGetCallPatternAndReset();
            cmdlet.MyInvocation.BoundParameters.Remove("ProviderNamespace");

            // 4. List only registered providers with location
            cmdlet.Location = "South US";
            cmdlet.ListAvailable = false;
            cmdlet.ProviderNamespace = null;

            commandRuntimeMock
                .Setup(m => m.WriteObject(It.IsAny<object>()))
                .Callback((object obj) =>
                {
                    Assert.IsType<PSResourceProvider[]>(obj);

                    var providers = (PSResourceProvider[])obj;
                    Assert.Empty(providers);
                });

            cmdlet.ParameterSetOverride = GetAzureProviderCmdlet.ListAvailableParameterSet;

            cmdlet.ExecuteCmdlet();

            VerifyListCallPatternAndReset();

            // 5. List all providers
            cmdlet.ListAvailable = true;
            cmdlet.Location = "South US";
            cmdlet.ProviderNamespace = null;

            commandRuntimeMock
              .Setup(m => m.WriteObject(It.IsAny<object>()))
              .Callback((object obj) =>
              {
                  var providers = (PSResourceProvider[])obj;
                  Assert.Empty(providers);

                  var provider = providers.Single();
                  Assert.Equal(UnregisteredProviderNamespace, provider.ProviderNamespace);

                  Assert.Single(provider.ResourceTypes);

                  var resourceType = provider.ResourceTypes.Single();
                  Assert.Equal(ResourceTypeName, resourceType.ResourceTypeName);
              });

            cmdlet.ExecuteCmdlet();

            VerifyListCallPatternAndReset();
        }

        /// <summary>
        /// Resets the calls on the mocks
        /// </summary>
        private void ResetCalls()
        {
            providerOperationsMock.ResetCalls();
            commandRuntimeMock.ResetCalls();
        }

        /// <summary>
        /// Verifies the right call patterns are made
        /// </summary>
        private void VerifyGetCallPatternAndReset()
        {
            providerOperationsMock.Verify(f => f.GetWithHttpMessagesAsync(It.IsAny<string>(), null, null, It.IsAny<CancellationToken>()), Times.Once());
            providerOperationsMock.Verify(f => f.ListWithHttpMessagesAsync(null, null, null, It.IsAny<CancellationToken>()), Times.Once());
            providerOperationsMock.Verify(f => f.ListNextWithHttpMessagesAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()), Times.Never);
            commandRuntimeMock.Verify(f => f.WriteObject(It.IsAny<object>(), It.IsAny<bool>()), Times.Once());
            ResetCalls();
        }

        /// <summary>
        /// Verifies the right call patterns are made
        /// </summary>
        private void VerifyListCallPatternAndReset()
        {
            providerOperationsMock.Verify(f => f.ListWithHttpMessagesAsync(null, null, null, It.IsAny<CancellationToken>()), Times.Once());
            providerOperationsMock.Verify(f => f.ListNextWithHttpMessagesAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()), Times.Never());
            commandRuntimeMock.Verify(f => f.WriteObject(It.IsAny<object>(), It.IsAny<bool>()), Times.Once());

            ResetCalls();
        }

        /// <summary>
        /// Helper class that enables setting the parameter set name
        /// </summary>
        private class GetAzureProviderCmdletTest : GetAzureProviderCmdlet
        {
            /// <summary>
            /// Sets the parameter set name to return
            /// </summary>
            public string ParameterSetOverride { private get; set; }

            /// <summary>
            /// Determines the parameter set name based on the <see cref="ParameterSetOverride"/> property
            /// </summary>
            public override string DetermineParameterSetName()
            {
                return ParameterSetOverride;
            }
        }
    }
}
