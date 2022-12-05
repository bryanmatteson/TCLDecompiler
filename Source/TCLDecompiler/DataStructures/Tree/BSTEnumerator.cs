#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;

namespace TCLDecompiler {
	internal class BSTEnumerator<T>: IEnumerator<T> where T : IComparable {
		private readonly bool _asc;
		private readonly BSTNodeBase<T> _root;
		private BSTNodeBase<T> _current;

		internal BSTEnumerator(BSTNodeBase<T> root, bool asc = true) {
			_root = root;
			_asc = asc;
		}

		public bool MoveNext() {
			if (_root == null) return false;

			if (_current == null) {
				_current = _asc ? _root.FindMin() : _root.FindMax();
				return true;
			}

			BSTNodeBase<T> next = _asc ? _current.NextHigher() : _current.NextLower();
			if (next != null) {
				_current = next;
				return true;
			}

			return false;
		}

		public void Reset() => _current = _root;

		public T Current => _current.Value;
		object IEnumerator.Current => Current;

		public void Dispose() => _current = null;
	}

}
