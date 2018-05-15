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

using AutoMapper;
using Microsoft.Azure.Commands.Common.Authentication;
using Microsoft.Azure.Commands.Common.Authentication.Abstractions;
using Microsoft.Azure.Commands.Common.Authentication.Models;
using Microsoft.Azure.Commands.Compute.Common;
using Microsoft.Azure.Commands.Compute.Extension.AEM;
using Microsoft.Azure.Commands.ResourceManager.Common.ArgumentCompleters;
using Microsoft.Azure.Management.Compute;
using Microsoft.Azure.Management.Compute.Models;
using Microsoft.Azure.Management.Storage;
using Microsoft.Azure.Management.Storage.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.Azure.Commands.Compute
{
    [Cmdlet(
        "Test",
        ProfileNouns.VirtualMachineAEMExtension)]
    [OutputType(typeof(AEMTestResult))]
    public class TestAzureRmVMAEMExtension : VirtualMachineExtensionBaseCmdlet
    {
        private AEMHelper _Helper = null;

        [Parameter(
                Mandatory = true,
                Position = 0,
                ValueFromPipelineByPropertyName = true,
                HelpMessage = "The resource group name.")]
        [ResourceGroupCompleter]
        [ValidateNotNullOrEmpty]
        public string ResourceGroupName { get; set; }

        [Alias("ResourceName")]
        [Parameter(
            Mandatory = true,
            Position = 1,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The virtual machine name.")]
        [ValidateNotNullOrEmpty]
        public string VMName { get; set; }

        [Parameter(
                Mandatory = false,
                Position = 2,
                ValueFromPipelineByPropertyName = false,
                HelpMessage = "Operating System Type of the virtual machines. Possible values: Windows | Linux")]
        public string OSType { get; set; }

        [Parameter(
                Mandatory = false,
                Position = 3,
                ValueFromPipelineByPropertyName = false,
                HelpMessage = "Time that should be waited for the Strorage Metrics or Diagnostics data to be available in minutes. Default is 15 minutes")]
        public int WaitTimeInMinutes { get; set; }

        [Parameter(
                Mandatory = false,
                Position = 4,
                ValueFromPipelineByPropertyName = false,
                HelpMessage = "Disables the test for table content")]
        public SwitchParameter SkipStorageCheck { get; set; }

        public TestAzureRmVMAEMExtension()
        {
            WaitTimeInMinutes = 15;
        }

        public override void ExecuteCmdlet()
        {
            _Helper = new AEMHelper(err => WriteError(err), msg => WriteVerbose(msg), msg => WriteWarning(msg),
                CommandRuntime.Host.UI,
                AzureSession.Instance.ClientFactory.CreateArmClient<StorageManagementClient>(DefaultProfile.DefaultContext, AzureEnvironment.Endpoint.ResourceManager),
                DefaultContext.Subscription);

            _Helper.WriteVerbose("Starting TestAzureRmVMAEMExtension");

            base.ExecuteCmdlet();

            ExecuteClientAction(() =>
            {
                AEMTestResult rootResult = new AEMTestResult();
                rootResult.TestName = "Azure Enhanced Monitoring Test";

                //#################################################
                //# Check if VM exists
                //#################################################
                _Helper.WriteHost("VM Existance check for {0} ...", false, VMName);
                var selectedVM = ComputeClient.ComputeManagementClient.VirtualMachines.Get(ResourceGroupName, VMName);
                var selectedVMStatus = ComputeClient.ComputeManagementClient.VirtualMachines.GetWithInstanceView(ResourceGroupName, VMName).Body.InstanceView;


                if (selectedVM == null)
                {
                    rootResult.PartialResults.Add(new AEMTestResult("VM Existance check for {0}", false, VMName));
                    _Helper.WriteHost("NOT OK ", ConsoleColor.Red);
                    return;
                }
                rootResult.PartialResults.Add(new AEMTestResult("VM Existance check for {0}", true, VMName));
                _Helper.WriteHost("OK ", ConsoleColor.Green);
                //#################################################    
                //#################################################
                var osdisk = selectedVM.StorageProfile.OsDisk;
                if (String.IsNullOrEmpty(OSType))
                {
                    OSType = osdisk.OsType.ToString();
                }
                if (String.IsNullOrEmpty(OSType))
                {
                    _Helper.WriteError("Could not determine Operating System of the VM. Please provide the Operating System type ({0} or {1}) via parameter OSType", AEMExtensionConstants.OSTypeWindows, AEMExtensionConstants.OSTypeLinux);
                    return;
                }
                //#################################################
                //# Check for Guest Agent
                //#################################################
                _Helper.WriteHost("VM Guest Agent check...", false);
                var vmAgentStatus = false;

                //# It is not possible to detect if VM Agent is installed on ARM
                vmAgentStatus = true;
                if (!vmAgentStatus)
                {
                    rootResult.PartialResults.Add(new AEMTestResult("VM Guest Agent check", false));
                    _Helper.WriteHost("NOT OK ", ConsoleColor.Red);
                    _Helper.WriteWarning(AEMExtensionConstants.MissingGuestAgentWarning);
                    return;
                }
                rootResult.PartialResults.Add(new AEMTestResult("VM Guest Agent check", true));
                _Helper.WriteHost("OK ", ConsoleColor.Green);
                //#################################################    
                //#################################################


                //#################################################
                //# Check for Azure Enhanced Monitoring Extension for SAP
                //#################################################
                _Helper.WriteHost("Azure Enhanced Monitoring Extension for SAP Installation check...", false);

                string monPublicConfig = null;
                var monExtension = _Helper.GetExtension(selectedVM, AEMExtensionConstants.AEMExtensionType[OSType], AEMExtensionConstants.AEMExtensionPublisher[OSType]);
                if (monExtension != null)
                {
                    monPublicConfig = monExtension.Settings.ToString();
                }

                if (monExtension == null || String.IsNullOrEmpty(monPublicConfig))
                {
                    rootResult.PartialResults.Add(new AEMTestResult("Azure Enhanced Monitoring Extension for SAP Installation check", false));
                    _Helper.WriteHost("NOT OK ", ConsoleColor.Red);
                }
                else
                {
                    rootResult.PartialResults.Add(new AEMTestResult("Azure Enhanced Monitoring Extension for SAP Installation check", true));
                    _Helper.WriteHost("OK ", ConsoleColor.Green);
                }
                //#################################################    
                //#################################################

                var accounts = new List<string>();
                //var osdisk = selectedVM.StorageProfile.OsDisk;

                var osaccountName = String.Empty;
                if (osdisk.ManagedDisk == null)
                {
                    var accountName = _Helper.GetStorageAccountFromUri(osdisk.Vhd.Uri);
                    osaccountName = accountName;
                    accounts.Add(accountName);
                }

                var dataDisks = selectedVM.StorageProfile.DataDisks;
                foreach (var disk in dataDisks)
                {
                    if (disk.ManagedDisk != null)
                    {                        
                        continue;
                    }
                    var accountName = _Helper.GetStorageAccountFromUri(disk.Vhd.Uri);
                    if (!accounts.Contains(accountName))
                    {
                        accounts.Add(accountName);
                    }
                }

                //#################################################
                //# Check storage metrics
                //#################################################
                _Helper.WriteHost("Storage Metrics check...");
                var metricsResult = new AEMTestResult("Storage Metrics check");
                rootResult.PartialResults.Add(metricsResult);
                if (!SkipStorageCheck.IsPresent)
                {
                    foreach (var account in accounts)
                    {
                        var accountResult = new AEMTestResult("Storage Metrics check for {0}", account);
                        metricsResult.PartialResults.Add(accountResult);

                        _Helper.WriteHost("\tStorage Metrics check for {0}...", account);
                        var storage = _Helper.GetStorageAccountFromCache(account);

                        if (!_Helper.IsPremiumStorageAccount(storage))
                        {
                            _Helper.WriteHost("\t\tStorage Metrics configuration check for {0}...", false, account);
                            var currentConfig = _Helper.GetStorageAnalytics(account);

                            bool storageConfigOk = false;
                            if (!_Helper.CheckStorageAnalytics(account, currentConfig))
                            {
                                accountResult.PartialResults.Add(new AEMTestResult("Storage Metrics configuration check for {0}", false, account));
                                _Helper.WriteHost("NOT OK ", ConsoleColor.Red);

                            }
                            else
                            {
                                accountResult.PartialResults.Add(new AEMTestResult("Storage Metrics configuration check for {0}", true, account));
                                _Helper.WriteHost("OK ", ConsoleColor.Green);
                                storageConfigOk = true;
                            }

                            _Helper.WriteHost("\t\tStorage Metrics data check for {0}...", false, account);
                            var filterMinute = WindowsAzure.Storage.Table.TableQuery.
                                GenerateFilterConditionForDate("Timestamp", "gt", DateTime.Now.AddMinutes(AEMExtensionConstants.ContentAgeInMinutes * -1));

                            if (storageConfigOk && _Helper.CheckTableAndContent(account, "$MetricsMinutePrimaryTransactionsBlob", filterMinute, ".", false, WaitTimeInMinutes))

                            {
                                _Helper.WriteHost("OK ", ConsoleColor.Green);
                                accountResult.PartialResults.Add(new AEMTestResult("Storage Metrics data check for {0}", true, account));
                            }
                            else
                            {
                                accountResult.PartialResults.Add(new AEMTestResult("Storage Metrics data check for {0}", false, account));
                                _Helper.WriteHost("NOT OK ", ConsoleColor.Red);
                            }
                        }
                        else
                        {
                            accountResult.PartialResults.Add(new AEMTestResult("Storage Metrics not available for Premium Storage account {0}", true, account));
                            _Helper.WriteHost("\t\tStorage Metrics not available for Premium Storage account {0}...", false, account);
                            _Helper.WriteHost("OK ", ConsoleColor.Green);
                        }
                    }
                    if (accounts.Count == 0)
                    {
                        metricsResult.Result = true;
                    }
                }
                else
                {
                    metricsResult.Result = true;
                    _Helper.WriteHost("Skipped ", ConsoleColor.Yellow);
                }
                //################################################# 
                //#################################################    


                //#################################################
                //# Check Azure Enhanced Monitoring Extension for SAP Configuration
                //#################################################
                _Helper.WriteHost("Azure Enhanced Monitoring Extension for SAP public configuration check...", false);
                var aemConfigResult = new AEMTestResult("Azure Enhanced Monitoring Extension for SAP public configuration check");
                rootResult.PartialResults.Add(aemConfigResult);

                JObject sapmonPublicConfig = null;
                if (monExtension != null)
                {
                    _Helper.WriteHost(""); //New Line

                    sapmonPublicConfig = JsonConvert.DeserializeObject(monPublicConfig) as JObject;

                    StorageAccount storage = null;
                    var osaccountIsPremium = false;
                    if (!String.IsNullOrEmpty(osaccountName))
                    {
                        storage = _Helper.GetStorageAccountFromCache(osaccountName);
                        osaccountIsPremium = _Helper.IsPremiumStorageAccount(osaccountName);
                    }

                    var vmSize = selectedVM.HardwareProfile.VmSize;
                    _Helper.CheckMonitoringProperty("Azure Enhanced Monitoring Extension for SAP public configuration check: VM Size", "vmsize", sapmonPublicConfig, vmSize, aemConfigResult);
                    _Helper.CheckMonitoringProperty("Azure Enhanced Monitoring Extension for SAP public configuration check: VM Memory", "vm.memory.isovercommitted", sapmonPublicConfig, 0, aemConfigResult);
                    _Helper.CheckMonitoringProperty("Azure Enhanced Monitoring Extension for SAP public configuration check: VM CPU", "vm.cpu.isovercommitted", sapmonPublicConfig, 0, aemConfigResult);
                    _Helper.MonitoringPropertyExists("Azure Enhanced Monitoring Extension for SAP public configuration check: Script Version", "script.version", sapmonPublicConfig, aemConfigResult);

                    var vmSLA = _Helper.GetVMSLA(selectedVM);
                    if (vmSLA.HasSLA)
                    {
                        _Helper.CheckMonitoringProperty("Azure Enhanced Monitoring Extension for SAP public configuration check: VM SLA IOPS", "vm.sla.iops", sapmonPublicConfig, vmSLA.IOPS, aemConfigResult);
                        _Helper.CheckMonitoringProperty("Azure Enhanced Monitoring Extension for SAP public configuration check: VM SLA Throughput", "vm.sla.throughput", sapmonPublicConfig, vmSLA.TP, aemConfigResult);
                    }

                    int wadEnabled;
                    if (_Helper.GetMonPropertyValue("wad.isenabled", sapmonPublicConfig, out wadEnabled))
                    {
                        if (wadEnabled == 1)
                        {
                            _Helper.MonitoringPropertyExists("Azure Enhanced Monitoring Extension for SAP public configuration check: WAD name", "wad.name", sapmonPublicConfig, aemConfigResult);
                            _Helper.MonitoringPropertyExists("Azure Enhanced Monitoring Extension for SAP public configuration check: WAD URI", "wad.uri", sapmonPublicConfig, aemConfigResult);
                        }
                        else
                        {
                            _Helper.MonitoringPropertyExists("Azure Enhanced Monitoring Extension for SAP public configuration check: WAD name", "wad.name", sapmonPublicConfig, aemConfigResult, false);
                            _Helper.MonitoringPropertyExists("Azure Enhanced Monitoring Extension for SAP public configuration check: WAD URI", "wad.uri", sapmonPublicConfig, aemConfigResult, false);
                        }
                    }
                    else
                    {
                        string message = "Azure Enhanced Monitoring Extension for SAP public configuration check:";
                        aemConfigResult.PartialResults.Add(new AEMTestResult(message, false));
                        _Helper.WriteHost(message + "...", false);
                        _Helper.WriteHost("NOT OK ", ConsoleColor.Red);
                    }

                    if (!osaccountIsPremium && storage != null)
                    {
                        var endpoint = _Helper.GetAzureSAPTableEndpoint(storage);
                        var minuteUri = endpoint + "$MetricsMinutePrimaryTransactionsBlob";

                        _Helper.CheckMonitoringProperty("Azure Enhanced Monitoring Extension for SAP public configuration check: VM OS disk URI Key", "osdisk.connminute", sapmonPublicConfig, osaccountName + ".minute", aemConfigResult);
                        //# TODO: check uri config
                        _Helper.CheckMonitoringProperty("Azure Enhanced Monitoring Extension for SAP public configuration check: VM OS disk URI Value", osaccountName + ".minute.uri", sapmonPublicConfig, minuteUri, aemConfigResult);
                        _Helper.CheckMonitoringProperty("Azure Enhanced Monitoring Extension for SAP public configuration check: VM OS disk URI Name", osaccountName + ".minute.name", sapmonPublicConfig, osaccountName, aemConfigResult);
                        _Helper.CheckMonitoringProperty("Azure Enhanced Monitoring Extension for SAP public configuration check: VM OS Disk Type", "osdisk.type", sapmonPublicConfig, AEMExtensionConstants.DISK_TYPE_STANDARD, aemConfigResult);

                    }
                    else if (storage != null)
                    {
                        var sla = _Helper.GetDiskSLA(osdisk);

                        _Helper.CheckMonitoringProperty("Azure Enhanced Monitoring Extension for SAP public configuration check: VM OS Disk Type", "osdisk.type", sapmonPublicConfig, AEMExtensionConstants.DISK_TYPE_PREMIUM, aemConfigResult);
                        _Helper.CheckMonitoringProperty("Azure Enhanced Monitoring Extension for SAP public configuration check: VM OS Disk SLA IOPS", "osdisk.sla.throughput", sapmonPublicConfig, sla.TP, aemConfigResult);
                        _Helper.CheckMonitoringProperty("Azure Enhanced Monitoring Extension for SAP public configuration check: VM OS Disk SLA Throughput", "osdisk.sla.iops", sapmonPublicConfig, sla.IOPS, aemConfigResult);

                    }
                    else
                    {
                        var osDiskMD = ComputeClient.ComputeManagementClient.Disks.Get(_Helper.GetResourceGroupFromId(osdisk.ManagedDisk.Id),
                            _Helper.GetResourceNameFromId(osdisk.ManagedDisk.Id));
                        if (osDiskMD.Sku.Name == StorageAccountTypes.PremiumLRS)
                        {
                            var sla = _Helper.GetDiskSLA(osDiskMD.DiskSizeGB, null);

                            _Helper.CheckMonitoringProperty("Azure Enhanced Monitoring Extension for SAP public configuration check: VM OS Disk Type", "osdisk.type", sapmonPublicConfig, AEMExtensionConstants.DISK_TYPE_PREMIUM_MD, aemConfigResult);
                            _Helper.CheckMonitoringProperty("Azure Enhanced Monitoring Extension for SAP public configuration check: VM OS Disk SLA IOPS", "osdisk.sla.throughput", sapmonPublicConfig, sla.TP, aemConfigResult);
                            _Helper.CheckMonitoringProperty("Azure Enhanced Monitoring Extension for SAP public configuration check: VM OS Disk SLA Throughput", "osdisk.sla.iops", sapmonPublicConfig, sla.IOPS, aemConfigResult);
                        }
                        else
                        {
                            _Helper.WriteWarning("[WARN] Standard Managed Disks are not supported.");
                        }
                    }

                    if (osdisk.ManagedDisk == null)
                    {
                        _Helper.CheckMonitoringProperty("Azure Enhanced Monitoring Extension for SAP public configuration check: VM OS disk name", "osdisk.name", sapmonPublicConfig, _Helper.GetDiskName(osdisk.Vhd.Uri), aemConfigResult);
                    }


                    var diskNumber = 1;
                    foreach (var disk in dataDisks)
                    {
                        if (disk.ManagedDisk != null)
                        {
                            var diskMD = ComputeClient.ComputeManagementClient.Disks.Get(_Helper.GetResourceGroupFromId(disk.ManagedDisk.Id),
                                _Helper.GetResourceNameFromId(disk.ManagedDisk.Id));

                            if (diskMD.Sku.Name == StorageAccountTypes.PremiumLRS)
                            {
                                var sla = _Helper.GetDiskSLA(diskMD.DiskSizeGB, null);

                                _Helper.CheckMonitoringProperty("Azure Enhanced Monitoring Extension for SAP public configuration check: VM Data Disk " + diskNumber + " Type", "disk.type." + diskNumber, sapmonPublicConfig, AEMExtensionConstants.DISK_TYPE_PREMIUM_MD, aemConfigResult);
                                _Helper.CheckMonitoringProperty("Azure Enhanced Monitoring Extension for SAP public configuration check: VM Data Disk " + diskNumber + " SLA IOPS", "disk.sla.throughput." + diskNumber, sapmonPublicConfig, sla.TP, aemConfigResult);
                                _Helper.CheckMonitoringProperty("Azure Enhanced Monitoring Extension for SAP public configuration check: VM Data Disk " + diskNumber + " SLA Throughput", "disk.sla.iops." + diskNumber, sapmonPublicConfig, sla.IOPS, aemConfigResult);
                            }
                            else
                            {
                                _Helper.WriteWarning("[WARN] Standard Managed Disks are not supported.");

                            }
                        }
                        else
                        {

                            var accountName = _Helper.GetStorageAccountFromUri(disk.Vhd.Uri);
                            storage = _Helper.GetStorageAccountFromCache(accountName);
                            var accountIsPremium = _Helper.IsPremiumStorageAccount(storage);

                            _Helper.CheckMonitoringProperty("Azure Enhanced Monitoring Extension for SAP public configuration check: VM Data Disk " + diskNumber + " LUN", "disk.lun." + diskNumber, sapmonPublicConfig, disk.Lun, aemConfigResult);
                            if (!accountIsPremium)
                            {
                                var endpoint = _Helper.GetAzureSAPTableEndpoint(storage);
                                var minuteUri = endpoint + "$MetricsMinutePrimaryTransactionsBlob";

                                _Helper.CheckMonitoringProperty("Azure Enhanced Monitoring Extension for SAP public configuration check: VM Data Disk " + diskNumber + " URI Key", "disk.connminute." + diskNumber, sapmonPublicConfig, accountName + ".minute", aemConfigResult);
                                _Helper.CheckMonitoringProperty("Azure Enhanced Monitoring Extension for SAP public configuration check: VM Data Disk " + diskNumber + " URI Value", accountName + ".minute.uri", sapmonPublicConfig, minuteUri, aemConfigResult);
                                _Helper.CheckMonitoringProperty("Azure Enhanced Monitoring Extension for SAP public configuration check: VM Data Disk " + diskNumber + " URI Name", accountName + ".minute.name", sapmonPublicConfig, accountName, aemConfigResult);
                                _Helper.CheckMonitoringProperty("Azure Enhanced Monitoring Extension for SAP public configuration check: VM Data Disk " + diskNumber + " Type", "disk.type." + diskNumber, sapmonPublicConfig, AEMExtensionConstants.DISK_TYPE_STANDARD, aemConfigResult);

                            }
                            else
                            {
                                var sla = _Helper.GetDiskSLA(disk);

                                _Helper.CheckMonitoringProperty("Azure Enhanced Monitoring Extension for SAP public configuration check: VM Data Disk " + diskNumber + " Type", "disk.type." + diskNumber, sapmonPublicConfig, AEMExtensionConstants.DISK_TYPE_PREMIUM, aemConfigResult);
                                _Helper.CheckMonitoringProperty("Azure Enhanced Monitoring Extension for SAP public configuration check: VM Data Disk " + diskNumber + " SLA IOPS", "disk.sla.throughput." + diskNumber, sapmonPublicConfig, sla.TP, aemConfigResult);
                                _Helper.CheckMonitoringProperty("Azure Enhanced Monitoring Extension for SAP public configuration check: VM Data Disk " + diskNumber + " SLA Throughput", "disk.sla.iops." + diskNumber, sapmonPublicConfig, sla.IOPS, aemConfigResult);
                            }

                            _Helper.CheckMonitoringProperty("Azure Enhanced Monitoring Extension for SAP public configuration check: VM Data Disk " + diskNumber + " name", "disk.name." + diskNumber, sapmonPublicConfig, _Helper.GetDiskName(disk.Vhd.Uri), aemConfigResult);
                        }

                        diskNumber += 1;
                    }
                    if (dataDisks.Count == 0)
                    {
                        aemConfigResult.PartialResults.Add(new AEMTestResult("Azure Enhanced Monitoring Extension for SAP public configuration check: VM Data Disks", true));
                        _Helper.WriteHost("\tAzure Enhanced Monitoring Extension for SAP public configuration check: VM Data Disks ", false);
                        _Helper.WriteHost("OK ", ConsoleColor.Green);
                    }
                }
                else
                {
                    aemConfigResult.Result = false;
                    _Helper.WriteHost("NOT OK ", ConsoleColor.Red);
                }
                //################################################# 
                //#################################################    


                //#################################################
                //# Check WAD Configuration
                //#################################################
                int iswadEnabled;
                if (_Helper.GetMonPropertyValue("wad.isenabled", sapmonPublicConfig, out iswadEnabled) && iswadEnabled == 1)
                {
                    var wadConfigResult = new AEMTestResult("IaaSDiagnostics check");
                    rootResult.PartialResults.Add(wadConfigResult);

                    string wadPublicConfig = null;
                    var wadExtension = _Helper.GetExtension(selectedVM, AEMExtensionConstants.WADExtensionType[OSType], AEMExtensionConstants.WADExtensionPublisher[OSType]);
                    if (wadExtension != null)
                    {
                        wadPublicConfig = wadExtension.Settings.ToString();
                    }

                    _Helper.WriteHost("IaaSDiagnostics check...", false);
                    if (wadExtension != null)
                    {
                        _Helper.WriteHost(""); //New Line
                        _Helper.WriteHost("\tIaaSDiagnostics configuration check...", false);

                        var currentJSONConfig = JsonConvert.DeserializeObject(wadPublicConfig) as JObject;
                        var base64 = currentJSONConfig["xmlCfg"] as JValue;
                        System.Xml.XmlDocument currentConfig = new System.Xml.XmlDocument();
                        currentConfig.LoadXml(Encoding.UTF8.GetString(Convert.FromBase64String(base64.Value.ToString())));


                        if (!_Helper.CheckWADConfiguration(currentConfig))
                        {
                            wadConfigResult.PartialResults.Add(new AEMTestResult("IaaSDiagnostics configuration check", false));
                            _Helper.WriteHost("NOT OK ", ConsoleColor.Red);
                        }
                        else
                        {
                            wadConfigResult.PartialResults.Add(new AEMTestResult("IaaSDiagnostics configuration check", true));
                            _Helper.WriteHost("OK ", ConsoleColor.Green);
                        }

                        _Helper.WriteHost("\tIaaSDiagnostics performance counters check...");
                        var wadPerfCountersResult = new AEMTestResult("IaaSDiagnostics performance counters check");
                        wadConfigResult.PartialResults.Add(wadPerfCountersResult);

                        foreach (var perfCounter in AEMExtensionConstants.PerformanceCounters[OSType])
                        {
                            _Helper.WriteHost("\t\tIaaSDiagnostics performance counters " + perfCounter.counterSpecifier + "check...", false);
                            var currentCounter = currentConfig.SelectSingleNode("/WadCfg/DiagnosticMonitorConfiguration/PerformanceCounters/PerformanceCounterConfiguration[@counterSpecifier = '" + perfCounter.counterSpecifier + "']");
                            if (currentCounter != null)
                            {
                                wadPerfCountersResult.PartialResults.Add(new AEMTestResult("IaaSDiagnostics performance counters " + perfCounter.counterSpecifier + "check...", true));
                                _Helper.WriteHost("OK ", ConsoleColor.Green);
                            }
                            else
                            {
                                wadPerfCountersResult.PartialResults.Add(new AEMTestResult("IaaSDiagnostics performance counters " + perfCounter.counterSpecifier + "check...", false));
                                _Helper.WriteHost("NOT OK ", ConsoleColor.Red);
                            }
                        }

                        string wadstorage;
                        if (!_Helper.GetMonPropertyValue<string>("wad.name", sapmonPublicConfig, out wadstorage))
                        {
                            wadstorage = null;
                        }

                        _Helper.WriteHost("\tIaaSDiagnostics data check...", false);

                        var deploymentId = String.Empty;
                        var roleName = String.Empty;

                        var extStatuses = _Helper.GetExtension(selectedVM, selectedVMStatus, AEMExtensionConstants.AEMExtensionType[OSType], AEMExtensionConstants.AEMExtensionPublisher[OSType]);
                        InstanceViewStatus aemStatus = null;
                        if (extStatuses != null && extStatuses.Statuses != null)
                        {
                            aemStatus = extStatuses.Statuses.FirstOrDefault(stat => Regex.Match(stat.Message, "deploymentId=(\\S*) roleInstance=(\\S*)").Success);
                        }

                        if (aemStatus != null)
                        {
                            var match = Regex.Match(aemStatus.Message, "deploymentId=(\\S*) roleInstance=(\\S*)");
                            deploymentId = match.Groups[1].Value;
                            roleName = match.Groups[2].Value;
                        }
                        else
                        {
                            _Helper.WriteWarning("DeploymentId and RoleInstanceName could not be parsed from extension status");
                        }


                        var ok = false;
                        if (!SkipStorageCheck.IsPresent && !String.IsNullOrEmpty(deploymentId) && !String.IsNullOrEmpty(roleName) && !String.IsNullOrEmpty(wadstorage))
                        {

                            if (OSType.Equals(AEMExtensionConstants.OSTypeLinux, StringComparison.InvariantCultureIgnoreCase))
                            {
                                ok = _Helper.CheckDiagnosticsTable(wadstorage, deploymentId,
                                    selectedVM.OsProfile.ComputerName, ".", OSType, WaitTimeInMinutes);
                            }
                            else
                            {
                                string filterMinute = "Role eq '" + AEMExtensionConstants.ROLECONTENT + "' and DeploymentId eq '"
                                    + deploymentId + "' and RoleInstance eq '" + roleName + "' and PartitionKey gt '0"
                                    + DateTime.UtcNow.AddMinutes(AEMExtensionConstants.ContentAgeInMinutes * -1).Ticks + "'";
                                ok = _Helper.CheckTableAndContent(wadstorage, AEMExtensionConstants.WadTableName,
                                    filterMinute, ".", false, WaitTimeInMinutes);
                            }


                        }
                        if (ok && !SkipStorageCheck.IsPresent)
                        {
                            wadConfigResult.PartialResults.Add(new AEMTestResult("IaaSDiagnostics data check", true));
                            _Helper.WriteHost("OK ", ConsoleColor.Green);
                        }
                        else if (!SkipStorageCheck.IsPresent)
                        {
                            wadConfigResult.PartialResults.Add(new AEMTestResult("IaaSDiagnostics data check", false));
                            _Helper.WriteHost("NOT OK ", ConsoleColor.Red);
                        }
                        else
                        {
                            _Helper.WriteHost("Skipped ", ConsoleColor.Yellow);
                        }
                    }
                    else
                    {
                        wadConfigResult.Result = false;
                        _Helper.WriteHost("NOT OK ", ConsoleColor.Red);
                    }
                }
                //################################################# 
                //#################################################

                if (!rootResult.Result)
                {
                    _Helper.WriteHost("The script found some configuration issues. Please run the Set-AzureRmVMExtension commandlet to update the configuration of the virtual machine!");
                }

                _Helper.WriteVerbose("TestAzureRmVMAEMExtension Done (" + rootResult.Result + ")");

                var result = ComputeAutoMapperProfile.Mapper.Map<AEMTestResult>(rootResult);
                WriteObject(result);
            });
        }
    }
}
