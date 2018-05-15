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

namespace Microsoft.Azure.Commands.LogicApp.Utilities
{
    using Management.Logic.Models;
    using Management.Logic;
    using System.Management.Automation;
    using System.Globalization;
    using Rest.Azure;

    /// <summary>
    /// LogicApp client partial class for integration account operations.
    /// </summary>
    public partial class IntegrationAccountClient
    {
        /// <summary>
        /// Creates the integration account.
        /// </summary>
        /// <param name="resourceGroupName">The integration account resource group name.</param>
        /// <param name="integrationAccountName">The integration account name.</param>
        /// <param name="integrationAccount">The integration account object.</param>
        /// <returns>Newly created integration account object.</returns>
        public IntegrationAccount CreateIntegrationAccount(string resourceGroupName, string integrationAccountName, IntegrationAccount integrationAccount)
        {
            if (!DoesIntegrationAccountExist(resourceGroupName, integrationAccountName))
            {
                return LogicManagementClient.IntegrationAccounts.CreateOrUpdate(resourceGroupName, integrationAccountName, integrationAccount);
            }
            throw new PSArgumentException(string.Format(CultureInfo.InvariantCulture,
                Properties.Resource.ResourceAlreadyExists, integrationAccountName, resourceGroupName));
        }

        /// <summary>
        /// Updates the integration account.
        /// </summary>
        /// <param name="resourceGroupName">The integration account resource group name.</param>
        /// <param name="integrationAccountName">Integration account name.</param>
        /// <param name="integrationAccount">Integration account object.</param>
        /// <returns>Newly created integration account object.</returns>
        public IntegrationAccount UpdateIntegrationAccount(string resourceGroupName, string integrationAccountName, IntegrationAccount integrationAccount)
        {
            return LogicManagementClient.IntegrationAccounts.CreateOrUpdate(resourceGroupName, integrationAccountName, integrationAccount);
        }

        /// <summary>
        /// Gets the integration account.
        /// </summary>
        /// <param name="resourceGroupName">The integration account resource group name.</param>
        /// <param name="integrationAccountName">The integration account name.</param>
        /// <returns>The integration account object</returns>
        public IntegrationAccount GetIntegrationAccount(string resourceGroupName, string integrationAccountName)
        {
            return LogicManagementClient.IntegrationAccounts.Get(resourceGroupName, integrationAccountName);
        }

        /// <summary>
        /// Gets the integration account callback URL.
        /// </summary>
        /// <param name="resourceGroupName">The integration account resource group name.</param>
        /// <param name="integrationAccountName">The integration account name.</param>
        /// <param name="callbackUrl">The integration account callback URL.</param>
        /// <returns>The integration account callback URL object</returns>
        public CallbackUrl GetIntegrationAccountCallbackUrl(string resourceGroupName, string integrationAccountName, GetCallbackUrlParameters callbackUrl)
        {
            return LogicManagementClient.IntegrationAccounts.GetCallbackUrl(resourceGroupName, integrationAccountName, callbackUrl);
        }
        
        /// <summary>
        /// Gets the integration accounts by resource group name.
        /// </summary>
        /// <param name="resourceGroupName">The integration account resource group name.</param>
        /// <returns>List of integration accounts.</returns>
        public IPage<IntegrationAccount> ListIntegrationAccount(string resourceGroupName)
        {
            if (string.IsNullOrEmpty(resourceGroupName))
            {
                return LogicManagementClient.IntegrationAccounts.ListBySubscription(1000);
            }
            return LogicManagementClient.IntegrationAccounts.ListByResourceGroup(resourceGroupName, 1000);
        }

        /// <summary>
        /// Removes the specified integration account.
        /// </summary>
        /// <param name="resourceGroupName">The integration account resource group name.</param>
        /// <param name="integrationAccountName">The integration account name.</param>
        public void RemoveIntegrationAccount(string resourceGroupName, string integrationAccountName)
        {
            LogicManagementClient.IntegrationAccounts.Delete(resourceGroupName, integrationAccountName);
        }

        /// <summary>
        /// Checks whether the integration account schema exists or not. 
        /// </summary>
        /// <param name="resourceGroupName">The integration account resource group name.</param>
        /// <param name="integrationAccountName">The integration account name.</param>
        /// <returns>Boolean result indicating whether the integration account  exists or not.</returns>
        private bool DoesIntegrationAccountExist(string resourceGroupName, string integrationAccountName)
        {
            bool result = false;
            try
            {
                var account = LogicManagementClient.IntegrationAccounts.Get(resourceGroupName, integrationAccountName);
                result = account != null;
            }
            catch
            {
                result = false;
            }
            return result;
        }
    }
}