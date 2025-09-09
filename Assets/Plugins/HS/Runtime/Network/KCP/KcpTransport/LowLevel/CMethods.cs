#pragma warning disable CS8500
#pragma warning disable CS8981

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using IINT32 = System.UInt32;
using IUINT16 = System.UInt16;
using IUINT32 = System.UInt32;
using IUINT8 = System.Byte;
using size_t = System.IntPtr;

namespace KcpTransport.LowLevel
{
    internal static unsafe class CMethods
    {
        public static void memcpy(void* dest, void* src, int n)
        {
            Buffer.MemoryCopy(src, dest, n, n);
        }

        public static void* malloc(size_t size)
        {
            return (void*)Marshal.AllocHGlobal((IntPtr)size);
        }

        public static void free(void* ptr)
        {
            Marshal.FreeHGlobal((IntPtr)ptr);
        }

        [Conditional("DEBUG")]
        public static void assert<T>(T _)
        {
        }

        [Conditional("DEBUG")]
        public static void assert(IKCPCB* _)
        {
        }

        [Conditional("DEBUG")]
        public static void assert(IKCPSEG* _)
        {
        }

        public static void abort()
        {
            //Environment.Exit(0);
        }
    }
}