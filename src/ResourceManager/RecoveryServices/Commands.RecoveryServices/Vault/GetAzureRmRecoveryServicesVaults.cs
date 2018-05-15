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
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Microsoft.Azure.Management.RecoveryServices.Models;
using Microsoft.Azure.Commands.ResourceManager.Common.ArgumentCompleters;

namespace Microsoft.Azure.Commands.RecoveryServices
{
    /// <summary>
    /// Retrieves Azure Recovery Services Vault.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureRmRecoveryServicesVault")]
    [OutputType(typeof(List<ARSVault>))]
    public class GetAzureRmRecoveryServicesVaults : RecoveryServicesCmdletBase
    {
        #region Parameters
        /// <summary>
        /// Gets or sets Resource Group name.
        /// </summary>
        [Parameter]
        [ResourceGroupCompleter]
        public string ResourceGroupName { get; set; }

        /// <summary>
        /// Gets or sets Resource Name.
        /// </summary>
        [Parameter]
        public string Name { get; set; }
        #endregion Parameters

        /// <summary>
        /// ProcessRecord of the command.
        /// </summary>
        public override void ExecuteCmdlet()
        {
            try
            {
                if (string.IsNullOrEmpty(ResourceGroupName))
                {
                    GetVaultsUnderAllResourceGroups();
                }
                else
                {
                    GetVaultsUnderResourceGroup(ResourceGroupName);
                }
            }
            catch (Exception exception)
            {
                HandleException(exception);
            }
        }

        /// <summary>
        /// Get vaults under a resouce group.
        /// </summary>
        private void GetVaultsUnderResourceGroup(string resourceGroupName)
        {
            List<Vault> vaultListResponse =
                RecoveryServicesClient.GetVaultsInResouceGroup(resourceGroupName);

            WriteVaults(vaultListResponse);
        }

        /// <summary>
        /// Get vaults under all resouce group.
        /// </summary>
        private void GetVaultsUnderAllResourceGroups()
        {
            
            foreach (var resourceGroup in RecoveryServicesClient.GetResouceGroups())
            {
                try
                {
                    GetVaultsUnderResourceGroup(resourceGroup.Name);
                }
                catch (Exception ex)
                {
                    WriteDebug("GetVaultsUnderResourceGroup failed: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Write Vaults.
        /// </summary>
        /// <param name="vaults">List of Vaults</param>
        private void WriteVaults(IList<Vault> vaults)
        {
            if (string.IsNullOrEmpty(Name))
            {
                WriteObject(vaults.Select(v => new ARSVault(v)), true);
            }
            else
            {
                foreach (Vault vault in vaults)
                {
                    if (0 == string.Compare(Name, vault.Name, true))
                    {
                        WriteObject(new ARSVault(vault));
                    }
                }
            }
        }
    }
}
