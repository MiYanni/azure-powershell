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
    public class GetAzureAutomationDscNodeReportTest : RMTestBase
    {
        private Mock<IAutomationClient> mockAutomationClient;

        private MockCommandRuntime mockCommandRuntime;

        private GetAzureAutomationDscNodeReport cmdlet;

        
        public GetAzureAutomationDscNodeReportTest()
        {
            mockAutomationClient = new Mock<IAutomationClient>();
            mockCommandRuntime = new MockCommandRuntime();
            cmdlet = new GetAzureAutomationDscNodeReport
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
            cmdlet.SetParameterSet("ByLatest");

            mockAutomationClient.Setup(f => f.GetLatestDscNodeReport(resourceGroupName, accountName, id));

            // Test
            cmdlet.ResourceGroupName = resourceGroupName;
            cmdlet.AutomationAccountName = accountName;
            cmdlet.NodeId = id;
            cmdlet.ExecuteCmdlet();

            // Assert
            mockAutomationClient.Verify(f => f.GetLatestDscNodeReport(resourceGroupName, accountName, id), Times.Once());
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]

        public void GetAzureAutomationDscNodeByName()
        {
            // Setup
            string resourceGroupName = "resourceGroup";
            string accountName = "account";
            Guid nodeId = Guid.NewGuid();
            Guid reportId = Guid.NewGuid();
            string nextLink = string.Empty;
            cmdlet.SetParameterSet("ById");

            mockAutomationClient.Setup(f => f.GetDscNodeReportByReportId(resourceGroupName, accountName, nodeId, reportId));

            // Test
            cmdlet.ResourceGroupName = resourceGroupName;
            cmdlet.AutomationAccountName = accountName;
            cmdlet.NodeId = nodeId;
            cmdlet.Id = reportId;
            cmdlet.ExecuteCmdlet();

            // Assert
            mockAutomationClient.Verify(f => f.GetDscNodeReportByReportId(resourceGroupName, accountName, nodeId, reportId), Times.Once());
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]

        public void GetAzureAutomationDscNodeByNodeConfiguration()
        {
            // Setup
            string resourceGroupName = "resourceGroup";
            string accountName = "account";
            Guid id = Guid.NewGuid();
            string nextLink = string.Empty;
            DateTimeOffset startTime = DateTimeOffset.Now;
            DateTimeOffset endTime = DateTimeOffset.Now;

            mockAutomationClient.Setup(f => f.ListDscNodeReports(resourceGroupName, accountName, id, startTime, endTime, ref nextLink));

            // Test
            cmdlet.ResourceGroupName = resourceGroupName;
            cmdlet.AutomationAccountName = accountName;
            cmdlet.NodeId = id;
            cmdlet.StartTime = startTime;
            cmdlet.EndTime = endTime;
            cmdlet.ExecuteCmdlet();

            // Assert
            mockAutomationClient.Verify(f => f.ListDscNodeReports(resourceGroupName, accountName, id, startTime, endTime, ref nextLink), Times.Once());
        }
    }
}