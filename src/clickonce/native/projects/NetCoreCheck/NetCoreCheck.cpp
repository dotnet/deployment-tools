// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "NetCoreCheck.h"
#include "Logger.h"
#include "TempRuntimeConfigFile.h"

#include "..\hostfxr.h"
#include "..\nethost.h"

// Forward declarations
void* GetExport(HMODULE h, const char* name);
int Exit(int ret);

// Globals
Logger* g_log;
HMODULE g_hostfxrLibrary = NULL;

int __cdecl wmain(int argc, WCHAR* argv[], WCHAR* envp[])
{
    if (argc != 3 && argc != 4)
    {
        return EXIT_FAILURE_INVALIDARGS;
    }

    // There's two valid sets of parameters.
    //  1. If the first parameter is 'UseExisting' the second parameter needs to be the path to a
    //     runtimeconfig.json file that we'll try to load.
    //     Example: NetCoreCheck.exe UseExisting Foo.runtimeconfig.json
    //  2. Otherwise we'll create a temporary runtime config file ourselves using the passed
    //     in framework name and framework version.
    //     Example: NetCoreCheck.exe Microsoft.WindowsDesktop.App 3.1.0
    // In both cases the optional third parameter is the path to the log file.
    bool useExistingFile = (0 == _wcsicmp(TEXT("UseExisting"), argv[1]));

    LPCWSTR logFilePath = (argc == 4) ? argv[3] : NULL;
    Logger::CreateLog(logFilePath);

    WCHAR hostfxrPath[MAX_PATH];
    size_t bufferSize = sizeof(hostfxrPath) / sizeof(WCHAR);
    int rc = get_hostfxr_path(hostfxrPath, &bufferSize, nullptr);
    if (rc != 0)
    {
        g_log->Log(TEXT("get_hostfxr_path failed: '%d'"), rc);
        return Exit(EXIT_FAILURE_LOADHOSTFXR);
    }

    g_log->Log(TEXT("Found HostFxr: '%s'"), hostfxrPath);
    
    // Load hostfxr and get desired exports
    g_hostfxrLibrary = ::LoadLibraryW(hostfxrPath);
    hostfxr_initialize_for_runtime_config_fn initFptr = (hostfxr_initialize_for_runtime_config_fn)GetExport(g_hostfxrLibrary, "hostfxr_initialize_for_runtime_config");
    hostfxr_close_fn closeFptr = (hostfxr_close_fn)GetExport(g_hostfxrLibrary, "hostfxr_close");
    if (!initFptr || !closeFptr)
    {
        g_log->Log(TEXT("Failed to get exports from hostfxr."));
        return Exit(EXIT_FAILURE_HOSTFXREXPORTS);
    }

    WCHAR runtimeConfigPath[MAX_PATH];
    if (useExistingFile)
    {
        wcscpy(runtimeConfigPath, argv[2]);
        g_log->Log(TEXT("Using existing runtimeconfig file '%s'"), runtimeConfigPath);
    }
    else
    {
        LPCWSTR frameworkName = argv[1];
        LPCWSTR frameworkVersion = argv[2];

        g_log->Log(TEXT("Framework Name:    '%s'"), frameworkName);
        g_log->Log(TEXT("Framework Version: '%s'"), frameworkVersion);

        DWORD ret = GetTempRuntimeConfigPath(runtimeConfigPath);
        if (ret != 0)
        {
            g_log->Log(TEXT("Failed to get runtime config file path."));
            return Exit(ret);
        }

        ret = CreateTempRuntimeConfigFile(runtimeConfigPath, frameworkName, frameworkVersion);
        if (ret != 0)
        {
            g_log->Log(TEXT("Failed to create temp runtime config file."));
            return Exit(ret);
        }
    }

    hostfxr_handle cxt = nullptr;
    g_log->Log(TEXT("Calling hostfxr_initialize_for_runtime_config..."));
    rc = initFptr(runtimeConfigPath, nullptr, &cxt);
    if (rc != 0 || cxt == nullptr)
    {
        g_log->Log(TEXT("hostfxr_initialize_for_runtime_config failed: '%d'"), rc);
        closeFptr(cxt);
        return Exit(EXIT_FAILURE_INITHOSTFXR);
    }

    g_log->Log(TEXT("hostfxr_initialize_for_runtime_config succeeded."));

    rc = closeFptr(cxt);
    if (rc != 0)
    {
        g_log->Log(TEXT("hostfxr_close failed: '%d'"), rc);
    }

    return Exit(EXIT_SUCCESS);
}

void* GetExport(HMODULE h, const char* name)
{
    return ::GetProcAddress(h, name);
}

int Exit(int ret)
{
    if (g_hostfxrLibrary != NULL && (!::FreeLibrary(g_hostfxrLibrary)))
    {
        g_log->Log(TEXT("FreeLibrary failed."));
    }
    
    Logger::CloseLog();
    return ret;
}
