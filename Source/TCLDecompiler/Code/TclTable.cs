using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace TCLDecompiler {
	public enum StringClassTable {
		Alnum, Alpha, Ascii, Control, Digit, Graph,
		Lower, Print, Punct, Space, Upper, Word, Xdigit,
	}

	public enum BuiltInMathFunction {
		Acos = 0, Asin, Atan, Atan2,
		Ceil, Cos, Cosh, Exp, Floor, Fmod, Hypot,
		Log, Log10, Pow, Sin, Sinh, Sqrt, Tan,
		Tanh, Abs, Double, Int, Rand, Round, Srand, Wide,
	}


	public static class TclTable {
		public static int GetArgumentCount(this BuiltInMathFunction func) => func switch {
			BuiltInMathFunction.Rand => 0,
			BuiltInMathFunction.Acos or BuiltInMathFunction.Asin or BuiltInMathFunction.Atan or BuiltInMathFunction.Ceil or BuiltInMathFunction.Cos or BuiltInMathFunction.Cosh or BuiltInMathFunction.Exp or BuiltInMathFunction.Floor or BuiltInMathFunction.Log or BuiltInMathFunction.Log10 or BuiltInMathFunction.Sin or BuiltInMathFunction.Sinh or BuiltInMathFunction.Sqrt or BuiltInMathFunction.Tan or BuiltInMathFunction.Tanh or BuiltInMathFunction.Abs or BuiltInMathFunction.Double or BuiltInMathFunction.Int or BuiltInMathFunction.Round or BuiltInMathFunction.Srand or BuiltInMathFunction.Wide => 1,
			BuiltInMathFunction.Atan2 or BuiltInMathFunction.Fmod or BuiltInMathFunction.Hypot or BuiltInMathFunction.Pow => 2,
			_ => throw new InvalidOperationException("unknown function"),
		};

		public static string GetFunctionName(this BuiltInMathFunction func) {
			string name = Enum.GetName(typeof(BuiltInMathFunction), func) ?? throw new InvalidOperationException("unknown function");
			return char.ToLower(name[0]) + new string(name.AsSpan()[1..]);
		}

		public static readonly Dictionary<Opcode, InstrDesc> InstructionDescriptors = new Dictionary<Opcode, InstrDesc> {
			/* Finish ByteCode execution and return stktop (top stack item) */
			[Opcode.Done] = new InstrDesc(opcode: Opcode.Done, argc: 1, name: "done", byteCount: 1, stackEffect: -1, operands: Array.Empty<OperandType>()),

			/* Push object at ByteCode objArray[op1] */
			[Opcode.Push1] = new InstrDesc(opcode: Opcode.Push1, argc: 0, name: "push1", byteCount: 2, stackEffect: +1, operands: new[] { OperandType.Lit1 }),

			/* Push object at ByteCode objArray[op4] */
			[Opcode.Push4] = new InstrDesc(opcode: Opcode.Push4, argc: 0, name: "push4", byteCount: 5, stackEffect: +1, operands: new[] { OperandType.Lit4 }),

			/* Pop the topmost stack object */
			[Opcode.Pop] = new InstrDesc(opcode: Opcode.Pop, argc: 1, name: "pop", byteCount: 1, stackEffect: -1, operands: Array.Empty<OperandType>()),

			/* Duplicate the topmost stack object and push the result */
			[Opcode.Dup] = new InstrDesc(opcode: Opcode.Dup, argc: 1, name: "dup", byteCount: 1, stackEffect: +1, operands: Array.Empty<OperandType>()),

			/* Concatenate the top op1 items and push result */
			[Opcode.StrConcat1] = new InstrDesc(opcode: Opcode.StrConcat1, argc: int.MinValue, name: "strcat", byteCount: 2, stackEffect: int.MinValue, operands: new[] { OperandType.UInt1 }),

			/* Invoke command named objv[0]; <objc,objv> = <op1,top op1> */
			[Opcode.InvokeStk1] = new InstrDesc(opcode: Opcode.InvokeStk1, argc: int.MinValue, name: "invokeStk1", byteCount: 2, stackEffect: int.MinValue, operands: new[] { OperandType.UInt1 }),

			/* Invoke command named objv[0]; <objc,objv> = <op4,top op4> */
			[Opcode.InvokeStk4] = new InstrDesc(opcode: Opcode.InvokeStk4, argc: int.MinValue, name: "invokeStk4", byteCount: 5, stackEffect: int.MinValue, operands: new[] { OperandType.UInt4 }),

			/* Evaluate command in stktop using Tcl_EvalObj. */
			[Opcode.EvalStk] = new InstrDesc(opcode: Opcode.EvalStk, argc: 1, name: "evalStk", byteCount: 1, stackEffect: 0, operands: Array.Empty<OperandType>()),

			/* Execute expression in stktop using Tcl_ExprStringObj. */
			[Opcode.ExprStk] = new InstrDesc(opcode: Opcode.ExprStk, argc: 1, name: "exprStk", byteCount: 1, stackEffect: 0, operands: Array.Empty<OperandType>()),

			/* Load scalar variable at index op1 <= 255 in call frame */
			[Opcode.LoadScalar1] = new InstrDesc(opcode: Opcode.LoadScalar1, argc: 0, name: "loadScalar1", byteCount: 2, stackEffect: +1, operands: new[] { OperandType.Lvt1 }),

			/* Load scalar variable at index op1 >= 256 in call frame */
			[Opcode.LoadScalar4] = new InstrDesc(opcode: Opcode.LoadScalar4, argc: 0, name: "loadScalar4", byteCount: 5, stackEffect: +1, operands: new[] { OperandType.Lvt4 }),

			/* Load scalar variable; scalar's name is stktop */
			[Opcode.LoadScalarStk] = new InstrDesc(opcode: Opcode.LoadScalarStk, argc: 1, name: "loadScalarStk", byteCount: 1, stackEffect: 0, operands: Array.Empty<OperandType>()),

			/* Load array element; array at slot op1<=255, element is stktop */
			[Opcode.LoadArray1] = new InstrDesc(opcode: Opcode.LoadArray1, argc: 1, name: "loadArray1", byteCount: 2, stackEffect: 0, operands: new[] { OperandType.Lvt1 }),

			/* Load array element; array at slot op1 > 255, element is stktop */
			[Opcode.LoadArray4] = new InstrDesc(opcode: Opcode.LoadArray4, argc: 1, name: "loadArray4", byteCount: 5, stackEffect: 0, operands: new[] { OperandType.Lvt4 }),

			/* Load array element; element is stktop, array name is stknext */
			[Opcode.LoadArrayStk] = new InstrDesc(opcode: Opcode.LoadArrayStk, argc: 2, name: "loadArrayStk", byteCount: 1, stackEffect: -1, operands: Array.Empty<OperandType>()),

			/* Load general variable; unparsed variable name is stktop */
			[Opcode.LoadStk] = new InstrDesc(opcode: Opcode.LoadStk, argc: 1, name: "loadStk", byteCount: 1, stackEffect: 0, operands: Array.Empty<OperandType>()),

			/* Store scalar variable at op1<=255 in frame; value is stktop */
			[Opcode.StoreScalar1] = new InstrDesc(opcode: Opcode.StoreScalar1, argc: 1, name: "storeScalar1", byteCount: 2, stackEffect: 0, operands: new[] { OperandType.Lvt1 }),

			/* Store scalar variable at op1 > 255 in frame; value is stktop */
			[Opcode.StoreScalar4] = new InstrDesc(opcode: Opcode.StoreScalar4, argc: 1, name: "storeScalar4", byteCount: 5, stackEffect: 0, operands: new[] { OperandType.Lvt4 }),

			/* Store scalar; value is stktop, scalar name is stknext */
			[Opcode.StoreScalarStk] = new InstrDesc(opcode: Opcode.StoreScalarStk, argc: 2, name: "storeScalarStk", byteCount: 1, stackEffect: -1, operands: Array.Empty<OperandType>()),

			/* Store array element; array at op1<=255, value is top then elem */
			[Opcode.StoreArray1] = new InstrDesc(opcode: Opcode.StoreArray1, argc: 2, name: "storeArray1", byteCount: 2, stackEffect: -1, operands: new[] { OperandType.Lvt1 }),

			/* Store array element; array at op1>=256, value is top then elem */
			[Opcode.StoreArray4] = new InstrDesc(opcode: Opcode.StoreArray4, argc: 2, name: "storeArray4", byteCount: 5, stackEffect: -1, operands: new[] { OperandType.Lvt4 }),

			/* Store array element; value is stktop, then elem, array names */
			[Opcode.StoreArrayStk] = new InstrDesc(opcode: Opcode.StoreArrayStk, argc: 3, name: "storeArrayStk", byteCount: 1, stackEffect: -2, operands: Array.Empty<OperandType>()),

			/* Store general variable; value is stktop, then unparsed name */
			[Opcode.StoreStk] = new InstrDesc(opcode: Opcode.StoreStk, argc: 2, name: "storeStk", byteCount: 1, stackEffect: -1, operands: Array.Empty<OperandType>()),

			/* Incr scalar at index op1<=255 in frame; incr amount is stktop */
			[Opcode.IncrScalar1] = new InstrDesc(opcode: Opcode.IncrScalar1, argc: 1, name: "incrScalar1", byteCount: 2, stackEffect: 0, operands: new[] { OperandType.Lvt1 }),

			/* Incr scalar; incr amount is stktop, scalar's name is stknext */
			[Opcode.IncrScalarStk] = new InstrDesc(opcode: Opcode.IncrScalarStk, argc: 2, name: "incrScalarStk", byteCount: 1, stackEffect: -1, operands: Array.Empty<OperandType>()),

			/* Incr array elem; arr at slot op1<=255, amount is top then elem */
			[Opcode.IncrArray1] = new InstrDesc(opcode: Opcode.IncrArray1, argc: 2, name: "incrArray1", byteCount: 2, stackEffect: -1, operands: new[] { OperandType.Lvt1 }),

			/* Incr array element; amount is top then elem then array names */
			[Opcode.IncrArrayStk] = new InstrDesc(opcode: Opcode.IncrArrayStk, argc: 3, name: "incrArrayStk", byteCount: 1, stackEffect: -2, operands: Array.Empty<OperandType>()),

			/* Incr general variable; amount is stktop then unparsed var name */
			[Opcode.IncrStk] = new InstrDesc(opcode: Opcode.IncrStk, argc: 2, name: "incrStk", byteCount: 1, stackEffect: -1, operands: Array.Empty<OperandType>()),

			/* Incr scalar at slot op1 <= 255; amount is 2nd operand byte */
			[Opcode.IncrScalar1Imm] = new InstrDesc(opcode: Opcode.IncrScalar1Imm, argc: 0, name: "incrScalar1Imm", byteCount: 3, stackEffect: +1, operands: new[] { OperandType.Lvt1, OperandType.Int1 }),

			/* Incr scalar; scalar name is stktop; incr amount is op1 */
			[Opcode.IncrScalarStkImm] = new InstrDesc(opcode: Opcode.IncrScalarStkImm, argc: 1, name: "incrScalarStkImm", byteCount: 2, stackEffect: 0, operands: new[] { OperandType.Int1 }),

			/* Incr array elem; array at slot op1 <= 255, elem is stktop, amount is 2nd operand byte */
			[Opcode.IncrArray1Imm] = new InstrDesc(opcode: Opcode.IncrArray1Imm, argc: 1, name: "incrArray1Imm", byteCount: 3, stackEffect: 0, operands: new[] { OperandType.Lvt1, OperandType.Int1 }),

			/* Incr array element; elem is top then array name, amount is op1 */
			[Opcode.IncrArrayStkImm] = new InstrDesc(opcode: Opcode.IncrArrayStkImm, argc: 2, name: "incrArrayStkImm", byteCount: 2, stackEffect: -1, operands: new[] { OperandType.Int1 }),

			/* Incr general variable; unparsed name is top, amount is op1 */
			[Opcode.IncrStkImm] = new InstrDesc(opcode: Opcode.IncrStkImm, argc: 1, name: "incrStkImm", byteCount: 2, stackEffect: 0, operands: new[] { OperandType.Int1 }),

			/* Jump relative to (pc + op1) */
			[Opcode.Jump1] = new InstrDesc(opcode: Opcode.Jump1, argc: 0, name: "jump1", byteCount: 2, stackEffect: 0, operands: new[] { OperandType.Offset1 }),

			/* Jump relative to (pc + op4) */
			[Opcode.Jump4] = new InstrDesc(opcode: Opcode.Jump4, argc: 0, name: "jump4", byteCount: 5, stackEffect: 0, operands: new[] { OperandType.Offset4 }),

			/* Jump relative to (pc + op1) if stktop expr object is true */
			[Opcode.JumpTrue1] = new InstrDesc(opcode: Opcode.JumpTrue1, argc: 1, name: "jumpTrue1", byteCount: 2, stackEffect: -1, operands: new[] { OperandType.Offset1 }),

			/* Jump relative to (pc + op4) if stktop expr object is true */
			[Opcode.JumpTrue4] = new InstrDesc(opcode: Opcode.JumpTrue4, argc: 1, name: "jumpTrue4", byteCount: 5, stackEffect: -1, operands: new[] { OperandType.Offset4 }),

			/* Jump relative to (pc + op1) if stktop expr object is false */
			[Opcode.JumpFalse1] = new InstrDesc(opcode: Opcode.JumpFalse1, argc: 1, name: "jumpFalse1", byteCount: 2, stackEffect: -1, operands: new[] { OperandType.Offset1 }),

			/* Jump relative to (pc + op4) if stktop expr object is false */
			[Opcode.JumpFalse4] = new InstrDesc(opcode: Opcode.JumpFalse4, argc: 1, name: "jumpFalse4", byteCount: 5, stackEffect: -1, operands: new[] { OperandType.Offset4 }),

			/* Logical or:	push (stknext || stktop) */
			[Opcode.Lor] = new InstrDesc(opcode: Opcode.Lor, argc: 2, name: "lor", byteCount: 1, stackEffect: -1, operands: Array.Empty<OperandType>()),

			/* Logical and:	push (stknext && stktop) */
			[Opcode.Land] = new InstrDesc(opcode: Opcode.Land, argc: 2, name: "land", byteCount: 1, stackEffect: -1, operands: Array.Empty<OperandType>()),

			/* Bitwise or:	push (stknext | stktop) */
			[Opcode.Bitor] = new InstrDesc(opcode: Opcode.Bitor, argc: 2, name: "bitor", byteCount: 1, stackEffect: -1, operands: Array.Empty<OperandType>()),

			/* Bitwise xor	push (stknext ^ stktop) */
			[Opcode.Bitxor] = new InstrDesc(opcode: Opcode.Bitxor, argc: 2, name: "bitxor", byteCount: 1, stackEffect: -1, operands: Array.Empty<OperandType>()),

			/* Bitwise and:	push (stknext & stktop) */
			[Opcode.Bitand] = new InstrDesc(opcode: Opcode.Bitand, argc: 2, name: "bitand", byteCount: 1, stackEffect: -1, operands: Array.Empty<OperandType>()),

			/* Equal:	push (stknext == stktop) */
			[Opcode.Eq] = new InstrDesc(opcode: Opcode.Eq, argc: 2, name: "eq", byteCount: 1, stackEffect: -1, operands: Array.Empty<OperandType>()),

			/* Not equal:	push (stknext != stktop) */
			[Opcode.Neq] = new InstrDesc(opcode: Opcode.Neq, argc: 2, name: "neq", byteCount: 1, stackEffect: -1, operands: Array.Empty<OperandType>()),

			/* Less:	push (stknext < stktop) */
			[Opcode.Lt] = new InstrDesc(opcode: Opcode.Lt, argc: 2, name: "lt", byteCount: 1, stackEffect: -1, operands: Array.Empty<OperandType>()),

			/* Greater:	push (stknext > stktop) */
			[Opcode.Gt] = new InstrDesc(opcode: Opcode.Gt, argc: 2, name: "gt", byteCount: 1, stackEffect: -1, operands: Array.Empty<OperandType>()),

			/* Less or equal: push (stknext <= stktop) */
			[Opcode.Le] = new InstrDesc(opcode: Opcode.Le, argc: 2, name: "le", byteCount: 1, stackEffect: -1, operands: Array.Empty<OperandType>()),

			/* Greater or equal: push (stknext >= stktop) */
			[Opcode.Ge] = new InstrDesc(opcode: Opcode.Ge, argc: 2, name: "ge", byteCount: 1, stackEffect: -1, operands: Array.Empty<OperandType>()),

			/* Left shift:	push (stknext << stktop) */
			[Opcode.Lshift] = new InstrDesc(opcode: Opcode.Lshift, argc: 2, name: "lshift", byteCount: 1, stackEffect: -1, operands: Array.Empty<OperandType>()),

			/* Right shift:	push (stknext >> stktop) */
			[Opcode.Rshift] = new InstrDesc(opcode: Opcode.Rshift, argc: 2, name: "rshift", byteCount: 1, stackEffect: -1, operands: Array.Empty<OperandType>()),

			/* Add:		push (stknext + stktop) */
			[Opcode.Add] = new InstrDesc(opcode: Opcode.Add, argc: 2, name: "add", byteCount: 1, stackEffect: -1, operands: Array.Empty<OperandType>()),

			/* Sub:		push (stkext - stktop) */
			[Opcode.Sub] = new InstrDesc(opcode: Opcode.Sub, argc: 2, name: "sub", byteCount: 1, stackEffect: -1, operands: Array.Empty<OperandType>()),

			/* Multiply:	push (stknext * stktop) */
			[Opcode.Mult] = new InstrDesc(opcode: Opcode.Mult, argc: 2, name: "mult", byteCount: 1, stackEffect: -1, operands: Array.Empty<OperandType>()),

			/* Divide:	push (stknext / stktop) */
			[Opcode.Div] = new InstrDesc(opcode: Opcode.Div, argc: 2, name: "div", byteCount: 1, stackEffect: -1, operands: Array.Empty<OperandType>()),

			/* Mod:		push (stknext % stktop) */
			[Opcode.Mod] = new InstrDesc(opcode: Opcode.Mod, argc: 2, name: "mod", byteCount: 1, stackEffect: -1, operands: Array.Empty<OperandType>()),

			/* Unary plus:	push +stktop */
			[Opcode.Uplus] = new InstrDesc(opcode: Opcode.Uplus, argc: 1, name: "uplus", byteCount: 1, stackEffect: 0, operands: Array.Empty<OperandType>()),

			/* Unary minus:	push -stktop */
			[Opcode.Uminus] = new InstrDesc(opcode: Opcode.Uminus, argc: 1, name: "uminus", byteCount: 1, stackEffect: 0, operands: Array.Empty<OperandType>()),

			/* Bitwise not:	push ~stktop */
			[Opcode.Bitnot] = new InstrDesc(opcode: Opcode.Bitnot, argc: 1, name: "bitnot", byteCount: 1, stackEffect: 0, operands: Array.Empty<OperandType>()),

			/* Logical not:	push !stktop */
			[Opcode.Not] = new InstrDesc(opcode: Opcode.Not, argc: 1, name: "not", byteCount: 1, stackEffect: 0, operands: Array.Empty<OperandType>()),

			/* Call builtin math function with index op1; any args are on stk */
			[Opcode.CallBuiltinFunc1] = new InstrDesc(opcode: Opcode.CallBuiltinFunc1, argc: 0, name: "callBuiltinFunc1", byteCount: 2, stackEffect: +1, operands: new[] { OperandType.UInt1 }),

			/* Call non-builtin func objv[0]; <objc,objv>=<op1,top op1> */
			[Opcode.CallFunc1] = new InstrDesc(opcode: Opcode.CallFunc1, argc: int.MinValue, name: "callFunc1", byteCount: 2, stackEffect: int.MinValue, operands: new[] { OperandType.UInt1 }),

			/* Try converting stktop to first int then double if possible. */
			[Opcode.TryCvtToNumeric] = new InstrDesc(opcode: Opcode.TryCvtToNumeric, argc: 1, name: "tryCvtToNumeric", byteCount: 1, stackEffect: 0, operands: Array.Empty<OperandType>()),

			/* Abort closest enclosing loop; if none, return TCL_BREAK code. */
			[Opcode.Break] = new InstrDesc(opcode: Opcode.Break, argc: 0, name: "break", byteCount: 1, stackEffect: 0, operands: Array.Empty<OperandType>()),

			/* Skip to next iteration of closest enclosing loop; if none, return TCL_CONTINUE code. */
			[Opcode.Continue] = new InstrDesc(opcode: Opcode.Continue, argc: 0, name: "continue", byteCount: 1, stackEffect: 0, operands: Array.Empty<OperandType>()),

			/* Initialize execution of a foreach loop. Operand is aux data index of the ForeachInfo structure for the foreach command. */
			[Opcode.ForeachStart4] = new InstrDesc(opcode: Opcode.ForeachStart4, argc: 0, name: "foreach_start4", byteCount: 5, stackEffect: 0, operands: new[] { OperandType.Aux4 }),

			/* "Step" or begin next iteration of foreach loop. Push 0 if to terminate loop, else push 1. */
			[Opcode.ForeachStep4] = new InstrDesc(opcode: Opcode.ForeachStep4, argc: 0, name: "foreach_step4", byteCount: 5, stackEffect: +1, operands: new[] { OperandType.Aux4 }),

			/* Record start of catch with the operand's exception index. Push the current stack depth onto a special catch stack. */
			[Opcode.BeginCatch4] = new InstrDesc(opcode: Opcode.BeginCatch4, argc: 0, name: "beginCatch4", byteCount: 5, stackEffect: 0, operands: new[] { OperandType.UInt4 }),

			/* End of last catch. Pop the bytecode interpreter's catch stack. */
			[Opcode.EndCatch] = new InstrDesc(opcode: Opcode.EndCatch, argc: 0, name: "endCatch", byteCount: 1, stackEffect: 0, operands: Array.Empty<OperandType>()),

			/* Push the interpreter's object result onto the stack. */
			[Opcode.PushResult] = new InstrDesc(opcode: Opcode.PushResult, argc: 0, name: "pushResult", byteCount: 1, stackEffect: +1, operands: Array.Empty<OperandType>()),

			/* Push interpreter's return code (e.g. TCL_OK or TCL_ERROR) as a new object onto the stack. */
			[Opcode.PushReturnCode] = new InstrDesc(opcode: Opcode.PushReturnCode, argc: 0, name: "pushReturnCode", byteCount: 1, stackEffect: +1, operands: Array.Empty<OperandType>()),

			/* Str Equal:	push (stknext eq stktop) */
			[Opcode.StrEq] = new InstrDesc(opcode: Opcode.StrEq, argc: 2, name: "streq", byteCount: 1, stackEffect: -1, operands: Array.Empty<OperandType>()),

			/* Str !Equal:	push (stknext neq stktop) */
			[Opcode.StrNeq] = new InstrDesc(opcode: Opcode.StrNeq, argc: 2, name: "strneq", byteCount: 1, stackEffect: -1, operands: Array.Empty<OperandType>()),

			/* Str Compare:	push (stknext cmp stktop) */
			[Opcode.StrCmp] = new InstrDesc(opcode: Opcode.StrCmp, argc: 2, name: "strcmp", byteCount: 1, stackEffect: -1, operands: Array.Empty<OperandType>()),

			/* Str Length:	push (strlen stktop) */
			[Opcode.StrLen] = new InstrDesc(opcode: Opcode.StrLen, argc: 1, name: "strlen", byteCount: 1, stackEffect: 0, operands: Array.Empty<OperandType>()),

			/* Str Index:	push (strindex stknext stktop) */
			[Opcode.StrIndex] = new InstrDesc(opcode: Opcode.StrIndex, argc: 2, name: "strindex", byteCount: 1, stackEffect: -1, operands: Array.Empty<OperandType>()),

			/* Str Match:	push (strmatch stknext stktop) opnd == nocase */
			[Opcode.StrMatch] = new InstrDesc(opcode: Opcode.StrMatch, argc: 2, name: "strmatch", byteCount: 2, stackEffect: -1, operands: new[] { OperandType.Int1 }),

			/* List:	push (stk1 stk2 ... stktop) */
			[Opcode.List] = new InstrDesc(opcode: Opcode.List, argc: int.MinValue, name: "list", byteCount: 5, stackEffect: int.MinValue, operands: new[] { OperandType.UInt4 }),

			/* List Index:	push (listindex stknext stktop) */
			[Opcode.ListIndex] = new InstrDesc(opcode: Opcode.ListIndex, argc: 2, name: "listIndex", byteCount: 1, stackEffect: -1, operands: Array.Empty<OperandType>()),

			/* List Len:	push (listlength stktop) */
			[Opcode.ListLength] = new InstrDesc(opcode: Opcode.ListLength, argc: 1, name: "listLength", byteCount: 1, stackEffect: 0, operands: Array.Empty<OperandType>()),

			/* Append scalar variable at op1<=255 in frame; value is stktop */
			[Opcode.AppendScalar1] = new InstrDesc(opcode: Opcode.AppendScalar1, argc: 1, name: "appendScalar1", byteCount: 2, stackEffect: 0, operands: new[] { OperandType.Lvt1 }),

			/* Append scalar variable at op1 > 255 in frame; value is stktop */
			[Opcode.AppendScalar4] = new InstrDesc(opcode: Opcode.AppendScalar4, argc: 1, name: "appendScalar4", byteCount: 5, stackEffect: 0, operands: new[] { OperandType.Lvt4 }),

			/* Append array element; array at op1<=255, value is top then elem */
			[Opcode.AppendArray1] = new InstrDesc(opcode: Opcode.AppendArray1, argc: 2, name: "appendArray1", byteCount: 2, stackEffect: -1, operands: new[] { OperandType.Lvt1 }),

			/* Append array element; array at op1>=256, value is top then elem */
			[Opcode.AppendArray4] = new InstrDesc(opcode: Opcode.AppendArray4, argc: 2, name: "appendArray4", byteCount: 5, stackEffect: -1, operands: new[] { OperandType.Lvt4 }),

			/* Append array element; value is stktop, then elem, array names */
			[Opcode.AppendArrayStk] = new InstrDesc(opcode: Opcode.AppendArrayStk, argc: 3, name: "appendArrayStk", byteCount: 1, stackEffect: -2, operands: Array.Empty<OperandType>()),

			/* Append general variable; value is stktop, then unparsed name */
			[Opcode.AppendStk] = new InstrDesc(opcode: Opcode.AppendStk, argc: 2, name: "appendStk", byteCount: 1, stackEffect: -1, operands: Array.Empty<OperandType>()),

			/* Lappend scalar variable at op1<=255 in frame; value is stktop */
			[Opcode.LappendScalar1] = new InstrDesc(opcode: Opcode.LappendScalar1, argc: 1, name: "lappendScalar1", byteCount: 2, stackEffect: 0, operands: new[] { OperandType.Lvt1 }),

			/* Lappend scalar variable at op1 > 255 in frame; value is stktop */
			[Opcode.LappendScalar4] = new InstrDesc(opcode: Opcode.LappendScalar4, argc: 1, name: "lappendScalar4", byteCount: 5, stackEffect: 0, operands: new[] { OperandType.Lvt4 }),

			/* Lappend array element; array at op1<=255, value is top then elem */
			[Opcode.LappendArray1] = new InstrDesc(opcode: Opcode.LappendArray1, argc: 2, name: "lappendArray1", byteCount: 2, stackEffect: -1, operands: new[] { OperandType.Lvt1 }),

			/* Lappend array element; array at op1>=256, value is top then elem */
			[Opcode.LappendArray4] = new InstrDesc(opcode: Opcode.LappendArray4, argc: 2, name: "lappendArray4", byteCount: 5, stackEffect: -1, operands: new[] { OperandType.Lvt4 }),

			/* Lappend array element; value is stktop, then elem, array names */
			[Opcode.LappendArrayStk] = new InstrDesc(opcode: Opcode.LappendArrayStk, argc: 3, name: "lappendArrayStk", byteCount: 1, stackEffect: -2, operands: Array.Empty<OperandType>()),

			/* Lappend general variable; value is stktop, then unparsed name */
			[Opcode.LappendStk] = new InstrDesc(opcode: Opcode.LappendStk, argc: 2, name: "lappendStk", byteCount: 1, stackEffect: -1, operands: Array.Empty<OperandType>()),

			/* Lindex with generalized args, operand is number of stacked objs used: (operand-1) entries from stktop are the indices; then list to process. */
			[Opcode.ListIndexMulti] = new InstrDesc(opcode: Opcode.ListIndexMulti, argc: int.MinValue, name: "lindexMulti", byteCount: 5, stackEffect: int.MinValue, operands: new[] { OperandType.UInt4 }),

			/* Duplicate the arg-th element from top of stack (TOS=0) */
			[Opcode.Over] = new InstrDesc(opcode: Opcode.Over, argc: -1, name: "over", byteCount: 5, stackEffect: +1, operands: new[] { OperandType.UInt4 }),

			/* Four-arg version of 'lset'. stktop is old value; next is new element value, next is the index list; pushes new value */
			[Opcode.LsetList] = new InstrDesc(opcode: Opcode.LsetList, argc: 4, name: "lsetList", byteCount: 1, stackEffect: -2, operands: Array.Empty<OperandType>()),

			/* Three- or >=5-arg version of 'lset', operand is number of stacked objs: stktop is old value, next is new element value, next come (operand-2) indices; pushes the new value. */
			[Opcode.LsetFlat] = new InstrDesc(opcode: Opcode.LsetFlat, argc: int.MinValue, name: "lsetFlat", byteCount: 5, stackEffect: int.MinValue, operands: new[] { OperandType.UInt4 }),

			/* Compiled [return], code, level are operands; options and result are on the stack. */
			[Opcode.ReturnImm] = new InstrDesc(opcode: Opcode.ReturnImm, argc: 2, name: "returnImm", byteCount: 9, stackEffect: -1, operands: new[] { OperandType.Int4, OperandType.UInt4 }),

			/* Binary exponentiation operator: push (stknext ** stktop) */
			[Opcode.Expon] = new InstrDesc(opcode: Opcode.Expon, argc: 2, name: "expon", byteCount: 1, stackEffect: -1, operands: Array.Empty<OperandType>()),

			/* Start of command with {*} (expanded) arguments */
			[Opcode.ExpandStart] = new InstrDesc(opcode: Opcode.ExpandStart, argc: -1, name: "expandStart", byteCount: 1, stackEffect: 0, operands: Array.Empty<OperandType>()),

			/* Expand the list at stacktop: push its elements on the stack */
			[Opcode.ExpandStktop] = new InstrDesc(opcode: Opcode.ExpandStktop, argc: -1, name: "expandStkTop", byteCount: 5, stackEffect: 0, operands: new[] { OperandType.UInt4 }),

			/* Invoke the command marked by the last 'expandStart' */
			[Opcode.InvokeExpanded] = new InstrDesc(opcode: Opcode.InvokeExpanded, argc: -1, name: "invokeExpanded", byteCount: 1, stackEffect: 0, operands: Array.Empty<OperandType>()),

			/* List Index:	push (lindex stktop op4) */
			[Opcode.ListIndexImm] = new InstrDesc(opcode: Opcode.ListIndexImm, argc: 1, name: "listIndexImm", byteCount: 5, stackEffect: 0, operands: new[] { OperandType.Idx4 }),

			/* List Range:	push (lrange stktop op4 op4) */
			[Opcode.ListRangeImm] = new InstrDesc(opcode: Opcode.ListRangeImm, argc: 1, name: "listRangeImm", byteCount: 9, stackEffect: 0, operands: new[] { OperandType.Idx4, OperandType.Idx4 }),

			/* Start of bytecoded command: op is the length of the cmd's code, op2 is number of commands here */
			[Opcode.StartCmd] = new InstrDesc(opcode: Opcode.StartCmd, argc: -1, name: "startCommand", byteCount: 9, stackEffect: 0, operands: new[] { OperandType.Offset4, OperandType.UInt4 }),

			/* List containment: push [lsearch stktop stknext]>=0) */
			[Opcode.ListIn] = new InstrDesc(opcode: Opcode.ListIn, argc: 2, name: "listIn", byteCount: 1, stackEffect: -1, operands: Array.Empty<OperandType>()),

			/* List negated containment: push [lsearch stktop stknext]<0) */
			[Opcode.ListNotIn] = new InstrDesc(opcode: Opcode.ListNotIn, argc: 2, name: "listNotIn", byteCount: 1, stackEffect: -1, operands: Array.Empty<OperandType>()),

			/* Push the interpreter's return option dictionary as an object on the stack. */
			[Opcode.PushReturnOptions] = new InstrDesc(opcode: Opcode.PushReturnOptions, argc: 0, name: "pushReturnOpts", byteCount: 1, stackEffect: +1, operands: Array.Empty<OperandType>()),

			/* Compiled [return]; options and result are on the stack, code and level are in the options. */
			[Opcode.ReturnStk] = new InstrDesc(opcode: Opcode.ReturnStk, argc: 2, name: "returnStk", byteCount: 1, stackEffect: -1, operands: Array.Empty<OperandType>()),

			/* The top op4 words (min 1) are a key path into the dictionary just below the keys on the stack, and all those values are replaced by the value read out of that key-path (like [dict get]).
			 * Stack:  ... dict key1 ... keyN => ... value */
			[Opcode.DictGet] = new InstrDesc(opcode: Opcode.DictGet, argc: int.MinValue, name: "dictGet", byteCount: 5, stackEffect: int.MinValue, operands: new[] { OperandType.UInt4 }),

			/* Update a dictionary value such that the keys are a path pointing to the value. op4#1 = numKeys, op4#2 = LVTindex
			 * Stack:  ... key1 ... keyN value => ... newDict */
			[Opcode.DictSet] = new InstrDesc(opcode: Opcode.DictSet, argc: int.MinValue, name: "dictSet", byteCount: 9, stackEffect: int.MinValue, operands: new[] { OperandType.UInt4, OperandType.Lvt4 }),

			/* Update a dictionary value such that the keys are not a path pointing to any value. op4#1 = numKeys, op4#2 = LVTindex
			 * Stack:  ... key1 ... keyN => ... newDict */
			[Opcode.DictUnset] = new InstrDesc(opcode: Opcode.DictUnset, argc: int.MinValue, name: "dictUnset", byteCount: 9, stackEffect: int.MinValue, operands: new[] { OperandType.UInt4, OperandType.Lvt4 }),

			/* Update a dictionary value such that the value pointed to by key is incremented by some value (or set to it if the key isn't in the dictionary at all). op4#1 = incrAmount, op4#2 = LVTindex
			 * Stack:  ... key => ... newDict */
			[Opcode.DictIncrImm] = new InstrDesc(opcode: Opcode.DictIncrImm, argc: 1, name: "dictIncrImm", byteCount: 9, stackEffect: 0, operands: new[] { OperandType.Int4, OperandType.Lvt4 }),

			/* Update a dictionary value such that the value pointed to by key has some value string-concatenated onto it. op4 = LVTindex
			 * Stack:  ... key valueToAppend => ... newDict */
			[Opcode.DictAppend] = new InstrDesc(opcode: Opcode.DictAppend, argc: 2, name: "dictAppend", byteCount: 5, stackEffect: -1, operands: new[] { OperandType.Lvt4 }),

			/* Update a dictionary value such that the value pointed to by key has some value list-appended onto it. op4 = LVTindex
			 * Stack:  ... key valueToAppend => ... newDict */
			[Opcode.DictLappend] = new InstrDesc(opcode: Opcode.DictLappend, argc: 2, name: "dictLappend", byteCount: 5, stackEffect: -1, operands: new[] { OperandType.Lvt4 }),

			/* Begin iterating over the dictionary, using the local scalar indicated by op4 to hold the iterator state. The local scalar should not refer to a named variable as the value is not wholly managed correctly.
			 * Stack:  ... dict => ... value key doneBool */
			[Opcode.DictFirst] = new InstrDesc(opcode: Opcode.DictFirst, argc: 1, name: "dictFirst", byteCount: 5, stackEffect: +2, operands: new[] { OperandType.Lvt4 }),

			/* Get the next iteration from the iterator in op4's local scalar.
			 * Stack:  ... => ... value key doneBool */
			[Opcode.DictNext] = new InstrDesc(opcode: Opcode.DictNext, argc: 0, name: "dictNext", byteCount: 5, stackEffect: +3, operands: new[] { OperandType.Lvt4 }),

			/* Terminate the iterator in op4's local scalar. Use unsetScalar instead (with 0 for flags). */
			[Opcode.DictDone] = new InstrDesc(opcode: Opcode.DictDone, argc: 0, name: "dictDone", byteCount: 5, stackEffect: 0, operands: new[] { OperandType.Lvt4 }),

			/* Create the variables (described in the aux data referred to by the second immediate argument) to mirror
			 * the state of the dictionary in the variable referred to by the first immediate argument. The list of keys (top of the stack, not popped) must be the same length as the list of variables.
			 * Stack:  ... keyList => ... keyList */
			[Opcode.DictUpdateStart] = new InstrDesc(opcode: Opcode.DictUpdateStart, argc: 1, name: "dictUpdateStart", byteCount: 9, stackEffect: 0, operands: new[] { OperandType.Lvt4, OperandType.Aux4 }),

			/* Reflect the state of local variables (described in the aux data referred to by the second immediate argument)
			 * back to the state of the dictionary in the variable referred to by the first immediate argument. The list of keys (popped from the stack) must be the same length as the list of variables.
			 * Stack:  ... keyList => ... */
			[Opcode.DictUpdateEnd] = new InstrDesc(opcode: Opcode.DictUpdateEnd, argc: 1, name: "dictUpdateEnd", byteCount: 9, stackEffect: -1, operands: new[] { OperandType.Lvt4, OperandType.Aux4 }),

			/* Jump according to the jump-table (in AuxData as indicated by the operand) and the argument popped from the list. Always executes the next instruction if no match against the table's entries was found.
			 * Stack:  ... value => ... */
			[Opcode.JumpTable] = new InstrDesc(opcode: Opcode.JumpTable, argc: 1, name: "jumpTable", byteCount: 5, stackEffect: -1, operands: new[] { OperandType.Aux4 }),

			/* finds level and otherName in stack, links to local variable at index op1. Leaves the level on stack. */
			[Opcode.Upvar] = new InstrDesc(opcode: Opcode.Upvar, argc: 2, name: "upvar", byteCount: 5, stackEffect: -1, operands: new[] { OperandType.Lvt4 }),

			/* finds namespace and otherName in stack, links to local variable at index op1. Leaves the namespace on stack. */
			[Opcode.Nsupvar] = new InstrDesc(opcode: Opcode.Nsupvar, argc: 2, name: "nsupvar", byteCount: 5, stackEffect: -1, operands: new[] { OperandType.Lvt4 }),

			/* finds namespace and otherName in stack, links to local variable at index op1. Leaves the namespace on stack. */
			[Opcode.Variable] = new InstrDesc(opcode: Opcode.Variable, argc: 2, name: "variable", byteCount: 5, stackEffect: -1, operands: new[] { OperandType.Lvt4 }),

			/* Compiled bytecodes to signal syntax error. Equivalent to returnImm except for the ERR_ALREADY_LOGGED flag in the interpreter. */
			[Opcode.Syntax] = new InstrDesc(opcode: Opcode.Syntax, argc: 1, name: "syntax", byteCount: 9, stackEffect: -1, operands: new[] { OperandType.Int4, OperandType.UInt4 }),

			/* Reverse the order of the arg elements at the top of stack */
			[Opcode.Reverse] = new InstrDesc(opcode: Opcode.Reverse, argc: int.MinValue, name: "reverse", byteCount: 5, stackEffect: 0, operands: new[] { OperandType.UInt4 }),

			/* Regexp:	push (regexp stknext stktop) opnd == nocase */
			[Opcode.Regexp] = new InstrDesc(opcode: Opcode.Regexp, argc: 2, name: "regexp", byteCount: 2, stackEffect: -1, operands: new[] { OperandType.Int1 }),

			/* Test if scalar variable at index op1 in call frame exists */
			[Opcode.ExistScalar] = new InstrDesc(opcode: Opcode.ExistScalar, argc: 0, name: "existScalar", byteCount: 5, stackEffect: +1, operands: new[] { OperandType.Lvt4 }),

			/* Test if array element exists; array at slot op1, element is stktop */
			[Opcode.ExistArray] = new InstrDesc(opcode: Opcode.ExistArray, argc: 1, name: "existArray", byteCount: 5, stackEffect: 0, operands: new[] { OperandType.Lvt4 }),

			/* Test if array element exists; element is stktop, array name is stknext */
			[Opcode.ExistArrayStk] = new InstrDesc(opcode: Opcode.ExistArrayStk, argc: 2, name: "existArrayStk", byteCount: 1, stackEffect: -1, operands: Array.Empty<OperandType>()),

			/* Test if general variable exists; unparsed variable name is stktop*/
			[Opcode.ExistStk] = new InstrDesc(opcode: Opcode.ExistStk, argc: 1, name: "existStk", byteCount: 1, stackEffect: 0, operands: Array.Empty<OperandType>()),

			/* Do nothing */
			[Opcode.Nop] = new InstrDesc(opcode: Opcode.Nop, argc: 0, name: "nop", byteCount: 1, stackEffect: 0, operands: Array.Empty<OperandType>()),

			/* Jump to next instruction based on the return code on top of stack: ERROR: +1; RETURN: +3; BREAK: +5;	CONTINUE: +7; Other non-OK: +9 */
			[Opcode.ReturnCodeBranch] = new InstrDesc(opcode: Opcode.ReturnCodeBranch, argc: 1, name: "returnCodeBranch", byteCount: 1, stackEffect: -1, operands: Array.Empty<OperandType>()),

			/* Make scalar variable at index op2 in call frame cease to exist; op1 is 1 for errors on problems, 0 otherwise */
			[Opcode.UnsetScalar] = new InstrDesc(opcode: Opcode.UnsetScalar, argc: 0, name: "unsetScalar", byteCount: 6, stackEffect: 0, operands: new[] { OperandType.UInt1, OperandType.Lvt4 }),

			/* Make array element cease to exist; array at slot op2, element is stktop; op1 is 1 for errors on problems, 0 otherwise */
			[Opcode.UnsetArray] = new InstrDesc(opcode: Opcode.UnsetArray, argc: 1, name: "unsetArray", byteCount: 6, stackEffect: -1, operands: new[] { OperandType.UInt1, OperandType.Lvt4 }),

			/* Make array element cease to exist; element is stktop, array name is stknext; op1 is 1 for errors on problems, 0 otherwise */
			[Opcode.UnsetArrayStk] = new InstrDesc(opcode: Opcode.UnsetArrayStk, argc: 2, name: "unsetArrayStk", byteCount: 2, stackEffect: -2, operands: new[] { OperandType.UInt1 }),

			/* Make general variable cease to exist; unparsed variable name is stktop; op1 is 1 for errors on problems, 0 otherwise */
			[Opcode.UnsetStk] = new InstrDesc(opcode: Opcode.UnsetStk, argc: 1, name: "unsetStk", byteCount: 2, stackEffect: -1, operands: new[] { OperandType.UInt1 }),

			/* Probe into a dict and extract it (or a subdict of it) into variables with matched names. Produces list of keys bound as result. Part of [dict with].
			 * Stack:  ... dict path => ... keyList */
			[Opcode.DictExpand] = new InstrDesc(opcode: Opcode.DictExpand, argc: 2, name: "dictExpand", byteCount: 1, stackEffect: -1, operands: Array.Empty<OperandType>()),

			/* Map variable contents back into a dictionary in a variable. Part of [dict with].
			 * Stack:  ... dictVarName path keyList => ... */
			[Opcode.DictRecombineStk] = new InstrDesc(opcode: Opcode.DictRecombineStk, argc: 3, name: "dictRecombineStk", byteCount: 1, stackEffect: -3, operands: Array.Empty<OperandType>()),

			/* Map variable contents back into a dictionary in the local variable indicated by the LVT index. Part of [dict with].
			 * Stack:  ... path keyList => ... */
			[Opcode.DictRecombineImm] = new InstrDesc(opcode: Opcode.DictRecombineImm, argc: 2, name: "dictRecombineImm", byteCount: 5, stackEffect: -2, operands: new[] { OperandType.Lvt4 }),

			/* The top op4 words (min 1) are a key path into the dictionary just below the keys on the stack, and all those values
			 * are replaced by a boolean indicating whether it is possible to read out a value from that key-path (like [dict exists]).
			 * Stack:  ... dict key1 ... keyN => ... boolean */
			[Opcode.DictExists] = new InstrDesc(opcode: Opcode.DictExists, argc: int.MinValue, name: "dictExists", byteCount: 5, stackEffect: int.MinValue, operands: new[] { OperandType.UInt4 }),

			/* Verifies that the word on the top of the stack is a dictionary, popping it if it is and throwing an error if it is not.
			 * Stack:  ... value => ... */
			[Opcode.DictVerify] = new InstrDesc(opcode: Opcode.DictVerify, argc: 1, name: "verifyDict", byteCount: 1, stackEffect: -1, operands: Array.Empty<OperandType>()),

			/* Simplified version of [string map] that only applies one change string, and only case-sensitively.
			 * Stack:  ... from to string => ... changedString */
			[Opcode.StrMap] = new InstrDesc(opcode: Opcode.StrMap, argc: 3, name: "strmap", byteCount: 1, stackEffect: -2, operands: Array.Empty<OperandType>()),

			/* Find the first index of a needle string in a haystack string, producing the index (integer) or -1 if nothing found.
			 * Stack:  ... needle haystack => ... index */
			[Opcode.StrFind] = new InstrDesc(opcode: Opcode.StrFind, argc: 2, name: "strfind", byteCount: 1, stackEffect: -1, operands: Array.Empty<OperandType>()),

			/* Find the last index of a needle string in a haystack string, producing the index (integer) or -1 if nothing found.
			 * Stack:  ... needle haystack => ... index */
			[Opcode.StrFindLast] = new InstrDesc(opcode: Opcode.StrFindLast, argc: 2, name: "strrfind", byteCount: 1, stackEffect: -1, operands: Array.Empty<OperandType>()),

			/* String Range: push (string range stktop op4 op4) */
			[Opcode.StrRangeImm] = new InstrDesc(opcode: Opcode.StrRangeImm, argc: 1, name: "strrangeImm", byteCount: 9, stackEffect: 0, operands: new[] { OperandType.Idx4, OperandType.Idx4 }),

			/* String Range with non-constant arguments.
			 * Stack:  ... string idxA idxB => ... substring */
			[Opcode.StrRange] = new InstrDesc(opcode: Opcode.StrRange, argc: 3, name: "strrange", byteCount: 1, stackEffect: -2, operands: Array.Empty<OperandType>()),

			/* Makes the current coroutine yield the value at the top of the stack, and places the response back on top of the stack when it resumes.
			 * Stack:  ... valueToYield => ... resumeValue */
			[Opcode.Yield] = new InstrDesc(opcode: Opcode.Yield, argc: 1, name: "yield", byteCount: 1, stackEffect: 0, operands: Array.Empty<OperandType>()),

			/* Push the name of the interpreter's current coroutine as an object on the stack. */
			[Opcode.CoroutineName] = new InstrDesc(opcode: Opcode.CoroutineName, argc: 0, name: "coroName", byteCount: 1, stackEffect: +1, operands: Array.Empty<OperandType>()),

			/* Do a tailcall with the opnd items on the stack as the thing to tailcall to; opnd must be greater than 0 for the semantics to work right. */
			[Opcode.Tailcall] = new InstrDesc(opcode: Opcode.Tailcall, argc: int.MinValue, name: "tailcall", byteCount: 2, stackEffect: int.MinValue, operands: new[] { OperandType.UInt1 }),

			/* Push the name of the interpreter's current namespace as an object on the stack. */
			[Opcode.NsCurrent] = new InstrDesc(opcode: Opcode.NsCurrent, argc: 0, name: "currentNamespace", byteCount: 1, stackEffect: +1, operands: Array.Empty<OperandType>()),

			/* Push the stack depth (i.e., [info level]) of the interpreter as an object on the stack. */
			[Opcode.InfoLevelNum] = new InstrDesc(opcode: Opcode.InfoLevelNum, argc: 0, name: "infoLevelNumber", byteCount: 1, stackEffect: +1, operands: Array.Empty<OperandType>()),

			/* Push the argument words to a stack depth (i.e., [info level <n>]) of the interpreter as an object on the stack.
			 * Stack:  ... depth => ... argList */
			[Opcode.InfoLevelArgs] = new InstrDesc(opcode: Opcode.InfoLevelArgs, argc: 1, name: "infoLevelArgs", byteCount: 1, stackEffect: 0, operands: Array.Empty<OperandType>()),

			/* Resolves the command named on the top of the stack to its fully qualified version, or produces the empty string if no such command exists. Never generates errors.
			 * Stack:  ... cmdName => ... fullCmdName */
			[Opcode.ResolveCommand] = new InstrDesc(opcode: Opcode.ResolveCommand, argc: 1, name: "resolveCmd", byteCount: 1, stackEffect: 0, operands: Array.Empty<OperandType>()),

			/* Push the identity of the current TclOO object (i.e., the name of its current public access command) on the stack. */
			[Opcode.TclooSelf] = new InstrDesc(opcode: Opcode.TclooSelf, argc: 0, name: "tclooSelf", byteCount: 1, stackEffect: +1, operands: Array.Empty<OperandType>()),

			/* Push the class of the TclOO object named at the top of the stack onto the stack.
			 * Stack:  ... object => ... class */
			[Opcode.TclooClass] = new InstrDesc(opcode: Opcode.TclooClass, argc: 1, name: "tclooClass", byteCount: 1, stackEffect: 0, operands: Array.Empty<OperandType>()),

			/* Push the namespace of the TclOO object named at the top of the stack onto the stack.
			 * Stack:  ... object => ... namespace */
			[Opcode.TclooNs] = new InstrDesc(opcode: Opcode.TclooNs, argc: 0, name: "tclooNamespace", byteCount: 1, stackEffect: 0, operands: Array.Empty<OperandType>()),

			/* Push whether the value named at the top of the stack is a TclOO object (i.e., a boolean). Can corrupt the interpreter result despite not throwing, so not safe for use in a post-exception context.
			 * Stack:  ... value => ... boolean */
			[Opcode.TclooIsObject] = new InstrDesc(opcode: Opcode.TclooIsObject, argc: 1, name: "tclooIsObject", byteCount: 1, stackEffect: 0, operands: Array.Empty<OperandType>()),

			/* Looks up the element on the top of the stack and tests whether it is an array. Pushes a boolean describing whether this is the case. Also runs the whole-array trace on the named variable, so can throw anything.
			 * Stack:  ... varName => ... boolean */
			[Opcode.ArrayExistsStk] = new InstrDesc(opcode: Opcode.ArrayExistsStk, argc: 1, name: "arrayExistsStk", byteCount: 1, stackEffect: 0, operands: Array.Empty<OperandType>()),

			/* Looks up the variable indexed by opnd and tests whether it is an array. Pushes a boolean describing whether this is the case. Also runs the whole-array trace on the named variable, so can throw anything.
			 * Stack:  ... => ... boolean */
			[Opcode.ArrayExistsImm] = new InstrDesc(opcode: Opcode.ArrayExistsImm, argc: 0, name: "arrayExistsImm", byteCount: 5, stackEffect: +1, operands: new[] { OperandType.Lvt4 }),

			/* Forces the element on the top of the stack to be the name of an array.
			 * Stack:  ... varName => ... */
			[Opcode.ArrayMakeStk] = new InstrDesc(opcode: Opcode.ArrayMakeStk, argc: 1, name: "arrayMakeStk", byteCount: 1, stackEffect: -1, operands: Array.Empty<OperandType>()),

			/* Forces the variable indexed by opnd to be an array. Does not touch the stack. */
			[Opcode.ArrayMakeImm] = new InstrDesc(opcode: Opcode.ArrayMakeImm, argc: 0, name: "arrayMakeImm", byteCount: 5, stackEffect: 0, operands: new[] { OperandType.Lvt4 }),

			/* Invoke command named objv[0], replacing the first two words with the word at the top of the stack;
			 * <objc,objv> = <op4,top op4 after popping 1> */
			[Opcode.InvokeReplace] = new InstrDesc(opcode: Opcode.InvokeReplace, argc: int.MinValue, name: "invokeReplace", byteCount: 6, stackEffect: int.MinValue, operands: new[] { OperandType.UInt4, OperandType.UInt1 }),

			/* Concatenates the two lists at the top of the stack into a single list and pushes that resulting list onto the stack.
			 * Stack: ... list1 list2 => ... [lconcat list1 list2] */
			[Opcode.ListConcat] = new InstrDesc(opcode: Opcode.ListConcat, argc: 2, name: "listConcat", byteCount: 1, stackEffect: -1, operands: Array.Empty<OperandType>()),

			/* Drops an element from the auxiliary stack, popping stack elements until the matching stack depth is reached. */
			[Opcode.ExpandDrop] = new InstrDesc(opcode: Opcode.ExpandDrop, argc: -1, name: "expandDrop", byteCount: 1, stackEffect: 0, operands: Array.Empty<OperandType>()),

			/* Initialize execution of a foreach loop. Operand is aux data index of the ForeachInfo structure for the foreach command.
			 * It pushes 2 elements which hold runtime params for foreach_step, they are later dropped by foreach_end together with the value lists.
			 * NOTE that the iterator-tracker and info reference must not be passed to bytecodes that handle normal Tcl values. NOTE that this
			 * instruction jumps to the foreach_step instruction paired with it; the stack info below is only nominal.
			 * Stack: ... listObjs... => ... listObjs... iterTracker info */
			[Opcode.ForeachStart] = new InstrDesc(opcode: Opcode.ForeachStart, argc: 1, name: "foreach_start", byteCount: 5, stackEffect: +2, operands: new[] { OperandType.Aux4 }),

			/* "Step" or begin next iteration of foreach loop. Assigns to foreach iteration variables. May jump to straight after the foreach_start that pushed the iterTracker and info values. MUST be followed immediately by a foreach_end.
			 * Stack: ... listObjs... iterTracker info => ... listObjs... iterTracker info */
			[Opcode.ForeachStep] = new InstrDesc(opcode: Opcode.ForeachStep, argc: 3, name: "foreach_step", byteCount: 1, stackEffect: 0, operands: Array.Empty<OperandType>()),

			/* Clean up a foreach loop by dropping the info value, the tracker value and the lists that were being iterated over.
			 * Stack: ... listObjs... iterTracker info => ... */
			[Opcode.ForeachEnd] = new InstrDesc(opcode: Opcode.ForeachEnd, argc: 3, name: "foreach_end", byteCount: 1, stackEffect: 0, operands: Array.Empty<OperandType>()),

			/* Appends the value at the top of the stack to the list located on the stack the "other side" of the foreach-related values.
			 * Stack: ... collector listObjs... iterTracker info value => ... collector listObjs... iterTracker info */
			[Opcode.LmapCollect] = new InstrDesc(opcode: Opcode.LmapCollect, argc: 5, name: "lmap_collect", byteCount: 1, stackEffect: -1, operands: Array.Empty<OperandType>()),

			/* [string trim] core: removes the characters (designated by the value at the top of the stack) from both ends of the string and pushes the resulting string.
			 * Stack: ... string charset => ... trimmedString */
			[Opcode.StrTrim] = new InstrDesc(opcode: Opcode.StrTrim, argc: 2, name: "strtrim", byteCount: 1, stackEffect: -1, operands: Array.Empty<OperandType>()),

			/* [string trimleft] core: removes the characters (designated by the value at the top of the stack) from the left of the string and pushes the resulting string.
			 * Stack: ... string charset => ... trimmedString */
			[Opcode.StrTrimLeft] = new InstrDesc(opcode: Opcode.StrTrimLeft, argc: 2, name: "strtrimLeft", byteCount: 1, stackEffect: -1, operands: Array.Empty<OperandType>()),

			/* [string trimright] core: removes the characters (designated by the value at the top of the stack) from the right of the string and pushes the resulting string.
			 * Stack: ... string charset => ... trimmedString */
			[Opcode.StrTrimRight] = new InstrDesc(opcode: Opcode.StrTrimRight, argc: 2, name: "strtrimRight", byteCount: 1, stackEffect: -1, operands: Array.Empty<OperandType>()),

			/* Wrapper round Tcl_ConcatObj(), used for [concat] and [eval]. opnd is number of values to concatenate.
			 * Operation:	push concat(stk1 stk2 ... stktop) */
			[Opcode.ConcatStk] = new InstrDesc(opcode: Opcode.ConcatStk, argc: int.MinValue, name: "concatStk", byteCount: 5, stackEffect: int.MinValue, operands: new[] { OperandType.UInt4 }),

			/* [string toupper] core: converts whole string to upper case using the default (extended "C" locale) rules.
			 * Stack: ... string => ... newString */
			[Opcode.StrUpper] = new InstrDesc(opcode: Opcode.StrUpper, argc: 1, name: "strcaseUpper", byteCount: 1, stackEffect: 0, operands: Array.Empty<OperandType>()),

			/* [string tolower] core: converts whole string to upper case using the default (extended "C" locale) rules.
			 * Stack: ... string => ... newString */
			[Opcode.StrLower] = new InstrDesc(opcode: Opcode.StrLower, argc: 1, name: "strcaseLower", byteCount: 1, stackEffect: 0, operands: Array.Empty<OperandType>()),

			/* [string totitle] core: converts whole string to upper case using the default (extended "C" locale) rules.
			 * Stack: ... string => ... newString */
			[Opcode.StrTitle] = new InstrDesc(opcode: Opcode.StrTitle, argc: 1, name: "strcaseTitle", byteCount: 1, stackEffect: 0, operands: Array.Empty<OperandType>()),

			/* [string replace] core: replaces a non-empty range of one string with the contents of another.
			 * Stack: ... string fromIdx toIdx replacement => ... newString */
			[Opcode.StrReplace] = new InstrDesc(opcode: Opcode.StrReplace, argc: 4, name: "strreplace", byteCount: 1, stackEffect: -3, operands: Array.Empty<OperandType>()),

			/* Reports which command was the origin (via namespace import chain) of the command named on the top of the stack.
			* Stack:  ... cmdName => ... fullOriginalCmdName */
			[Opcode.OriginCommand] = new InstrDesc(opcode: Opcode.OriginCommand, argc: 1, name: "originCmd", byteCount: 1, stackEffect: 0, operands: Array.Empty<OperandType>()),

			/* Call the next item on the TclOO call chain, passing opnd arguments (min 1, max 255, *includes* "next").
			 * The result of the invoked method implementation will be pushed on the stack in place of the arguments (similar to invokeStk).
			 ** Stack:  ... "next" arg2 arg3 -- argN => ... result */
			[Opcode.TclooNext] = new InstrDesc(opcode: Opcode.TclooNext, argc: int.MinValue, name: "tclooNext", byteCount: 2, stackEffect: int.MinValue, operands: new[] { OperandType.UInt1 }),

			/* Call the following item on the TclOO call chain defined by class className, passing opnd arguments (min 2, max 255, *includes* "nextto" and the class name).
			 * The result of the invoked method implementation will be pushed on the stack in place of the arguments (similar to invokeStk).
			 * Stack:  ... "nextto" className arg3 arg4 -- argN => ... result */
			[Opcode.TclooNextClass] = new InstrDesc(opcode: Opcode.TclooNextClass, argc: int.MinValue, name: "tclooNextClass", byteCount: 2, stackEffect: int.MinValue, operands: new[] { OperandType.UInt1 }),

			/* Makes the current coroutine yield the value at the top of the stack, invoking the given command/args with resolution in the given namespace
			 * (all packed into a list), and places the list of values that are the response back on top of the stack when it resumes.
			 * Stack:  ... [list ns cmd arg1 ... argN] => ... resumeList */
			[Opcode.YieldToInvoke] = new InstrDesc(opcode: Opcode.YieldToInvoke, argc: 1, name: "yieldToInvoke", byteCount: 1, stackEffect: 0, operands: Array.Empty<OperandType>()),

			/* Pushes the numeric type code of the word at the top of the stack.
			 * Stack:  ... value => ... typeCode */
			[Opcode.NumType] = new InstrDesc(opcode: Opcode.NumType, argc: 1, name: "numericType", byteCount: 1, stackEffect: 0, operands: Array.Empty<OperandType>()),

			/* Try converting stktop to boolean if possible. No errors.
			 * Stack:  ... value => ... value isStrictBool */
			[Opcode.TryCvtToBoolean] = new InstrDesc(opcode: Opcode.TryCvtToBoolean, argc: 1, name: "tryCvtToBoolean", byteCount: 1, stackEffect: +1, operands: Array.Empty<OperandType>()),

			/* See if all the characters of the given string are a member of the specified (by opnd) character class. Note that an empty string will satisfy the class check (standard definition of "all").
			 * Stack:  ... stringValue => ... boolean */
			[Opcode.StrClass] = new InstrDesc(opcode: Opcode.StrClass, argc: 1, name: "strclass", byteCount: 2, stackEffect: 0, operands: new[] { OperandType.Scls1 }),

			/* Lappend list to scalar variable at op4 in frame.
			* Stack:  ... list => ... listVarContents */
			[Opcode.LappendList] = new InstrDesc(opcode: Opcode.LappendList, argc: 1, name: "lappendList", byteCount: 5, stackEffect: 0, operands: new[] { OperandType.Lvt4 }),

			/* Lappend list to array element; array at op4.
			* Stack:  ... elem list => ... listVarContents */
			[Opcode.LappendListArray] = new InstrDesc(opcode: Opcode.LappendListArray, argc: 2, name: "lappendListArray", byteCount: 5, stackEffect: -1, operands: new[] { OperandType.Lvt4 }),

			/* Lappend list to array element.
			* Stack:  ... arrayName elem list => ... listVarContents */
			[Opcode.LappendListArrayStk] = new InstrDesc(opcode: Opcode.LappendListArrayStk, argc: 3, name: "lappendListArrayStk", byteCount: 1, stackEffect: -2, operands: Array.Empty<OperandType>()),

			/* Lappend list to general variable.
			* Stack:  ... varName list => ... listVarContents */
			[Opcode.LappendListStk] = new InstrDesc(opcode: Opcode.LappendListStk, argc: 2, name: "lappendListStk", byteCount: 1, stackEffect: -1, operands: Array.Empty<OperandType>()),

			/* Read clock out to the stack. Operand is which clock to read
			* 0=clicks, 1=microseconds, 2=milliseconds, 3=seconds.
			* Stack: ... => ... time */
			[Opcode.ClockRead] = new InstrDesc(opcode: Opcode.ClockRead, argc: 0, name: "clockRead", byteCount: 2, stackEffect: +1, operands: new[] { OperandType.UInt1 }),

		};
	}
}
