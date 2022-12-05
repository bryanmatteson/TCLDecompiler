using TCLDecompiler;

const string path = "/Users/bryan/Desktop/TBCDecompiler/Data/soccli.tbc";
var file = new TBCFile(path);
var decompiler = new Decompiler(new DecompilerOptions {
	TclVersion = file.Header.TclVersion,
	CompilerVersion = file.Header.CompilerVersion
});

file.Bytecode.PrintHeader();

decompiler.Decompile(file.Bytecode);
