using System;
using SapNwRfc.Internal.Interop;

namespace SapNwRfc.Internal.Metadata
{
    internal sealed class SapParameterMetadata : ISapParameterMetadata
    {
        private readonly RfcInterop _interop;
        private readonly RfcParameterDescription _parameterDescription;

        public SapParameterMetadata(RfcInterop interop, RfcParameterDescription parameterDescription)
        {
            _interop = interop;
            _parameterDescription = parameterDescription;
        }

        public string Name => _parameterDescription.Name;

        public SapRfcType Type => _parameterDescription.Type;

        public SapRfcParameterDirection Direction => _parameterDescription.Direction;

        public uint NucLength => _parameterDescription.NucLength;

        public uint UcLength => _parameterDescription.UcLength;

        public uint Decimals => _parameterDescription.Decimals;

        public string DefaultValue => _parameterDescription.DefaultValue;

        public bool Optional => _parameterDescription.Optional == 1;

        public string Description => _parameterDescription.ParameterText;

        public ISapTypeMetadata GetTypeMetadata()
        {
            if (_parameterDescription.TypeDescHandle == IntPtr.Zero)
                return null;

            IntPtr typeDescHandle = _interop.DescribeType(
                dataHandle: _parameterDescription.TypeDescHandle,
                errorInfo: out RfcErrorInfo errorInfo);

            errorInfo.ThrowOnError();

            return new SapTypeMetadata(_interop, typeDescHandle);
        }
    }
}
