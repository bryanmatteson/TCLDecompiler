using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace TCLDecompiler {
	public readonly struct DictUpdateInfo {
		public readonly int[] Indices;
		public DictUpdateInfo(int[] indices) => Indices = indices;

		public override bool Equals(object? obj) => obj is DictUpdateInfo info && EqualityComparer<int[]>.Default.Equals(Indices, info.Indices);
		public override int GetHashCode() => HashCode.Combine(Indices);
	}
}
