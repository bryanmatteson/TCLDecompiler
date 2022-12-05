#nullable disable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TCLDecompiler {
	public class IntervalTree<T>: IEnumerable<(T start, T end)> where T : IComparable {
		private readonly OneDimensionalIntervalTree<T> _tree;
		private readonly HashSet<(T start, T end)> _items = new HashSet<(T start, T end)>(new IntervalComparer<T>());
		public int Count { get; private set; }
		public IntervalTree() => _tree = new OneDimensionalIntervalTree<T>(_defaultValue);

		public (T start, T end) Min() {
			OneDimensionalInterval<T> min = _tree.Min();
			return (min.Start, min.End[min.MatchingEndIndex]);
		}

		public (T start, T end) Max() {
			OneDimensionalInterval<T> max = _tree.Max();
			return (max.Start, max.End[max.MatchingEndIndex]);
		}

		public void Insert(T start, T end) {
			if (_items.Contains((start, end))) throw new Exception("Inteval exists.");
			_tree.Insert(new OneDimensionalInterval<T>(start, end));
			_items.Add((start, end));
			Count++;
		}

		public void Delete(T start, T end) {
			if (!_items.Contains((start, end))) throw new Exception("Inteval does'nt exist.");
			_tree.Delete(new OneDimensionalInterval<T>(start, end));
			_items.Remove((start, end));
			Count--;
		}

		public bool HasOverlaps(T start, T end) => _tree.GetOverlaps(new OneDimensionalInterval<T>(start, end)).Count > 0;
		public List<(T start, T end)> GetOverlaps(T start, T end) => _tree.GetOverlaps(new OneDimensionalInterval<T>(start, end)).Select(n => (n.Start, n.End[n.MatchingEndIndex])).ToList();

		private readonly Lazy<T> _defaultValue = new Lazy<T>(() => {
			Type s = typeof(T);
			bool isValueType = s.IsValueType;
			if (isValueType) return (T)Convert.ChangeType(int.MinValue, s);
			return default;
		});

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		public IEnumerator<(T start, T end)> GetEnumerator() => _items.GetEnumerator();
	}

	internal class OneDimensionalIntervalTree<T> where T : IComparable {
		private readonly RedBlackTree<OneDimensionalInterval<T>> _redBlackTree = new RedBlackTree<OneDimensionalInterval<T>>();
		internal int Count { get; private set; }
		private readonly Lazy<T> _defaultValue;

		internal OneDimensionalIntervalTree(Lazy<T> defaultValue) => _defaultValue = defaultValue;

		internal void Insert(OneDimensionalInterval<T> newInterval) {
			sortInterval(newInterval);
			RedBlackTreeNode<OneDimensionalInterval<T>> existing = _redBlackTree.FindNode(newInterval);
			if (existing != null) existing.Value.End.Add(newInterval.End[0]);
			else existing = _redBlackTree.InsertAndReturnNode(newInterval).node;
			updateMax(existing);
			Count++;
		}

		internal OneDimensionalInterval<T> Min() => _redBlackTree.Min();
		internal OneDimensionalInterval<T> Max() => _redBlackTree.Max();

		internal void Delete(OneDimensionalInterval<T> interval) {
			sortInterval(interval);

			RedBlackTreeNode<OneDimensionalInterval<T>> existing = _redBlackTree.FindNode(interval);
			if (existing?.Value.End.Count > 1) {
				existing.Value.End.RemoveAt(existing.Value.End.Count - 1);
				updateMax(existing);
			}
			else if (existing != null) {
				_redBlackTree.Delete(interval);
				updateMax(existing.Parent);
			}
			else throw new Exception("Interval not found in this interval tree.");

			Count--;
		}

		internal OneDimensionalInterval<T> GetOverlap(OneDimensionalInterval<T> searchInterval) {
			sortInterval(searchInterval);
			return getOverlap(_redBlackTree.Root, searchInterval);
		}

		internal List<OneDimensionalInterval<T>> GetOverlaps(OneDimensionalInterval<T> searchInterval) {
			sortInterval(searchInterval);
			return getOverlaps(_redBlackTree.Root, searchInterval);
		}

		internal bool DoOverlap(OneDimensionalInterval<T> searchInterval) {
			sortInterval(searchInterval);
			return getOverlap(_redBlackTree.Root, searchInterval) != null;
		}

		private void sortInterval(OneDimensionalInterval<T> value) {
			if (value.Start.CompareTo(value.End[0]) <= 0) return;

			T tmp = value.End[0];
			value.End[0] = value.Start;
			value.Start = tmp;
		}

		private OneDimensionalInterval<T> getOverlap(RedBlackTreeNode<OneDimensionalInterval<T>> current, OneDimensionalInterval<T> searchInterval) {
			while (true) {
				if (current == null) return null;
				if (doOverlap(current.Value, searchInterval)) return current.Value;

				if (current.Left?.Value.MaxEnd.CompareTo(searchInterval.Start) >= 0) {
					current = current.Left;
					continue;
				}

				current = current.Right;
			}
		}

		private List<OneDimensionalInterval<T>> getOverlaps(RedBlackTreeNode<OneDimensionalInterval<T>> current, OneDimensionalInterval<T> searchInterval, List<OneDimensionalInterval<T>> result = null) {
			if (result == null) result = new List<OneDimensionalInterval<T>>();

			if (current == null) return result;

			if (doOverlap(current.Value, searchInterval)) result.Add(current.Value);

			if (current.Left?.Value.MaxEnd.CompareTo(searchInterval.Start) >= 0)
				getOverlaps(current.Left, searchInterval, result);

			getOverlaps(current.Right, searchInterval, result);

			return result;
		}

		private bool doOverlap(OneDimensionalInterval<T> a, OneDimensionalInterval<T> b) {
			a.MatchingEndIndex = -1;
			b.MatchingEndIndex = -1;

			for (int i = 0; i < a.End.Count; i++) {
				for (int j = 0; j < b.End.Count; j++) {

					if (a.Start.CompareTo(b.End[j]) > 0 || a.End[i].CompareTo(b.Start) < 0) {
						continue;
					}

					a.MatchingEndIndex = i;
					b.MatchingEndIndex = j;

					return true;
				}

			}

			return false;
		}

		private void updateMax(RedBlackTreeNode<OneDimensionalInterval<T>> node, T currentMax, bool recurseUp = true) {
			while (true) {
				if (node == null) return;

				if (node.Left != null && node.Right != null) {
					if (currentMax.CompareTo(node.Left.Value.MaxEnd) < 0) {
						currentMax = node.Left.Value.MaxEnd;
					}

					if (currentMax.CompareTo(node.Right.Value.MaxEnd) < 0) {
						currentMax = node.Right.Value.MaxEnd;
					}
				}
				else if (node.Left != null) {
					if (currentMax.CompareTo(node.Left.Value.MaxEnd) < 0) {
						currentMax = node.Left.Value.MaxEnd;
					}
				}
				else if (node.Right != null) {
					if (currentMax.CompareTo(node.Right.Value.MaxEnd) < 0) {
						currentMax = node.Right.Value.MaxEnd;
					}
				}

				foreach (T v in node.Value.End) {
					if (currentMax.CompareTo(v) < 0) {
						currentMax = v;
					}
				}

				node.Value.MaxEnd = currentMax;


				if (recurseUp) {
					node = node.Parent;
					continue;
				}

				break;
			}
		}

		private void updateMax(RedBlackTreeNode<OneDimensionalInterval<T>> newRoot, bool recurseUp = true) {
			if (newRoot == null)
				return;

			newRoot.Value.MaxEnd = _defaultValue.Value;

			if (newRoot.Left != null) {
				newRoot.Left.Value.MaxEnd = _defaultValue.Value;
				updateMax(newRoot.Left, newRoot.Left.Value.MaxEnd, recurseUp);
			}

			if (newRoot.Right != null) {
				newRoot.Right.Value.MaxEnd = _defaultValue.Value;
				updateMax(newRoot.Right, newRoot.Right.Value.MaxEnd, recurseUp);
			}

			updateMax(newRoot, newRoot.Value.MaxEnd, recurseUp);

		}

	}

	internal class OneDimensionalInterval<T>: IComparable where T : IComparable {
		public T Start { get; set; }
		public List<T> End { get; set; }
		internal T MaxEnd { get; set; }
		internal int MatchingEndIndex { get; set; }
		public int CompareTo(object obj) => Start.CompareTo(((OneDimensionalInterval<T>)obj).Start);

		public OneDimensionalInterval(T start, T end) {
			Start = start;
			End = new List<T> { end };
		}
	}

	internal class IntervalComparer<T>: IEqualityComparer<(T start, T end)> where T : IComparable {
		public bool Equals((T start, T end) x, (T start, T end) y) {
			if (!x.start.Equals(y.start)) return false;
			if (!x.end.Equals(y.end)) return false;
			return true;
		}

		public int GetHashCode((T start, T end) x) {
			unchecked {
				int hash = 17;
				hash = (hash * 31) + x.start.GetHashCode();
				hash = (hash * 31) + x.end.GetHashCode();
				return hash;
			}
		}
	}
}
