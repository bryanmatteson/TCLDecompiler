using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace TCLDecompiler {
	public enum LiteralType: ushort {
		Boolean = 'b',
		Bytecode = 'c',
		Double = 'd',
		Int = 'i',
		ProcBody = 'p',
		String = 's',
		XString = 'x'
	}
}
