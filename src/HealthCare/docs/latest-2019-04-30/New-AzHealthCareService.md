---
external help file:
Module Name: Az.HealthCare
online version: https://docs.microsoft.com/en-us/powershell/module/az.healthcare/new-azhealthcareservice
schema: 2.0.0
---

# New-AzHealthCareService

## SYNOPSIS
Create or update the metadata of a service instance.

## SYNTAX

### Create (Default)
```
New-AzHealthCareService -ResourceGroupName <String> -ResourceName <String> -SubscriptionId <String>
 [-ServiceDescription <IServicesDescription>] [-DefaultProfile <PSObject>] [-AsJob] [-NoWait] [-Confirm]
 [-WhatIf] [<CommonParameters>]
```

### CreateExpanded
```
New-AzHealthCareService -ResourceGroupName <String> -ResourceName <String> -SubscriptionId <String>
 -Location <String> [-AccessPolicy <IServiceAccessPolicyEntry[]>]
 [-AuthenticationConfigurationAudience <String>] [-AuthenticationConfigurationAuthority <String>]
 [-AuthenticationConfigurationSmartProxyEnabled] [-CorConfigurationAllowCredentials]
 [-CorConfigurationHeader <String[]>] [-CorConfigurationMaxAge <Int32>] [-CorConfigurationMethod <String[]>]
 [-CorConfigurationOrigin <String[]>] [-CosmoDbConfigurationOfferThroughput <Int32>] [-Etag <String>]
 [-Tag <Hashtable>] [-DefaultProfile <PSObject>] [-AsJob] [-NoWait] [-Confirm] [-WhatIf] [<CommonParameters>]
```

### CreateViaIdentityExpanded
```
New-AzHealthCareService -InputObject <IHealthCareIdentity> -Location <String>
 [-AccessPolicy <IServiceAccessPolicyEntry[]>] [-AuthenticationConfigurationAudience <String>]
 [-AuthenticationConfigurationAuthority <String>] [-AuthenticationConfigurationSmartProxyEnabled]
 [-CorConfigurationAllowCredentials] [-CorConfigurationHeader <String[]>] [-CorConfigurationMaxAge <Int32>]
 [-CorConfigurationMethod <String[]>] [-CorConfigurationOrigin <String[]>]
 [-CosmoDbConfigurationOfferThroughput <Int32>] [-Etag <String>] [-Tag <Hashtable>]
 [-DefaultProfile <PSObject>] [-AsJob] [-NoWait] [-Confirm] [-WhatIf] [<CommonParameters>]
```

### CreateViaIdentity
```
New-AzHealthCareService -InputObject <IHealthCareIdentity> [-ServiceDescription <IServicesDescription>]
 [-DefaultProfile <PSObject>] [-AsJob] [-NoWait] [-Confirm] [-WhatIf] [<CommonParameters>]
```

## DESCRIPTION
Create or update the metadata of a service instance.

## EXAMPLES

### Example 1: {{ Add title here }}
```powershell
PS C:\> {{ Add code here }}

{{ Add output here }}
```

{{ Add description here }}

### Example 2: {{ Add title here }}
```powershell
PS C:\> {{ Add code here }}

{{ Add output here }}
```

{{ Add description here }}

## PARAMETERS

### -AccessPolicy
The access policies of the service instance.
To construct, see NOTES section for ACCESSPOLICY properties and create a hash table.

```yaml
Type: Microsoft.Azure.PowerShell.Cmdlets.HealthCare.Models.Api20180820Preview.IServiceAccessPolicyEntry[]
Parameter Sets: CreateExpanded, CreateViaIdentityExpanded
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
Dynamic: False
```

### -AsJob
Run the command as a job

```yaml
Type: System.Management.Automation.SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
Dynamic: False
```

### -AuthenticationConfigurationAudience
The audience url for the service

```yaml
Type: System.String
Parameter Sets: CreateExpanded, CreateViaIdentityExpanded
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
Dynamic: False
```

### -AuthenticationConfigurationAuthority
The authority url for the service

```yaml
Type: System.String
Parameter Sets: CreateExpanded, CreateViaIdentityExpanded
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
Dynamic: False
```

### -AuthenticationConfigurationSmartProxyEnabled
If the SMART on FHIR proxy is enabled

```yaml
Type: System.Management.Automation.SwitchParameter
Parameter Sets: CreateExpanded, CreateViaIdentityExpanded
Aliases:

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
Dynamic: False
```

### -CorConfigurationAllowCredentials
If credentials are allowed via CORS.

```yaml
Type: System.Management.Automation.SwitchParameter
Parameter Sets: CreateExpanded, CreateViaIdentityExpanded
Aliases:

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
Dynamic: False
```

### -CorConfigurationHeader
The headers to be allowed via CORS.

```yaml
Type: System.String[]
Parameter Sets: CreateExpanded, CreateViaIdentityExpanded
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
Dynamic: False
```

### -CorConfigurationMaxAge
The max age to be allowed via CORS.

```yaml
Type: System.Int32
Parameter Sets: CreateExpanded, CreateViaIdentityExpanded
Aliases:

Required: False
Position: Named
Default value: 0
Accept pipeline input: False
Accept wildcard characters: False
Dynamic: False
```

### -CorConfigurationMethod
The methods to be allowed via CORS.

