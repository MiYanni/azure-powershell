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
using System.Collections.Generic;
using Microsoft.Azure.Management.DataFactory.Models;

namespace Microsoft.Azure.Commands.DataFactoryV2.Models
{
    public class PSPipelineRun
    {
        private PipelineRun pipelineRun;

        public PSPipelineRun()
        {
            pipelineRun = new PipelineRun();
        }

        public PSPipelineRun(PipelineRun pipelineRun, string resourceGroupName, string factoryName)
        {
            if (pipelineRun == null)
            {
                throw new ArgumentNullException("pipelineRun");
            }

            this.pipelineRun = pipelineRun;
            ResourceGroupName = resourceGroupName;
            DataFactoryName = factoryName;
        }

        public string ResourceGroupName { get; private set; }

        public string DataFactoryName { get; private set; }

        public string RunId
        {
            get
            {
                return pipelineRun.RunId;
            }
        }

        public string PipelineName
        {
            get
            {
                return pipelineRun.PipelineName;
            }
        }

        public DateTime? LastUpdated
        {
            get
            {
                return pipelineRun.LastUpdated;
            }
        }

        public IDictionary<string, string> Parameters
        {
            get
            {
                return pipelineRun.Parameters;
            }
        }
        
        public DateTime? RunStart
        {
            get
            {
                return pipelineRun.RunStart;
            }
        }

        public DateTime? RunEnd
        {
            get
            {
                return pipelineRun.RunEnd;
            }
        }

        public int? DurationInMs
        {
            get
            {
                return pipelineRun.DurationInMs;
            }
        }

        public string Status
        {
            get
            {
                return pipelineRun.Status;
            }
        }

        public string Message
        {
            get
            {
                return pipelineRun.Message;
            }
        }
    }
}
