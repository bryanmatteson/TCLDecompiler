
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Runtime.Serialization;

namespace TCLDecompiler {
	public class MultiValueDictionary<TKey, TValue>: Dictionary<TKey, HashSet<TValue>> where TKey : notnull {
		private readonly IEqualityComparer<TValue>? _valueComparer;
		public MultiValueDictionary() : this(16, null) { }
		public MultiValueDictionary(int capacity) : this(capacity, null) { }
		public MultiValueDictionary(int capacity, IEqualityComparer<TValue>? valueComparer) : base(capacity) => _valueComparer = valueComparer;

		public MultiValueDictionary<TKey, TValue> Clone() => (MultiValueDictionary<TKey, TValue>)MemberwiseClone();

		public void Add(TKey key) => Add(key, new HashSet<TValue>());

		public void Add(TKey key, TValue value) {
			if (!TryGetValue(key, out HashSet<TValue>? container)) {
				container = new HashSet<TValue>(_valueComparer);
				Add(key, container);
			}
			container.Add(value);
		}


		public void AddRange(TKey key, IEnumerable<TValue> values) {
			if (values == null) return;
			foreach (TValue value in values) {
				Add(key, value);
			}
		}


		public bool ContainsValue(TKey key, TValue value) {
			bool toReturn = false;
			if (TryGetValue(key, out HashSet<TValue>? values)) {
				toReturn = values.Contains(value);
			}
			return toReturn;
		}


		public void Remove(TKey key, TValue value) {
			if (TryGetValue(key, out HashSet<TValue>? container)) {
				container.Remove(value);
				if (container.Count == 0) Remove(key);
			}
		}


		public void MergeWith(MultiValueDictionary<TKey, TValue> toMergeWith) {
			if (toMergeWith == null) return;

			foreach (KeyValuePair<TKey, HashSet<TValue>> pair in toMergeWith) {
				foreach (TValue value in pair.Value) {
					Add(pair.Key, value);
				}
			}
		}

		public HashSet<TValue> ValuesFor(TKey key) {
			if (!TryGetValue(key, out HashSet<TValue>? toReturn)) {
				toReturn = new HashSet<TValue>(_valueComparer);
			}
			return toReturn;
		}
	}
}
