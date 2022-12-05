using System;
using System.IO;

namespace TCLDecompiler {
	public readonly struct Version {
		public readonly int Major;
		public readonly int Minor;
		public Version(int major, int minor) {
			Major = major;
			Minor = minor;
		}

		public override string ToString() => $"{Major}.{Minor}";
		public override bool Equals(object? obj) => obj is Version version && Major == version.Major && Minor == version.Minor;
		public override int GetHashCode() => HashCode.Combine(Major, Minor);
	}
}
