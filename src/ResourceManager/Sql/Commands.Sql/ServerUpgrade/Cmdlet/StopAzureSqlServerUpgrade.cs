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
using Microsoft.Azure.Commands.Sql.ServerUpgrade.Model;
using System.Collections.Generic;
using System.Globalization;
using System.Management.Automation;
using Microsoft.WindowsAzure.Commands.Common.CustomAttributes;

namespace Microsoft.Azure.Commands.Sql.ServerUpgrade.Cmdlet
{
    /// <summary>
    /// Defines the Get-AzureRmSqlDatabaseServer cmdlet
    /// </summary>
    [CmdletDeprecation(ChangeDescription = "All Azure SQL DB Servers now have version 12.0 so there is nothing to upgrade.")]
    [Cmdlet(VerbsLifecycle.Stop, "AzureRmSqlServerUpgrade", SupportsShouldProcess = true)]
    public class StopAzureSqlServerUpgrade : AzureSqlServerUpgradeCmdletBase<AzureSqlServerUpgradeModel>
    {
        /// <summary>
        /// Defines whether it is ok to skip the requesting of rule removal confirmation
        /// </summary>
        [Parameter(HelpMessage = "Skip confirmation message for performing the action")]
        public SwitchParameter Force { get; set; }

        /// <summary>
        /// Gets the entity to delete
        /// </summary>
        /// <returns>The entity going to be deleted</returns>
        protected override IEnumerable<AzureSqlServerUpgradeModel> GetEntity()
        {
            return new List<AzureSqlServerUpgradeModel>
            {
                ModelAdapter.GetUpgrade(ResourceGroupName, ServerName)
            };
        }

        /// <summary>
        /// Apply user input.  Here nothing to apply
        /// </summary>
        /// <param name="model">The result of GetEntity</param>
        /// <returns>The input model</returns>
        protected override IEnumerable<AzureSqlServerUpgradeModel> ApplyUserInputToModel(IEnumerable<AzureSqlServerUpgradeModel> model)
        {
            return model;
        }

        /// <summary>
        /// Stop the server upgrade.
        /// </summary>
        /// <param name="entity">The server to cancel upgrade</param>
        /// <returns>The server that was deleted</returns>
        protected override IEnumerable<AzureSqlServerUpgradeModel> PersistChanges(IEnumerable<AzureSqlServerUpgradeModel> entity)
        {
            if (!Force.IsPresent && !ShouldProcess(
                string.Format(CultureInfo.InvariantCulture, Properties.Resources.StopAzureSqlServerUpgradeDescription, ServerName),
                string.Format(CultureInfo.InvariantCulture, Properties.Resources.StopAzureSqlServerUpgradeWarning, ServerName),
                Properties.Resources.ShouldProcessCaption))
            {
                return null;
            }

            ModelAdapter.Cancel(ResourceGroupName, ServerName);

            return null;
        }

        /// <summary>
        /// Returns false so that the model is not written out
        /// </summary>
        /// <returns>Always false</returns>
        protected override bool WriteResult()
        {
            return false;
        }
    }
}
