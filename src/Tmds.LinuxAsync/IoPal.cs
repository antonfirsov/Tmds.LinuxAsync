using System;
using System.Buffers;
using System.Runtime.InteropServices;
using Tmds.Linux;
using Tmds.LinuxAsync.Tracing;
using static Tmds.Linux.LibC;

namespace Tmds.LinuxAsync
{
    static class IoPal
    {
        public static unsafe int Write(SafeHandle handle, ReadOnlySpan<byte> span)
        {
            bool refAdded = false;
            try
            {
                handle.DangerousAddRef(ref refAdded);

                int rv;
                fixed (byte* ptr = span)
                {
                    do
                    {
                        int fd = handle.DangerousGetHandle().ToInt32();
                        rv = (int)write(fd, ptr, span.Length);
                        Log.WriteFd(fd, rv);
                    } while (rv == -1 && errno == EINTR);
                }

                return rv;
            }
            finally
            {
                if (refAdded)
                    handle.DangerousRelease();
            }
        }

        public static unsafe int Read(SafeHandle handle, Span<byte> span)
        {
            bool refAdded = false;
            try
            {
                handle.DangerousAddRef(ref refAdded);

                int rv;
                fixed (byte* ptr = span)
                {
                    do
                    {
                        int fd = handle.DangerousGetHandle().ToInt32();
                        rv = (int)read(fd, ptr, span.Length);
                        Log.ReadFd(fd, rv);
                    } while (rv == -1 && errno == EINTR);
                }

                return rv;
            }
            finally
            {
                if (refAdded)
                    handle.DangerousRelease();
            }
        }
    }
}