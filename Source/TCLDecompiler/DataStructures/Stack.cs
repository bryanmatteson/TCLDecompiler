using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TCLDecompiler {
	public class Stack<T>: IEnumerable<T> {
		private readonly List<T> _collection;
		public int Count => _collection.Count;
		public bool IsEmpty => Count == 0;

		public Stack() : this(10) { }
		public Stack(int capacity) => _collection = new List<T>(capacity);

		public T? Top => _collection.LastOrDefault();
		public void Push(T item) => _collection.Add(item);
		public T Pop() {
			T top = Top ?? throw new InvalidOperationException("Stack is empty");
			_collection.RemoveAt(_collection.Count - 1);
			return top;
		}

		public IEnumerable<T> Pop(int count) {
			if (count > Count || count < 0) throw new ArgumentOutOfRangeException(nameof(count));

			for (int i = 0; i < count; i++) {
				yield return Pop();
			}
		}

		public void Clear() => _collection.Clear();


		public IEnumerator<T> GetEnumerator() => _collection.AsEnumerable().Reverse().GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
