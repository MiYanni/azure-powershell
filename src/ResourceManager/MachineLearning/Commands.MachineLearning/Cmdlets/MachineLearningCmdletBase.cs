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
using System.Linq;
using System.Management.Automation;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;

using Microsoft.Azure.Commands.ResourceManager.Common;
using Microsoft.Rest.Azure;

using Microsoft.WindowsAzure.Commands.Common;
using Microsoft.Azure.Commands.Common.Authentication.Abstractions;

namespace Microsoft.Azure.Commands.MachineLearning
{
    public abstract class MachineLearningCmdletBase : AzureRMCmdlet
    {
        /// <summary>
        /// The cancellation source.
        /// </summary>
        private CancellationTokenSource cancellationSource;

        protected CancellationToken? CancellationToken
        {
            get
            {
                return cancellationSource == null ? null : (CancellationToken?)cancellationSource.Token;
            }
        }

        #region Processing life cycle

        protected override void BeginProcessing()
        {
            try
            {
                if (cancellationSource == null)
                {
                    cancellationSource = new CancellationTokenSource();
                }

                base.BeginProcessing();
            }
            catch (Exception ex)
            {
                WriteVersionInfoToDebugChannel();
                if (IsFatalException(ex))
                {
                    ThrowTerminatingError(
                        new ErrorRecord(
                                ex,
                                string.Empty,
                                ErrorCategory.InvalidOperation,
                                this));
                }

                var capturedException = ExceptionDispatchInfo.Capture(ex);
                HandleException(capturedException);
            }
        }

        protected override void EndProcessing()
        {
            try
            {
                base.EndProcessing();
            }
            catch (Exception ex)
            {
                WriteVersionInfoToDebugChannel();
                if (IsFatalException(ex))
                {
                    ThrowTerminatingError(
                            new ErrorRecord(
                                    ex,
                                    string.Empty,
                                    ErrorCategory.InvalidOperation,
                                    this));
                }

                var capturedException = ExceptionDispatchInfo.Capture(ex);
                HandleException(capturedException);
            }
            finally
            {
                DisposeOfCancellationSource();
            }
        }

        protected override void StopProcessing()
        {
            try
            {
                if (cancellationSource != null &&
                    !cancellationSource.IsCancellationRequested)
                {
                    cancellationSource.Cancel();
                }

                base.StopProcessing();
            }
            catch (Exception ex)
            {
                WriteVersionInfoToDebugChannel();
                if (IsFatalException(ex))
                {
                    throw;
                }

                var capturedException = ExceptionDispatchInfo.Capture(ex);
                HandleException(capturedException);
            }
            finally
            {
                DisposeOfCancellationSource();
            }
        }

        /// <summary>
        /// Actual cmdlet logic goes here in child classes
        /// </summary>
        protected virtual void RunCmdlet()
        {
            // No op
        }

        public override void ExecuteCmdlet()
        {
            try
            {
                base.ExecuteCmdlet();
                RunCmdlet();
            }
            catch (Exception ex)
            {
               WriteVersionInfoToDebugChannel();

                if (IsFatalException(ex))
                {
                    throw;
                }

                var capturedException = ExceptionDispatchInfo.Capture(ex);
                HandleException(capturedException);
            }
        }

        #endregion

        private void DisposeOfCancellationSource()
        {
            if (cancellationSource != null)
            {
                if (!cancellationSource.IsCancellationRequested)
                {
                    cancellationSource.Cancel();
                }

                cancellationSource.Dispose();
                cancellationSource = null;
            }
        }

