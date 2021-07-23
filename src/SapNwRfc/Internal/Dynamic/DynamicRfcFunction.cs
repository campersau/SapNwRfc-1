using System;
using System.Collections.Generic;
using SapNwRfc.Internal.Interop;

namespace SapNwRfc.Internal.Dynamic
{
    internal sealed class DynamicRfcFunction : DynamicRfcObject
    {
        public DynamicRfcFunction(RfcInterop interop, IntPtr functionHandle, IntPtr functionDescHandle)
            : base(interop, functionHandle, functionDescHandle)
        {
        }

        protected override IntPtr Describe(RfcInterop interop, IntPtr handle)
        {
            IntPtr typeDesc = interop.DescribeFunction(
                rfcHandle: handle,
                errorInfo: out RfcErrorInfo errorInfo);

            errorInfo.ThrowOnError();

            return typeDesc;
        }

        protected override uint GetCount(RfcInterop interop, IntPtr descHandle)
        {
            RfcResultCode resultCode = interop.GetParameterCount(
                funcDesc: descHandle,
                count: out uint count,
                errorInfo: out RfcErrorInfo errorInfo);

            resultCode.ThrowOnError(errorInfo);

            return count;
        }

        protected override bool TryDescribeFieldByIndex(RfcInterop interop, IntPtr descHandle, uint index, out KeyValuePair<string, RfcType> result)
        {
            RfcResultCode resultCode = interop.GetParameterDescByIndex(
                funcDesc: descHandle,
                index: index,
                paramDesc: out RfcParameterDescription paramDesc,
                errorInfo: out RfcErrorInfo errorInfo);

            if (resultCode == RfcResultCode.RFC_INVALID_PARAMETER)
            {
                result = default;
                return false;
            }

            resultCode.ThrowOnError(errorInfo);

            result = new KeyValuePair<string, RfcType>(paramDesc.Name, paramDesc.Type);
            return true;
        }

        protected override bool TryDescribeFieldByName(RfcInterop interop, IntPtr descHandle, string name, out KeyValuePair<string, RfcType> result)
        {
            RfcResultCode resultCode = interop.GetParameterDescByName(
                funcDesc: descHandle,
                name: name,
                paramDesc: out RfcParameterDescription paramDesc,
                errorInfo: out RfcErrorInfo errorInfo);

            if (resultCode == RfcResultCode.RFC_INVALID_PARAMETER)
            {
                result = default;
                return false;
            }

            resultCode.ThrowOnError(errorInfo);

            result = new KeyValuePair<string, RfcType>(paramDesc.Name, paramDesc.Type);
            return true;
        }
    }
}
