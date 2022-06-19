using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Borks.IO.ProcessInterop
{
    internal unsafe static class Kernel32
    {
        public struct NtModuleInfo
        {
            public IntPtr BaseOfDll;
            public int SizeOfImage;
            public IntPtr EntryPoint;
        }

        [DllImport("kernel32", SetLastError = true)]
        public static extern bool ReadProcessMemory
        (
            IntPtr hProcess,
            long lpBaseAddress,
            byte[] lpBuffer,
            int nSize,
            out int lpNumberOfBytesRead
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory
        (
            IntPtr hProcess,
            long lpBaseAddress,
            byte* lpBuffer,
            int nSize,
            out int lpNumberOfBytesRead
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory
        (
            IntPtr hProcess,
            long lpBaseAddress,
            byte[] lpBuffer,
            int nSize,
            out int lpNumberOfBytesRead
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory
        (
            IntPtr hProcess,
            long lpBaseAddress,
            byte* lpBuffer,
            int nSize,
            out int lpNumberOfBytesRead
        );

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess
        (
            int dwDesiredAccess,
            bool bInheritHandle,
            int dwProcessId
        );

        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        public static extern bool IsWow64Process(IntPtr processHandle, out bool wow64Process);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "K32GetModuleInformation")]
        public static extern bool GetModuleInformation(IntPtr processHandle, IntPtr moduleHandle, out NtModuleInfo ntModuleInfo, int size);

        [Flags]
        public enum AllocationType
        {
            Commit = 0x1000,
            Reserve = 0x2000,
            Decommit = 0x4000,
            Release = 0x8000,
            Reset = 0x80000,
            Physical = 0x400000,
            TopDown = 0x100000,
            WriteWatch = 0x200000,
            LargePages = 0x20000000
        }

        [Flags]
        public enum MemoryProtection
        {
            Execute = 0x10,
            ExecuteRead = 0x20,
            ExecuteReadWrite = 0x40,
            ExecuteWriteCopy = 0x80,
            NoAccess = 0x01,
            ReadOnly = 0x02,
            ReadWrite = 0x04,
            WriteCopy = 0x08,
            GuardModifierflag = 0x100,
            NoCacheModifierflag = 0x200,
            WriteCombineModifierflag = 0x400
        }

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern IntPtr VirtualAllocEx(IntPtr hProcess,
                            IntPtr lpAddress,
                            ulong dwSize,
                            AllocationType flAllocationType,
                            MemoryProtection flProtect);
    }
}
