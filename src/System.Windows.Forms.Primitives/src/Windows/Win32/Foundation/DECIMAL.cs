﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Windows.Win32.Foundation
{
    internal partial struct DECIMAL
    {
        public decimal ToDecimal()
        {
            return new decimal(
                (int)Anonymous2.Anonymous.Lo32,
                (int)Anonymous2.Anonymous.Mid32,
                (int)Hi32,
                Anonymous1.Anonymous.sign == 0x80,
                Anonymous1.Anonymous.scale);
        }
    }
}
