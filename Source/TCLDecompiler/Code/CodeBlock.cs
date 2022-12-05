using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;


namespace TCLDecompiler {
	public class CodeBlock: ICodeUnit, IEquatable<CodeBlock> {
		private readonly CodeMap _codeMap;
		public int Location => _codeMap.Range.Start;
		public int Size => _codeMap.Range.Length;
		public int StackEffect => _codeMap.Units.Aggregate(0, (acc, u) => acc + u.StackEffect);
		public CodeRange Range => _codeMap.Range;
		public IEnumerable<BranchTarget> Targets => _codeMap.Units.LastOrDefault()?.Targets ?? Enumerable.Empty<BranchTarget>();
		public bool IsSequential => _codeMap.IsSequential;

		public CodeBlock() { _codeMap = new CodeMap(); }
		public CodeBlock(ICodeUnit unit) {
			_codeMap = new CodeMap();
			_codeMap.Add(unit);
		}

		public void Merge(params ICodeUnit[] units) => _codeMap.MergeRange(units);
		public CodeBlock(IEnumerable<ICodeUnit> units) => _codeMap = new CodeMap(units);
		public void Append(ICodeUnit unit) {
			if (Range.End != unit.Range.Start) throw new InvalidOperationException("unit does not fall on end boundary");
			Merge(unit);
		}

		public override string ToString() {
			var builder = new StringBuilder();
			builder.Append("Block ").Append(Location).Append('-').Append(Location + Size).AppendLine();
			foreach (ICodeUnit unit in _codeMap.Units) builder.AppendLine(unit.ToString());
			return builder.AppendLine().ToString();
		}

		public override bool Equals(object? obj) => obj is CodeBlock && Equals(obj);
		public bool Equals(CodeBlock? other) => other is not null && _codeMap.Units.SequenceEqual(other._codeMap.Units);
		public override int GetHashCode() => _codeMap.GetHashCode();

		public static bool operator ==(CodeBlock left, CodeBlock right) => left.Equals(right);
		public static bool operator !=(CodeBlock left, CodeBlock right) => !(left == right);
		public static bool operator <(CodeBlock left, CodeBlock right) => left.Location < right.Location;
		public static bool operator >(CodeBlock left, CodeBlock right) => left.Location > right.Location;
		public static bool operator <=(CodeBlock left, CodeBlock right) => left.Location <= right.Location;
		public static bool operator >=(CodeBlock left, CodeBlock right) => left.Location >= right.Location;
	}
}
