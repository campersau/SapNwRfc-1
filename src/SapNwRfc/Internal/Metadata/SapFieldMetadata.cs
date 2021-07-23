using System;
using SapNwRfc.Internal.Interop;

namespace SapNwRfc.Internal.Metadata
{
    internal sealed class SapFieldMetadata : ISapFieldMetadata
    {
        private readonly RfcInterop _interop;
        private readonly RfcFieldDescription _fieldDescription;

        public SapFieldMetadata(RfcInterop interop, RfcFieldDescription fieldDescription)
        {
            _interop = interop;
            _fieldDescription = fieldDescription;
        }

        public string Name => _fieldDescription.Name;

        public SapRfcType Type => _fieldDescription.Type;

        public uint NucLength => _fieldDescription.NucLength;

        public uint NucOffset => _fieldDescription.NucOffset;

        public uint UcLength => _fieldDescription.UcLength;

        public uint UcOffset => _fieldDescription.UcOffset;

        public uint Decimals => _fieldDescription.Decimals;

        public ISapTypeMetadata GetTypeMetadata()
        {
            if (_fieldDescription.TypeDescHandle == IntPtr.Zero)
                return null;

            IntPtr typeDescHandle = _interop.DescribeType(
                dataHandle: _fieldDescription.TypeDescHandle,
                errorInfo: out RfcErrorInfo errorInfo);

            errorInfo.ThrowOnError();

            return new SapTypeMetadata(_interop, typeDescHandle);
        }
    }
}
