using System;
using SapNwRfc.Internal.Interop;

namespace SapNwRfc.Internal.Metadata
{
    internal sealed class SapFunctionMetadata : ISapFunctionMetadata
    {
        private readonly RfcInterop _interop;
        private readonly IntPtr _functionDescHandle;

        public SapFunctionMetadata(RfcInterop interop, IntPtr functionDescHandle)
        {
            _interop = interop;
            _functionDescHandle = functionDescHandle;
        }

        public string GetName()
        {
            RfcResultCode resultCode = _interop.GetFunctionName(
                rfcHandle: _functionDescHandle,
                funcName: out string funcName,
                errorInfo: out RfcErrorInfo errorInfo);

            errorInfo.ThrowOnError();

            return funcName;
        }

        public ISapParameterMetadata GetParameterByIndex(uint index)
        {
            RfcResultCode resultCode = _interop.GetParameterDescByIndex(
                funcDesc: _functionDescHandle,
                index: index,
                paramDesc: out RfcParameterDescription paramDesc,
                errorInfo: out RfcErrorInfo errorInfo);

            errorInfo.ThrowOnError();

            return new SapParameterMetadata(_interop, paramDesc);
        }

        public ISapParameterMetadata GetParameterByName(string name)
        {
            RfcResultCode resultCode = _interop.GetParameterDescByName(
                funcDesc: _functionDescHandle,
                name: name,
                paramDesc: out RfcParameterDescription paramDesc,
                errorInfo: out RfcErrorInfo errorInfo);

            errorInfo.ThrowOnError();

            return new SapParameterMetadata(_interop, paramDesc);
        }

        public uint GetParameterCount()
        {
            RfcResultCode resultCode = _interop.GetParameterCount(
                funcDesc: _functionDescHandle,
                count: out uint count,
                errorInfo: out RfcErrorInfo errorInfo);

            errorInfo.ThrowOnError();

            return count;
        }
    }
}
