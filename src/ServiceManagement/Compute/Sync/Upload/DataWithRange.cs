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

using Microsoft.WindowsAzure.Commands.Tools.Vhd.Model;
using System;
using System.ServiceModel.Channels;

namespace Microsoft.WindowsAzure.Commands.Sync.Upload
{
    public class DataWithRange : IDisposable
    {
        private readonly BufferManager manager;
        private bool disposed;

        public DataWithRange(BufferManager manager)
        {
            this.manager = manager;
        }

        public bool IsAllZero()
        {
            var startIndex = Array.FindIndex(Data, 0, (int)Range.Length, b => b != 0);

            return startIndex == -1;
        }

        public byte[] Data { get; set; }
        public IndexRange Range { get; set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                manager.ReturnBuffer(Data);
                Data = null;
            }
            disposed = true;
        }
    }
}