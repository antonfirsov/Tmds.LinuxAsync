using System;
using System.Runtime.CompilerServices;
using IoUring;

namespace Tmds.LinuxAsync.Tracing
{
    internal static partial class Log
    {
        public static bool IsEnabled => SocketEventSource.Log.IsEnabled();
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Info(string message, [CallerMemberName] string methodName = "")
        {
            if (IsEnabled) SocketEventSource.Log.Info("", methodName, message);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Info(object contextObject, string message, [CallerMemberName] string methodName = "")
        {
            if (IsEnabled) SocketEventSource.Log.Info(IdOf(contextObject), methodName, message);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Info(ICustomLogId contextObject, string message, [CallerMemberName] string methodName = "")
        {
            if (IsEnabled) SocketEventSource.Log.Info(contextObject.LogId, methodName, message);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Enter(object contextObject, string argsStr = "", [CallerMemberName] string methodName = "")
        {
            if (IsEnabled) SocketEventSource.Log.Enter(IdOf(contextObject), methodName, argsStr);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Enter(ICustomLogId contextObject, string argsStr = "", [CallerMemberName] string methodName = "")
        {
            if (IsEnabled) SocketEventSource.Log.Enter(contextObject.LogId, methodName, argsStr);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Enter(string argsStr = "", [CallerMemberName] string methodName = "")
        {
            if (IsEnabled) SocketEventSource.Log.Enter("", methodName, argsStr);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Return<TRetVal>(object contextObject, TRetVal retVal, [CallerMemberName] string methodName = "")
        {
            if (IsEnabled) SocketEventSource.Log.Exit(IdOf(contextObject), methodName, $"{retVal}");
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Return<TRetVal>(ICustomLogId contextObject, TRetVal retVal, [CallerMemberName] string methodName = "")
        {
            if (IsEnabled) SocketEventSource.Log.Exit(contextObject.LogId, methodName, $"{retVal}");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Exit(ICustomLogId contextObject, [CallerMemberName] string methodName = "")
        {
            if (IsEnabled) SocketEventSource.Log.Exit(contextObject.LogId, methodName, $"");
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Exit(object contextObject, [CallerMemberName] string methodName = "")
        {
            if (IsEnabled) SocketEventSource.Log.Exit(IdOf(contextObject), methodName, $"");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Return<TRetVal>(TRetVal retVal, [CallerMemberName] string methodName = "")
        {
            if (IsEnabled) SocketEventSource.Log.Exit("", methodName, $"{retVal}");
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Exit([CallerMemberName] string methodName = "")
        {
            if (IsEnabled) SocketEventSource.Log.Exit("", methodName, $"");
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ReadFd(int fd, int rv)
        {
            if (IsEnabled) SocketEventSource.Log.ReadFd(fd, rv);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteFd(int fd, int rv)
        {
            if (IsEnabled) SocketEventSource.Log.WriteFd(fd, rv);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Send(int fd, int rv)
        {
            if (IsEnabled) SocketEventSource.Log.Send(fd, rv);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Recv(int fd, int rv)
        {
            if (IsEnabled) SocketEventSource.Log.Recv(fd, rv);
        }

        private static string IdOf(object? value) =>
            value != null ? $"{value.GetType().Name}#{value.GetHashCode()}" : "(null)";
    }
}