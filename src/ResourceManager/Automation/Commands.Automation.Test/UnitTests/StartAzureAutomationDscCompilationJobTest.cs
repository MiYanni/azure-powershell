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

using System.Collections.Generic;
using Microsoft.Azure.Commands.Automation.Cmdlet;
using Microsoft.Azure.Commands.Automation.Common;
using Microsoft.WindowsAzure.Commands.Common.Test.Mocks;
using Microsoft.WindowsAzure.Commands.ScenarioTest;
using Microsoft.WindowsAzure.Commands.Test.Utilities.Common;
using Moq;
using Xunit;

namespace Microsoft.Azure.Commands.ResourceManager.Automation.Test.UnitTests
{
    public class StartAzureAutomationDscCompilationJobTest : RMTestBase
    {
        private Mock<IAutomationClient> mockAutomationClient;

        private MockCommandRuntime mockCommandRuntime;

        private StartAzureAutomationDscCompilationJob cmdlet;

        public StartAzureAutomationDscCompilationJobTest()
        {
            mockAutomationClient = new Mock<IAutomationClient>();
            mockCommandRuntime = new MockCommandRuntime();
            cmdlet = new StartAzureAutomationDscCompilationJob
            {
                AutomationClient = mockAutomationClient.Object,
                CommandRuntime = mockCommandRuntime
            };
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void StartAzureAutomationDscCompilationJobTestSuccessful()
        {
            // Setup
            string resourceGroupName = "resourceGroup";
            string accountName = "automation";
            string configurationName = "runbook";
            bool incrementNodeConfigurationBuild = true;
            var parameters = new Dictionary<string, string>
            {
                {"Key1", "Value1"},
                {"Key2", "Value2"},
            };
            

            mockAutomationClient.Setup(
                f =>
                    f.StartCompilationJob(resourceGroupName, accountName, configurationName, parameters, null, incrementNodeConfigurationBuild)
                );

            //CompilationJob StartCompilationJob(string resourceGroupName, 
            // string automationAccountName, string configurationName, 
            // IDictionary parameters, IDictionary configurationData, bool incrementNodeConfigurationBuild = false);

            // Test
            cmdlet.ResourceGroupName = resourceGroupName;
            cmdlet.AutomationAccountName = accountName;
            cmdlet.ConfigurationName = configurationName;
            cmdlet.Parameters = parameters;
            cmdlet.IncrementNodeConfigurationBuild = incrementNodeConfigurationBuild;
            cmdlet.ConfigurationData = null;

            cmdlet.ExecuteCmdlet();

            // Assert
            mockAutomationClient.Verify(f => f.StartCompilationJob(resourceGroupName, accountName, configurationName, parameters, null, incrementNodeConfigurationBuild),
                Times.Once());
        }
    }
}
