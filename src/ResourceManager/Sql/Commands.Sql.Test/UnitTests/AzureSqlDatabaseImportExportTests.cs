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

using Microsoft.Azure.Commands.Sql.ImportExport.Cmdlet;
using Microsoft.Azure.Commands.Sql.Test.Utilities;
using Microsoft.Azure.ServiceManagemenet.Common.Models;
using Microsoft.WindowsAzure.Commands.ScenarioTest;
using System;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Azure.Commands.Sql.Test.UnitTests
{
    public class AzureSqlDatabaseImportExportTests
    {
        public AzureSqlDatabaseImportExportTests(ITestOutputHelper output)
        {
            XunitTracingInterceptor.AddToContext(new XunitTracingInterceptor(output));
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void NewAzureSqlDatabaseExportAttributes()
        {
            Type type = typeof(NewAzureSqlDatabaseExport);

            UnitTestHelper.CheckCmdletParameterAttributes(type, "ServerName", true, true);
            UnitTestHelper.CheckCmdletParameterAttributes(type, "DatabaseName", true, true);
            UnitTestHelper.CheckCmdletParameterAttributes(type, "StorageKeyType", true, false);
            UnitTestHelper.CheckCmdletParameterAttributes(type, "StorageKey", true, false);
            UnitTestHelper.CheckCmdletParameterAttributes(type, "StorageUri", true, false);
            UnitTestHelper.CheckCmdletParameterAttributes(type, "AdministratorLogin", true, false);
            UnitTestHelper.CheckCmdletParameterAttributes(type, "AdministratorLoginPassword", true, false);
            UnitTestHelper.CheckCmdletParameterAttributes(type, "AuthenticationType", false, false);
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void NewAzureSqlDatabaseImportAttributes()
        {
            Type type = typeof(NewAzureSqlDatabaseImport);

            UnitTestHelper.CheckCmdletParameterAttributes(type, "ServerName", true, true);
            UnitTestHelper.CheckCmdletParameterAttributes(type, "DatabaseName", true, false);
            UnitTestHelper.CheckCmdletParameterAttributes(type, "StorageKeyType", true, false);
            UnitTestHelper.CheckCmdletParameterAttributes(type, "StorageKey", true, false);
            UnitTestHelper.CheckCmdletParameterAttributes(type, "StorageUri", true, false);
            UnitTestHelper.CheckCmdletParameterAttributes(type, "AdministratorLogin", true, false);
            UnitTestHelper.CheckCmdletParameterAttributes(type, "AdministratorLoginPassword", true, false);
            UnitTestHelper.CheckCmdletParameterAttributes(type, "AuthenticationType", false, false);
            UnitTestHelper.CheckCmdletParameterAttributes(type, "Edition", true, false);
            UnitTestHelper.CheckCmdletParameterAttributes(type, "ServiceObjectiveName", true, false);
            UnitTestHelper.CheckCmdletParameterAttributes(type, "DatabaseMaxSizeBytes", true, false);
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void AzureRmSqlDatabaseImportExportStatusAttributes()
        {
            Type type = typeof(GetAzureSqlDatabaseImportExportStatus);

            UnitTestHelper.CheckCmdletParameterAttributes(type, "OperationStatusLink", true, true);
        }
    }
}
