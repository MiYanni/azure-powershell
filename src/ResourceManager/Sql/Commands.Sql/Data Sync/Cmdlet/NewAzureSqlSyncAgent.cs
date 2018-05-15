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

using System.Linq;
using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.Azure.Commands.Sql.DataSync.Model;
using Hyak.Common;

namespace Microsoft.Azure.Commands.Sql.DataSync.Cmdlet
{
    /// <summary>
    /// Cmdlet to create a new sync agent
    /// </summary>
    [Cmdlet(VerbsCommon.New, "AzureRmSqlSyncAgent", SupportsShouldProcess = true,
        DefaultParameterSetName = SyncDatabaseComponentSet,
        ConfirmImpact = ConfirmImpact.Medium), OutputType(typeof(AzureSqlSyncAgentModel))]
    public class NewAzureSqlSyncAgent : AzureSqlSyncAgentCmdletBase
    {
        /// <summary>
        /// Parameter set name for sync database resource ID
        /// </summary>
        private const string SyncDatabaseResourceIDSet = "SyncDatabaseResourceID";

        /// <summary>
        /// Parameter set name for sync database component including resource group name, server name, database name
        /// </summary>
        private const string SyncDatabaseComponentSet = "SyncDatabaseComponent";

        /// <summary>
        /// Gets or sets the sync agent name
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true,
            Position = 2,
            HelpMessage = "The sync agent name.")]
        [Alias("SyncAgentName")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the resource ID of the sync metadata database
        /// </summary>
        [Parameter(Mandatory = true,
           ParameterSetName = SyncDatabaseResourceIDSet,
           HelpMessage = "The resource ID of  the sync metadata database.")]
        [ValidateNotNullOrEmpty]
        public string SyncDatabaseResourceID { get; set; }

        /// <summary>
        /// Gets or sets the name of the database used to store sync related metadata
        /// </summary>
        [Parameter(Mandatory = true,
           ParameterSetName = SyncDatabaseComponentSet,
           HelpMessage = "The database used to store sync related metadata.")]
        [ValidateNotNullOrEmpty]
        public string SyncDatabaseName { get; set; }

        /// <summary>
        /// Gets or sets the name of the server on which the sync metadata database is hosted
        /// </summary>
        [Parameter(Mandatory = false,
           ParameterSetName = SyncDatabaseComponentSet,
           HelpMessage = "The server on which the sync metadata database is hosted.")]
        [ValidateNotNullOrEmpty]
        public string SyncDatabaseServerName { get; set; }

        /// <summary>
        /// Gets or sets the name of the resource group the sync metadata database belongs to
        /// </summary>
        [Parameter(Mandatory = false,
           ParameterSetName = SyncDatabaseComponentSet,
           HelpMessage = "The resource group the sync metadata database belongs to.")]
        [ValidateNotNullOrEmpty]
        public string SyncDatabaseResourceGroupName { get; set; }

        /// <summary>
        /// The id of database used to store sync related metadata
        /// </summary>
        private string syncDatabaseId = null;

        /// <summary>
        /// Get the entities from the service
        /// </summary>
        /// <returns>The list of entities</returns>
        protected override IEnumerable<AzureSqlSyncAgentModel> GetEntity()
        {
            // We try to get the sync agent.  Since this is a create, we don't want the sync agent to exist
            try
            {
                ModelAdapter.GetSyncAgent(ResourceGroupName, ServerName, Name);
            }
            catch (CloudException ex)
            {
                if (ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    // This is what we want.  We looked and there is no sync agent with this name.
                    return null;
                }

                // Unexpected exception encountered
                throw;
            }

            // The sync agent already exists
            throw new PSArgumentException(
                string.Format(Properties.Resources.SyncAgentNameExists, Name, ResourceGroupName),
                "SyncAgentName");
        }

        /// <summary>
        /// Create the model from user input
        /// </summary>
        /// <param name="model">Model retrieved from service</param>
        /// <returns>The model that was passed in</returns>
        protected override IEnumerable<AzureSqlSyncAgentModel> ApplyUserInputToModel(IEnumerable<AzureSqlSyncAgentModel> model)
        {
            List<AzureSqlSyncAgentModel> newEntity = new List<AzureSqlSyncAgentModel>();

            AzureSqlSyncAgentModel newModel = new AzureSqlSyncAgentModel
            {
                ResourceGroupName = ResourceGroupName,
                ServerName = ServerName,
                SyncAgentName = Name
            };

            if (ParameterSetName == SyncDatabaseResourceIDSet)
            {
                newModel.SyncDatabaseId = SyncDatabaseResourceID;
            }
            else
            {
                if (!MyInvocation.BoundParameters.ContainsKey("SyncDatabaseResourceGroupName"))
                {
                    SyncDatabaseResourceGroupName = ResourceGroupName;
                }
                if (!MyInvocation.BoundParameters.ContainsKey("SyncDatabaseServerName"))
                {
                    SyncDatabaseServerName = ServerName;
                }
                // "/subscriptions/{id}/" will be added in AzureSqlDataSyncCommunicator
                syncDatabaseId = string.Format("resourceGroups/{0}/providers/Microsoft.Sql/servers/{1}/databases/{2}",
                    SyncDatabaseResourceGroupName, SyncDatabaseServerName, SyncDatabaseName);
            }

            newEntity.Add(newModel);
            return newEntity;
        }

        /// <summary>
        /// Create the new sync agent
        /// </summary>
        /// <param name="entity">The output of apply user input to model</param>
        /// <returns>The input entity</returns>
        protected override IEnumerable<AzureSqlSyncAgentModel> PersistChanges(IEnumerable<AzureSqlSyncAgentModel> entity)
        {
            return new List<AzureSqlSyncAgentModel>
            {
                ModelAdapter.CreateSyncAgent(entity.First(), syncDatabaseId)
            };
        }
    }
}
