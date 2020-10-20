// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#pragma once

#include "NetCoreCheckCA.h"

#define LOG_BUFFER 2048
#define countof(ary) (sizeof(ary) / sizeof(ary[0]))

class MsiWrapper
{
public:
    void Log(LPCWSTR msg) const noexcept;
    void LogFailure(HRESULT hrFailure, LPCWSTR format, ...) const noexcept;
    HRESULT GetProperty(LPCWSTR propertyName, LPWSTR* propertyValue);
    HRESULT SetProperty(LPCWSTR propertyName, LPCWSTR propertyValue);

    MsiWrapper(MSIHANDLE msiHandle) noexcept;
    ~MsiWrapper(void) noexcept;

private:
    MSIHANDLE m_msiHandle;
};
