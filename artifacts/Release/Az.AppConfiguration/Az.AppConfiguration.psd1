@{
# region identity
  ModuleVersion="1.0"
  Description=""
  PowerShellVersion="3.0"
  Author="Microsoft Corporation"
  CompanyName="Microsoft Corporation"
  Copyright="Microsoft Corporation. All rights reserved."
# endregion

# region private data
  PrivateData = @{
    # Package Metadata for PowerShellGet
    PSData = @{
      # Tags applied to this module.These help with module discovery in online galleries.
      Tags = 'Azure', 'ServiceManagement'

      # A URL to the license for this module.
      LicenseUri = 'https://aka.ms/azps-license'

      # A URL to the main website for this project.
      ProjectUri = 'https://github.com/Azure/azure-powershell'

      # A URL to an icon representing this module.
      # IconUri = ''

      # ReleaseNotes of this module
      ReleaseNotes = ''

      # External dependent modules of this module
      # ExternalModuleDependencies = ''

    } # End of PSData hashtable
  } # End of PrivateData hashtable
# endregion

# region exports
  # don't export any actual cmdlets by default
  CmdletsToExport = ''

  # export the functions that we loaded(these are the proxy cmdlets)
  FunctionsToExport = '*-*'
# endregion

# region modules
  # Warning: This region is code-generated and will get replaced upon regeneration.

  RootModule = "Az.AppConfiguration.psm1"

  RequiredAssemblies = './bin/Az.AppConfiguration.private.dll'

  FormatsToProcess = './Az.AppConfiguration.format.ps1xml'

# endregion

}