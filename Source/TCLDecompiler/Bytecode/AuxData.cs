using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace TCLDecompiler {
	public readonly struct AuxData {
		public readonly AuxDataType Type;
		public readonly object Value;
		public AuxData(AuxDataType type, object value) {
			Type = type;
			Value = value;
		}
	}
}
