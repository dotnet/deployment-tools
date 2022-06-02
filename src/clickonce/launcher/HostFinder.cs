// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Deployment.Utilities;

namespace Microsoft.Deployment.Launcher
{
    internal class HostFinder
    {
        private readonly string applicationFilePath;
        private string arch;

        /// <summary>
        /// Gets full path to .NET host appropriate for activating the application.
        /// 
        /// .NET host's location can be obtained from multiple locations.
        ///
        /// Current code searches in default, global shared runtime, location only:
        /// %ProgramFiles%\dotnet and %ProgramFiles(x86)%\dotnet
        /// 
        /// Consider adding support for other non-standard registrations of host location.
        /// 1) Environment variables: DOTNET_ROOT or DOTNET_ROOT(x86)
        /// 2) Registry: HKLM\SOFTWARE\dotnet\Setup\InstalledVersions\{arch}\[InstallLocation]
        /// 
        /// Order of search should eventually be:
        /// 1) Environment variables
        /// 2) Registry
        /// 3) Global location
        /// </summary>
        /// <param name="applicationFilePath">Full path to application</param>
        /// <returns>Path to host</returns>
        internal static string GetHost(string applicationFilePath)
        {
            HostFinder hf = new HostFinder(applicationFilePath);
            return hf.GetHost();
        }

        /// <summary>
        /// Checks if running on Arm64 system
        /// </summary>
        private bool IsArm64System
        {
            get
            {
                string proc = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");
                if (!string.IsNullOrEmpty(proc) && proc.ToLower() == "arm64")
                {
                    return true;
                }

                proc = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432");
                if (!string.IsNullOrEmpty(proc) && proc.ToLower() == "arm64")
                {
                    return true;
                }

                return false;
            }
        }

        private HostFinder(string path)
        {
            applicationFilePath = path;
        }

        /// <summary>
        /// Gets full path to .NET host appropriate for activating the application.
        /// </summary>
        /// <returns>Path to host</returns>
        private string GetHost()
        {
            arch = GetProcessorArchitectureFromAssembly(applicationFilePath);

            if (arch == "x86")
            {
                return GetX86Host();
            }
            else if (arch == "amd64" || arch == "arm64")
            {
                return Get64bitHost();
            }
            else if (arch == "msil")
            {
                string host = Get64bitHost();
                return string.IsNullOrEmpty(host) ? GetX86Host() : host;
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets ProgramFiles folder for the specified bitness.
        /// </summary>
        /// <param name="is64bit">If 64-bit bitness is required</param>
        /// <returns></returns>
        private string GetProgramFilesFolder(bool is64bit = false)
        {
            return is64bit ?
                (Environment.Is64BitProcess ?
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) :
                    Environment.GetEnvironmentVariable("ProgramW6432")) :
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        }

        /// <summary>
        /// Get global host if it exists, for the specified bitness.
        /// </summary>
        /// <param name="is64bit">If 64-bit bitness is required</param>
        /// <returns></returns>
        private string GetGlobalHost(bool is64bit = false)
        {
            string folder = GetProgramFilesFolder(is64bit);
            string relativeHostPath = "dotnet\\dotnet.exe";

            // On Arm64 systems, x64 host is in "x64" sub-folder
            if (is64bit && arch == "amd64" && IsArm64System)
            {
                relativeHostPath = "dotnet\\x64\\dotnet.exe";
            }

            string host = !string.IsNullOrEmpty(folder) ? Path.Combine(folder, relativeHostPath) : string.Empty;
            return File.Exists(host) ? host : string.Empty;
        }

        /// <summary>
        /// Gets full path to x86 .NET host.
        /// </summary>
        /// <returns>X86 host</returns>
        private string GetX86Host()
        {
            return GetGlobalHost();
        }

        /// <summary>
        /// Gets full path to 64-bit .NET host.
        /// </summary>
        /// <returns>64-bit host</returns>
        private string Get64bitHost()
        {
            return Environment.Is64BitOperatingSystem ? GetGlobalHost(true) : string.Empty;
        }

        /// <summary>
        /// Gets processor architecture from assembly metadata.
        /// </summary>
        /// <param name="path">Assembly path</param>
        /// <returns></returns>
        private string GetProcessorArchitectureFromAssembly(string path)
        {
            string processorArchitecture = string.Empty;

            try
            {
                Guid riid = GetGuidOfType(typeof(NativeMethods.Clr.IReferenceIdentity));
                NativeMethods.Clr.IReferenceIdentity refid = (NativeMethods.Clr.IReferenceIdentity)NativeMethods.Clr.GetAssemblyIdentityFromFile(path, ref riid);
                if (refid != null)
                {
                    processorArchitecture = refid.GetAttribute(null, "processorArchitecture");
                }
            }
            catch (Exception)
            {
                // GetAssemblyIdentityFromFile throws an exception for architectures that don't exist in .NET FX, i.e. arm64.
            }

            // Default to "msil", to support failure scenarios, like GetAssemblyIdentityFromFile returning null
            // or encountering an unknown architecture.
            return string.IsNullOrEmpty(processorArchitecture) ?
                        "msil" :
                        processorArchitecture.ToLowerInvariant();
        }

        private Guid GetGuidOfType(Type type)
        {
            var guidAttr = (GuidAttribute)Attribute.GetCustomAttribute(type, typeof(GuidAttribute), false);
            return new Guid(guidAttr.Value);
        }
    }
}
