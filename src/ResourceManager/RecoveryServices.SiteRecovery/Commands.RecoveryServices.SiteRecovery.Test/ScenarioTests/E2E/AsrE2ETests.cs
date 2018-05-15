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

using Xunit;
using Xunit.Abstractions;
using Microsoft.Azure.ServiceManagemenet.Common.Models;
using Microsoft.WindowsAzure.Commands.ScenarioTest;


namespace RecoveryServices.SiteRecovery.Test
{
    public class AsrE2ETests : AsrTestsBase
    {
        public AsrE2ETests(
            ITestOutputHelper output)
        {
            XunitTracingInterceptor.AddToContext(new XunitTracingInterceptor(output));
            vaultSettingsFilePath = System.IO.Path.Combine(
                System.AppDomain.CurrentDomain.BaseDirectory,
                "ScenarioTests\\E2E\\E2E.VaultCredentials");
            powershellFile = System.IO.Path.Combine(
                System.AppDomain.CurrentDomain.BaseDirectory,
                "ScenarioTests\\E2E\\AsrE2ETests.ps1");
            initialize();
        }

        [Fact]
        [Trait(
            Category.AcceptanceType,
            Category.CheckIn)]
        public void FabricTests()
        {
            RunPowerShellTest(
                Constants.NewModel,
                "Test-SiteRecoveryFabricTest -vaultSettingsFilePath \"" +
                vaultSettingsFilePath +
                "\"");
        }

        [Fact]
        [Trait(
            Category.AcceptanceType,
            Category.CheckIn)]
        public void TestCreatePolicy()
        {
            RunPowerShellTest(
                Constants.NewModel,
                "Test-SiteRecoveryCreatePolicy -vaultSettingsFilePath \"" +
                vaultSettingsFilePath +
                "\"");
        }

        [Fact]
        [Trait(
            Category.AcceptanceType,
            Category.CheckIn)]
        public void TestCreatePCMap()
        {
            RunPowerShellTest(
                Constants.NewModel,
                "Test-CreatePCMap -vaultSettingsFilePath \"" + vaultSettingsFilePath + "\"");
        }

        [Fact]
        [Trait(
            Category.AcceptanceType,
            Category.CheckIn)]
        public void TestEnableDR()
        {
            RunPowerShellTest(
                Constants.NewModel,
                "Test-SiteRecoveryEnableDR -vaultSettingsFilePath \"" +
                vaultSettingsFilePath +
                "\"");
        }

        [Fact]
        [Trait(
            Category.AcceptanceType,
            Category.CheckIn)]
        public void TestMapNetwork()
        {
            RunPowerShellTest(
                Constants.NewModel,
                "Test-MapNetwork -vaultSettingsFilePath \"" +
                vaultSettingsFilePath +
                "\"");
        }

        [Fact]
        [Trait(
            Category.AcceptanceType,
            Category.CheckIn)]
        public void TestTFO()
        {
            RunPowerShellTest(
                Constants.NewModel,
                "Test-TFO -vaultSettingsFilePath \"" +
                vaultSettingsFilePath +
                "\"");
        }

        [Fact]
        [Trait(
            Category.AcceptanceType,
            Category.CheckIn)]
        public void TestPlannedFailover()
        {
            RunPowerShellTest(
                Constants.NewModel,
                "Test-PlannedFailover -vaultSettingsFilePath \"" +
                vaultSettingsFilePath +
                "\"");
        }

        [Fact]
        [Trait(
            Category.AcceptanceType,
            Category.CheckIn)]
        public void TestReprotect()
        {
            RunPowerShellTest(
                Constants.NewModel,
                "Test-Reprotect -vaultSettingsFilePath \"" +
                vaultSettingsFilePath +
                "\"");
        }

        [Fact]
        [Trait(
            Category.AcceptanceType,
            Category.CheckIn)]
        public void TestUFOandFailback()
        {
            RunPowerShellTest(
                Constants.NewModel,
                "Test-UFOandFailback -vaultSettingsFilePath \"" +
                vaultSettingsFilePath +
                "\"");
        }

        [Fact]
        [Trait(
           Category.AcceptanceType,
           Category.CheckIn)]
        public void TestEditRP()
        {
            RunPowerShellTest(
                Constants.NewModel,
                "Test-EditRecoveryPlan -vaultSettingsFilePath \"" +
                vaultSettingsFilePath +
                "\"");
        }

        [Fact]
        [Trait(
           Category.AcceptanceType,
           Category.CheckIn)]
        public void TestRemoveRP()
        {
            RunPowerShellTest(
                Constants.NewModel,
                "Test-SiteRecoveryRemoveRecoveryPlan -vaultSettingsFilePath \"" +
                vaultSettingsFilePath +
                "\"");
        }

        [Fact]
        [Trait(
            Category.AcceptanceType,
            Category.CheckIn)]
        public void TestDisableDR()
        {
            RunPowerShellTest(
                Constants.NewModel,
                "Test-SiteRecoveryDisableDR -vaultSettingsFilePath \"" +
                vaultSettingsFilePath +
                "\"");
        }

        [Fact]
        [Trait(
            Category.AcceptanceType,
            Category.CheckIn)]
        public void TestRemovePCMap()
        {
            RunPowerShellTest(
                Constants.NewModel,
                "Test-RemovePCMap -vaultSettingsFilePath \"" + vaultSettingsFilePath + "\"");
        }

        [Fact]
        [Trait(
            Category.AcceptanceType,
            Category.CheckIn)]
        public void TestRemovePolicy()
        {
            RunPowerShellTest(
                Constants.NewModel,
                "Test-SiteRecoveryRemovePolicy -vaultSettingsFilePath \"" +
                vaultSettingsFilePath +
                "\"");
        }

        [Fact]
        [Trait(
             Category.AcceptanceType,
             Category.CheckIn)]
        public void TestRemoveNetworkPairing()
        {
            RunPowerShellTest(
                Constants.NewModel,
                "Test-RemoveNetworkPairing -vaultSettingsFilePath \"" +
                vaultSettingsFilePath +
                "\"");
        }

        [Fact]
        [Trait(
             Category.AcceptanceType,
             Category.CheckIn)]
        public void TestRemoveFabric()
        {
            RunPowerShellTest(
                Constants.NewModel,
                "Test-RemoveFabric -vaultSettingsFilePath \"" +
                vaultSettingsFilePath +
                "\"");
        }
    }
}
