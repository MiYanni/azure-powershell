<#
.Synopsis
Lists the configuration stores for a given subscription.
.Description
Lists the configuration stores for a given subscription.
.Link
https://docs.microsoft.com/en-us/powershell/module/az.appconfiguration/get-azappconfigurationstore
#>
function Get-AzAppConfigurationStore {
[OutputType('Microsoft.Azure.PowerShell.Cmdlets.AppConfiguration.Models.Api20190201Preview.IConfigurationStore')]
[CmdletBinding(DefaultParameterSetName='__NoParameters')]
param(
    [Parameter(DontShow)]
    [System.Management.Automation.SwitchParameter]
    # Wait for .NET debugger to attach
    ${Break},

    [Parameter()]
    [Alias('AzureRMContext', 'AzureCredential')]
    [ValidateNotNull()]
    [System.Management.Automation.PSObject]
    # The credentials, account, tenant, and subscription used for communication with Azure.
    ${DefaultProfile},

    [Parameter(DontShow)]
    [ValidateNotNull()]
    [Microsoft.Azure.PowerShell.Cmdlets.AppConfiguration.Runtime.SendAsyncStep[]]
    # SendAsync Pipeline Steps to be appended to the front of the pipeline
    ${HttpPipelineAppend},

    [Parameter(DontShow)]
    [ValidateNotNull()]
    [Microsoft.Azure.PowerShell.Cmdlets.AppConfiguration.Runtime.SendAsyncStep[]]
    # SendAsync Pipeline Steps to be prepended to the front of the pipeline
    ${HttpPipelinePrepend},

    [Parameter(DontShow)]
    [System.Uri]
    # The URI for the proxy server to use
    ${Proxy},

    [Parameter(DontShow)]
    [ValidateNotNull()]
    [System.Management.Automation.PSCredential]
    # Credentials for a proxy server to use for the remote call
    ${ProxyCredential},

    [Parameter(DontShow)]
    [System.Management.Automation.SwitchParameter]
    # Use the default credentials for the proxy
    ${ProxyUseDefaultCredentials},

    [Parameter(ParameterSetName='ResourceGroupNameConfigStoreName', Mandatory)]
    [Parameter(ParameterSetName='SubscriptionIdResourceGroupNameConfigStoreName', Mandatory)]
    [System.String]
    # The name of the configuration store.
    ${Name},

    [Parameter(ParameterSetName='ResourceGroupNameConfigStoreName', Mandatory)]
    [Parameter(ParameterSetName='ResourceGroupResourceGroupName', Mandatory)]
    [Parameter(ParameterSetName='ResourceGroupSubscriptionIdResourceGroupName', Mandatory)]
    [Parameter(ParameterSetName='SubscriptionIdResourceGroupNameConfigStoreName', Mandatory)]
    [System.String]
    # The name of the resource group to which the container registry belongs.
    ${ResourceGroupName},

    [Parameter(ParameterSetName='ResourceGroupSubscriptionIdResourceGroupName', Mandatory)]
    [Parameter(ParameterSetName='SubscriptionId', Mandatory)]
    [Parameter(ParameterSetName='SubscriptionIdResourceGroupNameConfigStoreName', Mandatory)]
    [System.String]
    # The Microsoft Azure subscription ID.
    ${SubscriptionId}
)

begin {
    try {
        $outBuffer = $null
        if ($PSBoundParameters.TryGetValue('OutBuffer', [ref]$outBuffer)) {
            $PSBoundParameters['OutBuffer'] = 1
        }
        $parameterSet = $PsCmdlet.ParameterSetName
        $mapping = @{
            __NoParameters = 'Az.AppConfiguration.private\Get-AzAppConfigurationStore';
            ResourceGroupNameConfigStoreName = 'Az.AppConfiguration.private\Get-AzAppConfigurationStore_ResourceGroupNameConfigStoreName';
            ResourceGroupResourceGroupName = 'Az.AppConfiguration.private\Get-AzAppConfigurationStore_ResourceGroupResourceGroupName';
            ResourceGroupSubscriptionIdResourceGroupName = 'Az.AppConfiguration.private\Get-AzAppConfigurationStore_ResourceGroupSubscriptionIdResourceGroupName';
            SubscriptionId = 'Az.AppConfiguration.private\Get-AzAppConfigurationStore_SubscriptionId';
            SubscriptionIdResourceGroupNameConfigStoreName = 'Az.AppConfiguration.private\Get-AzAppConfigurationStore_SubscriptionIdResourceGroupNameConfigStoreName';
        }
        $wrappedCmd = $ExecutionContext.InvokeCommand.GetCommand(($mapping[$parameterSet]), [System.Management.Automation.CommandTypes]::Cmdlet)
        $scriptCmd = {& $wrappedCmd @PSBoundParameters}
        $steppablePipeline = $scriptCmd.GetSteppablePipeline($myInvocation.CommandOrigin)
        $steppablePipeline.Begin($PSCmdlet)
    } catch {
        throw
    }
}

process {
    try {
        $steppablePipeline.Process($_)
    } catch {
        throw
    }
}

end {
    try {
        $steppablePipeline.End()
    } catch {
        throw
    }
}
}
