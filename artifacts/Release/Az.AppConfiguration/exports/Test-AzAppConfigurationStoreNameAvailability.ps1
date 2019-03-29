<#
.Synopsis
Checks whether the configuration store name is available for use.
.Description
Checks whether the configuration store name is available for use.
.Link
https://docs.microsoft.com/en-us/powershell/module/az.appconfiguration/test-azappconfigurationstorenameavailability
#>
function Test-AzAppConfigurationStoreNameAvailability {
[OutputType('Microsoft.Azure.PowerShell.Cmdlets.AppConfiguration.Models.Api20190201Preview.INameAvailabilityStatus')]
[CmdletBinding(DefaultParameterSetName='NoType', SupportsShouldProcess, ConfirmImpact='Medium')]
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
    # The name to check for availability.
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

    [Parameter(ParameterSetName='NameAvailabilityNameTypeExpanded', Mandatory)]
    [Parameter(ParameterSetName='NameAvailabilitySubscriptionIdNameTypeExpanded', Mandatory)]
    [ArgumentCompleter([Microsoft.Azure.PowerShell.Cmdlets.AppConfiguration.Support.ConfigurationResourceType])]
    [Microsoft.Azure.PowerShell.Cmdlets.AppConfiguration.Support.ConfigurationResourceType]
    # The resource type to check for name availability.
    ${Type},

    [Parameter(ParameterSetName='NameAvailabilitySubscriptionIdNameTypeExpanded', Mandatory)]
    [Parameter(ParameterSetName='NoType')]
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
            NameAvailabilityNameTypeExpanded = 'Az.AppConfiguration.private\Test-AzAppConfigurationStoreNameAvailability_NameAvailabilityNameTypeExpanded';
            NameAvailabilitySubscriptionIdNameTypeExpanded = 'Az.AppConfiguration.private\Test-AzAppConfigurationStoreNameAvailability_NameAvailabilitySubscriptionIdNameTypeExpanded';
            NoType = 'Az.AppConfiguration.custom\Test-AzAppConfigurationStoreNameAvailability';
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
