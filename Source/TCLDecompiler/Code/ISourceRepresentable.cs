using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TCLDecompiler {

	public interface ISourceRepresentable {
		string ToSourceString();
	}
}
