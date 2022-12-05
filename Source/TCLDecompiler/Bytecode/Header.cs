using System;
using System.Collections.Generic;
using System.IO;

namespace TCLDecompiler {
	public readonly struct Header {
		public readonly int Format;
		public readonly int Build;
		public readonly Version CompilerVersion;
		public readonly Version TclVersion;

		public Header(int format, int build, Version compilerVersion, Version tclVersion) {
			Format = format;
			Build = build;
			CompilerVersion = compilerVersion;
			TclVersion = tclVersion;
		}

		public override bool Equals(object? obj) => obj is Header header &&
				   Format == header.Format &&
				   Build == header.Build &&
				   EqualityComparer<Version>.Default.Equals(CompilerVersion, header.CompilerVersion) &&
				   EqualityComparer<Version>.Default.Equals(TclVersion, header.TclVersion);

		public override int GetHashCode() => HashCode.Combine(Format, Build, CompilerVersion, TclVersion);
	}
}
