#nullable disable

using System;

namespace TCLDecompiler {
	internal abstract class BSTNodeBase<T> where T : IComparable {
		internal int Count { get; set; } = 1;

		internal virtual BSTNodeBase<T> Parent { get; set; }
		internal virtual BSTNodeBase<T> Left { get; set; }
		internal virtual BSTNodeBase<T> Right { get; set; }
		internal T Value { get; set; }
		internal bool IsLeftChild => Parent?.Left == this;
		internal bool IsRightChild => Parent?.Right == this;
		internal bool IsLeaf => Left == null && Right == null;
	}
}
