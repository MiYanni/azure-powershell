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
using Microsoft.Azure.Commands.Automation.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Commands.Common.Test.Mocks;
using Microsoft.WindowsAzure.Commands.Test.Utilities.Common;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using Moq;
using System.Collections.Generic;
namespace Microsoft.Azure.Commands.ResourceManager.Automation.Test.UnitTests
{
    [TestClass]
    public class GetAzureAutomationAccountTest : RMTestBase
    {
        private Mock<IAutomationClient> mockAutomationClient;

        private MockCommandRuntime mockCommandRuntime;

        private GetAzureAutomationAccount cmdlet;

        [TestInitialize]
        public void SetupTest()
        {
            mockAutomationClient = new Mock<IAutomationClient>();
            mockCommandRuntime = new MockCommandRuntime();
            cmdlet = new GetAzureAutomationAccount
            {
                AutomationClient = mockAutomationClient.Object,
                CommandRuntime = mockCommandRuntime
            };
        }

        [TestMethod]
        public void GetAzureAutomationAllAccountsSuccessfull()
        {
            // Setup
            string resourceGroupName = "resourceGroup";
            string nextLink = string.Empty;

            mockAutomationClient.Setup(f => f.ListAutomationAccounts(resourceGroupName, ref nextLink)).Returns((string a, string b) => new List<AutomationAccount>());

            // Test
            cmdlet.ResourceGroupName = resourceGroupName;
            cmdlet.ExecuteCmdlet();

            // Assert
            mockAutomationClient.Verify(f => f.ListAutomationAccounts(resourceGroupName, ref nextLink), Times.Once());
        }

        [TestMethod]
        public void GetAzureAutomationAccountByNameSuccessfull()
        {
            // Setup
            string resourceGroupName = "resourceGroup";
            string accountName = "account";
            cmdlet.SetParameterSet("ByAutomationAccountName");

            mockAutomationClient.Setup(f => f.GetAutomationAccount(resourceGroupName, accountName));

            // Test
            cmdlet.ResourceGroupName = resourceGroupName;
            cmdlet.Name = accountName;
            cmdlet.ExecuteCmdlet();

            // Assert
            mockAutomationClient.Verify(f => f.GetAutomationAccount(resourceGroupName, accountName), Times.Once());
        }
    }
}