```yaml
Type: System.String[]
Parameter Sets: CreateExpanded, CreateViaIdentityExpanded
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
Dynamic: False
```

### -CorConfigurationOrigin
The origins to be allowed via CORS.

```yaml
Type: System.String[]
Parameter Sets: CreateExpanded, CreateViaIdentityExpanded
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
Dynamic: False
```

### -CosmoDbConfigurationOfferThroughput
The provisioned throughput for the backing database.

```yaml
Type: System.Int32
Parameter Sets: CreateExpanded, CreateViaIdentityExpanded
Aliases:

Required: False
Position: Named
Default value: 0
Accept pipeline input: False
Accept wildcard characters: False
Dynamic: False
```

### -DefaultProfile
The credentials, account, tenant, and subscription used for communication with Azure.

```yaml
Type: System.Management.Automation.PSObject
Parameter Sets: (All)
Aliases: AzureRMContext, AzureCredential

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
Dynamic: False
```

### -Etag
An etag associated with the resource, used for optimistic concurrency when editing it.

```yaml
Type: System.String
Parameter Sets: CreateExpanded, CreateViaIdentityExpanded
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
Dynamic: False
```

### -InputObject
Identity Parameter

```yaml
Type: Microsoft.Azure.PowerShell.Cmdlets.HealthCare.Models.IHealthCareIdentity
Parameter Sets: CreateViaIdentityExpanded, CreateViaIdentity
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
Dynamic: False
```

### -Location
The resource location.

```yaml
Type: System.String
Parameter Sets: CreateExpanded, CreateViaIdentityExpanded
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
Dynamic: False
```

### -NoWait
Run the command asynchronously

```yaml
Type: System.Management.Automation.SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
Dynamic: False
```

### -ResourceGroupName
The name of the resource group that contains the service instance.

```yaml
Type: System.String
Parameter Sets: Create, CreateExpanded
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
Dynamic: False
```

### -ResourceName
The name of the service instance.

```yaml
Type: System.String
Parameter Sets: Create, CreateExpanded
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
Dynamic: False
```

### -ServiceDescription
The description of the service.
To construct, see NOTES section for SERVICEDESCRIPTION properties and create a hash table.

```yaml
Type: Microsoft.Azure.PowerShell.Cmdlets.HealthCare.Models.Api20180820Preview.IServicesDescription
Parameter Sets: Create, CreateViaIdentity
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
Dynamic: False
```

### -SubscriptionId
The subscription identifier.

```yaml
Type: System.String
Parameter Sets: Create, CreateExpanded
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
Dynamic: False
```

### -Tag
The resource tags.

```yaml
Type: System.Collections.Hashtable
Parameter Sets: CreateExpanded, CreateViaIdentityExpanded
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
Dynamic: False
```

### -Confirm
Prompts you for confirmation before running the cmdlet.

```yaml
Type: System.Management.Automation.SwitchParameter
Parameter Sets: (All)
Aliases: cf

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
Dynamic: False
```

### -WhatIf
Shows what would happen if the cmdlet runs.
The cmdlet is not run.

```yaml
Type: System.Management.Automation.SwitchParameter
Parameter Sets: (All)
Aliases: wi

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
Dynamic: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### Microsoft.Azure.PowerShell.Cmdlets.HealthCare.Models.IHealthCareIdentity

### Microsoft.Azure.PowerShell.Cmdlets.HealthCare.Models.Api20180820Preview.IServicesDescription

## OUTPUTS

### Microsoft.Azure.PowerShell.Cmdlets.HealthCare.Models.Api20180820Preview.IServicesDescription

## ALIASES

## NOTES

### COMPLEX PARAMETER PROPERTIES
To create the parameters described below, construct a hash table containing the appropriate properties. For information on hash tables, run Get-Help about_Hash_Tables.

#### ACCESSPOLICY <IServiceAccessPolicyEntry[]>: The access policies of the service instance.
  - `ObjectId <String>`: An object ID that is allowed access to the FHIR service.

#### SERVICEDESCRIPTION <IServicesDescription>: The description of the service.
  - `Location <String>`: The resource location.
  - `AccessPolicy <IServiceAccessPolicyEntry[]>`: The access policies of the service instance.
    - `ObjectId <String>`: An object ID that is allowed access to the FHIR service.
  - `[Etag <String>]`: An etag associated with the resource, used for optimistic concurrency when editing it.
  - `[Tag <IResourceTags>]`: The resource tags.
    - `[(Any) <String>]`: This indicates any property can be added to this object.
  - `[AuthenticationConfigurationAudience <String>]`: The audience url for the service
  - `[AuthenticationConfigurationAuthority <String>]`: The authority url for the service
  - `[AuthenticationConfigurationSmartProxyEnabled <Boolean?>]`: If the SMART on FHIR proxy is enabled
  - `[CorConfigurationAllowCredentials <Boolean?>]`: If credentials are allowed via CORS.
  - `[CorConfigurationHeader <String[]>]`: The headers to be allowed via CORS.
  - `[CorConfigurationMaxAge <Int32?>]`: The max age to be allowed via CORS.
  - `[CorConfigurationMethod <String[]>]`: The methods to be allowed via CORS.
  - `[CorConfigurationOrigin <String[]>]`: The origins to be allowed via CORS.
  - `[CosmoDbConfigurationOfferThroughput <Int32?>]`: The provisioned throughput for the backing database.

## RELATED LINKS

