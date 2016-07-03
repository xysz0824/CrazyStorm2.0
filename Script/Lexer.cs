/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2016 
 */
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CrazyStorm.Expression
{
    public class Lexer
    {
        static Regex NumberTokenRegex = new Regex(@"[-]?([0-9]+\.[0-9]+|[0-9]+)");
        static Regex IdentifierTokenRegex = new Regex(@"[A-Z_a-z][A-Z_a-z0-9\.]*");
        static Regex OperatorTokenRegex = new Regex(@"[+\-*/%>=<&|(,)\[\]]");
        List<Token> tokens;

        public void Load(string content)
        {
            tokens = new List<Token>();
            byte[] stringBytes = Encoding.Default.GetBytes(content);
            using (MemoryStream stream = new MemoryStream(stringBytes))
            {
                StreamReader reader = new StreamReader(stream);
                int lineNumber = 0;
                while (!reader.EndOfStream)
                {
                    string lineString = reader.ReadLine().Trim();
                    lineNumber++;

                    List<Token> lineTokens = new List<Token>();
                    var identifierTokens = IdentifierTokenRegex.Matches(lineString);
                    foreach (Capture token in identifierTokens)
                    {
                        lineTokens.Add(new IdentifierToken(lineNumber, token.Index, token.Value, false));
                        lineString = ReplaceBySpace(lineString, token.Index, token.Length);
                    }
                    var numberTokens = NumberTokenRegex.Matches(lineString);
                    foreach (Capture token in numberTokens)
                    {
                        lineTokens.Add(new NumberToken(lineNumber, token.Index, float.Parse(token.Value)));
                        lineString = ReplaceBySpace(lineString, token.Index, token.Length);
                    }
                    var operatorTokens = OperatorTokenRegex.Matches(lineString);
                    foreach (Capture token in operatorTokens)
                    {
                        lineTokens.Add(new IdentifierToken(lineNumber, token.Index, token.Value, true));
                        lineString = ReplaceBySpace(lineString, token.Index, token.Length);
                    }
                    lineTokens.Sort();
                    tokens.AddRange(lineTokens);
                }
            }
        }

        string ReplaceBySpace(string origin, int startIndex, int length)
        {
            //To avoid taking influence to next match, 
            //use write space for replacement.
            char[] charArray = origin.ToCharArray();
            for (int i = 0; i < length; ++i)
                charArray[i + startIndex] = ' ';

            return new string(charArray);
        }

        public Token Read()
        {
            if (tokens.Count == 0)
                return null;

            Token readToken = tokens[0];
            tokens.RemoveAt(0);
            return readToken;
        }

        public Token Peek(int index)
        {
            if (index < 0 || index >= tokens.Count)
                return null;

            return tokens[index];
        }
    }
}
