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

using Microsoft.Azure.Commands.Sql.Database.Cmdlet;
using Microsoft.Azure.Commands.Sql.Test.Utilities;
using Microsoft.Azure.ServiceManagemenet.Common.Models;
using Microsoft.WindowsAzure.Commands.ScenarioTest;
using System;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Azure.Commands.Sql.Test.UnitTests
{
    public class AzureSqlDatabaseAttributeTests
    {
        public AzureSqlDatabaseAttributeTests(ITestOutputHelper output)
        {
            XunitTracingInterceptor.AddToContext(new XunitTracingInterceptor(output));
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void NewAzureSqlDatabaseAttributes()
        {
            Type type = typeof(NewAzureSqlDatabase);
            UnitTestHelper.CheckCmdletModifiesData(type, false);
            UnitTestHelper.CheckConfirmImpact(type, System.Management.Automation.ConfirmImpact.Low);

            UnitTestHelper.CheckCmdletParameterAttributes(type, "ServerName", true, true);
            UnitTestHelper.CheckCmdletParameterAttributes(type, "DatabaseName", true, false);
            UnitTestHelper.CheckCmdletParameterAttributes(type, "CollationName", false, false);
            UnitTestHelper.CheckCmdletParameterAttributes(type, "CatalogCollation", false, false);
            UnitTestHelper.CheckCmdletParameterAttributes(type, "MaxSizeBytes", false, false);
            UnitTestHelper.CheckCmdletParameterAttributes(type, "Edition", false, false);
            UnitTestHelper.CheckCmdletParameterAttributes(type, "RequestedServiceObjectiveName", false, false);
            UnitTestHelper.CheckCmdletParameterAttributes(type, "Tags", false, false);
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void SetAzureSqlDatabaseAttributes()
        {
            Type type = typeof(SetAzureSqlDatabase);
            UnitTestHelper.CheckCmdletModifiesData(type, false);
            UnitTestHelper.CheckConfirmImpact(type, System.Management.Automation.ConfirmImpact.Medium);

            UnitTestHelper.CheckCmdletParameterAttributes(type, "ServerName", true, true);
            UnitTestHelper.CheckCmdletParameterAttributes(type, "DatabaseName", true, true);
            UnitTestHelper.CheckCmdletParameterAttributes(type, "MaxSizeBytes", false, false);
            UnitTestHelper.CheckCmdletParameterAttributes(type, "Edition", false, false);
            UnitTestHelper.CheckCmdletParameterAttributes(type, "RequestedServiceObjectiveName", false, false);
            UnitTestHelper.CheckCmdletParameterAttributes(type, "Tags", false, false);
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void RemoveAzureSqlDatabaseAttributes()
        {
            Type type = typeof(RemoveAzureSqlDatabase);
            UnitTestHelper.CheckCmdletModifiesData(type, true);

            UnitTestHelper.CheckCmdletParameterAttributes(type, "ServerName", true, true);
            UnitTestHelper.CheckCmdletParameterAttributes(type, "DatabaseName", true, true);
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void GetAzureSqlDatabaseAttributes()
        {
            Type type = typeof(GetAzureSqlDatabase);
            UnitTestHelper.CheckCmdletModifiesData(type, false);
            UnitTestHelper.CheckConfirmImpact(type, System.Management.Automation.ConfirmImpact.None);

            UnitTestHelper.CheckCmdletParameterAttributes(type, "ServerName", true, true);
            UnitTestHelper.CheckCmdletParameterAttributes(type, "DatabaseName", false, true);
        }
    }
}
