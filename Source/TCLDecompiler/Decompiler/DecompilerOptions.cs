using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace TCLDecompiler {
	public class DecompilerOptions {
		public Version TclVersion { get; set; }
		public Version CompilerVersion { get; set; }
		public bool Verbose { get; set; }
	}
}
