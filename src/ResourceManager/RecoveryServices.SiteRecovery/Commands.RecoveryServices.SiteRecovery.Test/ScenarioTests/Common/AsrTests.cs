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
using Xunit;
using Xunit.Abstractions;
using Microsoft.Azure.ServiceManagemenet.Common.Models;
using Microsoft.WindowsAzure.Commands.ScenarioTest;
using Microsoft.Azure.Commands.RecoveryServices.SiteRecovery;
using Microsoft.Azure.Portal.RecoveryServices.Models.Common;

namespace RecoveryServices.SiteRecovery.Test
{
    public class AsrCommonTests : AsrTestsBase
    {
        public AsrCommonTests(
            ITestOutputHelper output)
        {
            XunitTracingInterceptor.AddToContext(new XunitTracingInterceptor(output));
            vaultSettingsFilePath = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "ScenarioTests\\Common\\Common.VaultCredentials");
            powershellFile = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "ScenarioTests\\Common\\AsrTests.ps1");
            initialize();
        }

        [Fact]
        [Trait(
            Category.AcceptanceType,
            Category.CheckIn)]
        public void EnumerationTests()
        {
            RunPowerShellTest(
                Constants.NewModel,
                "Test-SiteRecoveryEnumerationTests -vaultSettingsFilePath \"" +
                vaultSettingsFilePath +
                "\"");
        }

        [Fact]
        [Trait(
            Category.AcceptanceType,
            Category.CheckIn)]
        public void V2AEvent()
        {
            RunPowerShellTest(
             Constants.NewModel,
             "Test-AsrEvent -vaultSettingsFilePath \"" + vaultSettingsFilePath + "\"");
        }

        [Fact]
        [Trait(
            Category.AcceptanceType,
            Category.CheckIn)]
        public void V2AGetJobTest()
        {
            RunPowerShellTest(
             Constants.NewModel,
             "Test-Job -vaultSettingsFilePath \"" + vaultSettingsFilePath + "\"");
        }

        [Fact]
        [Trait(
            Category.AcceptanceType,
            Category.CheckIn)]
        public void V2AGetNotificationTest()
        {
            RunPowerShellTest(
             Constants.NewModel,
             "Test-NotificationSettings -vaultSettingsFilePath \"" + vaultSettingsFilePath + "\"");
        }

        [Fact]
        [Trait(
            Category.AcceptanceType,
            Category.CheckIn)]
        public void CIKTokenValidation()
        {
            DateTime? dateTime = new DateTime(636604856296924385);
            PSRecoveryServicesClient.asrVaultCreds = new ASRVaultCreds();
            PSRecoveryServicesClient.asrVaultCreds.ChannelIntegrityKey = "RandomRandom";

            var cikToken =  PSRecoveryServicesClient.GenerateAgentAuthenticationHeader(
                  "e5ec3f71-75c6-4688-b557-6ef69d2e7514-2018-04-27 22:43:45Z-Ps",
                   dateTime);

            Assert.Equal(
                cikToken, 
                "{\"NotBeforeTimestamp\":\"\\/Date(1524865429692)\\/\",\"NotAfterTimestamp\":\"\\/Date(1525470229692)\\/\",\"ClientRequestId\":\"e5ec3f71-75c6-4688-b557-6ef69d2e7514-2018-04-27 22:43:45Z-Ps\",\"HashFunction\":\"HMACSHA256\",\"Hmac\":\"cYcaVjQ7BOG/lVrrl7dhwK5WXad6mvQdqm3ce3JSRY4=\",\"Version\":{\"Major\":1,\"Minor\":2,\"Build\":-1,\"Revision\":-1,\"MajorRevision\":-1,\"MinorRevision\":-1},\"PropertyBag\":{}}");
        }
    }
}
