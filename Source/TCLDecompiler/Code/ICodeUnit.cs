using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace TCLDecompiler {

	public interface ICodeUnit: IComparable, IComparable<ICodeUnit>, IEquatable<ICodeUnit> {
		int Location { get; }
		int Size { get; }
		int StackEffect { get; }
		public CodeRange Range => new(Location, Size);
		IEnumerable<BranchTarget> Targets { get; }


		int IComparable.CompareTo(object? obj) => Location.CompareTo((obj as ICodeUnit)?.Location);
		int IComparable<ICodeUnit>.CompareTo(ICodeUnit? other) => Location.CompareTo(other?.Location);
		bool IEquatable<ICodeUnit>.Equals(ICodeUnit? other) => Location == other?.Location && Size == other?.Size;
	}
}
