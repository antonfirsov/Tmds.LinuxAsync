using System.Runtime.CompilerServices;
using IoUring;

namespace Tmds.LinuxAsync.Tracing
{
    internal static partial class Log
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Ring_SubmitAndWait(bool waitForCompletion)
        {
            if (IsEnabled) SocketEventSource.Log.Ring_SubmitAndWait(waitForCompletion.ToString());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Ring_SubmitAndWaitResult(SubmitResult submitResult)
        {
            if (IsEnabled) SocketEventSource.Log.Ring_SubmitAndWait_Returned(submitResult.ToString());
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Ring_PrepareLinkedReadV(int fd, ulong userData)
        {
            if (IsEnabled) SocketEventSource.Log.Ring_PrepareLinkedReadV(fd, userData);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Ring_PrepareLinkedWriteV(int fd, ulong userData)
        {
            if (IsEnabled) SocketEventSource.Log.Ring_PrepareLinkedWriteV(fd, userData);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Ring_PreparePollAdd_PollIn(int fd, ulong userData)
        {
            if (IsEnabled) SocketEventSource.Log.Ring_PreparePollAdd_PollIn(fd, userData);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Ring_PreparePollAdd_PollOut(int fd, ulong userData)
        {
            if (IsEnabled) SocketEventSource.Log.Ring_PreparePollAdd_PollOut(fd, userData);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Ring_PrepareCancel(ulong userData)
        {
            if (IsEnabled) SocketEventSource.Log.Ring_PrepareCancel(userData);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Ring_Completion(in Completion completion)
        {
            if (IsEnabled) SocketEventSource.Log.Ring_Completion(completion.result, completion.userData);
        }
    }
}