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
    public class GetAzureAutomationDscNodeTest : RMTestBase
    {
        private Mock<IAutomationClient> mockAutomationClient;

        private MockCommandRuntime mockCommandRuntime;

        private GetAzureAutomationDscNode cmdlet;

        
        public GetAzureAutomationDscNodeTest()
        {
            mockAutomationClient = new Mock<IAutomationClient>();
            mockCommandRuntime = new MockCommandRuntime();
            cmdlet = new GetAzureAutomationDscNode
            {
                AutomationClient = mockAutomationClient.Object,
                CommandRuntime = mockCommandRuntime
            };
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]

        public void GetAzureAutomationGetNodeById()
        {
            // Setup
            string resourceGroupName = "resourceGroup";
            string accountName = "account";
            Guid id = Guid.NewGuid();
            cmdlet.SetParameterSet("ById");

            mockAutomationClient.Setup(f => f.GetDscNodeById(resourceGroupName, accountName, id)).Returns((string a, string b, Guid c) => new DscNode());

            // Test
            cmdlet.ResourceGroupName = resourceGroupName;
            cmdlet.AutomationAccountName = accountName;
            cmdlet.Id = id;
            cmdlet.ExecuteCmdlet();

            // Assert
            mockAutomationClient.Verify(f => f.GetDscNodeById(resourceGroupName, accountName, id), Times.Once());
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]

        public void GetAzureAutomationDscNodeByName()
        {
            // Setup
            string resourceGroupName = "resourceGroup";
            string accountName = "account";
            string nodeName = "configuration";
            string nextLink = string.Empty;
            string status = DscNodeStatus.Compliant.ToString();
            cmdlet.SetParameterSet("ByName");

            mockAutomationClient.Setup(f => f.ListDscNodesByName(resourceGroupName, accountName, nodeName, status, ref nextLink));

            // Test
            cmdlet.ResourceGroupName = resourceGroupName;
            cmdlet.AutomationAccountName = accountName;
            cmdlet.Name = nodeName;
            cmdlet.Status = DscNodeStatus.Compliant;
            cmdlet.ExecuteCmdlet();

            // Assert
            mockAutomationClient.Verify(f => f.ListDscNodesByName(resourceGroupName, accountName, nodeName, status, ref nextLink), Times.Once());
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]

        public void GetAzureAutomationDscNodeByNodeConfiguration()
        {
            // Setup
            string resourceGroupName = "resourceGroup";
            string accountName = "account";
            string nodeConfigurationName = "config.localhost";
            string nextLink = string.Empty;
            string status = DscNodeStatus.Compliant.ToString();
            cmdlet.SetParameterSet("ByNodeConfiguration");

            mockAutomationClient.Setup(f => f.ListDscNodesByNodeConfiguration(resourceGroupName, accountName, nodeConfigurationName, status, ref nextLink));

            // Test
            cmdlet.ResourceGroupName = resourceGroupName;
            cmdlet.AutomationAccountName = accountName;
            cmdlet.NodeConfigurationName = nodeConfigurationName;
            cmdlet.Status = DscNodeStatus.Compliant;
            cmdlet.ExecuteCmdlet();

            // Assert
            mockAutomationClient.Verify(f => f.ListDscNodesByNodeConfiguration(resourceGroupName, accountName, nodeConfigurationName, status, ref nextLink), Times.Once());
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void GetAzureAutomationDscNodeByConfiguration()
        {
            // Setup
            string resourceGroupName = "resourceGroup";
            string accountName = "account";
            string nextLink = string.Empty;
            string configurationName = "config";
            string status = DscNodeStatus.Compliant.ToString();
            cmdlet.SetParameterSet("ByConfiguration");

            mockAutomationClient.Setup(f => f.ListDscNodesByConfiguration(resourceGroupName, accountName, configurationName, status, ref nextLink));

            // Test
            cmdlet.ResourceGroupName = resourceGroupName;
            cmdlet.AutomationAccountName = accountName;
            cmdlet.Status = DscNodeStatus.Compliant;
            cmdlet.ConfigurationName = configurationName;
            cmdlet.ExecuteCmdlet();

            // Assert
            mockAutomationClient.Verify(f => f.ListDscNodesByConfiguration(resourceGroupName, accountName, configurationName, status, ref nextLink), Times.Once());
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void GetAzureAutomationDscNodes()
        {
            // Setup
            string resourceGroupName = "resourceGroup";
            string accountName = "account";
            string nextLink = string.Empty;
            string status = DscNodeStatus.Compliant.ToString();
            cmdlet.SetParameterSet("ByAll");

            mockAutomationClient.Setup(f => f.ListDscNodes(resourceGroupName, accountName, status, ref nextLink));

            // Test
            cmdlet.ResourceGroupName = resourceGroupName;
            cmdlet.AutomationAccountName = accountName;
            cmdlet.Status = DscNodeStatus.Compliant;
            cmdlet.ExecuteCmdlet();

            // Assert
            mockAutomationClient.Verify(f => f.ListDscNodes(resourceGroupName, accountName, status, ref nextLink), Times.Once());
        }
    }
}