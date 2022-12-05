using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace TCLDecompiler {
	public readonly struct BranchTarget {
		public readonly int Location;
		public readonly BranchType Type;
		public BranchTarget(int location, BranchType type) {
			Location = location;
			Type = type;
		}
	}
}
