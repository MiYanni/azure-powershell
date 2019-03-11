<#
.Synopsis
Lists the access key for the specified configuration store.
.Description
Lists the access key for the specified configuration store.
.Link
https://docs.microsoft.com/en-us/powershell/module/az.appconfiguration/get-azappconfigurationstorekey
#>
function Get-AzAppConfigurationStoreKey {
[OutputType('Microsoft.Azure.PowerShell.Cmdlets.AppConfiguration.Models.Api20190201Preview.IApiKey')]
[CmdletBinding(DefaultParameterSetName='KeysResourceGroupNameConfigStoreName', SupportsShouldProcess, ConfirmImpact='Medium')]
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

    [Parameter(Mandatory)]
    [System.String]
    # The name of the configuration store.
    ${Name},

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

    [Parameter(Mandatory)]
    [System.String]
    # The name of the resource group to which the container registry belongs.
    ${ResourceGroupName},

    [Parameter(ParameterSetName='KeysResourceGroupNameConfigStoreNameSkipToken', Mandatory)]
    [Parameter(ParameterSetName='KeysSubscriptionIdResourceGroupNameConfigStoreNameSkipToken', Mandatory)]
    [System.String]
    # A skip token is used to continue retrieving items after an operation returns a partial result. If a previous response contains a nextLink element, the value of the nextLink element will include a skipToken parameter that specifies a starting point to use for subsequent calls.
    ${SkipToken},

    [Parameter(ParameterSetName='KeysSubscriptionIdResourceGroupNameConfigStoreName', Mandatory)]
    [Parameter(ParameterSetName='KeysSubscriptionIdResourceGroupNameConfigStoreNameSkipToken', Mandatory)]
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
            KeysResourceGroupNameConfigStoreName = 'Az.AppConfiguration.private\Get-AzAppConfigurationStoreKey_KeysResourceGroupNameConfigStoreName';
            KeysResourceGroupNameConfigStoreNameSkipToken = 'Az.AppConfiguration.private\Get-AzAppConfigurationStoreKey_KeysResourceGroupNameConfigStoreNameSkipToken';
            KeysSubscriptionIdResourceGroupNameConfigStoreName = 'Az.AppConfiguration.private\Get-AzAppConfigurationStoreKey_KeysSubscriptionIdResourceGroupNameConfigStoreName';
            KeysSubscriptionIdResourceGroupNameConfigStoreNameSkipToken = 'Az.AppConfiguration.private\Get-AzAppConfigurationStoreKey_KeysSubscriptionIdResourceGroupNameConfigStoreNameSkipToken';
        }
        $wrappedCmd = $ExecutionContext.InvokeCommand.GetCommand("$($mapping["$parameterSet"])", [System.Management.Automation.CommandTypes]::Cmdlet)
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
