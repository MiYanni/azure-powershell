@{
  GUID = '7a722fdb-7183-4902-b93b-0366ba45145c'
  RootModule = './Az.HealthCare.psm1'
  ModuleVersion = '0.0.1'
  CompatiblePSEditions = 'Core', 'Desktop'
  Author = 'Microsoft Corporation'
  CompanyName = 'Microsoft Corporation'
  Copyright = 'Microsoft Corporation. All rights reserved.'
  Description = 'Microsoft Azure PowerShell: HealthCare cmdlets'
  PowerShellVersion = '5.1'
  DotNetFrameworkVersion = '4.7.2'
  RequiredAssemblies = './bin/Az.HealthCare.private.dll'
  FormatsToProcess = './Az.HealthCare.format.ps1xml'
  CmdletsToExport = 'Get-AzHealthCareOperationResult', 'Get-AzHealthCareService', 'New-AzHealthCareService', 'Remove-AzHealthCareService', 'Set-AzHealthCareService', 'Test-AzHealthCareServiceNameAvailability', 'Update-AzHealthCareService', '*'
  AliasesToExport = '*'
  PrivateData = @{
    PSData = @{
      Tags = 'Azure', 'ResourceManager', 'ARM', 'HealthCare'
      LicenseUri = 'https://aka.ms/azps-license'
      ProjectUri = 'https://github.com/Azure/azure-powershell'
      ReleaseNotes = ''
      Profiles = 'latest-2019-04-30'
    }
  }
}
