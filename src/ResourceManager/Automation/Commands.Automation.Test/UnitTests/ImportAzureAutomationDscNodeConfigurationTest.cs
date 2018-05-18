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

using Microsoft.Azure.Commands.Automation.Cmdlet;
using Microsoft.Azure.Commands.Automation.Common;
using Microsoft.WindowsAzure.Commands.Common.Test.Mocks;
using Microsoft.WindowsAzure.Commands.ScenarioTest;
using Microsoft.WindowsAzure.Commands.Test.Utilities.Common;
using Moq;
using Xunit;

namespace Microsoft.Azure.Commands.ResourceManager.Automation.Test.UnitTests
{
    public class ImportAzureAutomationDscNodeConfigurationTest : RMTestBase
    {
        private Mock<IAutomationClient> mockAutomationClient;

        private MockCommandRuntime mockCommandRuntime;

        private ImportAzureAutomationDscNodeConfiguration cmdlet;

        public ImportAzureAutomationDscNodeConfigurationTest()
        {
            mockAutomationClient = new Mock<IAutomationClient>();
            mockCommandRuntime = new MockCommandRuntime();
            
            cmdlet = new ImportAzureAutomationDscNodeConfiguration
            {
                AutomationClient = mockAutomationClient.Object,
                CommandRuntime = mockCommandRuntime
            };
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void ImportAzureAutomationDscNodeConfigurationTestWithNullNodeConfiguration()
        {
            // Setup
            string resourceGroupName = "resourceGroup";
            string accountName = "automation";
            string configurationName = "runbook";
            string path = "/path/to/configuration";
            string nodeConfigurationName = "runbook.configuration";
            bool incrementNodeConfigBuild = false;

            mockAutomationClient.Setup(
                f =>
                    f.CreateNodeConfiguration(resourceGroupName, accountName, path, configurationName, incrementNodeConfigBuild, false)
                );

            mockAutomationClient.Setup(
                f => f.GetNodeConfiguration(resourceGroupName, accountName, nodeConfigurationName, null)
                );

            // Test
            cmdlet.ResourceGroupName = resourceGroupName;
            cmdlet.AutomationAccountName = accountName;
            cmdlet.ConfigurationName = configurationName;
            cmdlet.Path = path;
            cmdlet.IncrementNodeConfigurationBuild = incrementNodeConfigBuild;
            cmdlet.Force = false;

            cmdlet.ExecuteCmdlet();

            // Assert
            mockAutomationClient.Verify(f => f.CreateNodeConfiguration(resourceGroupName, accountName, path, configurationName, incrementNodeConfigBuild, false),
                Times.Once());
        }
    }
}
