﻿using MoonSharp.Interpreter.Execution;

namespace MoonSharp.Interpreter.Tree.Statements
{
    internal class EmptyStatement : Statement
    {
        public EmptyStatement(ScriptLoadingContext lcontext)
            : base(lcontext)
        {
        }


        public override void Compile(Execution.VM.ByteCode bc)
        {
        }
    }
}