/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2015 
 */
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CrazyStorm.Script
{
    public class Lexer
    {
        static Regex NumberTokenRegex = new Regex(@"[-]?([0-9]+\.[0-9]+|[0-9]+)");
        static Regex IdentifierTokenRegex = new Regex(@"[A-Z_a-z][A-Z_a-z0-9]*|[+\-*/%()]");
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
                    var numberTokens = NumberTokenRegex.Matches(lineString);
                    foreach (Capture token in numberTokens)
                        lineTokens.Add(new NumberToken(lineNumber, token.Index, double.Parse(token.Value)));

                    var identifierTokens = IdentifierTokenRegex.Matches(lineString);
                    foreach (Capture token in identifierTokens)
                        lineTokens.Add(new IdentifierToken(lineNumber, token.Index, token.Value));

                    lineTokens.Sort();
                    tokens.AddRange(lineTokens);
                }
            }
        }

        public Token Read()
        {
            if (tokens.Count == 0)
                return null;

            Token readToken = tokens[0];
            tokens.RemoveAt(0);
            return readToken;
        }

        public Token Peek(int i )
        {
            if (i >= tokens.Count)
                return null;

            return tokens[i];
        }
    }
}
