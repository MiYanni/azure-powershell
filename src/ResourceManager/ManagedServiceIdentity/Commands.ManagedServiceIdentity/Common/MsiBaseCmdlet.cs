using System;
using System.Collections.Generic;
using Microsoft.Azure.Commands.Common.Authentication;
using Microsoft.Azure.Commands.Common.Authentication.Abstractions;
using Microsoft.Azure.Commands.ManagedServiceIdentity.Models;
using Microsoft.Azure.Commands.ResourceManager.Common;
using Microsoft.Azure.Management.Internal.Resources;
using Microsoft.Azure.Management.ManagedServiceIdentity;
using Microsoft.Azure.Management.ManagedServiceIdentity.Models;
using Microsoft.WindowsAzure.Commands.Utilities.Common;

namespace Microsoft.Azure.Commands.ManagedServiceIdentity.Common
{
    public class MsiBaseCmdlet : AzureRMCmdlet
    {
        private ResourceManagementClient _armClient;
        private ManagedServiceIdentityClient _msiClient;

        public ManagedServiceIdentityClient MsiClient
        {
            get
            {
                return _msiClient ??
                       (_msiClient =
                           AzureSession.Instance.ClientFactory.CreateArmClient<ManagedServiceIdentityClient>(
                               DefaultContext,
                               AzureEnvironment.Endpoint.ResourceManager));
            }
            set
            {
                _msiClient = value;
            }
        }

        public ResourceManagementClient ArmClient
        {
            get
            {
                return _armClient ??
                       (_armClient = AzureSession.Instance.ClientFactory.CreateArmClient<ResourceManagementClient>(
                           DefaultContext,
                           AzureEnvironment.Endpoint.ResourceManager));
            }
            set
            {
                _armClient = value;
            }
        }

        protected void WriteIdentity(Identity identity)
        {
            WriteObject(new PsUserAssignedIdentity(identity));
        }

        protected void WriteIdentityList(IEnumerable<Identity> identities)
        {
            List<PsUserAssignedIdentity> output = new List<PsUserAssignedIdentity>();
            identities.ForEach(identity => output.Add(new PsUserAssignedIdentity(identity)));
            WriteObject(output, true);
        }
    }
}
