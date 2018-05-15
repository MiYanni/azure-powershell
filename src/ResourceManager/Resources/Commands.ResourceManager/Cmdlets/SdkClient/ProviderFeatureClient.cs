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

namespace Microsoft.Azure.Commands.ResourceManager.Cmdlets.SdkClient
{
    using Commands.Common.Authentication;
    using Commands.Common.Authentication.Models;
    using SdkExtensions;
    using SdkModels;
    using Management.ResourceManager;
    using Management.ResourceManager.Models;
    using Rest.Azure;
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using ProjectResources = Properties.Resources;
    using Commands.Common.Authentication.Abstractions;

    /// <summary>
    /// Helper client for performing operations on features
    /// </summary>
    public class ProviderFeatureClient
    {
        /// <summary>
        /// The Registered state
        /// </summary>
        public const string RegisteredStateName = "Registered";

        /// <summary>
        /// Initializes a new instance of the <see cref="ProviderFeatureClient"/> class.
        /// </summary>
        public ProviderFeatureClient()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProviderFeatureClient"/> class.
        /// </summary>
        /// <param name="context">The azure context</param>
        public ProviderFeatureClient(IAzureContext context)
        {
            FeaturesManagementClient = AzureSession.Instance.ClientFactory.CreateArmClient<FeatureClient>(context, AzureEnvironment.Endpoint.ResourceManager);
        }

        /// <summary>
        /// The features management client
        /// </summary>
        public IFeatureClient FeaturesManagementClient { get; set; }


        /// <summary>
        /// Lists the features that ARM knows about
        /// </summary>
        /// <param name="resourceProviderNamespace">When specified, returns all features that are defined by this resource provider namespace</param>
        /// <param name="featureName">When specified, returns all features that have this name</param>
        public virtual PSProviderFeature[] ListPSProviderFeatures(string resourceProviderNamespace = null, string featureName = null)
        {
            return ListPSProviderFeatures(resourceProviderNamespace, featureName, false);
        }

        /// <summary>
        /// Lists the features that ARM knows about
        /// </summary>
        /// <param name="resourceProviderNamespace">When specified, returns all features that are defined by this resource provider namespace</param>
        /// <param name="listAvailable">When set to true, lists all features that are available including those not registered on the current subscription</param>
        public virtual PSProviderFeature[] ListPSProviderFeatures(bool listAvailable, string resourceProviderNamespace = null)
        {
            return ListPSProviderFeatures(resourceProviderNamespace, listAvailable: listAvailable, featureName: null);
        }

        /// <summary>
        /// Lists the features that ARM knows about
        /// </summary>
        /// <param name="resourceProviderNamespace">When specified, returns all features that are defined by this resource provider namespace</param>
        /// <param name="featureName">When specified, returns all features that have this name</param>
        /// <param name="listAvailable">When set to true, lists all features that are available including those not registered on the current subscription</param>
        private PSProviderFeature[] ListPSProviderFeatures(string resourceProviderNamespace = null, string featureName = null, bool listAvailable = false)
        {
            if (!string.IsNullOrEmpty(featureName) && !string.IsNullOrWhiteSpace(resourceProviderNamespace))
            {
                var featureResponse = FeaturesManagementClient.Features.Get(resourceProviderNamespace, featureName);

                if (featureResponse == null)
                {
                    throw new KeyNotFoundException(string.Format(ProjectResources.FeatureNotFound, featureName, resourceProviderNamespace));
                }

                return new[] { featureResponse.ToPSProviderFeature() };
            }
            Func<IPage<FeatureResult>> listFunc;
            Func<string, IPage<FeatureResult>> listNextFunc;

            if (string.IsNullOrWhiteSpace(resourceProviderNamespace))
            {
                listFunc = () => FeaturesManagementClient.Features.ListAll();
                listNextFunc = next => FeaturesManagementClient.Features.ListAllNext(next);
            }
            else
            {
                listFunc = () => FeaturesManagementClient.Features.List(resourceProviderNamespace);
                listNextFunc = next => FeaturesManagementClient.Features.ListNext(next);
            }

            var returnList = new List<FeatureResult>(); 
            var tempResult = listFunc();

            returnList.AddRange(tempResult);

            while(!string.IsNullOrWhiteSpace(tempResult.NextPageLink))
            {
                tempResult = listNextFunc(tempResult.NextPageLink);
                returnList.AddRange(tempResult);
            }

            var retVal = listAvailable
                ? returnList
                : returnList.Where(IsFeatureRegistered);

            return retVal
                .Select(val => val.ToPSProviderFeature())
                .ToArray();
        }

        /// <summary>
        /// Registers a feature on the current subscription
        /// </summary>
        /// <param name="providerName">The name of the resource provider</param>
        /// <param name="featureName">The name of the feature</param>
        public PSProviderFeature RegisterProviderFeature(string providerName, string featureName)
        {
            return FeaturesManagementClient.Features.Register(providerName, featureName).ToPSProviderFeature();
        }

        /// <summary>
        /// Checks if a feature is registered with the current subscription
        /// </summary>
        /// <param name="feature">The feature</param>
        private bool IsFeatureRegistered(FeatureResult feature)
        {
            return string.Equals(feature.Properties.State, RegisteredStateName, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
