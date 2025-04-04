﻿using System.Collections.Generic;
using System.Linq;
using MoonSharp.Interpreter.Debugging;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Execution.VM;
using MoonSharp.Interpreter.Tree.Expressions;

namespace MoonSharp.Interpreter.Tree.Statements
{
    internal class ForEachLoopStatement : Statement
    {
        private RuntimeScopeBlock m_StackFrame;
        private SymbolRef[] m_Names;
        private IVariable[] m_NameExps;
        private Expression m_RValues;
        private Statement m_Block;
        private SourceRef m_RefFor, m_RefEnd;

        public ForEachLoopStatement(ScriptLoadingContext lcontext, Token firstNameToken, Token forToken)
            : base(lcontext)
        {
            //	for namelist in explist do block end | 		

            var names = new List<string>();
            names.Add(firstNameToken.Text);

            while (lcontext.Lexer.Current.Type == TokenType.Comma)
            {
                lcontext.Lexer.Next();
                var name = CheckTokenType(lcontext, TokenType.Name);
                names.Add(name.Text);
            }

            CheckTokenType(lcontext, TokenType.In);

            m_RValues = new ExprListExpression(Expression.ExprList(lcontext), lcontext);

            lcontext.Scope.PushBlock();

            m_Names = names
                .Select(n => lcontext.Scope.TryDefineLocal(n))
                .ToArray();

            m_NameExps = m_Names
                .Select(s => new SymbolRefExpression(lcontext, s))
                .Cast<IVariable>()
                .ToArray();

            m_RefFor = forToken.GetSourceRef(CheckTokenType(lcontext, TokenType.Do));

            m_Block = new CompositeStatement(lcontext);

            m_RefEnd = CheckTokenType(lcontext, TokenType.End).GetSourceRef();

            m_StackFrame = lcontext.Scope.PopBlock();

            lcontext.Source.Refs.Add(m_RefFor);
            lcontext.Source.Refs.Add(m_RefEnd);
        }


        public override void Compile(ByteCode bc)
        {
            //for var_1, ···, var_n in explist do block end

            bc.PushSourceRef(m_RefFor);

            var L = new Loop()
            {
                Scope = m_StackFrame
            };
            bc.LoopTracker.Loops.Push(L);

            // get iterator tuple
            m_RValues.Compile(bc);

            // prepares iterator tuple - stack : iterator-tuple
            bc.Emit_IterPrep();

            // loop start - stack : iterator-tuple
            var start = bc.GetJumpPointForNextInstruction();
            bc.Emit_Enter(m_StackFrame);

            // expand the tuple - stack : iterator-tuple, f, var, s
            bc.Emit_ExpTuple(0);

            // calls f(s, var) - stack : iterator-tuple, iteration result
            bc.Emit_Call(2, "for..in");

            // perform assignment of iteration result- stack : iterator-tuple, iteration result
            for (var i = 0; i < m_NameExps.Length; i++)
                m_NameExps[i].CompileAssignment(bc, 0, i);

            // pops  - stack : iterator-tuple
            bc.Emit_Pop();

            // repushes the main iterator var - stack : iterator-tuple, main-iterator-var
            bc.Emit_Load(m_Names[0]);

            // updates the iterator tuple - stack : iterator-tuple, main-iterator-var
            bc.Emit_IterUpd();

            // checks head, jumps if nil - stack : iterator-tuple, main-iterator-var
            var endjump = bc.Emit_Jump(OpCode.JNil, -1);

            // executes the stuff - stack : iterator-tuple
            m_Block.Compile(bc);

            bc.PopSourceRef();
            bc.PushSourceRef(m_RefEnd);

            // loop back again - stack : iterator-tuple
            bc.Emit_Leave(m_StackFrame);
            bc.Emit_Jump(OpCode.Jump, start);

            bc.LoopTracker.Loops.Pop();

            var exitpointLoopExit = bc.GetJumpPointForNextInstruction();
            bc.Emit_Leave(m_StackFrame);

            var exitpointBreaks = bc.GetJumpPointForNextInstruction();

            bc.Emit_Pop();

            foreach (var i in L.BreakJumps)
                i.NumVal = exitpointBreaks;

            endjump.NumVal = exitpointLoopExit;

            bc.PopSourceRef();
        }
    }
}