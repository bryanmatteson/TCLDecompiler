namespace TCLDecompiler {
	public readonly struct InstrDesc {
		public readonly Opcode Opcode;
		public readonly int ArgumentCount;
		public readonly string Name;
		public readonly int ByteCount;
		public readonly int StackEffect;
		public readonly OperandType[] OperandTypes;

		public InstrDesc(Opcode opcode, int argc, string name, int byteCount, int stackEffect, OperandType[] operands) {
			Opcode = opcode;
			ArgumentCount = argc;
			Name = name;
			ByteCount = byteCount;
			StackEffect = stackEffect;
			OperandTypes = operands;
		}
	}
}
