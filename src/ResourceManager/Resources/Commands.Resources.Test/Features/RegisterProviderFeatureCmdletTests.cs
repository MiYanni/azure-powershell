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
    using Rest.Azure;
    using WindowsAzure.Commands.Common.Test.Mocks;
    using WindowsAzure.Commands.ScenarioTest;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Threading;
    using System.Threading.Tasks;
    using ServiceManagemenet.Common.Models;
    using WindowsAzure.Commands.Test.Utilities.Common;
    using Xunit;
    using Xunit.Abstractions;
    /// <summary>
    /// Tests the Azure Provider Feature cmdlets
    /// </summary>
    public class RegisterAzureProviderFeatureCmdletTests : RMTestBase
    {
        /// <summary>
        /// An instance of the cmdlet
        /// </summary>
        private readonly RegisterAzureProviderFeatureCmdlet cmdlet;

        /// <summary>
        /// A mock of the client
        /// </summary>
        private readonly Mock<IFeaturesOperations> featureOperationsMock;

        /// <summary>
        /// A mock of the command runtime
        /// </summary>
        private readonly Mock<ICommandRuntime> commandRuntimeMock;
        private MockCommandRuntime mockRuntime;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetAzureProviderFeatureCmdletTests"/> class.
        /// </summary>
        public RegisterAzureProviderFeatureCmdletTests(ITestOutputHelper output)
        {
            featureOperationsMock = new Mock<IFeaturesOperations>();
            var featureClient = new Mock<IFeatureClient>();

            featureClient
                .SetupGet(client => client.Features)
                .Returns(() => featureOperationsMock.Object);

            commandRuntimeMock = new Mock<ICommandRuntime>();

            commandRuntimeMock
              .Setup(m => m.ShouldProcess(It.IsAny<string>(), It.IsAny<string>()))
              .Returns(() => true);

            cmdlet = new RegisterAzureProviderFeatureCmdlet
            {
                //CommandRuntime = commandRuntimeMock.Object,
                ProviderFeatureClient = new ProviderFeatureClient
                {
                    FeaturesManagementClient = featureClient.Object
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
        public void RegisterResourceProviderFeatureTests()
        {
            const string ProviderName = "Providers.Test";
            const string FeatureName = "Feature1";

            var registeredFeature = new FeatureResult
            {
                Id = "featureId1",
                Name = ProviderName + "/" + FeatureName,
                Properties = new FeatureProperties
                {
                    State = ProviderFeatureClient.RegisteredStateName,
                },
                Type = "Microsoft.Features/feature"
            };

            featureOperationsMock
                .Setup(client => client.RegisterWithHttpMessagesAsync(It.IsAny<string>(), It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
                .Callback((string providerName, string featureName, Dictionary<string, List<string>> customHeaders, CancellationToken ignored) =>
                {
                    Assert.Equal(ProviderName, providerName, StringComparer.OrdinalIgnoreCase);
                    Assert.Equal(FeatureName, featureName, StringComparer.OrdinalIgnoreCase);
                })
                .Returns(() => Task.FromResult(new AzureOperationResponse<FeatureResult>
                {
                    Body = registeredFeature
                }));
            
            cmdlet.ProviderNamespace = ProviderName;
            cmdlet.FeatureName = FeatureName;

            commandRuntimeMock
                .Setup(m => m.WriteObject(It.IsAny<object>()))
                .Callback((object obj) =>
                {
                    Assert.IsType<PSProviderFeature>(obj);
                    var feature = (PSProviderFeature)obj;
                    Assert.Equal(ProviderName, feature.ProviderName, StringComparer.OrdinalIgnoreCase);
                    Assert.Equal(FeatureName, feature.FeatureName, StringComparer.OrdinalIgnoreCase);
                });

            cmdlet.ExecuteCmdlet();

            VerifyCallPatternAndReset(true);
        }

        /// <summary>
        /// Verifies the right call patterns are made
        /// </summary>
        private void VerifyCallPatternAndReset(bool succeeded)
        {
            featureOperationsMock.Verify(f => f.RegisterWithHttpMessagesAsync(It.IsAny<string>(),
                It.IsAny<string>(), null, It.IsAny<CancellationToken>()), Times.Once());
            commandRuntimeMock.Verify(f => f.WriteObject(It.IsAny<object>()), succeeded ? Times.Once() : Times.Never());

            featureOperationsMock.ResetCalls();
            commandRuntimeMock.ResetCalls();
        }
    }
}
