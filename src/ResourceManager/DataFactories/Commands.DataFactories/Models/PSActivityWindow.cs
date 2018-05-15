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

using Microsoft.Azure.Management.DataFactories.Models;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.Azure.Commands.DataFactories.Models
{
    public class PSActivityWindow
    {
        private ActivityWindow _activityWindow;

        public PSActivityWindow()
        {
            _activityWindow = new ActivityWindow();
        }

        public PSActivityWindow(ActivityWindow activityWindow)
        {
            if (activityWindow == null)
            {
                throw new ArgumentNullException("activityWindow");
            }

            this._activityWindow = activityWindow;
        }

        public string ResourceGroupName
        {
            get
            {
                return _activityWindow.ResourceGroupName;
            }
            internal set
            {
                _activityWindow.ResourceGroupName = value;
            }
        }

        public string DataFactoryName
        {
            get
            {
                return _activityWindow.DataFactoryName;
            }
            internal set
            {
                _activityWindow.DataFactoryName = value;
            }
        }

        public string PipelineName
        {
            get
            {
                return _activityWindow.PipelineName;
            }
            internal set
            {
                _activityWindow.PipelineName = value;
            }
        }

        public string ActivityName
        {
            get
            {
                return _activityWindow.ActivityName;
            }
            internal set
            {
                _activityWindow.ActivityName = value;
            }
        }

        public string ActivityType
        {
            get
            {
                return _activityWindow.ActivityType;
            }
            internal set
            {
                _activityWindow.ActivityType = value;
            }
        }

        public string LinkedServiceName
        {
            get
            {
                return _activityWindow.LinkedServiceName;
            }
            internal set
            {
                _activityWindow.LinkedServiceName = value;
            }
        }

        public string WindowState
        {
            get
            {
                return _activityWindow.WindowState;
            }
            internal set
            {
                _activityWindow.WindowState = value;
            }
        }

        public string WindowSubstate
        {
            get
            {
                return _activityWindow.WindowSubstate;
            }
            internal set
            {
                _activityWindow.WindowSubstate = value;
            }
        }

        public TimeSpan? Duration
        {
            get
            {
                return _activityWindow.Duration;
            }
            internal set
            {
                _activityWindow.Duration = value;
            }
        }

        public IList<string> InputDatasets
        {
            get
            {
                return _activityWindow.InputDatasets;
            }
            internal set
            {
                _activityWindow.InputDatasets = value;
            }
        }

        public IList<string> OutputDatasets
        {
            get
            {
                return _activityWindow.OutputDatasets;
            }
            internal set
            {
                _activityWindow.OutputDatasets = value;
            }
        }

        public int? PercentComplete
        {
            get
            {
                return _activityWindow.PercentComplete;
            }
            internal set
            {
                _activityWindow.PercentComplete = value;
            }
        }

        public int RunAttempts
        {
            get
            {
                return _activityWindow.RunAttempts;
            }
            internal set
            {
                _activityWindow.RunAttempts = value;
            }
        }

        public DateTime? RunStart
        {
            get
            {
                return _activityWindow.RunStart;
            }
            internal set
            {
                _activityWindow.RunStart = value;
            }
        }

        public DateTime? RunEnd
        {
            get
            {
                return _activityWindow.RunEnd;
            }
            internal set
            {
                _activityWindow.RunEnd = value;
            }
        }

        public DateTime WindowStart
        {
            get
            {
                return _activityWindow.WindowStart;
            }
            internal set
            {
                _activityWindow.WindowStart = value;
            }
        }

        public DateTime WindowEnd
        {
            get
            {
                return _activityWindow.WindowEnd;
            }
            internal set
            {
                _activityWindow.WindowEnd = value;
            }
        }

        public bool IsEqualTo(PSActivityWindow activityWindow)
        {
            Type type = typeof(PSActivityWindow);
            foreach (PropertyInfo pi in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                object actualValue = type.GetProperty(pi.Name).GetValue(activityWindow, null);
                object expectedValue = type.GetProperty(pi.Name).GetValue(this, null);

                if (actualValue != expectedValue && (actualValue == null || !actualValue.Equals(expectedValue)))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
