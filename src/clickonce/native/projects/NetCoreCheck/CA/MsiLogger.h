// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#pragma once

#include "MsiWrapper.h"

class MsiLogger : public Logger
{
public:
    void Initialize(MsiWrapper *msiWrapper);
    void Log(LPCWSTR format, ...) const noexcept;

    MsiLogger(void) noexcept;
    ~MsiLogger(void) noexcept;

private:
    MsiWrapper *m_msiWrapper;
};
