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
using Microsoft.Azure.Commands.Automation.Cmdlet;
using Microsoft.Azure.Commands.Automation.Common;
using Microsoft.Azure.Commands.Automation.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Commands.Common.Test.Mocks;
using Microsoft.WindowsAzure.Commands.ScenarioTest;
using Microsoft.WindowsAzure.Commands.Test.Utilities.Common;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace Microsoft.Azure.Commands.ResourceManager.Automation.Test.UnitTests
{
    public class GetAzureAutomationDscNodeConfigurationTest : RMTestBase
    {
        private Mock<IAutomationClient> mockAutomationClient;

        private MockCommandRuntime mockCommandRuntime;

        private GetAzureAutomationDscNodeConfiguration cmdlet;

        
        public GetAzureAutomationDscNodeConfigurationTest()
        {
            mockAutomationClient = new Mock<IAutomationClient>();
            mockCommandRuntime = new MockCommandRuntime();
            cmdlet = new GetAzureAutomationDscNodeConfiguration
            {
                AutomationClient = mockAutomationClient.Object,
                CommandRuntime = mockCommandRuntime
            };
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]

        public void GetAzureAutomationGetDscNodeConfigurationByName()
        {
            // Setup
            string resourceGroupName = "resourceGroup";
            string accountName = "account";
            string nodeConfigurationName = "config.localhost";
            string rollupStatus = "Good";
            
            mockAutomationClient.Setup(f => f.GetNodeConfiguration(resourceGroupName, accountName, nodeConfigurationName, rollupStatus));

            // Test
            cmdlet.ResourceGroupName = resourceGroupName;
            cmdlet.AutomationAccountName = accountName;
            cmdlet.Name = nodeConfigurationName;
            cmdlet.RollupStatus = rollupStatus;
            cmdlet.ExecuteCmdlet();

            // Assert
            mockAutomationClient.Verify(f => f.GetNodeConfiguration(resourceGroupName, accountName, nodeConfigurationName, rollupStatus), Times.Once());
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]

        public void GetAzureAutomationDscNodeConfigurationByConfigurationName()
        {
            // Setup
            string resourceGroupName = "resourceGroup";
            string accountName = "account";
            string configurationName = "configuration";
            string rollupStatus = "Good";
            string nextLink = string.Empty;
            
            mockAutomationClient.Setup(f => f.ListNodeConfigurationsByConfigurationName(resourceGroupName, accountName, configurationName, rollupStatus, ref nextLink));

            // Test
            cmdlet.ResourceGroupName = resourceGroupName;
            cmdlet.AutomationAccountName = accountName;
            cmdlet.ConfigurationName = configurationName;
            cmdlet.RollupStatus = rollupStatus;
            cmdlet.ExecuteCmdlet();

            // Assert
            mockAutomationClient.Verify(f => f.ListNodeConfigurationsByConfigurationName(resourceGroupName, accountName, configurationName, rollupStatus, ref nextLink), Times.Once());
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]

        public void GetAzureAutomationDscNodeConfigurations()
        {
            // Setup
            string resourceGroupName = "resourceGroup";
            string accountName = "account";
            string rollupStatus = "Good";
            string nextLink = string.Empty;
            
            mockAutomationClient.Setup(f => f.ListNodeConfigurations(resourceGroupName, accountName, rollupStatus, ref nextLink));

            // Test
            cmdlet.ResourceGroupName = resourceGroupName;
            cmdlet.AutomationAccountName = accountName;
            cmdlet.RollupStatus = rollupStatus;
            cmdlet.ExecuteCmdlet();

            // Assert
            mockAutomationClient.Verify(f => f.ListNodeConfigurations(resourceGroupName, accountName, rollupStatus, ref nextLink), Times.Once());
        }
    }
}