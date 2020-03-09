using System.Diagnostics.Tracing;

namespace Tmds.LinuxAsync.Tracing
{
    public partial class SocketEventSource
    {
        [Event(100, Keywords = Keywords.Interop | Keywords.URing)]
        public void Ring_SubmitAndWait(string waitForCompletion)
        {
            WriteEvent(100, waitForCompletion);
        }

        [Event(101, Keywords = Keywords.Interop | Keywords.URing)]
        public void Ring_SubmitAndWait_Returned(string submitResult)
        {
            WriteEvent(101, submitResult);
        }

        [Event(102, Keywords = Keywords.URing)]
        public void Ring_PrepareLinkedReadV(int fd, ulong userData)
        {
            WriteEvent(102, fd, userData);
        }

        [Event(103, Keywords = Keywords.URing)]
        public void Ring_PrepareLinkedWriteV(int fd, ulong userData)
        {
            WriteEvent(103, fd, userData);
        }

        [Event(104, Keywords = Keywords.URing)]
        public void Ring_PreparePollAdd_PollIn(int fd, ulong userData)
        {
            WriteEvent(104, fd, userData);
        }
        
        [Event(105, Keywords = Keywords.URing)]
        public void Ring_PreparePollAdd_PollOut(int fd, ulong userData)
        {
            WriteEvent(105, fd, userData);
        }
        
        [Event(106, Keywords = Keywords.URing)]
        public void Ring_PrepareCancel(ulong userData)
        {
            WriteEvent(106, userData);
        }

        [Event(107, Keywords = Keywords.URing)]
        public void Ring_Completion(int result, ulong userData)
        {
            WriteEvent(107, result, userData);
        }
    }
}