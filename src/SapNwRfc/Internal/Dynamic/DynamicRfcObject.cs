using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using SapNwRfc.Internal.Interop;

namespace SapNwRfc.Internal.Dynamic
{
    internal abstract class DynamicRfcObject : DynamicObject, IReadOnlyDictionary<string, object>
    {
        private readonly RfcInterop _interop;
        private readonly IntPtr _handle;
        private IntPtr _descHandle;
        private uint? _fieldCount;

        public DynamicRfcObject(RfcInterop interop, IntPtr handle)
        {
            _interop = interop;
            _handle = handle;
        }

        public DynamicRfcObject(RfcInterop interop, IntPtr handle, IntPtr descHandle)
        {
            _interop = interop;
            _handle = handle;
            _descHandle = descHandle;
        }

        protected abstract IntPtr Describe(RfcInterop interop, IntPtr handle);

        protected abstract uint GetCount(RfcInterop interop, IntPtr descHandle);

        protected abstract bool TryDescribeFieldByIndex(RfcInterop interop, IntPtr descHandle, uint index, out KeyValuePair<string, RfcType> field);

        protected abstract bool TryDescribeFieldByName(RfcInterop interop, IntPtr descHandle, string name, out KeyValuePair<string, RfcType> field);

        private IntPtr DescHandle
        {
            get
            {
                if (_descHandle == IntPtr.Zero)
                    _descHandle = Describe(_interop, _handle);

                return _descHandle;
            }
        }

        private uint FieldCount
        {
            get
            {
                if (!_fieldCount.HasValue)
                    _fieldCount = GetCount(_interop, DescHandle);

                return _fieldCount.Value;
            }
        }

        public IEnumerable<string> Keys
        {
            get
            {
                for (uint index = 0, count = FieldCount; index < count; index++)
                {
                    if (TryDescribeFieldByIndex(_interop, DescHandle, index, out KeyValuePair<string, RfcType> fieldDesc))
                    {
                        yield return fieldDesc.Key;
                    }
                    else
                    {
                        throw new Exception(); // TODO
                    }
                }
            }
        }

        public IEnumerable<object> Values
        {
            get
            {
                for (uint index = 0, count = FieldCount; index < count; index++)
                {
                    if (TryGetValueByIndex(index, typeof(object), out _, out object value))
                    {
                        yield return value;
                    }
                    else
                    {
                        throw new Exception(); // TODO
                    }
                }
            }
        }

        public int Count => (int)FieldCount;

        public object this[string key]
        {
            get
            {
                if (TryGetValueByName(key, typeof(object), out object result))
                    return result;

                throw new KeyNotFoundException();
            }
        }

        public bool ContainsKey(string key)
        {
            return TryDescribeFieldByName(_interop, DescHandle, key, out _);
        }

        public bool TryGetValue(string key, out object value)
        {
            return TryGetValueByName(key, typeof(object), out value);
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return Keys;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return TryGetValueByName(binder.Name, binder.ReturnType, out result);
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            if (indexes.Length != 1)
                throw new ArgumentException(nameof(indexes));

            var index = indexes[0];

            if (index is string name)
                return TryGetValueByName(name, binder.ReturnType, out result);

            if (index is int i)
                return TryGetValueByIndex((uint)i, binder.ReturnType, out _, out result);

            if (index is uint ui)
                return TryGetValueByIndex(ui, binder.ReturnType, out _, out result);

            throw new ArgumentException(nameof(indexes));
        }

        private bool TryGetValueByName(string name, Type returnType, out object result)
        {
            if (TryDescribeFieldByName(_interop, DescHandle, name, out KeyValuePair<string, RfcType> fieldDesc) &&
                DynamicRfc.TryGetRfcValue(_interop, _handle, fieldDesc.Key, fieldDesc.Value, returnType, out result))
            {
                return true;
            }

            result = null;
            return false;
        }

        private bool TryGetValueByIndex(uint index, Type returnType, out string name, out object result)
        {
            if (TryDescribeFieldByIndex(_interop, DescHandle, index, out KeyValuePair<string, RfcType> fieldDesc) &&
                DynamicRfc.TryGetRfcValue(_interop, _handle, fieldDesc.Key, fieldDesc.Value, returnType, out result))
            {
                name = fieldDesc.Key;
                return true;
            }

            name = null;
            result = null;
            return false;
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private struct Enumerator : IEnumerator<KeyValuePair<string, object>>
        {
            private readonly DynamicRfcObject _dynamic;
            private uint _index;

            public Enumerator(DynamicRfcObject dynamic)
            {
                _dynamic = dynamic;
                _index = 0;
                Current = default;
            }

            public KeyValuePair<string, object> Current { get; private set; }

            object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                if (_index < _dynamic.FieldCount && _dynamic.TryGetValueByIndex(_index++, typeof(object), out string name, out object result))
                {
                    Current = new KeyValuePair<string, object>(name, result);
                    return true;
                }

                return false;
            }

            public void Reset()
            {
                _index = 0;
                Current = default;
            }

            public void Dispose()
            {
            }
        }
    }
}
