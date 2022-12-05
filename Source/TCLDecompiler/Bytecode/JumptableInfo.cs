using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace TCLDecompiler {
	public readonly struct JumptableInfo {
		public readonly Dictionary<string, int> Dict;

		public JumptableInfo(Dictionary<string, int> dict) => Dict = dict;

		public override bool Equals(object? obj) => obj is JumptableInfo info && EqualityComparer<Dictionary<string, int>>.Default.Equals(Dict, info.Dict);
		public override int GetHashCode() => HashCode.Combine(Dict);
	}
}
