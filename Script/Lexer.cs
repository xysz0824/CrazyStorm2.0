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
        static Regex IdentifierTokenRegex = new Regex(@"[A-Z_a-z][A-Z_a-z0-9]*|[+\-*/%(,)\[\.\]]");
        static Regex OperatorTokenRegex = new Regex(@"[+\-*/%(,)\[\.\]]");
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
                        if (OperatorTokenRegex.IsMatch(token.Value))
                            lineTokens.Add(new IdentifierToken(lineNumber, token.Index, token.Value, true));
                        else
                            lineTokens.Add(new IdentifierToken(lineNumber, token.Index, token.Value, false));

                        //To ensure that the index of each token is unchanged,
                        //keep the same length as the original.
                        string space = "";
                        for (int i = 0; i < token.Length; ++i)
                            space += " ";
                        //To avoid taking influence to next match, 
                        //use write space for replacement.
                        lineString = lineString.Replace(token.Value, space);
                    }
                    var numberTokens = NumberTokenRegex.Matches(lineString);
                    foreach (Capture token in numberTokens)
                        lineTokens.Add(new NumberToken(lineNumber, token.Index, float.Parse(token.Value)));

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
            if (i < 0 || i >= tokens.Count)
                return null;

            return tokens[i];
        }
    }
}
