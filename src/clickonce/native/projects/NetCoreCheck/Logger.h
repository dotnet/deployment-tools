// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#pragma once

#include "NetCoreCheck.h"
#include <tchar.h>
#include <stdio.h>

class Logger
{
public:
    void Log(LPCWSTR pszFormat, ...) const throw();

    // Static methods to create and close the logger
    static void CreateLog(LPCWSTR logFilePath);
    static void CloseLog();

private:
    Logger(void) throw();
    Logger(LPCWSTR logFilePath) throw();
    ~Logger(void) noexcept;

    void Initialize(LPCWSTR logFilePath);
    FILE* m_file;
};
