using System.Linq;
using System.Management.Automation;
using Microsoft.Azure.Commands.ManagedServiceIdentity.Common;
using Microsoft.Azure.Commands.ManagedServiceIdentity.Models;
using Microsoft.Azure.Commands.ResourceManager.Common.ArgumentCompleters;

namespace Microsoft.Azure.Commands.ManagedServiceIdentity.UserAssignedIdentities
{
    [Cmdlet(VerbsCommon.Get, "AzureRmUserAssignedIdentity", DefaultParameterSetName = Constants.SubscriptionParameterSet
        )]
    [OutputType(typeof (PsUserAssignedIdentity))]
    public class GetAzureRmUserAssignedIdentityCmdlet : MsiBaseCmdlet
    {
        [Parameter(
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The resource group name.",
            ParameterSetName = Constants.ResourceGroupParameterSet)]
        [ResourceGroupCompleter]
        [ValidateNotNullOrEmpty]
        public string ResourceGroupName { get; set; }

        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The Identity name.",
            ParameterSetName = Constants.ResourceGroupParameterSet)]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        public override void ExecuteCmdlet()
        {
            base.ExecuteCmdlet();

            if (ParameterSetName.Equals(Constants.SubscriptionParameterSet))
            {
                var result =
                    MsiClient.UserAssignedIdentities
                        .ListBySubscriptionWithHttpMessagesAsync().GetAwaiter().GetResult();
                var resultList = result.Body.ToList();
                var nextPageLink = result.Body.NextPageLink;
                while (!string.IsNullOrEmpty(nextPageLink))
                {
                    var pageResult =
                        MsiClient.UserAssignedIdentities
                            .ListBySubscriptionNextWithHttpMessagesAsync(nextPageLink).GetAwaiter().GetResult();
                    resultList.AddRange(pageResult.Body.ToList());
                    nextPageLink = pageResult.Body.NextPageLink;
                }

                WriteIdentityList(resultList);
            }
            else if (ParameterSetName.Equals(Constants.ResourceGroupParameterSet))
            {
                if (string.IsNullOrEmpty(Name))
                {
                    var result =
                        MsiClient.UserAssignedIdentities
                            .ListByResourceGroupWithHttpMessagesAsync(ResourceGroupName).GetAwaiter().GetResult();
                    var resultList = result.Body.ToList();
                    var nextPageLink = result.Body.NextPageLink;
                    while (!string.IsNullOrEmpty(nextPageLink))
                    {
                        var pageResult = MsiClient.UserAssignedIdentities
                            .ListByResourceGroupNextWithHttpMessagesAsync(nextPageLink).GetAwaiter().GetResult();
                        resultList.AddRange(pageResult.Body.ToList());
                        nextPageLink = pageResult.Body.NextPageLink;
                    }

                    WriteIdentityList(resultList);
                }
                else
                {
                    var result =
                        MsiClient.UserAssignedIdentities.GetWithHttpMessagesAsync(
                            ResourceGroupName,
                            Name).GetAwaiter().GetResult();
                    WriteIdentity(result.Body);
                }
            }
        }
    }
}
