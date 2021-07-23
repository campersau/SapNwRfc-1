using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using SapNwRfc.Internal.Interop;

namespace SapNwRfc.Internal.Dynamic
{
    internal sealed class DynamicRfcTable : DynamicObject, IReadOnlyList<object>
    {
        private readonly RfcInterop _interop;
        private readonly IntPtr _tableHandle;
        private uint? _rowCount;

        public DynamicRfcTable(RfcInterop interop, IntPtr dataHandle)
        {
            _interop = interop;
            _tableHandle = dataHandle;
        }

        private uint RowCount
        {
            get
            {
                if (!_rowCount.HasValue)
                {
                    RfcResultCode resultCode = _interop.GetRowCount(
                        tableHandle: _tableHandle,
                        rowCount: out uint rowCount,
                        errorInfo: out RfcErrorInfo errorInfo);

                    errorInfo.ThrowOnError();

                    _rowCount = rowCount;
                }

                return _rowCount.Value;
            }
        }

        public int Count => (int)RowCount;

        public object this[int index]
        {
            get
            {
                if (TryGetValueByIndex((uint)index, out object result))
                    return result;

                throw new Exception();
            }
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            yield return "Count";
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (binder.Name == "Count")
            {
                result = RowCount;
                return true;
            }

            return base.TryGetMember(binder, out result);
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            if (indexes.Length != 1)
                throw new ArgumentException(nameof(indexes));

            var index = indexes[0];

            if (index is int i)
                return TryGetValueByIndex((uint)i, out result);

            if (index is uint ui)
                return TryGetValueByIndex(ui, out result);

            throw new ArgumentException(nameof(indexes));
        }

        private bool TryGetValueByIndex(uint index, out object result)
        {
            RfcResultCode resultCode = _interop.MoveTo(
                tableHandle: _tableHandle,
                index: index,
                errorInfo: out RfcErrorInfo errorInfo);

            resultCode.ThrowOnError(errorInfo);

            IntPtr rowHandle = _interop.GetCurrentRow(
                tableHandle: _tableHandle,
                errorInfo: out errorInfo);

            errorInfo.ThrowOnError();

            result = new DynamicRfcStructure(_interop, rowHandle);
            return true;
        }

        public IEnumerator<object> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private struct Enumerator : IEnumerator<object>
        {
            private readonly DynamicRfcTable _dynamic;
            private bool _first;

            public Enumerator(DynamicRfcTable dynamic)
            {
                _dynamic = dynamic;
                _first = true;
                Current = default;
            }

            public object Current { get; private set; }

            public bool MoveNext()
            {
                if (_first)
                {
                    RfcResultCode resultCode = _dynamic._interop.MoveToFirstRow(
                        tableHandle: _dynamic._tableHandle,
                        errorInfo: out RfcErrorInfo errorInfo);

                    if (resultCode == RfcResultCode.RFC_TABLE_MOVE_BOF)
                        return false;

                    resultCode.ThrowOnError(errorInfo);

                    _first = false;
                }
                else
                {
                    RfcResultCode resultCode = _dynamic._interop.MoveToNextRow(
                        tableHandle: _dynamic._tableHandle,
                        errorInfo: out RfcErrorInfo errorInfo);

                    if (resultCode == RfcResultCode.RFC_TABLE_MOVE_EOF)
                        return false;

                    resultCode.ThrowOnError(errorInfo);
                }

                {
                    IntPtr rowHandle = _dynamic._interop.GetCurrentRow(
                        tableHandle: _dynamic._tableHandle,
                        errorInfo: out RfcErrorInfo errorInfo);

                    errorInfo.ThrowOnError();

                    Current = new DynamicRfcStructure(_dynamic._interop, rowHandle);
                    return true;
                }
            }

            public void Reset()
            {
                _first = true;
                Current = default;
            }

            public void Dispose()
            {
            }
        }
    }
}
