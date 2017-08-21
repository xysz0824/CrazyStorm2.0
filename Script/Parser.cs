/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2017 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrazyStorm.Expression
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
        IDictionary<string, Precedence> operators;

        public Parser(Lexer lexer)
        {
            this.lexer = lexer;
            operators = new Dictionary<string, Precedence>();
            operators["&"] = new Precedence(1, true);
            operators["|"] = new Precedence(1, true);

            operators[">"] = new Precedence(2, true);
            operators["<"] = new Precedence(2, true);
            operators["="] = new Precedence(2, true);

            operators["+"] = new Precedence(3, true);
            operators["-"] = new Precedence(3, true);
            
            operators["*"] = new Precedence(4, true);
            operators["/"] = new Precedence(4, true);
            operators["%"] = new Precedence(4, true);
        }

        bool IsBool(IdentifierToken token)
        {
            return (string)token.GetValue() == "True" ||
                (string)token.GetValue() == "False";
        }

        bool IsIdentifierToken(string name)
        {
            Token token = lexer.Peek(0);
            return (token is IdentifierToken && name == (string)token.GetValue());
        }

        Token IdentifierToken(string name)
        {
            Token token = lexer.Read();
            if (!(token is IdentifierToken && name == (string)token.GetValue()))
                throw new ExpressionException("SyntaxError");

            return token;
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

        public SyntaxTree Vector()
        {
            SyntaxTree expression = Expression();
            List<SyntaxTree> coordinateList = new List<SyntaxTree>();
            coordinateList.Add(expression);
            int dimension = 1;
            while (IsIdentifierToken(","))
            {
                dimension++;
                IdentifierToken(",");
                coordinateList.Add(Expression());
            }
            if (dimension == 2)
                return new Vector2(coordinateList[0], coordinateList[1]);
            else if (dimension == 3)
                return new RGB(coordinateList[0], coordinateList[1], coordinateList[2]);
            else
                throw new ExpressionException("SyntaxError");
        }

        public SyntaxTree Factor()
        {
            if (IsIdentifierToken("-"))
                return new NegativeExpression(IdentifierToken("-"), Primary());

            return Primary();
        }

        public SyntaxTree Primary()
        {
            if (IsIdentifierToken("("))
            {
                IdentifierToken("(");
                SyntaxTree expression = Expression();
                IdentifierToken(")");
                return expression;
            }
            else if (IsIdentifierToken("["))
            {
                IdentifierToken("[");
                SyntaxTree vector = Vector();
                IdentifierToken("]");
                return vector;
            }
            else
            {
                Token token = lexer.Read();
                if (token is NumberToken)
                    return new Number(token);
                else if (token is IdentifierToken && !(token as IdentifierToken).IsOperator)
                {
                    if (IsIdentifierToken("("))
                        return new Call(token, Call());
                    else if (IsBool(token as IdentifierToken))
                        return new Bool(token);
                    else
                        return new Name(token);
                }
                else
                    throw new ExpressionException("SyntaxError");
            }
        }

        public SyntaxTree Call()
        {
            IdentifierToken("(");
            SyntaxTree arguments = null;
            if (!IsIdentifierToken(")"))
                arguments = Arguments();
            else
                arguments = new Arguments(new List<SyntaxTree>());

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
