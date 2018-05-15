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

namespace Microsoft.Azure.Commands.LogicApp.Utilities
{
    using ResourceManager.Common;

    public abstract class LogicAppBaseCmdlet : AzureRMCmdlet
    {
        /// <summary>
        /// IntegrationAccount client
        /// </summary>
        private IntegrationAccountClient _integartionAccountClient = null;

        /// <summary>
        /// Websites client
        /// </summary>
        private WebsitesClient _websitesClient = null;

        /// <summary>
        /// Gets or sets the IntegrationAccount client used in the PowerShell commands.
        /// </summary>
        public IntegrationAccountClient IntegrationAccountClient
        {
            get
            {
                _integartionAccountClient = new IntegrationAccountClient(DefaultProfile.DefaultContext);

                _integartionAccountClient.VerboseLogger = WriteVerboseWithTimestamp;
                _integartionAccountClient.ErrorLogger = WriteErrorWithTimestamp;
                                
                return _integartionAccountClient;
            }

            set
            {
                _integartionAccountClient = value;                 
            }
        }

        /// <summary>
        /// Gets or sets the WebsitesClient client used in the PowerShell commands.
        /// </summary>
        public WebsitesClient WebsitesClient
        {
            get
            {
                _websitesClient = new WebsitesClient(DefaultProfile.DefaultContext)
                {
                    VerboseLogger = WriteVerboseWithTimestamp,
                    ErrorLogger = WriteErrorWithTimestamp
                };

                return _websitesClient;
            }

            set
            {
                _websitesClient = value;
            }
        }

        /// <summary>
        /// LogicApp client
        /// </summary>
        private LogicAppClient _logicAppClient = null;

        /// <summary>
        /// Gets or sets the LogicApp client used in the PowerShell commands.
        /// </summary>
        public LogicAppClient LogicAppClient
        {
            get
            {
                _logicAppClient = new LogicAppClient(DefaultProfile.DefaultContext)
                {
                    VerboseLogger = WriteVerboseWithTimestamp,
                    ErrorLogger = WriteErrorWithTimestamp
                };

                return _logicAppClient;
            }

            set
            {
                _logicAppClient = value;
            }
        }

    }
}