// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Win32;

namespace HostPath
{
    public class Program
    {
        // See https://docs.microsoft.com/en-us/windows/win32/procthread/environment-variables
        private static readonly string s_EnvironmentKeyName = @"SYSTEM\CurrentControlSet\Control\Session Manager\Environment";

        public static void Main(string[] args)
        {
            try
            {
                if (!Environment.Is64BitOperatingSystem)
                {
                    return;
                }

                using (RegistryKey envKey = Registry.LocalMachine.OpenSubKey(s_EnvironmentKeyName))
                {
                    if (envKey == null)
                    {
                        return;
                    }

                    // Environment.GetEnvironmentVariable will expand environment variables inside the PATH variable
                    string path = (string)envKey.GetValue("Path", string.Empty, RegistryValueOptions.DoNotExpandEnvironmentNames);

                    // Check for paths with and without a trailing backslash
                    string x86DotNetPath1 = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                        "dotnet");
                    string x86DotNetPath2 = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                        @"dotnet\");

                    List<string> paths = path.Split(';').ToList();
                    paths.Remove(x86DotNetPath1);
                    paths.Remove(x86DotNetPath2);
                    path = string.Join(";", paths);

                    Environment.SetEnvironmentVariable("Path", path, EnvironmentVariableTarget.Machine);
                }
            }
            catch
            {

            }
        }
    }
}