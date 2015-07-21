/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2015 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrazyStorm.Script
{
    class Precedence
    {
        int level;
        bool leftAssociative;

        public int Level { get { return level; } }
        public bool LeftAssociative { get { return leftAssociative; } }

        public Precedence(int level, bool leftAssociative)
        {
            this.level = level;
            this.leftAssociative = leftAssociative;
        }
    }

    public class Parser
    {
        Lexer lexer;
        Dictionary<string, Precedence> operators;

        public Parser(Lexer lexer)
        {
            this.lexer = lexer;
            operators = new Dictionary<string, Precedence>();
            operators["+"] = new Precedence(1, true);
            operators["-"] = new Precedence(1, true);
            operators["*"] = new Precedence(2, true);
            operators["/"] = new Precedence(2, true);
            operators["%"] = new Precedence(2, true);
        }

        bool IsIdentifierToken(string name)
        {
            Token token = lexer.Peek(0);
            return (token is IdentifierToken && name == (string)token.GetValue());
        }

        void IdentifierToken(string name)
        {
            Token token = lexer.Read();
            if (!(token is IdentifierToken && name == (string)token.GetValue()))
                throw new CompileException("Syntax error.");
        }

        Precedence GetNextOperator()
        {
            Token token = lexer.Peek(0);
            if (token is IdentifierToken && operators.ContainsKey((string)token.GetValue()))
                return operators[(string)token.GetValue()];

            return null;
        }

        SyntaxTree PrecedenceShift(SyntaxTree left, int level)
        {
            Token operatorToken = lexer.Read();
            SyntaxTree right = Factor();
            Precedence nextOperator;
            while ((nextOperator = GetNextOperator()) != null && RightIsExpression(level, nextOperator))
                right = PrecedenceShift(right, nextOperator.Level);

            return new BinaryExpression(operatorToken, left, right);
        }

        static bool RightIsExpression(int level, Precedence next)
        {
            if (next.LeftAssociative)
                return level < next.Level;
            else
                return level <= next.Level;
        }

        public SyntaxTree Expression()
        {
            SyntaxTree left = Factor();
            Precedence nextOperator;
            while ((nextOperator = GetNextOperator()) != null)
                left = PrecedenceShift(left, nextOperator.Level);

            return left;
        }

        public SyntaxTree Factor()
        {
            if (IsIdentifierToken("("))
            {
                IdentifierToken("(");
                SyntaxTree expression = Expression();
                IdentifierToken(")");
                return expression;
            }
            else
            {
                Token token = lexer.Read();
                if (token is NumberToken)
                    return new Number(token);
                else if (token is IdentifierToken)
                {
                    if (IsIdentifierToken("("))
                        return new Call(token, Call());
                    else
                        return new Name(token);
                }
                else
                    throw new CompileException("Syntax error.");
            }
        }

        public SyntaxTree Call()
        {
            IdentifierToken("(");
            SyntaxTree arguments = null;
            if (!IsIdentifierToken(")"))
                arguments = Arguments();

            IdentifierToken(")");
            return arguments;
        }

        public SyntaxTree Arguments()
        {
            SyntaxTree expression = Expression();
            List<SyntaxTree> argumentList = new List<SyntaxTree>();
            argumentList.Add(expression);
            while (IsIdentifierToken(","))
            {
                IdentifierToken(",");
                argumentList.Add(Expression());
            }

            SyntaxTree arguments = new Arguments(argumentList);
            return arguments;
        }
    }
}
