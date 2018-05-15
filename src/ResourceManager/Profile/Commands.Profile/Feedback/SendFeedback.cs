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

using Microsoft.Azure.Commands.Profile.Models;
using Microsoft.Azure.Commands.Profile.Properties;
using Microsoft.Azure.Commands.ResourceManager.Common;
using Microsoft.WindowsAzure.Commands.Common;
using System;
using System.Linq;
using System.Management.Automation;

namespace Microsoft.Azure.Commands.Profile
{
    [Cmdlet(VerbsCommunications.Send, "Feedback"), OutputType(typeof(void))]
    public class SendFeedbackCommand : AzureRMCmdlet
    {
        private const string _eventName = "feedback";

        protected override void BeginProcessing()
        {
            //cmdlet is failing due to _metrichelper being null, since we skipped beging processing. 
            base.BeginProcessing(); 

            if (!CheckIfInteractive())
                throw new PSInvalidOperationException(String.Format(Resources.SendFeedbackNonInteractiveMessage, nameof(SendFeedbackCommand)));
        }

        public override void ExecuteCmdlet()
        {
            WriteQuestion(Resources.SendFeedbackRecommendationQuestion);
            int recommendation;
            if (!int.TryParse(Host.UI.ReadLine(), out recommendation) || recommendation < 0 || recommendation > 10)
                throw new PSArgumentOutOfRangeException(Resources.SendFeedbackOutOfRangeMessage);

            WriteQuestion(Resources.SendFeedbackPositiveCommentsQuestion);
            var positiveComments = Host.UI.ReadLine();

            WriteQuestion(Resources.SendFeedbackNegativeCommentsQuestion);
            var negativeComments = Host.UI.ReadLine();

            WriteQuestion(Resources.SendFeedbackEmailQuestion);
            var email = Host.UI.ReadLine();

            var loggedIn = DefaultProfile != null
                && DefaultProfile.DefaultContext != null
                && DefaultProfile.DefaultContext.Account != null
                && DefaultProfile.DefaultContext.Tenant != null
                && DefaultProfile.DefaultContext.Subscription != null
                && DefaultProfile.DefaultContext.Environment != null;

            var feedbackPayload = new PSAzureFeedback
            {
                ModuleName = ModuleName, 
                ModuleVersion = ModuleVersion, 
                SubscriptionId = loggedIn ? DefaultContext.Subscription.Id : Guid.Empty.ToString(), 
                TenantId = loggedIn ? DefaultContext.Tenant.Id : Guid.Empty.ToString(), 
                Environment = loggedIn ? DefaultContext.Environment.Name : null, 
                Recommendation = recommendation, 
                PositiveComments = positiveComments, 
                NegativeComments = negativeComments, 
                Email = email
            };

            Host.UI.WriteLine();

            // Log the event with force since the user specifically issued this command to provide feedback.

            _metricHelper.LogCustomEvent(_eventName, feedbackPayload, true /* force */);
        }

        private void WriteQuestion(string question)
        {
            Host.UI.WriteLine(ConsoleColor.Cyan, Host.UI.RawUI.BackgroundColor, $"{Environment.NewLine}{question}{Environment.NewLine}");
        }
    }
}
