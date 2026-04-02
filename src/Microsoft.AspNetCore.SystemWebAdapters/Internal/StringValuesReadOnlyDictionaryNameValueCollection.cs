using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.SystemWebAdapters.Internal
{
    internal class StringValuesReadOnlyDictionaryNameValueCollection : HttpValueCollection
    {
        private readonly IReadOnlyDictionary<string, StringValues> _values;

        public static NameValueCollection Empty { get; } = new StringValuesReadOnlyDictionaryNameValueCollection();

        public StringValuesReadOnlyDictionaryNameValueCollection()
            : this(new Dictionary<string, StringValues>())
        {
        }

        public StringValuesReadOnlyDictionaryNameValueCollection(IReadOnlyDictionary<string, StringValues> values)
        {
            _values = values;
            IsReadOnly = true;
        }

        public override string?[] AllKeys => _values.Keys.ToArray();

        public override int Count => _values.Count;

        public override string? Get(int index)
            => Get(GetKey(index));

        public override string? GetKey(int index)
            => index >= 0 && index < _values.Count ? _values.Keys.ElementAt(index) : null;

        public override string[]? GetValues(int index)
            => GetValues(GetKey(index));

        public override string[]? GetValues(string? name)
            => name is not null && _values.TryGetValue(name, out var values) ? values : default;

        public override string? Get(string? name)
            => name is not null && _values.TryGetValue(name, out var values) ? values : default;

        public override IEnumerator GetEnumerator() => _values.Keys.GetEnumerator();
    }
}
