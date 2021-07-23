using System;
using SapNwRfc.Internal.Interop;

namespace SapNwRfc.Internal.Metadata
{
    internal sealed class SapTypeMetadata : ISapTypeMetadata
    {
        private readonly RfcInterop _interop;
        private readonly IntPtr _typeDescription;

        public SapTypeMetadata(RfcInterop interop, IntPtr typeDescription)
        {
            _interop = interop;
            _typeDescription = typeDescription;
        }

        public ISapFieldMetadata GetFieldByIndex(uint index)
        {
            RfcResultCode resultCode = _interop.GetFieldDescByIndex(
                typeHandle: _typeDescription,
                index: index,
                fieldDesc: out RfcFieldDescription fieldDesc,
                errorInfo: out RfcErrorInfo errorInfo);

            errorInfo.ThrowOnError();

            return new SapFieldMetadata(_interop, fieldDesc);
        }

        public ISapFieldMetadata GetFieldByName(string name)
        {
            RfcResultCode resultCode = _interop.GetFieldDescByName(
                typeHandle: _typeDescription,
                name: name,
                fieldDesc: out RfcFieldDescription fieldDesc,
                errorInfo: out RfcErrorInfo errorInfo);

            errorInfo.ThrowOnError();

            return new SapFieldMetadata(_interop, fieldDesc);
        }

        public uint GetFieldCount()
        {
            RfcResultCode resultCode = _interop.GetFieldCount(
                typeHandle: _typeDescription,
                count: out uint count,
                errorInfo: out RfcErrorInfo errorInfo);

            errorInfo.ThrowOnError();

            return count;
        }

        public string GetTypeName()
        {
            RfcResultCode resultCode = _interop.GetTypeName(
                rfcHandle: _typeDescription,
                typeName: out string typeName,
                errorInfo: out RfcErrorInfo errorInfo);

            errorInfo.ThrowOnError();

            return typeName;
        }
    }
}
