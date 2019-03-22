# region Initialization
  # Get this module's instance
  $instance = [Microsoft.Azure.PowerShell.Cmdlets.AppConfiguration.Module]::Instance

  # Load the custom script module
  $scriptModulePath = Join-Path $PSScriptRoot './custom/Az.AppConfiguration.custom.psm1'
  if(Test-Path $scriptModulePath) {
    $null = Import-Module -Name $scriptModulePath
  }

  # Load the private module dll
  $null = Import-Module -Name (Join-Path $PSScriptRoot './bin/Az.AppConfiguration.private.dll')

  # Export nothing to clear implicit exports
  Export-ModuleMember
# endregion

# region AzureInitialization
  # Load required Az.Accounts module
  $sharedModule = Get-Module -Name Az.Accounts
  if (-not $sharedModule) {
    $accountsName = 'Az.Accounts'
    $localAccounts = Get-ChildItem -Path (Join-Path $PSScriptRoot 'generated\modules') -Recurse -Include 'Az.Accounts.psd1' | Select-Object -Last 1
    if($localAccounts -and (Test-Path -Path $localAccounts)) {
      $accountsName = $localAccounts.FullName
    }
    $sharedModule = Import-Module -Name $accountsName -MinimumVersion 1.4.0 -Scope Global -PassThru
  } elseif ($sharedModule.Version -lt [System.Version]'1.4.0') {
    Write-Error 'This module requires Az.Accounts version 1.4.0 or greater. An earlier version of Az.Accounts is imported in the current PowerShell session. Please open a new session before importing this module. This error could indicate that multiple incompatible versions of the Azure PowerShell cmdlets are installed on your system. Please see https://aka.ms/azps-version-error for troubleshooting information.' -ErrorAction Stop
  }
  Write-Information "Loaded Module '$($sharedModule.Name)'"

  # Ask for the shared functionality table
  $VTable = Register-AzModule

  # Tweaks the pipeline on module load
  $instance.OnModuleLoad = $VTable.OnModuleLoad

  # Tweaks the pipeline per call
  $instance.OnNewRequest = $VTable.OnNewRequest

  # Gets shared parameter values
  $instance.GetParameterValue = $VTable.GetParameterValue

  # Allows shared module to listen to events from this module
  $instance.EventListener = $VTable.EventListener

  # Gets shared argument completers
  $instance.ArgumentCompleter = $VTable.ArgumentCompleter

  # The name of the currently selected Azure profile
  $instance.ProfileName = $VTable.ProfileName
# endregion

# region LoadExports
  # Export proxy cmdlet scripts
  $exportsPath = Join-Path $PSScriptRoot './exports'
  $directories = Get-ChildItem -Directory -Path $exportsPath
  $profileDirectory = $null
  if($instance.ProfileName) {
    if(($directories | ForEach-Object { $_.Name }) -contains $instance.ProfileName) {
      $profileDirectory = $directories | Where-Object { $_.Name -eq $instance.ProfileName }
    } else {
      # Don't export anything if the profile doesn't exist for the module
      $exportsPath = $null
      Write-Warning "Selected Azure profile '$($instance.ProfileName)' does not exist for module '$($instance.Name)'. No cmdlets were loaded."
    }
  } elseif(($directories | Measure-Object).Count -gt 0) {
    # Load the last folder if no profile is selected
    $profileDirectory = $directories | Select-Object -Last 1
  }

  if($profileDirectory) {
    Write-Information "Loaded Azure profile '$($profileDirectory.Name)' for module '$($instance.Name)'"
    $exportsPath = $profileDirectory.FullName
  }

  if($exportsPath) {
    Get-ChildItem -Path $exportsPath -Recurse -Filter '*.ps1' -File | Sort-Object Name | ForEach-Object {
      Write-Verbose "Loading script file: $($_.Name)"
      . $_.FullName
      Export-ModuleMember -Function $_.BaseName
    }
  }
# endregion

# region Finalization
  # Finalize initialization of this module
  $instance.Init();
  Write-Information "Loaded Module '$($instance.Name)'"
# endregion
