#nullable disable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TCLDecompiler {
	public class RedBlackTree<T>: IEnumerable<T> where T : IComparable {
		internal RedBlackTreeNode<T> Root { get; set; }

		internal readonly Dictionary<T, BSTNodeBase<T>> NodeLookUp;
		public int Count => Root?.Count ?? 0;

		public RedBlackTree(bool enableNodeLookUp = false, IEqualityComparer<T> equalityComparer = null) {
			if (enableNodeLookUp) {
				if (!typeof(T).GetTypeInfo().IsValueType && equalityComparer == null) {
					throw new ArgumentException("equalityComparer parameter is required when node lookup us enabled and T is not a value type.");
				}
				NodeLookUp = new Dictionary<T, BSTNodeBase<T>>(equalityComparer ?? EqualityComparer<T>.Default);
			}
		}

		public RedBlackTree(IEnumerable<T> sortedCollection, bool enableNodeLookUp = false, IEqualityComparer<T> equalityComparer = null) {
			BSTHelpers.ValidateSortedCollection(sortedCollection);
			RedBlackTreeNode<T>[] nodes = sortedCollection.Select(x => new RedBlackTreeNode<T>(null, x)).ToArray();
			Root = (RedBlackTreeNode<T>)BSTHelpers.ToBST(nodes);
			assignColors(Root);
			BSTHelpers.AssignCount(Root);

			if (enableNodeLookUp) {
				if (!typeof(T).GetTypeInfo().IsValueType && equalityComparer == null) {
					throw new ArgumentException("equalityComparer parameter is required when node lookup us enabled and T is not a value type.");
				}

				NodeLookUp = nodes.ToDictionary(x => x.Value, x => x as BSTNodeBase<T>, equalityComparer ?? EqualityComparer<T>.Default);
			}
		}

		public bool HasItem(T value) {
			if (Root == null) {
				return false;
			}

			if (NodeLookUp != null) {
				return NodeLookUp.ContainsKey(value);
			}

			return Find(value).node != null;
		}

		internal void Clear() => Root = null;

		public T Max() {
			var max = Root.FindMax();
			return max == null ? default : max.Value;
		}

		private RedBlackTreeNode<T> findMax(RedBlackTreeNode<T> node) => node.FindMax() as RedBlackTreeNode<T>;

		public T Min() {
			BSTNodeBase<T> min = Root.FindMin();
			return min == null ? default : min.Value;
		}

		public int IndexOf(T item) => Root.Position(item);

		public T ElementAt(int index) {
			if (index < 0 || index >= Count) {
				throw new ArgumentNullException(nameof(index));
			}

			return Root.KthSmallest(index).Value;
		}

		internal RedBlackTreeNode<T> FindNode(T value) => Root == null ? null : Find(value).node;

		internal bool Exists(T value) => FindNode(value) != null;

		internal (RedBlackTreeNode<T> node, int position) Find(T value) {
			if (NodeLookUp != null) {
				if (NodeLookUp.ContainsKey(value)) {
					var node = NodeLookUp[value] as RedBlackTreeNode<T>;
					return (node, Root.Position(value));
				}

				return (null, -1);
			}

			(BSTNodeBase<T> node, int position) result = Root.Find(value);
			return (result.node as RedBlackTreeNode<T>, result.position);
		}

		public int Insert(T value) {
			var node = InsertAndReturnNode(value);
			return node.position;
		}

		internal (RedBlackTreeNode<T> node, int position) InsertAndReturnNode(T value) {
			if (Root == null) {
				Root = new RedBlackTreeNode<T>(null, value) { NodeColor = RedBlackTreeNodeColor.Black };
				if (NodeLookUp != null) {
					NodeLookUp[value] = Root;
				}

				return (Root, 0);
			}

			var newNode = insert(Root, value);

			if (NodeLookUp != null) {
				NodeLookUp[value] = newNode.node;
			}

			return newNode;
		}

		private (RedBlackTreeNode<T> node, int position) insert(RedBlackTreeNode<T> currentNode, T newNodeValue) {
			var insertionPosition = 0;

			while (true) {
				var compareResult = currentNode.Value.CompareTo(newNodeValue);

				if (compareResult < 0) {
					insertionPosition += (currentNode.Left?.Count ?? 0) + 1;

					if (currentNode.Right == null) {
						RedBlackTreeNode<T> node = currentNode.Right = new RedBlackTreeNode<T>(currentNode, newNodeValue);
						balanceInsertion(currentNode.Right);
						return (node, insertionPosition);
					}

					currentNode = currentNode.Right;
				}
				else if (compareResult > 0) {
					if (currentNode.Left == null) {
						RedBlackTreeNode<T> node = currentNode.Left = new RedBlackTreeNode<T>(currentNode, newNodeValue);
						balanceInsertion(currentNode.Left);
						return (node, insertionPosition);
					}

					currentNode = currentNode.Left;
				}
				else {
					throw new Exception("Item with same key exists");
				}
			}
		}

		private void balanceInsertion(RedBlackTreeNode<T> nodeToBalance) {

			while (true) {
				if (nodeToBalance == Root) {
					nodeToBalance.NodeColor = RedBlackTreeNodeColor.Black;
					break;
				}

				if (nodeToBalance.NodeColor == RedBlackTreeNodeColor.Red && nodeToBalance.Parent.NodeColor == RedBlackTreeNodeColor.Red) {
					if (nodeToBalance.Parent.Sibling?.NodeColor == RedBlackTreeNodeColor.Red) {
						nodeToBalance.Parent.Sibling.NodeColor = RedBlackTreeNodeColor.Black;
						nodeToBalance.Parent.NodeColor = RedBlackTreeNodeColor.Black;

						if (nodeToBalance.Parent.Parent != Root) {
							nodeToBalance.Parent.Parent.NodeColor = RedBlackTreeNodeColor.Red;
						}

						nodeToBalance.UpdateCounts();
						nodeToBalance.Parent.UpdateCounts();
						nodeToBalance = nodeToBalance.Parent.Parent;
					}
					else if (nodeToBalance.Parent.Sibling == null || nodeToBalance.Parent.Sibling.NodeColor == RedBlackTreeNodeColor.Black) {
						if (nodeToBalance.IsLeftChild && nodeToBalance.Parent.IsLeftChild) {
							var newRoot = nodeToBalance.Parent;
							swapColors(nodeToBalance.Parent, nodeToBalance.Parent.Parent);
							rightRotate(nodeToBalance.Parent.Parent);

							if (newRoot == Root) {
								Root.NodeColor = RedBlackTreeNodeColor.Black;
							}

							nodeToBalance.UpdateCounts();
							nodeToBalance = newRoot;
						}
						else if (nodeToBalance.IsLeftChild && nodeToBalance.Parent.IsRightChild) {
							rightRotate(nodeToBalance.Parent);

							var newRoot = nodeToBalance;

							swapColors(nodeToBalance.Parent, nodeToBalance);
							leftRotate(nodeToBalance.Parent);

							if (newRoot == Root) {
								Root.NodeColor = RedBlackTreeNodeColor.Black;
							}

							nodeToBalance.UpdateCounts();
							nodeToBalance = newRoot;
						}
						else if (nodeToBalance.IsRightChild && nodeToBalance.Parent.IsRightChild) {
							var newRoot = nodeToBalance.Parent;
							swapColors(nodeToBalance.Parent, nodeToBalance.Parent.Parent);
							leftRotate(nodeToBalance.Parent.Parent);

							if (newRoot == Root) {
								Root.NodeColor = RedBlackTreeNodeColor.Black;
							}

							nodeToBalance.UpdateCounts();
							nodeToBalance = newRoot;
						}
						else if (nodeToBalance.IsRightChild && nodeToBalance.Parent.IsLeftChild) {
							leftRotate(nodeToBalance.Parent);

							RedBlackTreeNode<T> newRoot = nodeToBalance;

							swapColors(nodeToBalance.Parent, nodeToBalance);
							rightRotate(nodeToBalance.Parent);

							if (newRoot == Root) {
								Root.NodeColor = RedBlackTreeNodeColor.Black;
							}

							nodeToBalance.UpdateCounts();
							nodeToBalance = newRoot;
						}
					}
				}

				if (nodeToBalance.Parent != null) {
					nodeToBalance.UpdateCounts();
					nodeToBalance = nodeToBalance.Parent;
					continue;
				}

				break;
			}

			nodeToBalance.UpdateCounts(true);

		}

		private void swapColors(RedBlackTreeNode<T> node1, RedBlackTreeNode<T> node2) {
			RedBlackTreeNodeColor tmpColor = node2.NodeColor;
			node2.NodeColor = node1.NodeColor;
			node1.NodeColor = tmpColor;
		}

		public int Delete(T value) {
			if (Root == null) {
				return -1;
			}

			var node = Find(value);

			if (node.node == null) {
				return -1;
			}

			var position = node.position;

			delete(node.node);

			NodeLookUp?.Remove(value);

			return position;
		}


		public T RemoveAt(int index) {
			if (index < 0 || index >= Count) {
				throw new ArgumentException("index");
			}

			var node = Root.KthSmallest(index) as RedBlackTreeNode<T>;

			var deletedValue = node.Value;

			delete(node);

			NodeLookUp?.Remove(deletedValue);

			return node.Value;
		}

		private void delete(RedBlackTreeNode<T> node) {
			if (node.IsLeaf) {
				if (node.NodeColor == RedBlackTreeNodeColor.Red) {
					deleteLeaf(node);
					node.Parent?.UpdateCounts(true);
					return;
				}

				deleteLeaf(node);
				balanceNode(node.Parent);
			}
			else {
				if (node.Left != null && node.Right == null) {
					deleteLeftNode(node);
					balanceNode(node.Left);
				}
				else if (node.Right != null && node.Left == null) {
					deleteRightNode(node);
					balanceNode(node.Right);
				}
				else {
					var maxLeftNode = findMax(node.Left);

					if (NodeLookUp != null) {
						NodeLookUp[node.Value] = maxLeftNode;
						NodeLookUp[maxLeftNode.Value] = node;
					}

					node.Value = maxLeftNode.Value;

					delete(maxLeftNode);
					return;
				}
			}
		}

		private void balanceNode(RedBlackTreeNode<T> nodeToBalance) {
			while (nodeToBalance != null) {
				nodeToBalance.UpdateCounts();
				nodeToBalance = handleDoubleBlack(nodeToBalance);
			}
		}

		private void deleteLeaf(RedBlackTreeNode<T> node) {
			if (node.Parent == null) {
				Root = null;
			}
			else if (node.IsLeftChild) {
				node.Parent.Left = null;
			}
			else {
				node.Parent.Right = null;
			}
		}

		private void deleteRightNode(RedBlackTreeNode<T> node) {
			if (node.Parent == null) {
				Root.Right.Parent = null;
				Root = Root.Right;
				Root.NodeColor = RedBlackTreeNodeColor.Black;
				return;
			}

			if (node.IsLeftChild) {
				node.Parent.Left = node.Right;
			}
			else {
				node.Parent.Right = node.Right;
			}

			node.Right.Parent = node.Parent;

			if (node.Right.NodeColor != RedBlackTreeNodeColor.Red) {
				return;
			}

			node.Right.NodeColor = RedBlackTreeNodeColor.Black;

		}

		private void deleteLeftNode(RedBlackTreeNode<T> node) {
			if (node.Parent == null) {
				Root.Left.Parent = null;
				Root = Root.Left;
				Root.NodeColor = RedBlackTreeNodeColor.Black;
				return;
			}

			if (node.IsLeftChild) {
				node.Parent.Left = node.Left;
			}
			else {
				node.Parent.Right = node.Left;
			}

			node.Left.Parent = node.Parent;

			if (node.Left.NodeColor != RedBlackTreeNodeColor.Red) {
				return;
			}

			node.Left.NodeColor = RedBlackTreeNodeColor.Black;
		}

		private void rightRotate(RedBlackTreeNode<T> node) {
			var prevRoot = node;
			var leftRightChild = prevRoot.Left.Right;

			var newRoot = node.Left;

			prevRoot.Left.Parent = prevRoot.Parent;

			if (prevRoot.Parent != null) {
				if (prevRoot.Parent.Left == prevRoot) {
					prevRoot.Parent.Left = prevRoot.Left;
				}
				else {
					prevRoot.Parent.Right = prevRoot.Left;
				}
			}

			newRoot.Right = prevRoot;
			prevRoot.Parent = newRoot;

			newRoot.Right.Left = leftRightChild;
			if (newRoot.Right.Left != null) {
				newRoot.Right.Left.Parent = newRoot.Right;
			}

			if (prevRoot == Root) {
				Root = newRoot;
			}

			newRoot.Left.UpdateCounts();
			newRoot.Right.UpdateCounts();
			newRoot.UpdateCounts();
		}

		private void leftRotate(RedBlackTreeNode<T> node) {
			var prevRoot = node;
			var rightLeftChild = prevRoot.Right.Left;

			var newRoot = node.Right;

			prevRoot.Right.Parent = prevRoot.Parent;

			if (prevRoot.Parent != null) {
				if (prevRoot.Parent.Left == prevRoot) {
					prevRoot.Parent.Left = prevRoot.Right;
				}
				else {
					prevRoot.Parent.Right = prevRoot.Right;
				}
			}

			newRoot.Left = prevRoot;
			prevRoot.Parent = newRoot;

			newRoot.Left.Right = rightLeftChild;
			if (newRoot.Left.Right != null) {
				newRoot.Left.Right.Parent = newRoot.Left;
			}

			if (prevRoot == Root) {
				Root = newRoot;
			}

			newRoot.Left.UpdateCounts();
			newRoot.Right.UpdateCounts();
			newRoot.UpdateCounts();
		}

		private RedBlackTreeNode<T> handleDoubleBlack(RedBlackTreeNode<T> node) {
			if (node == Root) {
				node.NodeColor = RedBlackTreeNodeColor.Black;
				return null;
			}

			if (node.Parent?.NodeColor == RedBlackTreeNodeColor.Black && node.Sibling?.NodeColor == RedBlackTreeNodeColor.Red && ((node.Sibling.Left == null && node.Sibling.Right == null) || (node.Sibling.Left != null && node.Sibling.Right != null && node.Sibling.Left.NodeColor == RedBlackTreeNodeColor.Black && node.Sibling.Right.NodeColor == RedBlackTreeNodeColor.Black))) {
				node.Parent.NodeColor = RedBlackTreeNodeColor.Red;
				node.Sibling.NodeColor = RedBlackTreeNodeColor.Black;

				if (node.Sibling.IsRightChild) {
					leftRotate(node.Parent);
				}
				else {
					rightRotate(node.Parent);
				}

				return node;
			}
			if (node.Parent?.NodeColor == RedBlackTreeNodeColor.Black && node.Sibling?.NodeColor == RedBlackTreeNodeColor.Black && ((node.Sibling.Left == null && node.Sibling.Right == null) || (node.Sibling.Left != null && node.Sibling.Right != null && node.Sibling.Left.NodeColor == RedBlackTreeNodeColor.Black && node.Sibling.Right.NodeColor == RedBlackTreeNodeColor.Black))) {
				node.Sibling.NodeColor = RedBlackTreeNodeColor.Red;
				return node.Parent;
			}


			if (node.Parent?.NodeColor == RedBlackTreeNodeColor.Red && node.Sibling?.NodeColor == RedBlackTreeNodeColor.Black && ((node.Sibling.Left == null && node.Sibling.Right == null) || (node.Sibling.Left != null && node.Sibling.Right != null && node.Sibling.Left.NodeColor == RedBlackTreeNodeColor.Black && node.Sibling.Right.NodeColor == RedBlackTreeNodeColor.Black))) {
				node.Parent.NodeColor = RedBlackTreeNodeColor.Black;
				node.Sibling.NodeColor = RedBlackTreeNodeColor.Red;
				node.UpdateCounts(true);
				return null;
			}


			if (node.Parent != null && node.Parent.NodeColor == RedBlackTreeNodeColor.Black && node.Sibling != null && node.Sibling.IsRightChild && node.Sibling.NodeColor == RedBlackTreeNodeColor.Black && node.Sibling.Left?.NodeColor == RedBlackTreeNodeColor.Red && node.Sibling.Right?.NodeColor == RedBlackTreeNodeColor.Black) {
				node.Sibling.NodeColor = RedBlackTreeNodeColor.Red;
				node.Sibling.Left.NodeColor = RedBlackTreeNodeColor.Black;
				rightRotate(node.Sibling);
				return node;
			}

			if (node.Parent != null && node.Parent.NodeColor == RedBlackTreeNodeColor.Black && node.Sibling != null && node.Sibling.IsLeftChild && node.Sibling.NodeColor == RedBlackTreeNodeColor.Black && node.Sibling.Left?.NodeColor == RedBlackTreeNodeColor.Black && node.Sibling.Right?.NodeColor == RedBlackTreeNodeColor.Red) {
				node.Sibling.NodeColor = RedBlackTreeNodeColor.Red;
				node.Sibling.Right.NodeColor = RedBlackTreeNodeColor.Black;
				leftRotate(node.Sibling);
				return node;
			}

			if (node.Parent != null && node.Parent.NodeColor == RedBlackTreeNodeColor.Black && node.Sibling != null && node.Sibling.IsRightChild && node.Sibling.NodeColor == RedBlackTreeNodeColor.Black && node.Sibling.Right?.NodeColor == RedBlackTreeNodeColor.Red) {
				node.Sibling.Right.NodeColor = RedBlackTreeNodeColor.Black;
				leftRotate(node.Parent);
				node.UpdateCounts(true);
				return null;
			}

			if (node.Parent != null && node.Parent.NodeColor == RedBlackTreeNodeColor.Black && node.Sibling != null && node.Sibling.IsLeftChild && node.Sibling.NodeColor == RedBlackTreeNodeColor.Black && node.Sibling.Left?.NodeColor == RedBlackTreeNodeColor.Red) {
				node.Sibling.Left.NodeColor = RedBlackTreeNodeColor.Black;
				rightRotate(node.Parent);
				node.UpdateCounts(true);
				return null;
			}

			node.UpdateCounts(true);
			return null;
		}

		private void assignColors(RedBlackTreeNode<T> current) {
			if (current == null) {
				return;
			}

			assignColors(current.Left);
			assignColors(current.Right);

			if (current.IsLeaf) {
				current.NodeColor = RedBlackTreeNodeColor.Red;
			}
			else {
				current.NodeColor = RedBlackTreeNodeColor.Black;
			}
		}

		public T NextLower(T value) {
			var node = FindNode(value);
			if (node == null) {
				return default(T);
			}

			var next = (node as BSTNodeBase<T>).NextLower();
			return next != null ? next.Value : default(T);
		}

		public T NextHigher(T value) {
			var node = FindNode(value);
			if (node == null) {
				return default(T);
			}

			var next = (node as BSTNodeBase<T>).NextHigher();
			return next != null ? next.Value : default(T);
		}

		// public IEnumerable<T> AsEnumerableDesc() => GetEnumeratorDesc().AsEnumerable();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		public IEnumerator<T> GetEnumerator() => new BSTEnumerator<T>(Root);
		public IEnumerator<T> GetEnumeratorDesc() => new BSTEnumerator<T>(Root, false);
	}

	internal enum RedBlackTreeNodeColor {
		Black,
		Red
	}

	internal class RedBlackTreeNode<T>: BSTNodeBase<T> where T : IComparable {
		internal new RedBlackTreeNode<T> Parent {
			get => (RedBlackTreeNode<T>)base.Parent;
			set => base.Parent = value;
		}

		internal new RedBlackTreeNode<T> Left {
			get => (RedBlackTreeNode<T>)base.Left;
			set => base.Left = value;
		}

		internal new RedBlackTreeNode<T> Right {
			get => (RedBlackTreeNode<T>)base.Right;
			set => base.Right = value;
		}

		internal RedBlackTreeNodeColor NodeColor { get; set; }
		internal RedBlackTreeNode<T> Sibling => Parent.Left == this ? Parent.Right : Parent.Left;
		internal RedBlackTreeNode(RedBlackTreeNode<T> parent, T value) {
			Parent = parent;
			Value = value;
			NodeColor = RedBlackTreeNodeColor.Red;
		}
	}
}
