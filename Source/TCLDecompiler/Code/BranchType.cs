using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace TCLDecompiler {
	public enum BranchType {
		None = 0,
		Unconditional,
		Conditional,
		Fallthrough,
	}
}
