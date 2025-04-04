﻿using System;
using System.Collections.Generic;
using MoonSharp.Interpreter.Debugging;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Tree.Expressions;

namespace MoonSharp.Interpreter.Tree.Statements
{
    internal class AssignmentStatement : Statement
    {
        private List<IVariable> m_LValues = new();
        private List<Expression> m_RValues;
        private SourceRef m_Ref;


        public AssignmentStatement(ScriptLoadingContext lcontext, Token startToken)
            : base(lcontext)
        {
            var names = new List<string>();

            var first = startToken;

            while (true)
            {
                var name = CheckTokenType(lcontext, TokenType.Name);
                names.Add(name.Text);

                if (lcontext.Lexer.Current.Type != TokenType.Comma)
                    break;

                lcontext.Lexer.Next();
            }

            if (lcontext.Lexer.Current.Type == TokenType.Op_Assignment)
            {
                CheckTokenType(lcontext, TokenType.Op_Assignment);
                m_RValues = Expression.ExprList(lcontext);
            }
            else
            {
                m_RValues = new List<Expression>();
            }

            foreach (var name in names)
            {
                var localVar = lcontext.Scope.TryDefineLocal(name);
                var symbol = new SymbolRefExpression(lcontext, localVar);
                m_LValues.Add(symbol);
            }

            var last = lcontext.Lexer.Current;
            m_Ref = first.GetSourceRefUpTo(last);
            lcontext.Source.Refs.Add(m_Ref);
        }


        public AssignmentStatement(ScriptLoadingContext lcontext, Expression firstExpression, Token first)
            : base(lcontext)
        {
            m_LValues.Add(CheckVar(lcontext, firstExpression));

            while (lcontext.Lexer.Current.Type == TokenType.Comma)
            {
                lcontext.Lexer.Next();
                var e = Expression.PrimaryExp(lcontext);
                m_LValues.Add(CheckVar(lcontext, e));
            }

            CheckTokenType(lcontext, TokenType.Op_Assignment);

            m_RValues = Expression.ExprList(lcontext);

            var last = lcontext.Lexer.Current;
            m_Ref = first.GetSourceRefUpTo(last);
            lcontext.Source.Refs.Add(m_Ref);
        }

        private IVariable CheckVar(ScriptLoadingContext lcontext, Expression firstExpression)
        {
            var v = firstExpression as IVariable;

            if (v == null)
                throw new SyntaxErrorException(lcontext.Lexer.Current, "unexpected symbol near '{0}' - not a l-value",
                    lcontext.Lexer.Current);

            return v;
        }


        public override void Compile(Execution.VM.ByteCode bc)
        {
            using (bc.EnterSource(m_Ref))
            {
                foreach (var exp in m_RValues) exp.Compile(bc);

                for (var i = 0; i < m_LValues.Count; i++)
                    m_LValues[i].CompileAssignment(bc,
                        Math.Max(m_RValues.Count - 1 - i, 0), // index of r-value
                        i - Math.Min(i, m_RValues.Count - 1)); // index in last tuple

                bc.Emit_Pop(m_RValues.Count);
            }
        }
    }
}