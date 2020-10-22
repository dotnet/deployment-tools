// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "NetCoreCheckExe.h"

// Globals
Logger *g_log;

int __cdecl wmain(int argc, WCHAR* argv[])
{
    if (argc != 3 && argc != 4)
    {
        return EXIT_FAILURE_INVALIDARGS;
    }

    LPCWSTR logFilePath = (argc == 4) ? argv[3] : NULL;
    FileLogger logger;
    logger.Initialize(logFilePath);
    g_log = &logger;

    // There's two valid sets of parameters.
    //  1. If the first parameter is 'UseExisting' the second parameter needs to be the path to a
    //     runtimeconfig.json file that we'll try to load.
    //     Example: NetCoreCheck.exe UseExisting Foo.runtimeconfig.json
    //  2. Otherwise we'll create a temporary runtime config file ourselves using the passed
    //     in framework name and framework version.
    //     Example: NetCoreCheck.exe Microsoft.WindowsDesktop.App 3.1.0
    // In both cases the optional third parameter is the path to the log file.
    int result;
    bool useExistingFile = (0 == _wcsicmp(TEXT("UseExisting"), argv[1]));
    if (useExistingFile)
    {
        LPCWSTR existingRuntimeConfigFile = argv[2];

        result = CheckRuntime(NULL, NULL, existingRuntimeConfigFile, false);
    }
    else
    {
        LPCWSTR frameworkName = argv[1];
        LPCWSTR frameworkVersion = argv[2];

        result = CheckRuntime(frameworkName, frameworkVersion, NULL, false);
    }

    return result;
}
