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

using Microsoft.Azure.Management.Internal.Resources.Models;
using System.Text;

namespace Microsoft.Azure.Commands.ServiceFabric.Models
{
    public class PSDeploymentExtended : DeploymentExtended
    {
        public PSDeploymentExtended(DeploymentExtended deploymentExtended) :
            base(deploymentExtended.Name, deploymentExtended.Id, deploymentExtended.Properties)
        { 
        }


        public override string ToString()
        {
            const string spaces = "";
            var sb = new StringBuilder();
            sb.AppendLine(string.Format("{0}{1} : {2}", spaces, "Name",Name));
            sb.AppendLine(string.Format("{0}{1} : {2}", spaces, "Id", Id));
            sb.AppendLine(string.Format("{0}{1} : {2}", spaces, "CorrelationId", Properties.CorrelationId));
            sb.AppendLine(string.Format("{0}{1} : {2}", spaces, "Mode", Properties.Mode));
            sb.AppendLine(string.Format("{0}{1} : {2}", spaces, "ProvisioningState", Properties.ProvisioningState));
            sb.AppendLine(string.Format("{0}{1} : {2}", spaces, "Timestamp", Properties.Timestamp));
            return sb.ToString();

        }
    }
}
