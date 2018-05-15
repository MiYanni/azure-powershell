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

using System;
using System.Management.Automation;
using Microsoft.Azure.Commands.RecoveryServices.SiteRecovery.Properties;
using Microsoft.Azure.Management.RecoveryServices.SiteRecovery.Models;

namespace Microsoft.Azure.Commands.RecoveryServices.SiteRecovery
{
    /// <summary>
    ///     Resumes Azure Site Recovery Job.
    /// </summary>
    [Cmdlet(
        VerbsLifecycle.Resume,
        "AzureRmRecoveryServicesAsrJob",
        DefaultParameterSetName = ASRParameterSets.ByObject,
        SupportsShouldProcess = true)]
    [Alias("Resume-ASRJob")]
    [OutputType(typeof(ASRJob))]
    public class ResumeAzureRmRecoveryServicesAsrJob : SiteRecoveryCmdletBase
    {
        /// <summary>
        ///     Gets or sets Job ID.
        /// </summary>
        [Parameter(
            ParameterSetName = ASRParameterSets.ByName,
            Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets Job Object.
        /// </summary>
        [Parameter(
            ParameterSetName = ASRParameterSets.ByObject,
            Mandatory = true,
            ValueFromPipeline = true)]
        [ValidateNotNullOrEmpty]
        [Alias("Job")]
        public ASRJob InputObject { get; set; }

        /// <summary>
        ///     Gets or sets job comments.
        /// </summary>
        [Parameter(Mandatory = false)]
        [ValidateNotNullOrEmpty]
        [Alias("Comments")]
        public string Comment { get; set; }

        /// <summary>
        ///     ProcessRecord of the command.
        /// </summary>
        public override void ExecuteSiteRecoveryCmdlet()
        {
            base.ExecuteSiteRecoveryCmdlet();

            if (InputObject != null)
            {
                Name = InputObject.Name;
            }

            if (ShouldProcess(
                Name,
                VerbsLifecycle.Resume))
            {
                switch (ParameterSetName)
                {
                    case ASRParameterSets.ByObject:
                        Name = InputObject.Name;
                        ResumesByName();
                        break;

                    case ASRParameterSets.ByName:
                        ResumesByName();
                        break;
                }
            }
        }

        /// <summary>
        ///     Resumes by Name.
        /// </summary>
        private void ResumesByName()
        {
            var resumeJobParams = new ResumeJobParams();
            if (string.IsNullOrEmpty(Comment))
            {
                Comment = " ";
            }

            resumeJobParams.Properties = new ResumeJobParamsProperties();
            resumeJobParams.Properties.Comments = Comment;
            var job = RecoveryServicesClient.GetAzureSiteRecoveryJobDetails(Name);
            if (job != null)
            {
                if (job.Properties.ScenarioName.Equals("TestFailover", StringComparison.CurrentCultureIgnoreCase))
                {
                    throw new InvalidOperationException(
                      string.Format(Resources.ResumeTFOJobNotSupported, job.Name));
                }
            }
            var response = RecoveryServicesClient.ResumeAzureSiteRecoveryJob(
                Name,
                resumeJobParams);

            var jobResponse = RecoveryServicesClient.GetAzureSiteRecoveryJobDetails(
                PSRecoveryServicesClient.GetJobIdFromReponseLocation(response.Location));

            WriteObject(new ASRJob(jobResponse));
        }
    }
}