using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace TCLDecompiler {
	public readonly struct CodeRange: IEquatable<CodeRange> {
		private static readonly CodeRange _empty;
		public static ref readonly CodeRange Empty => ref _empty;

		public readonly int Start;
		public readonly int Length;
		public readonly int End => Start + Length;
		public CodeRange(int start, int length) {
			Start = start;
			Length = length;
		}

		public readonly bool IsEmpty => Length == 0;
		public readonly bool Contains(int location) => unchecked((uint)(location - Start) < (uint)Length);
		public readonly bool Contains(CodeRange range) => Contains(range.Start, range.End);
		public readonly bool Contains(int start, int end) => start >= Start && end <= End;


		public readonly bool OverlapsWith(CodeRange range) => OverlapsWith(range.Start, range.End);
		public readonly bool OverlapsWith(int start, int end) => Math.Max(Start, start) < Math.Min(End, end);

		public readonly CodeRange? Overlap(CodeRange range) {
			int overlapStart = Math.Max(Start, range.Start);
			int overlapEnd = Math.Min(End, range.End);
			return overlapStart < overlapEnd ? FromBounds(overlapStart, overlapEnd) : (CodeRange?)null;
		}

		public readonly bool IntersectsWith(CodeRange range) => IntersectsWith(range.Start, range.End);
		public readonly bool IntersectsWith(int start, int end) => start <= End && end >= Start;
		public readonly bool IntersectsWith(int position) => unchecked((uint)(position - Start) <= (uint)Length);

		public readonly CodeRange Intersection(CodeRange range) {
			int intersectStart = Math.Max(Start, range.Start);
			int intersectEnd = Math.Min(End, range.End);
			return intersectStart <= intersectEnd ? FromBounds(intersectStart, intersectEnd) : Empty;
		}

		public static CodeRange FromBounds(int start, int end) {
			if (start < 0) throw new ArgumentOutOfRangeException(nameof(start));
			if (end < start) throw new ArgumentOutOfRangeException(nameof(end));
			return new CodeRange(start, end - start);
		}



		public override readonly bool Equals(object? obj) => obj is CodeRange range && Equals(range);
		public readonly bool Equals(CodeRange range) => Start == range.Start && Length == range.Length;
		public override readonly int GetHashCode() => HashCode.Combine(Start, Length);

		public readonly int CompareTo(object obj) => obj is CodeRange span ? CompareTo(span) : -1;
		public readonly int CompareTo(CodeRange other) {
			int diff = Start - other.Start;
			return diff != 0 ? diff : Length - other.Length;
		}

		public static bool operator ==(CodeRange lhs, CodeRange rhs) => lhs.Equals(rhs);
		public static bool operator !=(CodeRange lhs, CodeRange rhs) => !lhs.Equals(rhs);
		public static bool operator <(CodeRange lhs, CodeRange rhs) => lhs.CompareTo(rhs) < 0;
		public static bool operator >(CodeRange lhs, CodeRange rhs) => lhs.CompareTo(rhs) > 0;
		public static bool operator <=(CodeRange lhs, CodeRange rhs) => lhs.CompareTo(rhs) <= 0;
		public static bool operator >=(CodeRange lhs, CodeRange rhs) => lhs.CompareTo(rhs) >= 0;
	}
}
