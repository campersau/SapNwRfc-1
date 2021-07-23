﻿namespace SapNwRfc.Internal.Interop
{
    internal enum RfcDirection
    {
        RFC_IMPORT = 0x01,
        RFC_EXPORT = 0x02,
        RFC_CHANGING = RFC_IMPORT | RFC_EXPORT,
        RFC_TABLES = 0x04 | RFC_CHANGING,
    }
}
