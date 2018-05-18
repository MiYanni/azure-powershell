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
    using System.Linq;
    using System.Management.Automation;
    using System.Threading;
    using System.Threading.Tasks;
    using WindowsAzure.Commands.Test.Utilities.Common;
    using Xunit;
    using Xunit.Abstractions;
    /// <summary>
    /// Tests the Azure Provider Feature cmdlets
    /// </summary>
    public class GetAzureProviderFeatureCmdletTests : RMTestBase
    {
        /// <summary>
        /// An instance of the cmdlet
        /// </summary>
        private readonly GetAzureProviderFeatureCmdletTest cmdlet;

        /// <summary>
        /// A mock of the command runtime
        /// </summary>
        private readonly Mock<ICommandRuntime> commandRuntimeMock;
        private MockCommandRuntime mockRuntime;

        /// <summary>
        /// A mock of the client
        /// </summary>
        private readonly Mock<IFeaturesOperations> featureOperationsMock;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetAzureProviderFeatureCmdletTests"/> class.
        /// </summary>
        public GetAzureProviderFeatureCmdletTests(ITestOutputHelper output)
        {
            featureOperationsMock = new Mock<IFeaturesOperations>();
            var featureClient = new Mock<IFeatureClient>();

            featureClient
                .SetupGet(client => client.Features)
                .Returns(() => featureOperationsMock.Object);

            commandRuntimeMock = new Mock<ICommandRuntime>();

            cmdlet = new GetAzureProviderFeatureCmdletTest
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
        /// Validates all Get-AzureRmResourceProvider parameter combinations
        /// </summary>
        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void GetProviderFeatureTests()
        {
            // setup return values
            const string Provider1Namespace = "Providers.Test1";
            const string Feature1Name = "feature1";

            const string Provider2Namespace = "Providers.Test2";
            const string Feature2Name = "feature2";

            var provider1RegisteredFeature = new FeatureResult
            {
                Id = "featureId1",
                Name = Provider1Namespace + "/" + Feature1Name,
                Properties = new FeatureProperties
                {
                    State = ProviderFeatureClient.RegisteredStateName,
                },
                Type = "Microsoft.Features/feature"
            };

            var provider1UnregisteredFeature = new FeatureResult
            {
                Id = "featureId1",
                Name = Provider1Namespace + "/" + Feature2Name,
                Properties = new FeatureProperties
                {
                    State = "Unregistered",
                },
                Type = "Microsoft.Features/feature"
            };

            var provider2UnregisteredFeature = new FeatureResult
            {
                Id = "featureId2",
                Name = Provider2Namespace + "/" + Feature1Name,
                Properties = new FeatureProperties
                {
                    State = "Unregistered",
                },
                Type = "Microsoft.Features/feature"
            };

            var pagableResult = new Page<FeatureResult>();
            //var listResult = new[] { provider1RegisteredFeature, provider1UnregisteredFeature, provider2UnregisteredFeature };
            var listResult = new List<FeatureResult> { provider1RegisteredFeature, provider1UnregisteredFeature, provider2UnregisteredFeature };
            pagableResult.SetItemValue(listResult);
            var result = new AzureOperationResponse<IPage<FeatureResult>>
            {
                Body = pagableResult
            };

            featureOperationsMock
                .Setup(f => f.ListAllWithHttpMessagesAsync(null, It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult(result));

            // 1. List only registered features of providers
            commandRuntimeMock
                .Setup(m => m.WriteObject(It.IsAny<object>()))
                .Callback((object obj) =>
                {
                    Assert.IsType<PSProviderFeature[]>(obj);

                    var features = (PSProviderFeature[])obj;
                    Assert.Single(features);

                    var provider = features.Single();
                    Assert.Equal(Provider1Namespace, provider.ProviderName, StringComparer.OrdinalIgnoreCase);
                    Assert.Equal(Feature1Name, provider.FeatureName, StringComparer.OrdinalIgnoreCase);
                    Assert.Equal(ProviderFeatureClient.RegisteredStateName, provider.RegistrationState, StringComparer.OrdinalIgnoreCase);
                });

            cmdlet.ParameterSetOverride = GetAzureProviderFeatureCmdlet.ListAvailableParameterSet;

            cmdlet.ExecuteCmdlet();

            VerifyListAllCallPatternAndReset();

            // 2. List all features of all providers
            cmdlet.ListAvailable = true;

            commandRuntimeMock
              .Setup(m => m.WriteObject(It.IsAny<object>()))
              .Callback((object obj) =>
              {
                  Assert.IsType<PSProviderFeature[]>(obj);
                  var features = (PSProviderFeature[])obj;
                  Assert.Equal(listResult.Count(), features.Length);
              });

            cmdlet.ExecuteCmdlet();

            VerifyListAllCallPatternAndReset();

            // 3.a. List only registered features of a particular provider - and they exist
            string providerOfChoice = Provider1Namespace;
            cmdlet.ListAvailable = false;
            cmdlet.ProviderNamespace = providerOfChoice;
            //pagableResult.SetItemValue<FeatureResult>(new List<FeatureResult>() { provider1RegisteredFeature, provider1UnregisteredFeature });

            featureOperationsMock
                .Setup(f => f.ListWithHttpMessagesAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
                .Callback((string providerName, Dictionary<string, List<string>> customHeaders, CancellationToken ignored) => Assert.Equal(providerOfChoice, providerName, StringComparer.OrdinalIgnoreCase))
                .Returns(() => Task.FromResult(
                    new AzureOperationResponse<IPage<FeatureResult>>
                    {
                        Body = pagableResult
                    }));

            commandRuntimeMock
                .Setup(m => m.WriteObject(It.IsAny<object>()))
                .Callback((object obj) =>
                {
                    Assert.IsType<PSProviderFeature[]>(obj);

                    var features = (PSProviderFeature[])obj;
                    Assert.Single(features);

                    var provider = features.Single();
                    Assert.Equal(Provider1Namespace, provider.ProviderName, StringComparer.OrdinalIgnoreCase);
                    Assert.Equal(Feature1Name, provider.FeatureName, StringComparer.OrdinalIgnoreCase);
                    Assert.Equal(ProviderFeatureClient.RegisteredStateName, provider.RegistrationState, StringComparer.OrdinalIgnoreCase);
                });

            cmdlet.ParameterSetOverride = GetAzureProviderFeatureCmdlet.GetFeatureParameterSet;

            cmdlet.ExecuteCmdlet();

            VerifyListProviderFeaturesCallPatternAndReset();

            // 3.b. List only registered features of a particular provider - and they do not exist
            providerOfChoice = Provider2Namespace;
            cmdlet.ListAvailable = false;
            cmdlet.ProviderNamespace = providerOfChoice;
            //pagableResult.SetItemValue<FeatureResult>(new List<FeatureResult>() { provider2UnregisteredFeature });

            commandRuntimeMock
                .Setup(m => m.WriteObject(It.IsAny<object>()))
                .Callback((object obj) =>
                {
                    Assert.IsType<PSProviderFeature[]>(obj);

                    var features = (PSProviderFeature[])obj;
                    Assert.Empty(features);
                });

            cmdlet.ExecuteCmdlet();

            VerifyListProviderFeaturesCallPatternAndReset();

            // 4. List all features of a particular provider
            providerOfChoice = Provider1Namespace;
            cmdlet.ProviderNamespace = providerOfChoice;
            cmdlet.ListAvailable = true;
            //pagableResult.SetItemValue<FeatureResult>(new List<FeatureResult>() { provider1RegisteredFeature, provider1UnregisteredFeature });

            commandRuntimeMock
              .Setup(m => m.WriteObject(It.IsAny<object>()))
              .Callback((object obj) =>
              {
                  Assert.IsType<PSProviderFeature[]>(obj);
                  var features = (PSProviderFeature[])obj;
                  Assert.Equal(2, features.Length);
                  Assert.Contains(features, feature => string.Equals(feature.FeatureName, Feature1Name, StringComparison.OrdinalIgnoreCase));
                  Assert.Contains(features, feature => string.Equals(feature.FeatureName, Feature2Name, StringComparison.OrdinalIgnoreCase));
                  Assert.True(features.All(feature => string.Equals(feature.ProviderName, Provider1Namespace, StringComparison.OrdinalIgnoreCase)));
              });

            cmdlet.ParameterSetOverride = GetAzureProviderFeatureCmdlet.ListAvailableParameterSet;

            cmdlet.ExecuteCmdlet();

            VerifyListProviderFeaturesCallPatternAndReset();

            // 5. get a single provider feature by name
            cmdlet.ProviderNamespace = Provider2Namespace;
            cmdlet.FeatureName = Feature1Name;
            cmdlet.ListAvailable = false;

            featureOperationsMock
              .Setup(f => f.GetWithHttpMessagesAsync(It.IsAny<string>(), It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
              .Callback((string providerName, string featureName, Dictionary<string, List<string>> customHeaders, CancellationToken ignored) =>
              {
                  Assert.Equal(Provider2Namespace, providerName, StringComparer.OrdinalIgnoreCase);
                  Assert.Equal(Feature1Name, featureName, StringComparer.OrdinalIgnoreCase);
              })
              .Returns(() => Task.FromResult(new AzureOperationResponse<FeatureResult>
                {
                  Body = provider2UnregisteredFeature
              }));

            commandRuntimeMock
                .Setup(m => m.WriteObject(It.IsAny<object>()))
                .Callback((object obj) =>
                {
                    Assert.IsType<PSProviderFeature[]>(obj);
                    var features = (PSProviderFeature[])obj;
                    Assert.Single(features);
                    var feature = features.Single();
                    Assert.Equal(Provider2Namespace, feature.ProviderName, StringComparer.OrdinalIgnoreCase);
                    Assert.Equal(Feature1Name, feature.FeatureName, StringComparer.OrdinalIgnoreCase);
                    Assert.Equal("Unregistered", feature.RegistrationState, StringComparer.OrdinalIgnoreCase);
                });

            cmdlet.ParameterSetOverride = GetAzureProviderFeatureCmdlet.GetFeatureParameterSet;

            cmdlet.ExecuteCmdlet();

            VerifyGetCallPatternAndReset();
        }

        /// <summary>
        /// Resets the calls on the mocks
        /// </summary>
        private void ResetCalls()
        {
            featureOperationsMock.ResetCalls();
            commandRuntimeMock.ResetCalls();
        }

        /// <summary>
        /// Verifies the right call patterns are made
        /// </summary>
        private void VerifyGetCallPatternAndReset()
        {
            featureOperationsMock.Verify(f => f.GetWithHttpMessagesAsync(It.IsAny<string>(), It.IsAny<string>(), null, It.IsAny<CancellationToken>()), Times.Once());
            featureOperationsMock.Verify(f => f.ListWithHttpMessagesAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()), Times.Never);
            featureOperationsMock.Verify(f => f.ListAllWithHttpMessagesAsync(null, It.IsAny<CancellationToken>()), Times.Never);
            featureOperationsMock.Verify(f => f.ListNextWithHttpMessagesAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()), Times.Never);
            featureOperationsMock.Verify(f => f.ListAllNextWithHttpMessagesAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()), Times.Never);
            commandRuntimeMock.Verify(f => f.WriteObject(It.IsAny<object>(), It.IsAny<bool>()), Times.Once());

            ResetCalls();
        }

        /// <summary>
        /// Verifies the right call patterns are made
        /// </summary>
        private void VerifyListAllCallPatternAndReset()
        {
            featureOperationsMock.Verify(f => f.ListAllWithHttpMessagesAsync(null, It.IsAny<CancellationToken>()), Times.Once());
            featureOperationsMock.Verify(f => f.ListAllNextWithHttpMessagesAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()), Times.Never);
            commandRuntimeMock.Verify(f => f.WriteObject(It.IsAny<object>(), It.IsAny<bool>()), Times.Once());

            ResetCalls();
        }

        /// <summary>
        /// Verifies the right call patterns are made
        /// </summary>
        private void VerifyListProviderFeaturesCallPatternAndReset()
        {
            featureOperationsMock.Verify(f => f.ListWithHttpMessagesAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()), Times.Once());
            featureOperationsMock.Verify(f => f.ListNextWithHttpMessagesAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()), Times.Never);
            commandRuntimeMock.Verify(f => f.WriteObject(It.IsAny<object>(), It.IsAny<bool>()), Times.Once());

            ResetCalls();
        }

        /// <summary>
        /// Helper class that enables setting the parameter set name
        /// </summary>
        private class GetAzureProviderFeatureCmdletTest : GetAzureProviderFeatureCmdlet
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
