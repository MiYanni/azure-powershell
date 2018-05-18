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

using Microsoft.Azure.Commands.Sql.FirewallRule.Cmdlet;
using Microsoft.Azure.Commands.Sql.Test.Utilities;
using Microsoft.Azure.ServiceManagemenet.Common.Models;
using Microsoft.WindowsAzure.Commands.ScenarioTest;
using System;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Azure.Commands.Sql.Test.UnitTests
{
    public class AzureSqlServerFirewallRuleAttributeTests
    {
        public AzureSqlServerFirewallRuleAttributeTests(ITestOutputHelper output)
        {
            XunitTracingInterceptor.AddToContext(new XunitTracingInterceptor(output));
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void NewAzureSqlServerFirewallRuleAttributes()
        {
            Type type = typeof(NewAzureSqlServerFirewallRule);
            UnitTestHelper.CheckCmdletModifiesData(type, false);
            UnitTestHelper.CheckConfirmImpact(type, System.Management.Automation.ConfirmImpact.Low);

            UnitTestHelper.CheckCmdletParameterAttributes(type, "ServerName", true, true);
            UnitTestHelper.CheckCmdletParameterAttributes(type, "FirewallRuleName", true, false);
            UnitTestHelper.CheckCmdletParameterAttributes(type, "StartIpAddress", true, false);
            UnitTestHelper.CheckCmdletParameterAttributes(type, "EndIpAddress", true, false);
            UnitTestHelper.CheckCmdletParameterAttributes(type, "AllowAllAzureIPs", false, false);
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void SetAzureSqlServerFirewallRuleAttributes()
        {
            Type type = typeof(SetAzureSqlServerFirewallRule);
            UnitTestHelper.CheckCmdletModifiesData(type, false);
            UnitTestHelper.CheckConfirmImpact(type, System.Management.Automation.ConfirmImpact.Low);

            UnitTestHelper.CheckCmdletParameterAttributes(type, "ServerName", true, true);
            UnitTestHelper.CheckCmdletParameterAttributes(type, "FirewallRuleName", true, true);
            UnitTestHelper.CheckCmdletParameterAttributes(type, "StartIpAddress", true, false);
            UnitTestHelper.CheckCmdletParameterAttributes(type, "EndIpAddress", true, false);
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void RemoveAzureSqlServerFirewallRuleAttributes()
        {
            Type type = typeof(RemoveAzureSqlServerFirewallRule);
            UnitTestHelper.CheckCmdletModifiesData(type, true);

            UnitTestHelper.CheckCmdletParameterAttributes(type, "ServerName", true, true);
            UnitTestHelper.CheckCmdletParameterAttributes(type, "FirewallRuleName", true, true);
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void GetAzureSqlServerFirewallRuleAttributes()
        {
            Type type = typeof(GetAzureSqlServerFirewallRule);
            UnitTestHelper.CheckCmdletModifiesData(type, false);
            UnitTestHelper.CheckConfirmImpact(type, System.Management.Automation.ConfirmImpact.None);

            UnitTestHelper.CheckCmdletParameterAttributes(type, "ServerName", true, true);
            UnitTestHelper.CheckCmdletParameterAttributes(type, "FirewallRuleName", false, true);
        }
    }
}
