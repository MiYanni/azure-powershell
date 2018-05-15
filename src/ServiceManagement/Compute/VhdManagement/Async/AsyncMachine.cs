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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Microsoft.WindowsAzure.Commands.Tools.Common.General
{
    //
    // The asynchronous machine below supports two modes, either all operations are required to complete or 
    // only a subset of them (quorum) is required to proceed. There is also a completion port optimization 
    // for a single asynchronous operation (CompletionPort.SingleOperation) to avoid unnecessary memory allocations.
    // 
    // Examples:
    //
    // Scenario 1: Single asynchronous operation without an explicit timeout
    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //
    // ...
    // AsyncMachine<Message> machine = new AsyncMachine<Message>();
    // machine.Start(this.ProcessMessage(asyncMachine, message));
    // ...
    //
    // IEnumerable<CompletionPort> ProcessMessage(AsyncMachine<Message> machine, Message message)
    // {
    //     ...
    //     WebRequest request = WebRequest.Create();
    //
    //     request.BeginGetResponse(machine.CompletionCallback, null);
    //
    //     yield return CompletionPort.SingleOperation;
    //
    //     ProcessWebResponse(request, machine.CompletionResult);
    //     ...
    //
    //
    // Scenario 2: Multiple asynchronous operations with time out
    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //
    // IEnumerable<CompletionPort> ProcessMessage(AsyncMachine<Message> machine, Message message)
    // {
    //     ...
    //     CompletionPort port = machine.CreateCompletionPort(TimeSpan.FromSeconds(10));
    //
    //     foreach (WebRequest request in pendingRequests)
    //     {
    //         request.BeginGetResponse(port[request].CompletionCallback, null);
    //     }
    //
    //     yield return port;
    //
    //     foreach (WebRequest request in pendingRequests)
    //     {
    //         // The operation is not completed if it's timed out
    //         if (port[request].IsCompleted)
    //         {
    //             ProcessWebResponse(request, port[request].CompletionResult);
    //         }
    //         else
    //         {
    //             port[request].Cancel(request.EndGetResponse);
    //         }
    //     }
    //     ...
    //
    //
    // Scenario 3: Multiple asynchronous operations with quorum and time out
    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //
    // IEnumerable<CompletionPort> ProcessMessage(AsyncMachine<Message> machine, Message message)
    // {
    //     ...
    //
    //     // The async machine will resume if 3 operations out of 5 will complete
    //     CompletionPort port = machine.CreateCompletionPort(5, 3, TimeSpan.FromSeconds(10));
    //
    //     for (int c = 0; c < 5; ++c)
    //     {
    //         WebRequest request = pendingRequests[c];
    //
    //         request.BeginGetResponse(port[request].CompletionCallback, null);
    //     }
    //
    //     yield return port;
    //
    //     for (int c = 0; c < 5; ++c)
    //     {
    //         WebRequest request = pendingRequests[c];
    //
    //         // The operation is not completed if it's timed out
    //         if (port[request].IsCompleted)
    //         {
    //             ProcessWebResponse(request, port[request].CompletionResult);
    //         }
    //         else
    //         {
    //             port[request].Cancel(request.EndGetResponse);
    //         }
    //     }
    //     ...
    //

    // Represents an asynchronous operation
    public class AsyncOperation
    {
        #region Constructors

        internal AsyncOperation(Action<AsyncOperation> checkOperationCompletion)
        {
            Debug.Assert(checkOperationCompletion != null);

            CheckOperationCompletion = checkOperationCompletion;
            CompletionCallback = AsyncCallback;
        }

        #endregion

        public AsyncCallback CompletionCallback { get; private set; }

        public IAsyncResult CompletionResult
        {
            get
            {
                if (completionResult == null)
                {
                    throw new InvalidOperationException("Completion result is not available yet.");
                }

                return completionResult;
            }
        }

        // Indicates whether a particular asynchronous operation is completed
        public bool IsCompleted { get; internal set; }

        // Cancels pending async operation meaning that we'll call the corresponding end operation whenever it completes
        // (since there is no currently real cancelation mechanism for .NET async operations)
        public void Cancel(AsyncCallback endAsyncOperation)
        {
            if (endAsyncOperation == null)
            {
                throw new ArgumentNullException("endAsyncOperation");
            }

            EndAsynchronousOperation = endAsyncOperation;

            if (CompletionResult != null && Interlocked.CompareExchange(ref isEndOperationCalled, True, False) == False)
            {
                // The operation is completed but end async operation is not called yet, need to call it
                EndAsynchronousOperation(CompletionResult);
            }
        }

        #region Private Members

        // Gets invoked when an asynchronous operation has completed
        private void AsyncCallback(IAsyncResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            completionResult = result;

            // this.CheckOperationCompletion will set AsyncOperation.IsCompleted to false if quorum is reached or timeout is expired,
            // and async machine has resumed its execution. In this case we'll consider the operation not completed in time and 
            // require user to schedule cancelation on completion port
            CheckOperationCompletion(this);

            if (!IsCompleted && EndAsynchronousOperation != null && Interlocked.CompareExchange(ref isEndOperationCalled, True, False) == False)
            {
                // Call an end of asynchronous operation method if callback above returned false, this means
                // it's us who is responsible for calling the end of asynchronous operation
                EndAsynchronousOperation(CompletionResult);
            }
        }

        private Action<AsyncOperation> CheckOperationCompletion { get; set; }
        private AsyncCallback EndAsynchronousOperation { get; set; }
        private IAsyncResult completionResult;
        private int isEndOperationCalled;

        const int True = -1;
        const int False = 0;

        #endregion
    }

    // Defines a completion port which keeps track of asynchronous operations which can be run concurrently
    // and results of their execution
    public class CompletionPort
    {
        #region Constructors

        internal CompletionPort(
            Action callback,
            int totalOperationsCount,
            int quorumOperationsCount,
            TimeSpan timeout
            )
        {
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }

            if (quorumOperationsCount > totalOperationsCount)
            {
                throw new ArgumentException("Invalid quorum operations count.");
            }

            AsyncMachineResume = callback;
            TotalOperationsCount = totalOperationsCount;
            QuorumOperationsCount = quorumOperationsCount;
            Timeout = timeout;

            OperationsLock = new object();
            Operations = new Dictionary<object, AsyncOperation>(totalOperationsCount);
        }

        internal CompletionPort(bool isSingleOperation)
        {
            Debug.Assert(isSingleOperation);

            IsSingleOperation = isSingleOperation;
        }

        #endregion

        // Gets an asynchronous operation instance to be used for a given actor
        public AsyncOperation this[object actor]
        {
            get
            {
                if (Operations == null)
                {
                    throw new InvalidOperationException("CompletionPort.SingleOperation is not mutable.");
                }

                AsyncOperation operation;

                lock (OperationsLock)
                {
                    if (!Operations.TryGetValue(actor, out operation))
                    {
                        // Check if async machine resume callback is already called, this means that
                        // this completion port is being reused which we don't allow
                        if (asyncMachineResumed == True)
                        {
                            throw new InvalidOperationException("Attempt to reuse a completion port.");
                        }

                        operation = new AsyncOperation(CheckOperationCompletion);
                        Operations.Add(actor, operation);
                    }
                }

                return operation;
            }
        }

        // Total number of operations in this completion token
        public int TotalOperationsCount { get; private set; }

        // Number of operations to treat as a success to advance
        public int QuorumOperationsCount { get; private set; }

        // Timeout indicating when wait on this completion port expires
        public TimeSpan Timeout { get; private set; }

        // Provides a completion port to be used for single asynchronous operations
        public static CompletionPort SingleOperation { get; private set; }

        #region Internal Members

        // Called by the async machine when this port is timed out and all operations have to marked as such
        internal void TimeOutPort()
        {
            if (Interlocked.CompareExchange(ref asyncMachineResumed, True, False) == False)
            {
                //
                // Async machine is not resumed at this point despite that some operations may be completed by this time.
                // We'll mark all operations as incomplete and resume async machine execution so that user would schedule
                // cancelation on all port operations.
                //

                lock (OperationsLock)
                {
                    foreach (var pair in Operations)
                    {
                        pair.Value.IsCompleted = false;
                    }
                }

                Debug.Assert(AsyncMachineResume != null);

                AsyncMachineResume();
            }
        }

        internal bool IsSingleOperation { get; private set; }
        internal static readonly TimeSpan InfiniteWait = new TimeSpan(0, 0, 0, 0, -1);

        #endregion

        #region Private Members

        // Creates a singleton instance of completion port which can be used when async machine runs single 
        // asynchronous operations
        static CompletionPort()
        {
            SingleOperation = new CompletionPort(true);
        }

        // Invoked by a completed asynchronous operation
        private void CheckOperationCompletion(AsyncOperation operation)
        {
            // Mark async operation as completed if async machine is not resumed yet
            operation.IsCompleted = asyncMachineResumed == False;

            if (operation.IsCompleted)
            {
                //
                // Check if all or quorum (in case we're in quorum mode) number of operations have completed,
                // in this case we need to notify the state machine that it can resume
                //

                Debug.Assert(QuorumOperationsCount > 0);

                if (Interlocked.Increment(ref completedOperations) >= QuorumOperationsCount &&
                    Interlocked.CompareExchange(ref asyncMachineResumed, True, False) == False)
                {
                    Debug.Assert(AsyncMachineResume != null);

                    AsyncMachineResume();
                }
            }
        }

        private Action AsyncMachineResume { get; set; }
        private Dictionary<object, AsyncOperation> Operations { get; set; }
        private object OperationsLock { get; set; }
        private int asyncMachineResumed;
        private int completedOperations;

        const int True = -1;
        const int False = 0;

        #endregion
    }

    public delegate IEnumerable<CompletionPort> MachineEngine(AsyncMachine machine);
    public delegate IEnumerable<CompletionPort> MachineEngine<P>(AsyncMachine machine, P param);
    public delegate IEnumerable<CompletionPort> MachineEngine<P1, P2>(AsyncMachine machine, P1 param1, P2 param2);
    public delegate IEnumerable<CompletionPort> MachineEngine<P1, P2, P3>(AsyncMachine machine, P1 param1, P2 param2, P3 param3);
    public delegate IEnumerable<CompletionPort> MachineEngine<P1, P2, P3, P4>(AsyncMachine machine, P1 param1, P2 param2, P3 param3, P4 param4);
    public delegate IEnumerable<CompletionPort> MachineEngine<P1, P2, P3, P4, P5>(AsyncMachine machine, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5);
    public delegate IEnumerable<CompletionPort> MachineEngine<P1, P2, P3, P4, P5, P6>(AsyncMachine machine, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6);
    public delegate IEnumerable<CompletionPort> MachineEngine<P1, P2, P3, P4, P5, P6, P7>(AsyncMachine machine, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7);
    public delegate IEnumerable<CompletionPort> MachineEngine<P1, P2, P3, P4, P5, P6, P7, P8>(AsyncMachine machine, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8);
    public delegate IEnumerable<CompletionPort> MachineEngine<P1, P2, P3, P4, P5, P6, P7, P8, P9>(AsyncMachine machine, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, P9 param9);
    public delegate IEnumerable<CompletionPort> MachineEngine<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>(AsyncMachine machine, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, P9 param9, P10 param10);
    public delegate IEnumerable<CompletionPort> MachineEngine<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>(AsyncMachine machine, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, P9 param9, P10 param10, P11 param11);

    // General purpose async machine which supports single (optimized) and multiple 
    // asynchronous operations
    public class AsyncMachine : IAsyncResult, IDisposable
    {
        #region Helper static methods

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "EndAsyncMachine does the disposing")]
        public static IAsyncResult BeginAsyncMachine(MachineEngine engine, AsyncCallback callback, object asyncState)
        {
            AsyncMachine machine = new AsyncMachine(callback, asyncState);
            machine.Start(engine(machine));
            return machine;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "EndAsyncMachine does the disposing")]
        public static IAsyncResult BeginAsyncMachine<P>(MachineEngine<P> engine, P param, AsyncCallback callback, object asyncState)
        {
            AsyncMachine machine = new AsyncMachine(callback, asyncState);
            machine.Start(engine(machine, param));
            return machine;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "EndAsyncMachine does the disposing")]
        public static IAsyncResult BeginAsyncMachine<P1, P2>(MachineEngine<P1, P2> engine, P1 param1, P2 param2, AsyncCallback callback, object asyncState)
        {
            AsyncMachine machine = new AsyncMachine(callback, asyncState);
            machine.Start(engine(machine, param1, param2));
            return machine;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "EndAsyncMachine does the disposing")]
        public static IAsyncResult BeginAsyncMachine<P1, P2, P3>(MachineEngine<P1, P2, P3> engine, P1 param1, P2 param2, P3 param3, AsyncCallback callback, object asyncState)
        {
            AsyncMachine machine = new AsyncMachine(callback, asyncState);
            machine.Start(engine(machine, param1, param2, param3));
            return machine;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "EndAsyncMachine does the disposing")]
        public static IAsyncResult BeginAsyncMachine<P1, P2, P3, P4>(MachineEngine<P1, P2, P3, P4> engine, P1 param1, P2 param2, P3 param3, P4 param4, AsyncCallback callback, object asyncState)
        {
            AsyncMachine machine = new AsyncMachine(callback, asyncState);
            machine.Start(engine(machine, param1, param2, param3, param4));
            return machine;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "EndAsyncMachine does the disposing")]
        public static IAsyncResult BeginAsyncMachine<P1, P2, P3, P4, P5>(MachineEngine<P1, P2, P3, P4, P5> engine, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, AsyncCallback callback, object asyncState)
        {
            AsyncMachine machine = new AsyncMachine(callback, asyncState);
            machine.Start(engine(machine, param1, param2, param3, param4, param5));
            return machine;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "EndAsyncMachine does the disposing")]
        public static IAsyncResult BeginAsyncMachine<P1, P2, P3, P4, P5, P6>(MachineEngine<P1, P2, P3, P4, P5, P6> engine, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, AsyncCallback callback, object asyncState)
        {
            AsyncMachine machine = new AsyncMachine(callback, asyncState);
            machine.Start(engine(machine, param1, param2, param3, param4, param5, param6));
            return machine;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "EndAsyncMachine does the disposing")]
        public static IAsyncResult BeginAsyncMachine<P1, P2, P3, P4, P5, P6, P7>(MachineEngine<P1, P2, P3, P4, P5, P6, P7> engine, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, AsyncCallback callback, object asyncState)
        {
            AsyncMachine machine = new AsyncMachine(callback, asyncState);
            machine.Start(engine(machine, param1, param2, param3, param4, param5, param6, param7));
            return machine;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "EndAsyncMachine does the disposing")]
        public static IAsyncResult BeginAsyncMachine<P1, P2, P3, P4, P5, P6, P7, P8>(MachineEngine<P1, P2, P3, P4, P5, P6, P7, P8> engine, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, AsyncCallback callback, object asyncState)
        {
            AsyncMachine machine = new AsyncMachine(callback, asyncState);
            machine.Start(engine(machine, param1, param2, param3, param4, param5, param6, param7, param8));
            return machine;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "EndAsyncMachine does the disposing")]
        public static IAsyncResult BeginAsyncMachine<P1, P2, P3, P4, P5, P6, P7, P8, P9>(MachineEngine<P1, P2, P3, P4, P5, P6, P7, P8, P9> engine, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, P9 param9, AsyncCallback callback, object asyncState)
        {
            AsyncMachine machine = new AsyncMachine(callback, asyncState);
            machine.Start(engine(machine, param1, param2, param3, param4, param5, param6, param7, param8, param9));
            return machine;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "EndAsyncMachine does the disposing")]
        public static IAsyncResult BeginAsyncMachine<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>(MachineEngine<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10> engine, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, P9 param9, P10 param10, AsyncCallback callback, object asyncState)
        {
            AsyncMachine machine = new AsyncMachine(callback, asyncState);
            machine.Start(engine(machine, param1, param2, param3, param4, param5, param6, param7, param8, param9, param10));
            return machine;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "EndAsyncMachine does the disposing")]
        public static IAsyncResult BeginAsyncMachine<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>(MachineEngine<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11> engine, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, P9 param9, P10 param10, P11 param11, AsyncCallback callback, object asyncState)
        {
            AsyncMachine machine = new AsyncMachine(callback, asyncState);
            machine.Start(engine(machine, param1, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11));
            return machine;
        }


        public static void EndAsyncMachine(IAsyncResult result)
        {
            using (AsyncMachine machine = (AsyncMachine)result)
            {
                machine.End();
            }
        }

        #endregion

        #region Constructors

        public AsyncMachine(AsyncCallback callback, object state)
        {
            asyncMachineCompletionCallback = callback;
            AsyncState = state;

            CompletionCallback = SingleOperationCompletionCallback;
            moveNextLock = new object();
        }

        #endregion


        // Create completion port with infinite timeout
        public CompletionPort CreateCompletionPort(int totalOperationsCount)
        {
            return CreateCompletionPort(totalOperationsCount, CompletionPort.InfiniteWait);
        }

        // Create completion port with specified timeout
        public CompletionPort CreateCompletionPort(int totalOperationsCount, TimeSpan timeout)
        {
            return CreateCompletionPort(totalOperationsCount, totalOperationsCount, timeout);
        }

        // Creates completion port in the quorum mode with a specified timeout (you can use CompletionPort.InfiniteWait
        // for infinite timeout)
        public CompletionPort CreateCompletionPort(int totalOperationsCount, int quorumOperationsCount)
        {
            return CreateCompletionPort(
                totalOperationsCount,
                quorumOperationsCount,
                CompletionPort.InfiniteWait);
        }

        // Creates completion port in the quorum mode with a specified timeout (you can use CompletionPort.InfiniteWait
        // for infinite timeout)
        public CompletionPort CreateCompletionPort(int totalOperationsCount, int quorumOperationsCount, TimeSpan timeout)
        {
            if (Enumerator == null)
            {
                throw new InvalidOperationException("Async machine either hasn't started yet or has already completed.");
            }

            return new CompletionPort(
                AsyncMachineResumeCallback,
                totalOperationsCount,
                quorumOperationsCount,
                timeout);
        }

        public delegate void ExceptionCleanupDelegate(object sender, EventArgs e);

        // Event which is fired when exception cleanup is required
        public event ExceptionCleanupDelegate ExceptionCleanup;

        // Starts executing of the async machine
        public void Start(IEnumerable<CompletionPort> machine)
        {
            Debug.Assert(null == Enumerator || IsCompleted);

            if (null == machine)
            {
                throw new ArgumentNullException("machine");
            }

            Enumerator = machine.GetEnumerator();
            IsCompleted = false;
            Error = null;

            if (waitHandle != null)
            {
                waitHandle.Reset();
            }

            lock (moveNextLock)
            {
                CompletedSynchronously = true;

                MoveNext();

                if (!IsCompleted)
                {
                    CompletedSynchronously = false;
                }
            }
        }

        // Ends the async machine and waits for its completion if necessary
        public void End()
        {
            if (!IsCompleted)
            {
                AsyncWaitHandle.WaitOne();
            }

            Debug.Assert(IsCompleted);

            if (Error != null)
            {
                throw Error.PrepareServerStackForRethrow();
            }
        }

        // A callback supplied to a single asynchronous operation to get notified when it's completed
        public AsyncCallback CompletionCallback { get; private set; }

        // A result of completed single asynchronous operation when AsyncMachine.CompletionCallback is used
        public IAsyncResult CompletionResult
        {
            get
            {
                if (completionResult == null)
                {
                    throw new InvalidOperationException("Completion result is not available yet.");
                }

                return completionResult;
            }
        }

        #region IAsyncResult Members

        public object AsyncState { get; private set; }

        public WaitHandle AsyncWaitHandle
        {
            get
            {
                if (null == waitHandle)
                {
                    lock (moveNextLock)
                    {
                        if (null == waitHandle)
                        {
                            waitHandle = new EventWaitHandle(IsCompleted, EventResetMode.ManualReset);
                        }
                    }
                }

                return waitHandle;
            }
        }

        public bool CompletedSynchronously { get; private set; }

        public bool IsCompleted { get; private set; }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                if (waitHandle != null)
                {
                    waitHandle.Close();
                    waitHandle = null;
                }

                if (WakeUpTimer != null)
                {
                    WakeUpTimer.Dispose();
                    WakeUpTimer = null;
                }
            }
        }

        #endregion

        #region Private Members

        private void MoveNext()
        {
            lock (moveNextLock)
            {
                using (EnterThreadContext())
                {
                    MoveNextInternal();
                }
            }
        }

        private void MoveNextInternal()
        {
            Debug.Assert(Enumerator != null);
            Debug.Assert(!IsCompleted);
            Debug.Assert(null == Error);

            if (!InsideMoveNext)
            {
                try
                {
                    try
                    {
                        InsideMoveNext = true;
                        IsCompleted = !Enumerator.MoveNext();

                        if (!IsCompleted)
                        {
                            if (Enumerator.Current == null)
                            {
                                throw new InvalidOperationException("Completion port for current iteration is null.");
                            }

                            if (!Enumerator.Current.IsSingleOperation)
                            {
                                if (Enumerator.Current.TotalOperationsCount == 0)
                                {
                                    // No operations are specified on the port, do another machine iteration
                                    InsideMoveNext = false;
                                    MoveNext();
                                    return;
                                }
                                else
                                {
                                    // Check if we should schedule a wake up for the current completion port. That needs to happen
                                    // when we use non-optimized completion port with a timeout
                                    if (!Enumerator.Current.Timeout.Equals(CompletionPort.InfiniteWait))
                                    {
                                        if (WakeUpTimer != null)
                                        {
                                            WakeUpTimer.Dispose();
                                        }

                                        //
                                        // BUGBUG: Too expensive. Use single active task to implement timers
                                        //
                                        WakeUpTimer = new Timer(
                                            CompletionPortTimeoutCallback,
                                            Enumerator.Current,
                                            Enumerator.Current.Timeout,
                                            CompletionPort.InfiniteWait);
                                    }
                                }
                            }
                        }
                    }
                    finally
                    {
                        InsideMoveNext = false;
                    }
                }
                catch (Exception e)
                {
                    if (e.IsCritical())
                    {
                        Trace.TraceError("Failed to forward message: {0}", e);
                        throw;
                    }

                    Trace.TraceWarning("Failed to forward message: {0}", e);

                    Error = e;
                    IsCompleted = true;

                    if (ExceptionCleanup != null)
                    {
                        ExceptionCleanup(this, EventArgs.Empty);
                    }
                }

                CompletedMoveNext();

                if (AsyncMethodCompletedSynchronously && !IsCompleted)
                {
                    AsyncMethodCompletedSynchronously = false;

                    MoveNext();
                }
            }
            else
            {
                //
                // Call to asynchronous I/O was completed synchronously (i.e. this thread is currently executing MoveNext down the stack)
                //

                Debug.Assert(!AsyncMethodCompletedSynchronously);

                AsyncMethodCompletedSynchronously = true;
            }
        }

        private void CompletedMoveNext()
        {
            if (IsCompleted)
            {
                Enumerator = null;

                if (null != waitHandle)
                {
                    waitHandle.Set();
                }

                if (asyncMachineCompletionCallback != null)
                {
                    asyncMachineCompletionCallback(this);
                }
            }
        }

        // Called by the WakeUpTimer when a scheduled timeout wait for the given completion
        // port is expired
        private void CompletionPortTimeoutCallback(object state)
        {
            Debug.Assert(state != null);

            CompletionPort port = (CompletionPort)state;

            port.TimeOutPort();
        }

        // Called by the completion port when quorum of asynchronous operations are completed
        private void AsyncMachineResumeCallback()
        {
            // Check if quorum was reached with all operations finishing synchronously
            if (!InsideMoveNext)
            {
                IEnumerator<CompletionPort> enumerator = Enumerator;

                // It should never happen that this callback is called for CompletionPort with single operation
                if (enumerator != null && enumerator.Current != null && enumerator.Current.IsSingleOperation)
                {
                    throw new InvalidOperationException("CompletionPort.SingleOperation was used.");
                }
            }

            MoveNext();
        }

        // Called by the completed single asynchronous operation
        private void SingleOperationCompletionCallback(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }

            Debug.Assert(InsideMoveNext || Enumerator != null);
            Debug.Assert(InsideMoveNext || Enumerator.Current != null);

            completionResult = asyncResult;

            MoveNext();
        }

        #region Thread Context helpers

        private IDisposable EnterThreadContext()
        {
            return null;
        }
        #endregion

        private IEnumerator<CompletionPort> Enumerator { get; set; }
        private Exception Error { get; set; }
        private bool InsideMoveNext { get; set; }
        private bool AsyncMethodCompletedSynchronously { get; set; }
        private Timer WakeUpTimer { get; set; }
        private EventWaitHandle waitHandle;
        private IAsyncResult completionResult;
        private readonly AsyncCallback asyncMachineCompletionCallback;
        private readonly object moveNextLock;
        #endregion
    }

    #region Parameterized async machines

    public delegate IEnumerable<CompletionPort> MachineEngineT<T>(AsyncMachine<T> machine);
    public delegate IEnumerable<CompletionPort> MachineEngineT<T, P>(AsyncMachine<T> machine, P param);
    public delegate IEnumerable<CompletionPort> MachineEngineT<T, P1, P2>(AsyncMachine<T> machine, P1 param1, P2 param2);
    public delegate IEnumerable<CompletionPort> MachineEngineT<T, P1, P2, P3>(AsyncMachine<T> machine, P1 param1, P2 param2, P3 param3);
    public delegate IEnumerable<CompletionPort> MachineEngineT<T, P1, P2, P3, P4>(AsyncMachine<T> machine, P1 param1, P2 param2, P3 param3, P4 param4);
    public delegate IEnumerable<CompletionPort> MachineEngineT<T, P1, P2, P3, P4, P5>(AsyncMachine<T> machine, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5);
    public delegate IEnumerable<CompletionPort> MachineEngineT<T, P1, P2, P3, P4, P5, P6>(AsyncMachine<T> machine, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6);
    public delegate IEnumerable<CompletionPort> MachineEngineT<T, P1, P2, P3, P4, P5, P6, P7>(AsyncMachine<T> machine, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7);
    public delegate IEnumerable<CompletionPort> MachineEngineT<T, P1, P2, P3, P4, P5, P6, P7, P8>(AsyncMachine<T> machine, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8);
    public delegate IEnumerable<CompletionPort> MachineEngineT<T, P1, P2, P3, P4, P5, P6, P7, P8, P9>(AsyncMachine<T> machine, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, P9 param9);
    public delegate IEnumerable<CompletionPort> MachineEngineT<T, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>(AsyncMachine<T> machine, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, P9 param9, P10 param10);
    public delegate IEnumerable<CompletionPort> MachineEngineT<T, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>(AsyncMachine<T> machine, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, P9 param9, P10 param10, P11 param11);

    public sealed class AsyncMachine<T> : AsyncMachine
    {
        #region Helper static methods

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "EndAsyncMachine does the disposing")]
        public static IAsyncResult BeginAsyncMachine(MachineEngineT<T> engine, AsyncCallback callback, object asyncState)
        {
            AsyncMachine<T> machine = new AsyncMachine<T>(callback, asyncState);
            machine.Start(engine(machine));
            return machine;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "EndAsyncMachine does the disposing")]
        public static IAsyncResult BeginAsyncMachine<P>(MachineEngineT<T, P> engine, P param, AsyncCallback callback, object asyncState)
        {
            AsyncMachine<T> machine = new AsyncMachine<T>(callback, asyncState);
            machine.Start(engine(machine, param));
            return machine;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "EndAsyncMachine does the disposing")]
        public static IAsyncResult BeginAsyncMachine<P1, P2>(MachineEngineT<T, P1, P2> engine, P1 param1, P2 param2, AsyncCallback callback, object asyncState)
        {
            AsyncMachine<T> machine = new AsyncMachine<T>(callback, asyncState);
            machine.Start(engine(machine, param1, param2));
            return machine;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "EndAsyncMachine does the disposing")]
        public static IAsyncResult BeginAsyncMachine<P1, P2, P3>(MachineEngineT<T, P1, P2, P3> engine, P1 param1, P2 param2, P3 param3, AsyncCallback callback, object asyncState)
        {
            AsyncMachine<T> machine = new AsyncMachine<T>(callback, asyncState);
            machine.Start(engine(machine, param1, param2, param3));
            return machine;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "EndAsyncMachine does the disposing")]
        public static IAsyncResult BeginAsyncMachine<P1, P2, P3, P4>(MachineEngineT<T, P1, P2, P3, P4> engine, P1 param1, P2 param2, P3 param3, P4 param4, AsyncCallback callback, object asyncState)
        {
            AsyncMachine<T> machine = new AsyncMachine<T>(callback, asyncState);
            machine.Start(engine(machine, param1, param2, param3, param4));
            return machine;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "EndAsyncMachine does the disposing")]
        public static IAsyncResult BeginAsyncMachine<P1, P2, P3, P4, P5>(MachineEngineT<T, P1, P2, P3, P4, P5> engine, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, AsyncCallback callback, object asyncState)
        {
            AsyncMachine<T> machine = new AsyncMachine<T>(callback, asyncState);
            machine.Start(engine(machine, param1, param2, param3, param4, param5));
            return machine;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "EndAsyncMachine does the disposing")]
        public static IAsyncResult BeginAsyncMachine<P1, P2, P3, P4, P5, P6>(MachineEngineT<T, P1, P2, P3, P4, P5, P6> engine, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, AsyncCallback callback, object asyncState)
        {
            AsyncMachine<T> machine = new AsyncMachine<T>(callback, asyncState);
            machine.Start(engine(machine, param1, param2, param3, param4, param5, param6));
            return machine;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "EndAsyncMachine does the disposing")]
        public static IAsyncResult BeginAsyncMachine<P1, P2, P3, P4, P5, P6, P7>(MachineEngineT<T, P1, P2, P3, P4, P5, P6, P7> engine, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, AsyncCallback callback, object asyncState)
        {
            AsyncMachine<T> machine = new AsyncMachine<T>(callback, asyncState);
            machine.Start(engine(machine, param1, param2, param3, param4, param5, param6, param7));
            return machine;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "EndAsyncMachine does the disposing")]
        public static IAsyncResult BeginAsyncMachine<P1, P2, P3, P4, P5, P6, P7, P8>(MachineEngineT<T, P1, P2, P3, P4, P5, P6, P7, P8> engine, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, AsyncCallback callback, object asyncState)
        {
            AsyncMachine<T> machine = new AsyncMachine<T>(callback, asyncState);
            machine.Start(engine(machine, param1, param2, param3, param4, param5, param6, param7, param8));
            return machine;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "EndAsyncMachine does the disposing")]
        public static IAsyncResult BeginAsyncMachine<P1, P2, P3, P4, P5, P6, P7, P8, P9>(MachineEngineT<T, P1, P2, P3, P4, P5, P6, P7, P8, P9> engine, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, P9 param9, AsyncCallback callback, object asyncState)
        {
            AsyncMachine<T> machine = new AsyncMachine<T>(callback, asyncState);
            machine.Start(engine(machine, param1, param2, param3, param4, param5, param6, param7, param8, param9));
            return machine;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "EndAsyncMachine does the disposing")]
        public static IAsyncResult BeginAsyncMachine<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>(MachineEngineT<T, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10> engine, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, P9 param9, P10 param10, AsyncCallback callback, object asyncState)
        {
            AsyncMachine<T> machine = new AsyncMachine<T>(callback, asyncState);
            machine.Start(engine(machine, param1, param2, param3, param4, param5, param6, param7, param8, param9, param10));
            return machine;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "EndAsyncMachine does the disposing")]
        public static IAsyncResult BeginAsyncMachine<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>(MachineEngineT<T, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11> engine, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, P9 param9, P10 param10, P11 param11, AsyncCallback callback, object asyncState)
        {
            AsyncMachine<T> machine = new AsyncMachine<T>(callback, asyncState);
            machine.Start(engine(machine, param1, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11));
            return machine;
        }

        public new static T EndAsyncMachine(IAsyncResult result)
        {
            using (AsyncMachine<T> machine = (AsyncMachine<T>)result)
            {
                machine.End();
                return machine.ParameterValue;
            }
        }

        #endregion

        public AsyncMachine()
            : this(null, null)
        {
        }

        public AsyncMachine(AsyncCallback callback, object state)
            : base(callback, state)
        {
        }

        public T ParameterValue { get; set; }
    }

    public delegate IEnumerable<CompletionPort> MachineEngineRT<TReturn, T>(AsyncMachine<TReturn, T> machine);
    public delegate IEnumerable<CompletionPort> MachineEngineRT<TReturn, T, P>(AsyncMachine<TReturn, T> machine, P param);
    public delegate IEnumerable<CompletionPort> MachineEngineRT<TReturn, T, P1, P2>(AsyncMachine<TReturn, T> machine, P1 param1, P2 param2);
    public delegate IEnumerable<CompletionPort> MachineEngineRT<TReturn, T, P1, P2, P3>(AsyncMachine<TReturn, T> machine, P1 param1, P2 param2, P3 param3);
    public delegate IEnumerable<CompletionPort> MachineEngineRT<TReturn, T, P1, P2, P3, P4>(AsyncMachine<TReturn, T> machine, P1 param1, P2 param2, P3 param3, P4 param4);
    public delegate IEnumerable<CompletionPort> MachineEngineRT<TReturn, T, P1, P2, P3, P4, P5>(AsyncMachine<TReturn, T> machine, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5);
    public delegate IEnumerable<CompletionPort> MachineEngineRT<TReturn, T, P1, P2, P3, P4, P5, P6>(AsyncMachine<TReturn, T> machine, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6);
    public delegate IEnumerable<CompletionPort> MachineEngineRT<TReturn, T, P1, P2, P3, P4, P5, P6, P7>(AsyncMachine<TReturn, T> machine, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7);
    public delegate IEnumerable<CompletionPort> MachineEngineRT<TReturn, T, P1, P2, P3, P4, P5, P6, P7, P8>(AsyncMachine<TReturn, T> machine, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8);
    public delegate IEnumerable<CompletionPort> MachineEngineRT<TReturn, T, P1, P2, P3, P4, P5, P6, P7, P8, P9>(AsyncMachine<TReturn, T> machine, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, P9 param9);
    public delegate IEnumerable<CompletionPort> MachineEngineRT<TReturn, T, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>(AsyncMachine<TReturn, T> machine, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, P9 param9, P10 param10);
    public delegate IEnumerable<CompletionPort> MachineEngineRT<TReturn, T, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>(AsyncMachine<TReturn, T> machine, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, P9 param9, P10 param10, P11 param11);

    public sealed class AsyncMachine<TReturn, T> : AsyncMachine
    {
        #region Helper static methods

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "EndAsyncMachine does the disposing")]
        public static IAsyncResult BeginAsyncMachine(MachineEngineRT<TReturn, T> engine, AsyncCallback callback, object asyncState)
        {
            AsyncMachine<TReturn, T> machine = new AsyncMachine<TReturn, T>(callback, asyncState);
            machine.Start(engine(machine));
            return machine;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "EndAsyncMachine does the disposing")]
        public static IAsyncResult BeginAsyncMachine<P>(MachineEngineRT<TReturn, T, P> engine, P param, AsyncCallback callback, object asyncState)
        {
            AsyncMachine<TReturn, T> machine = new AsyncMachine<TReturn, T>(callback, asyncState);
            machine.Start(engine(machine, param));
            return machine;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "EndAsyncMachine does the disposing")]
        public static IAsyncResult BeginAsyncMachine<P1, P2>(MachineEngineRT<TReturn, T, P1, P2> engine, P1 param1, P2 param2, AsyncCallback callback, object asyncState)
        {
            AsyncMachine<TReturn, T> machine = new AsyncMachine<TReturn, T>(callback, asyncState);
            machine.Start(engine(machine, param1, param2));
            return machine;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "EndAsyncMachine does the disposing")]
        public static IAsyncResult BeginAsyncMachine<P1, P2, P3>(MachineEngineRT<TReturn, T, P1, P2, P3> engine, P1 param1, P2 param2, P3 param3, AsyncCallback callback, object asyncState)
        {
            AsyncMachine<TReturn, T> machine = new AsyncMachine<TReturn, T>(callback, asyncState);
            machine.Start(engine(machine, param1, param2, param3));
            return machine;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "EndAsyncMachine does the disposing")]
        public static IAsyncResult BeginAsyncMachine<P1, P2, P3, P4>(MachineEngineRT<TReturn, T, P1, P2, P3, P4> engine, P1 param1, P2 param2, P3 param3, P4 param4, AsyncCallback callback, object asyncState)
        {
            AsyncMachine<TReturn, T> machine = new AsyncMachine<TReturn, T>(callback, asyncState);
            machine.Start(engine(machine, param1, param2, param3, param4));
            return machine;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "EndAsyncMachine does the disposing")]
        public static IAsyncResult BeginAsyncMachine<P1, P2, P3, P4, P5>(MachineEngineRT<TReturn, T, P1, P2, P3, P4, P5> engine, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, AsyncCallback callback, object asyncState)
        {
            AsyncMachine<TReturn, T> machine = new AsyncMachine<TReturn, T>(callback, asyncState);
            machine.Start(engine(machine, param1, param2, param3, param4, param5));
            return machine;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "EndAsyncMachine does the disposing")]
        public static IAsyncResult BeginAsyncMachine<P1, P2, P3, P4, P5, P6>(MachineEngineRT<TReturn, T, P1, P2, P3, P4, P5, P6> engine, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, AsyncCallback callback, object asyncState)
        {
            AsyncMachine<TReturn, T> machine = new AsyncMachine<TReturn, T>(callback, asyncState);
            machine.Start(engine(machine, param1, param2, param3, param4, param5, param6));
            return machine;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "EndAsyncMachine does the disposing")]
        public static IAsyncResult BeginAsyncMachine<P1, P2, P3, P4, P5, P6, P7>(MachineEngineRT<TReturn, T, P1, P2, P3, P4, P5, P6, P7> engine, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, AsyncCallback callback, object asyncState)
        {
            AsyncMachine<TReturn, T> machine = new AsyncMachine<TReturn, T>(callback, asyncState);
            machine.Start(engine(machine, param1, param2, param3, param4, param5, param6, param7));
            return machine;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "EndAsyncMachine does the disposing")]
        public static IAsyncResult BeginAsyncMachine<P1, P2, P3, P4, P5, P6, P7, P8>(MachineEngineRT<TReturn, T, P1, P2, P3, P4, P5, P6, P7, P8> engine, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, AsyncCallback callback, object asyncState)
        {
            AsyncMachine<TReturn, T> machine = new AsyncMachine<TReturn, T>(callback, asyncState);
            machine.Start(engine(machine, param1, param2, param3, param4, param5, param6, param7, param8));
            return machine;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "EndAsyncMachine does the disposing")]
        public static IAsyncResult BeginAsyncMachine<P1, P2, P3, P4, P5, P6, P7, P8, P9>(MachineEngineRT<TReturn, T, P1, P2, P3, P4, P5, P6, P7, P8, P9> engine, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, P9 param9, AsyncCallback callback, object asyncState)
        {
            AsyncMachine<TReturn, T> machine = new AsyncMachine<TReturn, T>(callback, asyncState);
            machine.Start(engine(machine, param1, param2, param3, param4, param5, param6, param7, param8, param9));
            return machine;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "EndAsyncMachine does the disposing")]
        public static IAsyncResult BeginAsyncMachine<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>(MachineEngineRT<TReturn, T, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10> engine, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, P9 param9, P10 param10, AsyncCallback callback, object asyncState)
        {
            AsyncMachine<TReturn, T> machine = new AsyncMachine<TReturn, T>(callback, asyncState);
            machine.Start(engine(machine, param1, param2, param3, param4, param5, param6, param7, param8, param9, param10));
            return machine;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "EndAsyncMachine does the disposing")]
        public static IAsyncResult BeginAsyncMachine<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>(MachineEngineRT<TReturn, T, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11> engine, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, P9 param9, P10 param10, P11 param11, AsyncCallback callback, object asyncState)
        {
            AsyncMachine<TReturn, T> machine = new AsyncMachine<TReturn, T>(callback, asyncState);
            machine.Start(engine(machine, param1, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11));
            return machine;
        }

        public static TReturn EndAsyncMachine(IAsyncResult result, out T parameterValue)
        {
            using (AsyncMachine<TReturn, T> machine = (AsyncMachine<TReturn, T>)result)
            {
                machine.End();
                parameterValue = machine.ParameterValue;
                return machine.ReturnValue;
            }
        }

        #endregion

        public AsyncMachine()
            : this(null, null)
        {
        }

        public AsyncMachine(AsyncCallback callback, object state)
            : base(callback, state)
        {
        }

        public TReturn ReturnValue { get; set; }
        public T ParameterValue { get; set; }
    }

    #endregion
}
