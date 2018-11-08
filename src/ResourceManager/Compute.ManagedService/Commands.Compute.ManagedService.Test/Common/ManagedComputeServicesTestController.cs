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

using Microsoft.Azure.Commands.Common.Authentication;
using Microsoft.Azure.Management.Compute;
using Microsoft.Azure.Management.Network;
using Microsoft.Azure.Management.Storage;
using Microsoft.Azure.ServiceManagemenet.Common.Models;
using Microsoft.Azure.Test.HttpRecorder;
using Microsoft.WindowsAzure.Commands.ScenarioTest;
using Microsoft.WindowsAzure.Commands.Test.Utilities.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Azure.Management.Internal.Resources;
using RestTestFramework = Microsoft.Rest.ClientRuntime.Azure.TestFramework;

namespace ManagedComputeServicesTests
{
    public sealed class ManagedComputeServicesTestController : RMTestBase
    {
        private readonly EnvironmentSetupHelper _helper;

        public ResourceManagementClient ResourceManagementClient { get; private set; }

        public StorageManagementClient StorageClient { get; private set; }

        public NetworkManagementClient NetworkManagementClient { get; private set; }

        public ComputeManagementClient ComputeManagementClient { get; private set; }

        public static ManagedComputeServicesTestController NewInstance => new ManagedComputeServicesTestController();

        public ManagedComputeServicesTestController()
        {
            _helper = new EnvironmentSetupHelper();
        }

        public void RunPsTest(XunitTracingInterceptor logger, params string[] scripts)
        {
            var sf = new StackTrace().GetFrame(1);
            var callingClassType = sf.GetMethod().ReflectedType?.ToString();
            var mockName = sf.GetMethod().Name;

            _helper.TracingInterceptor = logger;
            RunPsTestWorkflow(
                () => scripts,
                // no custom cleanup 
                null,
                callingClassType,
                mockName);
        }

        public void RunPsTestWorkflow(
            Func<string[]> scriptBuilder,
            Action cleanup,
            string callingClassType,
            string mockName)
        {
            var d = new Dictionary<string, string>
            {
                {"Microsoft.Resources", null},
                {"Microsoft.Features", null},
                {"Microsoft.Authorization", null},
                {"Microsoft.Compute", null}
            };
            var providersToIgnore = new Dictionary<string, string>
            {
                {"Microsoft.Azure.Management.Resources.ResourceManagementClient", "2016-02-01"}
            };
            HttpMockServer.Matcher = new PermissiveRecordMatcherWithApiExclusion(true, d, providersToIgnore);

            HttpMockServer.RecordsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SessionRecords");
            using (var context = RestTestFramework.MockContext.Start(callingClassType, mockName))
            {
                SetupManagementClients(context);

                _helper.SetupEnvironment(AzureModule.AzureResourceManager);

                var callingClassName = callingClassType.Split(new[] { "." }, StringSplitOptions.RemoveEmptyEntries).Last();
                _helper.SetupModules(AzureModule.AzureResourceManager,
                    "ScenarioTests\\Common.ps1",
                    "ScenarioTests\\ComputeTestCommon.ps1",
                    "ScenarioTests\\" + callingClassName + ".ps1",
                    _helper.RMProfileModule,
                    _helper.RMResourceModule,
// TODO: Remove IfDef code
#if !NETSTANDARD
                    _helper.RMStorageDataPlaneModule,
#endif
                    _helper.RMStorageModule,
                    _helper.GetRMModulePath("AzureRM.Compute.ManagedService.psd1"),
                    _helper.GetRMModulePath("AzureRM.Compute.psd1"),
                    _helper.GetRMModulePath("AzureRM.Network.psd1"),
                    "AzureRM.Storage.ps1",
                    "AzureRM.Resources.ps1");

                try
                {
                    var psScripts = scriptBuilder?.Invoke();
                    if (psScripts != null)
                    {
                        _helper.RunPowerShellTest(psScripts);
                    }
                }
                finally
                {
                    cleanup?.Invoke();
                }
            }
        }

        private void SetupManagementClients(RestTestFramework.MockContext context)
        {
            ResourceManagementClient = GetResourceManagementClient(context);
            StorageClient = GetStorageManagementClient(context);
            NetworkManagementClient = GetNetworkManagementClientClient(context);
            ComputeManagementClient = GetComputeManagementClient(context);

            _helper.SetupManagementClients(
                ResourceManagementClient,
                StorageClient,
                NetworkManagementClient,
                ComputeManagementClient);
        }

        private static ResourceManagementClient GetResourceManagementClient(RestTestFramework.MockContext context)
        {
            return context.GetServiceClient<ResourceManagementClient>(RestTestFramework.TestEnvironmentFactory.GetTestEnvironment());
        }

        private static StorageManagementClient GetStorageManagementClient(RestTestFramework.MockContext context)
        {
            return context.GetServiceClient<StorageManagementClient>(RestTestFramework.TestEnvironmentFactory.GetTestEnvironment());
        }

        private static NetworkManagementClient GetNetworkManagementClientClient(RestTestFramework.MockContext context)
        {
            return context.GetServiceClient<NetworkManagementClient>(RestTestFramework.TestEnvironmentFactory.GetTestEnvironment());
        }

        private static ComputeManagementClient GetComputeManagementClient(RestTestFramework.MockContext context)
        {
            return context.GetServiceClient<ComputeManagementClient>(RestTestFramework.TestEnvironmentFactory.GetTestEnvironment());
        }
    }
}
