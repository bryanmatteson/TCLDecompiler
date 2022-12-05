using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace TCLDecompiler {
	public enum AuxDataType: ushort {
		DictUpdate = 'D',
		Foreach = 'F',
		JumpTable = 'J',
		NewForeach = 'f'
	}
}
