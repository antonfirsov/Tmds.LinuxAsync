using System.Diagnostics;
using System.Threading;
using Tmds.LinuxAsync.Tracing;

namespace Tmds.LinuxAsync
{
    // AsyncOperation for executing SocketAsyncEventArgs operation.
    sealed class AsyncSocketEventArgsOperation : AsyncSocketOperation, IThreadPoolWorkItem
    {
        public AsyncSocketEventArgsOperation(SocketAsyncEventArgs saea) :
            base(saea)
        { }

        public override void Complete()
        {
            Log.Enter(this);
            Debug.Assert((CompletionFlags & (OperationCompletionFlags.OperationCancelled | OperationCompletionFlags.OperationFinished)) != 0);

            bool runContinuationsAsync = Saea.RunContinuationsAsynchronously;

            ResetOperationState();

            bool completeSync = (CompletionFlags & OperationCompletionFlags.CompletedSync) != 0;
            if (completeSync || !runContinuationsAsync)
            {
                Log.Info(this, "Completing synchronously");
                ((IThreadPoolWorkItem)this).Execute();
            }
            else
            {
                Log.Info(this, "Posting to ThreadPool");
                ThreadPool.UnsafeQueueUserWorkItem(this, preferLocal: false);
            }
            
            Log.Exit(this);
        }

        void IThreadPoolWorkItem.Execute()
        {
            Log.Enter(this);
            // Capture state.
            OperationCompletionFlags completionStatus = CompletionFlags;

            // Reset state.
            CompletionFlags = OperationCompletionFlags.None;
            CurrentAsyncContext = null;

            // Complete.
            Saea.Complete(completionStatus);
            Log.Exit(this);
        }
    }
}