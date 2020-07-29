// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;

namespace Microsoft.Deployment.Utilities
{
    internal static class NativeMethods
    {
        // Warning - Launcher resource type and name should never be changed
        public static readonly IntPtr Launcher_CustomResourceTypePtr = new IntPtr(50);
        public const string Launcher_ResourceName = "FILENAME";

        public const UInt32 LOAD_LIBRARY_AS_DATAFILE = 0x00000002;

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern IntPtr BeginUpdateResourceW(String fileName, bool deleteExistingResource);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool UpdateResourceW(IntPtr hUpdate, IntPtr lpType, String lpName, short wLanguage, byte[] data, int cbData);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool EndUpdateResource(IntPtr hUpdate, bool fDiscard);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        internal static extern IntPtr GetModuleHandle(string moduleName);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern IntPtr FindResource(IntPtr hModule, string lpName, IntPtr lpType);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern uint SizeofResource(IntPtr hModule, IntPtr hResInfo);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern IntPtr LoadResource(IntPtr hModule, IntPtr hResInfo);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern IntPtr LockResource(IntPtr hResData);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr LoadLibraryExW(string strFileName, IntPtr hFile, UInt32 ulFlags);

        [DllImport("clr.dll", CharSet = CharSet.Unicode, ExactSpelling = true, PreserveSig = false)]
        [return: MarshalAs(UnmanagedType.IUnknown)]
        public static extern object GetAssemblyIdentityFromFile([In, MarshalAs(UnmanagedType.LPWStr)] string filePath, [In] ref Guid riid);

        [ComImport]
        [Guid("6eaf5ace-7917-4f3c-b129-e046a9704766")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IReferenceIdentity
        {
            [return: MarshalAs(UnmanagedType.LPWStr)]
            string GetAttribute([In, MarshalAs(UnmanagedType.LPWStr)] string Namespace, [In, MarshalAs(UnmanagedType.LPWStr)] string Name);
            void SetAttribute();
            void EnumAttributes();
            void Clone();
        }
    }
}
