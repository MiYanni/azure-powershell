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

namespace Microsoft.Azure.Commands.LogicApp.Cmdlets
{
    using System;
    using System.Globalization;
    using System.Management.Automation;
    using Management.Logic.Models;
    using Utilities;
    using ResourceManager.Common.ArgumentCompleters;

    /// <summary>
    /// Updates the integration account received interchange control number.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "AzureRmIntegrationAccountReceivedIcn", SupportsShouldProcess = true)]
    [OutputType(typeof(IntegrationAccountControlNumber))]
    public class UpdateAzureIntegrationAccountReceivedIcnCommand : LogicAppBaseCmdlet
    {
        #region Input Paramters

        /// <summary>
        /// Gets or sets the resource group name.
        /// </summary>
        [Parameter(Mandatory = true, HelpMessage = "The integration account resource group name.",
            ValueFromPipelineByPropertyName = true)]
        [ResourceGroupCompleter]
        [ValidateNotNullOrEmpty]
        public string ResourceGroupName { get; set; }

        /// <summary>
        /// Gets or sets the integration account name.
        /// </summary>
        [Parameter(Mandatory = true, HelpMessage = "The integration account name.")]
        [Alias("ResourceName")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the agreement name.
        /// </summary>
        [Parameter(Mandatory = true, HelpMessage = "The integration account agreement name.")]
        [ValidateNotNullOrEmpty]
        public string AgreementName { get; set; }

        /// <summary>
        /// Gets or sets the control number value.
        /// </summary>
        [Parameter(Mandatory = true, HelpMessage = "The integration account control number value.")]
        [ValidateNotNullOrEmpty]
        public string ControlNumberValue { get; set; }

        /// <summary>
        /// Gets or sets the received message processing status.
        /// </summary>
        [Parameter(Mandatory = true, HelpMessage = "The received message processing status.")]
        [ValidateNotNullOrEmpty]
        public bool IsMessageProcessingFailed { get; set; }

        /// <summary>
        /// Gets or sets the agreement type.
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "The integration account agreement type.")]
        [Alias("MessageType")]
        [ValidateSet("X12", "Edifact", IgnoreCase = true)]
        public string AgreementType { get; set; }

        #endregion Input Parameters

        /// <summary>
        /// Executes the integration account generated control number update command.
        /// </summary>
        public override void ExecuteCmdlet()
        {
            base.ExecuteCmdlet();

            if (string.IsNullOrEmpty(AgreementType))
            {
                WriteWarning(Constants.NoAgreementTypeParameterWarningMessage);
                AgreementType = "X12";
            }

            var integrationAccountReceivedIcn = IntegrationAccountClient.GetIntegrationAccountReceivedControlNumber(
                ResourceGroupName,
                Name,
                AgreementName,
                (AgreementType)Enum.Parse(typeof(AgreementType), AgreementType, true),
                ControlNumberValue);

            integrationAccountReceivedIcn.MessageType = (MessageType)Enum.Parse(typeof(MessageType), AgreementType, true);
            integrationAccountReceivedIcn.ControlNumber = ControlNumberValue;
            integrationAccountReceivedIcn.IsMessageProcessingFailed = IsMessageProcessingFailed;
            integrationAccountReceivedIcn.ControlNumberChangedTime = DateTime.UtcNow > integrationAccountReceivedIcn.ControlNumberChangedTime ?
                DateTime.UtcNow :
                integrationAccountReceivedIcn.ControlNumberChangedTime.AddTicks(1);

            ConfirmAction(
                string.Format(CultureInfo.InvariantCulture, Properties.Resource.UpdateReceivedControlNumberMessage, ControlNumberValue, "Microsoft.Logic/integrationAccounts/agreements", Name),
                Name,
                () =>
                {
                    WriteObject(
                        IntegrationAccountClient.UpdateIntegrationAccountReceivedIcn(
                            ResourceGroupName,
                            Name,
                            AgreementName,
                            (AgreementType)Enum.Parse(typeof(AgreementType), AgreementType, true),
                            integrationAccountReceivedIcn),
                        true);
                });
        }
    }
}