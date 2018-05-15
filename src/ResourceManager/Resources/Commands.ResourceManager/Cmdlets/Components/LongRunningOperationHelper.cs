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

namespace Microsoft.Azure.Commands.ResourceManager.Cmdlets.Components
{
    using Commands.Common.Authentication.Abstractions;
    using Collections;
    using Entities.Resources;
    using Extensions;
    using RestClients;
    using Utilities;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Management.Automation;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// A helper class for tracking long running operations.
    /// </summary>
    internal class LongRunningOperationHelper
    {
        /// <summary>
        /// The default retry after interval.
        /// </summary>
        private static readonly TimeSpan DefaultRetryAfter = TimeSpan.FromSeconds(60);

        /// <summary>
        /// A delegate for fetching the resources client.
        /// </summary>
        private Func<ResourceManagerRestRestClient> ResourcesClientFactory { get; set; }

        /// <summary>
        /// The cancellation token.
        /// </summary>
        private CancellationToken CancellationToken { get; set; }

        /// <summary>
        /// The progress tracker.
        /// </summary>
        private ProgressTracker ProgressTrackerObject { get; set; }

        /// <summary>
        /// A value indicating if this is a resource create.
        /// </summary>
        private bool IsResourceCreateOrUpdate { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LongRunningOperationHelper"/> class.
        /// </summary>
        /// <param name="activityName">The name of the activity to report in progress.</param>
        /// <param name="resourcesClientFactory">A delegate for fetching the resources client/</param>
        /// <param name="writeProgressAction">A delegate for writing progress.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="isResourceCreateOrUpdate">Set to true if this tracker will be used to track a resource creation.</param>
        internal LongRunningOperationHelper(string activityName, Func<ResourceManagerRestRestClient> resourcesClientFactory, Action<ProgressRecord> writeProgressAction, CancellationToken cancellationToken, bool isResourceCreateOrUpdate)
        {
            ResourcesClientFactory = resourcesClientFactory;
            CancellationToken = cancellationToken;
            ProgressTrackerObject = new ProgressTracker(activityName, resourcesClientFactory, writeProgressAction, cancellationToken);
            IsResourceCreateOrUpdate = isResourceCreateOrUpdate;
        }

        /// <summary>
        /// Waits for the operation to complete.
        /// </summary>
        /// <param name="operationResult">The operation result.</param>
        internal string WaitOnOperation(OperationResult operationResult)
        {
            // TODO: Re-factor this mess.
            ProgressTrackerObject.UpdateProgress("Starting", 0);

            var trackingResult = HandleOperationResponse(operationResult, IsResourceCreateOrUpdate ? operationResult.OperationUri : operationResult.LocationUri);

            while (trackingResult != null && trackingResult.ShouldWait)
            {
                operationResult =
                    GetResourcesClient()
                        .GetOperationResult<JToken>(trackingResult.TrackingUri, CancellationToken)
                        .Result;

                trackingResult = HandleOperationResponse(operationResult, trackingResult.TrackingUri);

                if (trackingResult.ShouldWait && trackingResult.RetryAfter != null)
                {
                    Task.Delay(trackingResult.RetryAfter.Value, CancellationToken).Wait(CancellationToken);
                }
            }

            ProgressTrackerObject.UpdateProgress("Complete", 100);

            return operationResult.Value;
        }

        /// <summary>
        /// Handles the operation response/
        /// </summary>
        /// <param name="operationResult">The result of the operation.</param>
        /// <param name="lastRequestUri">The last <see cref="Uri"/> that was used to request operation state.</param>
        private TrackingOperationResult HandleOperationResponse(OperationResult operationResult, Uri lastRequestUri)
        {
            if (!operationResult.HttpStatusCode.IsSuccessfulRequest())
            {
                if (operationResult.HttpStatusCode.IsServerFailureRequest() ||
                    operationResult.HttpStatusCode == HttpStatusCodeExt.TooManyRequests)
                {
                    var result = new TrackingOperationResult
                    {
                        ShouldWait = true,
                        Failed = false,
                        RetryAfter = operationResult.RetryAfter ?? DefaultRetryAfter,
                        TrackingUri = lastRequestUri,
                        OperationResult = operationResult,
                    };

                    UpdateProgress(result);
                    return result;
                }
                FailedResult(
                    operationResult,
                    string.Format("The operation failed because the resource provider returned an unexpected HTTP status code of: '{0}'.", (int)operationResult.HttpStatusCode));
            }

            if (operationResult.HttpStatusCode == HttpStatusCode.Accepted)
            {
                return WaitResult(operationResult);
            }

            if (operationResult.HttpStatusCode != HttpStatusCode.OK &&
                operationResult.HttpStatusCode != HttpStatusCode.Created && !IsResourceCreateOrUpdate && operationResult.HttpStatusCode != HttpStatusCode.NoContent)
            {
                FailedResult(
                    operationResult,
                    string.Format("The operation failed because the resource provider returned an unexpected HTTP status code of: '{0}'.", (int)operationResult.HttpStatusCode));
            }

            return IsResourceCreateOrUpdate
                ? HandleCreateOrUpdateResponse(operationResult)
                : SuccessfulResult(operationResult);

        }

        /// <summary>
        /// Extra code for handling resource create/update calls.
        /// </summary>
        /// <param name="operationResult">The result of the operation.</param>
        private TrackingOperationResult HandleCreateOrUpdateResponse(OperationResult operationResult)
        {
            Resource<InsensitiveDictionary<JToken>> resource;
            if (!operationResult.Value.TryConvertTo<Resource<InsensitiveDictionary<JToken>>>(out resource))
            {
                return null;
            }

            if (resource == null && operationResult.HttpStatusCode == HttpStatusCode.Created)
            {
                return SuccessfulResult(operationResult);
            }

            if (resource == null)
            {
                FailedResult(
                    operationResult,
                    "The operation failed because resource could not be de-serialized.");
            }

            resource.Properties = resource.Properties ?? new InsensitiveDictionary<JToken>();

            JToken provisioningStateJToken;
            if (resource.Properties.TryGetValue("provisioningState", out provisioningStateJToken))
            {
                TerminalProvisioningStates resourceProvisioningState;
                if (Enum.TryParse(provisioningStateJToken.ToString(), true, out resourceProvisioningState))
                {
                    if (resourceProvisioningState == TerminalProvisioningStates.Succeeded ||
                        resourceProvisioningState == TerminalProvisioningStates.Ready)
                    {
                        return SuccessfulResult(operationResult);
                    }

                    if (resourceProvisioningState == TerminalProvisioningStates.Failed ||
                        resourceProvisioningState == TerminalProvisioningStates.Canceled)
                    {
                        FailedResult(
                            operationResult,
                            string.Format("The operation failed because resource is in the: '{0}' state. Please check the logs for more details.", provisioningStateJToken));
                    }
                }
                else
                {
                    return WaitResult(operationResult);
                }
            }

            return SuccessfulResult(operationResult);
        }

        /// <summary>
        /// Indicates a successful terminal state.
        /// </summary>
        /// <param name="operationResult">The result of the operation.</param>
        private TrackingOperationResult SuccessfulResult(OperationResult operationResult)
        {
            var result = new TrackingOperationResult
            {
                ShouldWait = false,
                Failed = false,
                RetryAfter = operationResult.RetryAfter ?? DefaultRetryAfter,
                TrackingUri = operationResult.LocationUri ?? operationResult.OperationUri,
                OperationResult = operationResult,
            };

            UpdateProgress(result);
            return result;
        }

        /// <summary>
        /// Indicates an in-progress state.
        /// </summary>
        /// <param name="operationResult">The result of the operation.</param>
        private TrackingOperationResult WaitResult(OperationResult operationResult)
        {
            var result = new TrackingOperationResult
            {
                ShouldWait = true,
                Failed = false,
                RetryAfter = operationResult.RetryAfter ?? DefaultRetryAfter,
                TrackingUri = operationResult.LocationUri ?? operationResult.OperationUri,
                OperationResult = operationResult,
            };

            UpdateProgress(result);
            return result;
        }

        /// <summary>
        /// Indicates a failed terminal state.
        /// </summary>
        /// <param name="operationResult">The result of the operation.</param>
        /// <param name="message">The exception message.</param>
        private void FailedResult(OperationResult operationResult, string message)
        {
            var result = new TrackingOperationResult
            {
                ShouldWait = false,
                Failed = true,
                RetryAfter = operationResult.RetryAfter ?? DefaultRetryAfter,
                TrackingUri = operationResult.LocationUri ?? operationResult.OperationUri,
                OperationResult = operationResult,
            };

            UpdateProgress(result);

            throw new InvalidOperationException(message);
        }

        /// <summary>
        /// Gets the resources client.
        /// </summary>
        private ResourceManagerRestRestClient GetResourcesClient()
        {
            return ResourcesClientFactory();
        }

        /// <summary>
        /// Logs the fact that the operation has progressed.
        /// </summary>
        /// <param name="result">The operation result</param>
        private void UpdateProgress(TrackingOperationResult result)
        {
            ProgressTrackerObject.UpdateProgress(result, IsResourceCreateOrUpdate);
        }

        /// <summary>
        /// The progress tracker.
        /// </summary>
        private class ProgressTracker
        {
            /// <summary>
            /// A delegate for fetching the resources client.
            /// </summary>
            private Func<ResourceManagerRestRestClient> ResourcesClientFactory { get; set; }

            /// <summary>
            /// A delegate for writing progress to PowerShell.
            /// </summary>
            private Action<ProgressRecord> WriteProgressAction { get; set; }

            /// <summary>
            /// The cancellation token.
            /// </summary>
            private CancellationToken CancellationToken { get; set; }

            /// <summary>
            /// The progress record.
            /// </summary>
            private ProgressRecord ProgressRecord { get; set; }

            /// <summary>
            /// The last state that was observed.
            /// </summary>
            private string LastState { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="ProgressTracker"/> class.
            /// </summary>
            /// <param name="activityName">The name of the activity to report in progress.</param>
            /// <param name="resourcesClientFactory">A delegate for fetching the resources client/</param>
            /// <param name="writeProgressAction">A delegate for writing progress.</param>
            /// <param name="cancellationToken">The cancellation token.</param>
            internal ProgressTracker(string activityName, Func<ResourceManagerRestRestClient> resourcesClientFactory, Action<ProgressRecord> writeProgressAction, CancellationToken cancellationToken)
            {
                ResourcesClientFactory = resourcesClientFactory;
                WriteProgressAction = writeProgressAction;
                CancellationToken = cancellationToken;
                LastState = "Starting";
                ProgressRecord = new ProgressRecord(0, activityName, "Starting - 0.00% completed.");
            }

            internal void SetProgressPercentageAndWriteProgress(double percentage)
            {
                SetProgressRecordPercentComplete(percentage);
                WriteProgressAction(ProgressRecord);
            }

            /// <summary>
            /// Logs the fact that the operation has progressed.
            /// </summary>
            /// <param name="result">The operation result</param>
            /// <param name="isResourceCreateOrUpdate">Is Create or Update operation, other option include Move etc.</param>
            internal void UpdateProgress(TrackingOperationResult result, bool isResourceCreateOrUpdate)
            {
                if (isResourceCreateOrUpdate)
                {
                    var currentState = GetOperationState(result.OperationResult);

                    if (result.Failed || currentState == null || !LastState.EqualsInsensitively(currentState))
                    {
                        SetProgressPercentageAndWriteProgress(100.0);
                    }

                    if (currentState == null)
                    {
                        return;
                    }

                    LastState = currentState;
                }
                else
                {
                    if(result.Failed)
                    {
                        SetProgressPercentageAndWriteProgress(100.0);
                    }
                }

                SetProgressRecordPercentComplete(result.OperationResult.PercentComplete);
                WriteProgressAction(ProgressRecord);
            }

            /// <summary>
            /// Updates the progress of the operation.
            /// </summary>
            /// <param name="currentState">The current state.</param>
            /// <param name="percentCompleted">The percent completed.</param>
            internal void UpdateProgress(string currentState, double percentCompleted)
            {
                if (currentState == null || !LastState.EqualsInsensitively(currentState))
                {
                    SetProgressRecordPercentComplete(100.0);
                    WriteProgressAction(ProgressRecord);
                }

                if (currentState == null)
                {
                    return;
                }

                LastState = currentState;
                SetProgressRecordPercentComplete(percentCompleted);
                WriteProgressAction(ProgressRecord);
            }

            /// <summary>
            /// Sets the progress completed and updates the status description.
            /// </summary>
            /// <param name="percentComplete">The percent completed.</param>
            private void SetProgressRecordPercentComplete(double? percentComplete)
            {
                if (percentComplete == null)
                {
                    return;
                }

                var value = Math.Max(Math.Min(100.0, percentComplete.Value), 0.0);

                ProgressRecord.PercentComplete = (int)value;

                ProgressRecord.StatusDescription = string.Format("{0} - {1:P2} completed.", LastState, value / 100.0);

                ProgressRecord.RecordType = 100.0 - value <= double.Epsilon
                    ? ProgressRecordType.Completed
                    : ProgressRecordType.Processing;
            }

            /// <summary>
            /// Extra code for handling resource create/update calls.
            /// </summary>
            /// <param name="operationResult">The result of the operation.</param>
            private string GetOperationState(OperationResult operationResult)
            {
                return operationResult.AzureAsyncOperationUri != null
                    ? GetAzureAsyncOperationState(operationResult)
                    : GetResourceState(operationResult);
            }

            /// <summary>
            /// Gets the azure async operation state.
            /// </summary>
            /// <param name="operationResult">The operation result.</param>
            private string GetAzureAsyncOperationState(OperationResult operationResult)
            {
                try
                {
                    var result = ResourcesClientFactory()
                        .GetAzureAsyncOperationResource(operationResult.AzureAsyncOperationUri, CancellationToken)
                        .Result;

                    return result == null
                        ? null
                        : result.Status;
                }
                catch (Exception e)
                {
                    if (e.IsFatal())
                    {
                        throw;
                    }

                    return null;
                }
            }

            /// <summary>
            /// Gets the state from the resource.
            /// </summary>
            /// <param name="operationResult">The operation result.</param>
            private static string GetResourceState(OperationResult operationResult)
            {
                Resource<InsensitiveDictionary<JToken>> resource;
                if (!operationResult.Value.TryConvertTo<Resource<InsensitiveDictionary<JToken>>>(out resource))
                {
                    return null;
                }

                if (resource == null)
                {
                    return null;
                }

                resource.Properties = resource.Properties ?? new InsensitiveDictionary<JToken>();
                JToken provisioningStateJToken;

                return resource.Properties.TryGetValue("provisioningState", out provisioningStateJToken)
                    ? provisioningStateJToken.ToString()
                    : null;
            }
        }

        /// <summary>
        /// A simple class that is used to track progress.
        /// </summary>
        private class TrackingOperationResult
        {
            /// <summary>
            /// Gets or sets the retry interval.
            /// </summary>
            internal TimeSpan? RetryAfter { get; set; }

            /// <summary>
            /// Gets or sets the tracking <see cref="Uri"/>.
            /// </summary>
            internal Uri TrackingUri { get; set; }

            /// <summary>
            /// Gets or sets the operation result.
            /// </summary>
            internal OperationResult OperationResult { get; set; }

            /// <summary>
            /// Gets or sets a value that indicates if the polling method should continue to wait.
            /// </summary>
            internal bool ShouldWait { get; set; }

            /// <summary>
            /// Gets or sets a method that indicates that the operation has terminally failed.
            /// </summary>
            internal bool Failed { get; set; }
        }
    }
}
