// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

using Microsoft.Azure.Commands.Billing.Common;
using Microsoft.Azure.Commands.Billing.Models;
using Microsoft.Azure.Management.Billing;
using Microsoft.Azure.Management.Billing.Models;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace Microsoft.Azure.Commands.Billing.Cmdlets
{
    [Cmdlet("Get", ResourceManager.Common.AzureRMConstants.AzureRMPrefix + "CustomFormatTest", DefaultParameterSetName = Constants.ParameterSetNames.ListParameterSet), OutputType(typeof(PSStorageSyncValidation))]
    public class GetCustomFormatTest : AzureBillingCmdletBase
    {
        public override void ExecuteCmdlet()
        {
            var validation = new PSStorageSyncValidation
            {
                ComputerName = "localhost",
                OSVersionCheckResult = false,
                FileSystemCheckResult = true,
                Path = @"C:\logs",
                FileScanCount = 348,
                DirectoryScanCount = 2,
                Results = new List<PSStorageSyncValidationResult>
                {
                    new PSStorageSyncValidationResult
                    {
                        Type = PSValidationResultType.OsVersion,
                        Level = PSValidationResultLevel.Error,
                        Description = "OS edition 'Enterprise Edition' is not supported. Supported editions are: Standard Server Edition, Data..."
                    }
                }
            };

            WriteObject(validation);
        }
    }
}
