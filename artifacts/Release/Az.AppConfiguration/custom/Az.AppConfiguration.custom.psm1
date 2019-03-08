# region Initialization
  # Load the private module dll
  $null = Import-Module -PassThru -Name (Join-Path $PSScriptRoot ..\bin\Az.AppConfiguration.private.dll)
  # Export nothing to clear implicit exports
  Export-ModuleMember
# endregion

# region LoadScripts
  # https://stackoverflow.com/a/40969712/294804
  $currentFunctions = Get-ChildItem function:
  Get-ChildItem -Path $PSScriptRoot -Recurse -Filter '*.ps1' -File | ForEach-Object { . $_.FullName }
  $scriptFunctions = Get-ChildItem function: | Where-Object { $currentFunctions -notcontains $_ }

  # Export custom scripts
  if(($scriptFunctions | Measure-Object).Count -gt 0) {
    $scriptFunctions | ForEach-Object { Export-ModuleMember -Function $_.Name }
  }
# endregion