        /// <summary>
        /// Provides specialized exception handling.
        /// </summary>
        /// <param name="capturedException">The captured exception</param>
        private void HandleException(ExceptionDispatchInfo capturedException)
        {
            try
            {
                ErrorRecord errorRecord;
                var cloudException = capturedException.SourceException as CloudException;
                if (cloudException != null)
                {
                    errorRecord = CreateErrorRecordForCloudException(cloudException);
                }
                else
                {
                    var errorResponseException =
                            capturedException.SourceException as ErrorResponseMessageException;
                    if (errorResponseException != null)
                    {
                        errorRecord = errorResponseException.ToErrorRecord();
                    }
                    else
                    {
                        var aggregateException =
                                capturedException.SourceException as AggregateException;
                        if (aggregateException != null)
                        {
                            errorResponseException =
                                aggregateException.InnerException as ErrorResponseMessageException;
                            if (errorResponseException != null)
                            {
                                errorRecord = errorResponseException.ToErrorRecord();
                            }
                            else
                            {
                                errorRecord = new ErrorRecord(
                                                    aggregateException.InnerException,
                                                    aggregateException.InnerException.Message,
                                                    ErrorCategory.CloseError,
                                                    null);
                            }
                        }
                        else
                        {
                            errorRecord = new ErrorRecord(
                                                    capturedException.SourceException,
                                                    capturedException.SourceException.Message,
                                                    ErrorCategory.CloseError,
                                                    null);
                        }
                    }
                }

                WriteError(errorRecord);
            }
            finally
            {
                DisposeOfCancellationSource();
            }
        }


        /// <summary>
        /// Converts <see cref="CloudException"/> objects into <see cref="ErrorRecord"/>
        /// </summary>
        /// <param name="cloudException">The exception</param>
        private ErrorRecord CreateErrorRecordForCloudException(CloudException cloudException)
        {
            var errorReport = new StringBuilder();

            string requestId = cloudException.RequestId;
            if (string.IsNullOrWhiteSpace(requestId) && cloudException.Response != null)
            {
                // Try to obtain the request id from the HTTP response associated with the exception
                IEnumerable<string> headerValues = Enumerable.Empty<string>();
                if (cloudException.Response.Headers != null &&
                    cloudException.Response.Headers.TryGetValue("x-ms-request-id", out headerValues))
                {
                    requestId = headerValues.First();
                }
            }

            errorReport.AppendLine();
            errorReport.AppendLine("Request Id: {0}".FormatInvariant(requestId));
            if (cloudException.Body != null)
            {
                errorReport.AppendLine("Error Code: {0}".FormatInvariant(cloudException.Body.Code));
                errorReport.AppendLine("Error Message: {0}".FormatInvariant(cloudException.Body.Message));
                errorReport.AppendLine("Error Target: {0}".FormatInvariant(cloudException.Body.Target));
                if (cloudException.Body.Details.Any())
                {
                    errorReport.AppendLine("Error Details:");
                    foreach (var errorDetail in cloudException.Body.Details)
                    {
                        errorReport.AppendLine(
                                        "\t[Code={0}, Message={1}]".FormatInvariant(
                                                                        errorDetail.Code,
                                                                        errorDetail.Message));
                    }
                }
            }
            else
            {
                if (cloudException.Response != null)
                {
                    errorReport.AppendLine("Error Content: {0}".FormatInvariant(cloudException.Response.Content));
                }
            }

            var returnedError = new Exception(errorReport.ToString(), cloudException);
            return new ErrorRecord(returnedError, "Resource Provider Error", ErrorCategory.CloseError, null);
        }

        /// <summary>
        /// Test if an exception is a fatal exception. 
        /// </summary>
        /// <param name="ex">Exception object.</param>
        private bool IsFatalException(Exception ex)
        {
            if (ex is AggregateException)
            {
                return ((AggregateException)ex).Flatten().InnerExceptions.Any(exception => IsFatalException(exception));
            }

            if (ex.InnerException != null && IsFatalException(ex.InnerException))
            {
                return true;
            }

            return
                ex is TypeInitializationException ||
                ex is AppDomainUnloadedException ||
                ex is ThreadInterruptedException ||
                ex is AccessViolationException ||
                ex is InvalidProgramException ||
                ex is BadImageFormatException ||
                ex is StackOverflowException ||
                ex is ThreadAbortException ||
                ex is OutOfMemoryException ||
                ex is SecurityException ||
                ex is SEHException;
        }

        private void WriteVersionInfoToDebugChannel()
        {
            var versionInfo = MyInvocation.MyCommand.Module.Version;
            WriteDebug(Resources.VersionInfo.FormatInvariant(versionInfo.ToString(3)));
        }
    }
}
