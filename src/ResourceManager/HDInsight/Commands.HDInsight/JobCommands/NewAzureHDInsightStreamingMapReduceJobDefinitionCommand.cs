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

using Microsoft.Azure.Commands.HDInsight.Commands;
using Microsoft.Azure.Commands.HDInsight.Models;
using Microsoft.WindowsAzure.Commands.Common;
using System.Collections;
using System.Management.Automation;

namespace Microsoft.Azure.Commands.HDInsight
{
    [Cmdlet(
        VerbsCommon.New,
        Constants.JobDefinitions.AzureHDInsightStreamingMapReduceJobDefinition),
    OutputType(
        typeof(AzureHDInsightStreamingMapReduceJobDefinition))]
    public class NewAzureHDInsightStreamingMapReduceJobDefinitionCommand : HDInsightCmdletBase
    {
        private readonly AzureHDInsightStreamingMapReduceJobDefinition _job;

        #region Input Parameter Definitions

        [Parameter(HelpMessage = "The arguments for the jobDetails.")]
        public string[] Arguments { get; set; }

        [Parameter(HelpMessage = "Local file to be made available to tasks")]
        public string File
        {
            get { return _job.File; }
            set { _job.File = value; }
        }

        [Parameter(HelpMessage = "List of files to be copied to the cluster.")]
        public string[] Files { get; set; }

        [Parameter(HelpMessage = "The output location to use for the job.")]
        public string StatusFolder
        {
            get { return _job.StatusFolder; }
            set { _job.StatusFolder = value; }
        }

        [Parameter(HelpMessage = "The command line environment for the mappers or the reducers.")]
        public Hashtable CommandEnvironment { get; set; }

        [Parameter(HelpMessage = "The parameters for the jobDetails.")]
        public Hashtable Defines { get; set; }

        [Parameter(Mandatory = true,
            HelpMessage = "The input path to use for the jobDetails.")]
        public string InputPath
        {
            get { return _job.Input; }
            set { _job.Input = value; }
        }

        [Parameter(HelpMessage = "The Mapper to use for the jobDetails.")]
        public string Mapper
        {
            get { return _job.Mapper; }
            set { _job.Mapper = value; }
        }

        [Parameter(HelpMessage = "The output path to use for the jobDetails.")]
        public string OutputPath
        {
            get { return _job.Output; }
            set { _job.Output = value; }
        }

        [Parameter(HelpMessage = "The Reducer to use for the jobDetails.")]
        public string Reducer
        {
            get { return _job.Reducer; }
            set { _job.Reducer = value; }
        }

        #endregion

        public NewAzureHDInsightStreamingMapReduceJobDefinitionCommand()
        {
            Arguments = new string[] { };
            Files = new string[] { };
            CommandEnvironment = new Hashtable();
            Defines = new Hashtable();
            _job = new AzureHDInsightStreamingMapReduceJobDefinition();
        }

        public override void ExecuteCmdlet()
        {
            foreach (var arg in Arguments)
            {
                _job.Arguments.Add(arg);
            }

            var cmdEnvDic = CommandEnvironment.ToDictionary(false);
            foreach (var cmdEnv in cmdEnvDic)
            {
                _job.CommandEnvironment.Add(cmdEnv.Key, cmdEnv.Value.ToString());
            }

            var defineDic = Defines.ToDictionary(false);
            foreach (var define in defineDic)
            {
                _job.Defines.Add(define.Key, define.Value.ToString());
            }

            foreach (var file in Files)
            {
                _job.Files.Add(file);
            }

            WriteObject(_job);
        }
    }
}
