using System;
using System.Collections.Generic;
using SapNwRfc.Internal.Interop;

namespace SapNwRfc.Internal.Dynamic
{
    internal sealed class DynamicRfcStructure : DynamicRfcObject
    {
        public DynamicRfcStructure(RfcInterop interop, IntPtr dataHandle)
            : base(interop, dataHandle)
        {
        }

        protected override IntPtr Describe(RfcInterop interop, IntPtr handle)
        {
            IntPtr typeDesc = interop.DescribeType(
                dataHandle: handle,
                errorInfo: out RfcErrorInfo errorInfo);

            errorInfo.ThrowOnError();

            return typeDesc;
        }

        protected override uint GetCount(RfcInterop interop, IntPtr descHandle)
        {
            RfcResultCode resultCode = interop.GetFieldCount(
                typeHandle: descHandle,
                count: out uint count,
                errorInfo: out RfcErrorInfo errorInfo);

            resultCode.ThrowOnError(errorInfo);

            return count;
        }

        protected override bool TryDescribeFieldByIndex(RfcInterop interop, IntPtr descHandle, uint index, out KeyValuePair<string, RfcType> result)
        {
            RfcResultCode resultCode = interop.GetFieldDescByIndex(
                typeHandle: descHandle,
                index: index,
                fieldDesc: out RfcFieldDescription fieldDesc,
                errorInfo: out RfcErrorInfo errorInfo);

            if (resultCode == RfcResultCode.RFC_INVALID_PARAMETER)
            {
                result = default;
                return false;
            }

            resultCode.ThrowOnError(errorInfo);

            result = new KeyValuePair<string, RfcType>(fieldDesc.Name, fieldDesc.Type);
            return true;
        }

        protected override bool TryDescribeFieldByName(RfcInterop interop, IntPtr descHandle, string name, out KeyValuePair<string, RfcType> result)
        {
            RfcResultCode resultCode = interop.GetFieldDescByName(
                typeHandle: descHandle,
                name: name,
                fieldDesc: out RfcFieldDescription fieldDesc,
                errorInfo: out RfcErrorInfo errorInfo);

            if (resultCode == RfcResultCode.RFC_INVALID_PARAMETER)
            {
                result = default;
                return false;
            }

            resultCode.ThrowOnError(errorInfo);

            result = new KeyValuePair<string, RfcType>(fieldDesc.Name, fieldDesc.Type);
            return true;
        }
    }
}
