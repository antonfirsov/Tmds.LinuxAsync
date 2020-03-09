using System.Diagnostics.Tracing;
using System.Runtime.InteropServices;

namespace Tmds.LinuxAsync.Tracing
{
    [EventSource]
    internal class SocketEventSource : EventSource
    {
        public static readonly SocketEventSource Log = new SocketEventSource();
        
        public class Keywords
        {
            public const EventKeywords Generic = (EventKeywords)0x01;
            public const EventKeywords PInvoke = (EventKeywords)0x02;
            public const EventKeywords Epoll = (EventKeywords)0x04;
            public const EventKeywords Aio = (EventKeywords)0x08;
            public const EventKeywords IOUring = (EventKeywords)0x0F;
        }

        [Event(1, Keywords = Keywords.Generic)]
        public void Info(string contextObject, string methodName, string message)
        {
            WriteEvent(1, message);
        }

        [Event(2, Keywords = Keywords.Generic)]
        public void Enter(string contextObject, string methodName, string argsStr)
        {
            WriteEvent(2, contextObject, methodName, argsStr);
        }
        
        [Event(3, Keywords = Keywords.Generic)]
        public void Exit(string contextObject, string methodName, string retVal)
        {
            WriteEvent(3, contextObject, methodName, retVal);
        }

        [Event(10, Keywords = Keywords.PInvoke)]
        public void ReadFd(int fd, int rv)
        {
            WriteEvent(10, fd, rv);
        }

        [Event(11, Keywords = Keywords.PInvoke)]
        public void WriteFd(int fd, int rv)
        {
            WriteEvent(11, fd, rv);
        }
        
        [Event(12, Keywords = Keywords.PInvoke)]
        public void Send(int fd, int rv)
        {
            WriteEvent(12, fd, rv);
        }

        [Event(13, Keywords = Keywords.PInvoke)]
        public void Recv(int fd, int rv)
        {
            WriteEvent(13, fd, rv);
        }


    }
}