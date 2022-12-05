#nullable disable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;


namespace TCLDecompiler {

	public class RangeTree<T>: IEnumerable<T> where T : IComparable {
		private readonly OneDimensionalRangeTree<T> _tree = new OneDimensionalRangeTree<T>();
		private readonly HashSet<T> _items = new HashSet<T>();
		public int Count { get; private set; }

		public void Insert(T value) {
			if (_items.Contains(value)) throw new Exception("value exists.");
			_tree.Insert(value);
			_items.Add(value);
			Count++;
		}

		public void Delete(T value) {
			if (!_items.Contains(value)) throw new Exception("Item not found.");

			_tree.Delete(value);
			_items.Remove(value);
			Count--;
		}

		public IEnumerable<T> RangeSearch(T start, T end) {
			if (Count == 0) return Enumerable.Empty<T>();
			return _tree.RangeSearch(start, end).SelectMany(x => x.Values);
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
	}



	internal class OneDimensionalRangeTree<T> where T : IComparable {
		private readonly RedBlackTree<RangeTreeNode<T>> _tree = new RedBlackTree<RangeTreeNode<T>>();
		internal int Count => _tree.Count;

		public T Min() {
			RangeTreeNode<T> min = _tree.Min();
			return min.Values.Min();
		}

		public T Max() {
			RangeTreeNode<T> max = _tree.Max();
			return max.Values.Max();
		}

		internal RangeTreeNode<T> Find(T value) {
			RedBlackTreeNode<RangeTreeNode<T>> result = _tree.FindNode(new RangeTreeNode<T>(value));
			if (result == null) {
				throw new Exception("Item not found in this tree.");
			}

			return result.Value;
		}

		internal RangeTreeNode<T> Insert(T value) {
			var newNode = new RangeTreeNode<T>(value);

			RedBlackTreeNode<RangeTreeNode<T>> existing = _tree.FindNode(newNode);
			if (existing != null) {
				existing.Value.Values.Add(value);
				return existing.Value;
			}

			_tree.Insert(newNode);
			return newNode;
		}

		internal void Delete(T value) {
			RedBlackTreeNode<RangeTreeNode<T>> existing = _tree.FindNode(new RangeTreeNode<T>(value));

			if (existing.Value.Values.Count == 1) {
				_tree.Delete(new RangeTreeNode<T>(value));
				return;
			}

			existing.Value.Values.RemoveAt(existing.Value.Values.Count - 1);

		}

		internal List<RangeTreeNode<T>> RangeSearch(T start, T end) =>
			getInRange(new List<RangeTreeNode<T>>(), new Dictionary<RedBlackTreeNode<RangeTreeNode<T>>, bool>(), _tree.Root, start, end);

		private List<RangeTreeNode<T>> getInRange(List<RangeTreeNode<T>> result, Dictionary<RedBlackTreeNode<RangeTreeNode<T>>, bool> visited, RedBlackTreeNode<RangeTreeNode<T>> currentNode, T start, T end) {

			if (currentNode.IsLeaf) {
				if (!inRange(currentNode, start, end)) return result;
				result.Add(currentNode.Value);
			}
			else {
				if (start.CompareTo(currentNode.Value.Value) <= 0) {
					if (currentNode.Left != null) getInRange(result, visited, currentNode.Left, start, end);
					if (!visited.ContainsKey(currentNode) && inRange(currentNode, start, end)) {
						result.Add(currentNode.Value);
						visited.Add(currentNode, false);
					}
				}

				if (end.CompareTo(currentNode.Value.Value) < 0) return result;

				if (currentNode.Right != null) getInRange(result, visited, currentNode.Right, start, end);
				if (visited.ContainsKey(currentNode) || !inRange(currentNode, start, end)) return result;

				result.Add(currentNode.Value);
				visited.Add(currentNode, false);
			}

			return result;
		}

		private bool inRange(RedBlackTreeNode<RangeTreeNode<T>> currentNode, T start, T end) =>
			start.CompareTo(currentNode.Value.Value) <= 0 && end.CompareTo(currentNode.Value.Value) >= 0;
	}

	internal class RangeTreeNode<T>: IComparable where T : IComparable {
		internal T Value => Values[0];
		internal List<T> Values { get; set; }
		internal RangeTreeNode(T value) => Values = new List<T> { value };
		public int CompareTo(object obj) => Value.CompareTo(((RangeTreeNode<T>)obj).Value);
	}
}
