using System;
using SapNwRfc.Internal.Fields;
using SapNwRfc.Internal.Interop;

namespace SapNwRfc.Internal.Dynamic
{
    internal static class DynamicRfc
    {
        internal static bool TryGetRfcValue(RfcInterop interop, IntPtr dataHandle, string name, RfcType type, Type returnType, out object result)
        {
            result = GetRfcValue(interop, dataHandle, name, type);

            if (returnType == typeof(object))
            {
                return true;
            }

            try
            {
                result = Convert.ChangeType(result, returnType);
                return true;
            }
            catch { }

            result = null;
            return false;
        }

        private static object GetRfcValue(RfcInterop interop, IntPtr dataHandle, string name, RfcType type)
        {
            switch (type)
            {
            case RfcType.RFCTYPE_CHAR:
            case RfcType.RFCTYPE_NUM:
            case RfcType.RFCTYPE_BCD:
            case RfcType.RFCTYPE_STRING:
                return StringField.Extract(interop, dataHandle, name).Value;

            case RfcType.RFCTYPE_INT:
            case RfcType.RFCTYPE_INT1:
            case RfcType.RFCTYPE_INT2:
                return IntField.Extract(interop, dataHandle, name).Value;

            case RfcType.RFCTYPE_INT8:
                return LongField.Extract(interop, dataHandle, name).Value;

            case RfcType.RFCTYPE_FLOAT:
                return DoubleField.Extract(interop, dataHandle, name).Value;

            case RfcType.RFCTYPE_DECF16:
            case RfcType.RFCTYPE_DECF34:
                return DecimalField.Extract(interop, dataHandle, name).Value;

            case RfcType.RFCTYPE_DATE:
                return DateField.Extract(interop, dataHandle, name).Value;

            case RfcType.RFCTYPE_TIME:
                return TimeField.Extract(interop, dataHandle, name).Value;

            case RfcType.RFCTYPE_BYTE:
            case RfcType.RFCTYPE_XSTRING:
                return BytesField.Extract(interop, dataHandle, name, bufferLength: 0).Value;

            case RfcType.RFCTYPE_TABLE:
                {
                    RfcResultCode resultCode = interop.GetTable(
                        dataHandle: dataHandle,
                        name: name,
                        tableHandle: out IntPtr tableHandle,
                        errorInfo: out RfcErrorInfo errorInfo);

                    resultCode.ThrowOnError(errorInfo);

                    return new DynamicRfcTable(interop, tableHandle);
                }

            case RfcType.RFCTYPE_STRUCTURE:
                {
                    RfcResultCode resultCode = interop.GetStructure(
                        dataHandle: dataHandle,
                        name: name,
                        structHandle: out IntPtr structHandle,
                        errorInfo: out RfcErrorInfo errorInfo);

                    resultCode.ThrowOnError(errorInfo);

                    return new DynamicRfcStructure(interop, structHandle);
                }

            case RfcType.RFCTYPE_NULL:
                return null;
            }

            throw new NotSupportedException($"Parameter type {type} is not supported");
        }
    }
}
