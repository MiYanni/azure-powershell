//----------------------------------------------------------------------------------
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
    using Commands.Resources.Models;
    using Management.ResourceManager;
    using Management.ResourceManager.Models;
    using ServiceManagemenet.Common.Models;
    using Rest.Azure;
    using WindowsAzure.Commands.Common.Test.Mocks;
    using WindowsAzure.Commands.ScenarioTest;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Threading;
    using System.Threading.Tasks;
    using WindowsAzure.Commands.Test.Utilities.Common;
    using Xunit;
    using Xunit.Abstractions;
    /// <summary>
    /// Tests the AzureProvider cmdlets
    /// </summary>
    public class RegisterAzureProviderCmdletTests : RMTestBase
    {
        /// <summary>
        /// An instance of the cmdlet
        /// </summary>
        private readonly RegisterAzureProviderCmdlet cmdlet;

        /// <summary>
        /// A mock of the client
        /// </summary>
        private readonly Mock<IProvidersOperations> providerOperationsMock;

        /// <summary>
        /// A mock of the command runtime
        /// </summary>
        private readonly Mock<ICommandRuntime> commandRuntimeMock;
        private MockCommandRuntime mockRuntime;

        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterAzureProviderCmdletTests"/> class.
        /// </summary>
        public RegisterAzureProviderCmdletTests(ITestOutputHelper output)
        {
            providerOperationsMock = new Mock<IProvidersOperations>();
            XunitTracingInterceptor.AddToContext(new XunitTracingInterceptor(output));
            var resourceManagementClient = new Mock<IResourceManagementClient>();

            resourceManagementClient
                .SetupGet(client => client.Providers)
                .Returns(() => providerOperationsMock.Object);

            commandRuntimeMock = new Mock<ICommandRuntime>();

            commandRuntimeMock
              .Setup(m => m.ShouldProcess(It.IsAny<string>(), It.IsAny<string>()))
              .Returns(() => true);

            cmdlet = new RegisterAzureProviderCmdlet
            {
                ResourceManagerSdkClient = new ResourceManagerSdkClient
                {
                    ResourceManagementClient = resourceManagementClient.Object
                }
            };

            PSCmdletExtensions.SetCommandRuntimeMock(cmdlet, commandRuntimeMock.Object);
            mockRuntime = new MockCommandRuntime();
            commandRuntimeMock.Setup(f => f.Host).Returns(mockRuntime.Host);
        }

        /// <summary>
        /// Validates all Register-AzureRmResourceProvider scenarios
        /// </summary>
        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void RegisterResourceProviderTests()
        {
            const string ProviderName = "Providers.Test";

            var provider = new Provider(
                namespaceProperty: ProviderName,
                registrationState: ResourcesClient.RegisteredStateName,
                resourceTypes: new[]
                {
                    new ProviderResourceType
                    {
                        Locations = new[] {"West US", "East US"},
                        //Name = "TestResource2"
                    }
                });

            var registrationResult = provider;

            providerOperationsMock
                .Setup(client => client.RegisterWithHttpMessagesAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
                .Callback((string providerName, Dictionary<string, List<string>> customHeaders, CancellationToken ignored) =>
                        Assert.Equal(ProviderName, providerName, StringComparer.OrdinalIgnoreCase))
                .Returns(() => Task.FromResult(new AzureOperationResponse<Provider>
                {
                    Body = registrationResult
                }));

            providerOperationsMock
              .Setup(f => f.GetWithHttpMessagesAsync(It.IsAny<string>(), null, null, It.IsAny<CancellationToken>()))
              .Returns(() => Task.FromResult( new AzureOperationResponse<Provider>
                {
                   Body = provider
              }));

            cmdlet.ProviderNamespace = ProviderName;

            // 1. register succeeds
            commandRuntimeMock
                .Setup(m => m.WriteObject(It.IsAny<object>()))
                .Callback((object obj) =>
                {
                    Assert.IsType<PSResourceProvider>(obj);
                    var providerResult = (PSResourceProvider)obj;
                    Assert.Equal(ProviderName, providerResult.ProviderNamespace, StringComparer.OrdinalIgnoreCase);
                });

            //registrationResult.StatusCode = HttpStatusCode.OK;

            cmdlet.ExecuteCmdlet();

            VerifyCallPatternAndReset(true);

            // 2. register fails w/ error
            //registrationResult.StatusCode = HttpStatusCode.NotFound;
            registrationResult = null;

            try
            {
                cmdlet.ExecuteCmdlet();
                Assert.False(true, "The cmdlet succeeded when it should have failed.");
            }
            catch (KeyNotFoundException)
            {
                VerifyCallPatternAndReset(false);
            }
        }

        /// <summary>
        /// Verifies the right call patterns are made
        /// </summary>
        private void VerifyCallPatternAndReset(bool succeeded)
        {
            providerOperationsMock.Verify(f => f.RegisterWithHttpMessagesAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()), Times.Once());
            commandRuntimeMock.Verify(f => f.WriteObject(It.IsAny<object>()), succeeded ? Times.Once() : Times.Never());

            providerOperationsMock.ResetCalls();
            commandRuntimeMock.ResetCalls();
        }
    }
}
