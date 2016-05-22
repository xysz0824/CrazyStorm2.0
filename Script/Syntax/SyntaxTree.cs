/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2015 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrazyStorm.Expression
{
    public abstract class SyntaxTree
    {
        List<SyntaxTree> children;
        Token token;

        public Token Token 
        { 
            get { return token; }
            set { token = value; }
        }

        public SyntaxTree()
        {
            children = new List<SyntaxTree>();
            token = null;
        }

        public bool HasChildren() { return children.Count > 0; }

        public void AddChild(SyntaxTree tree) { children.Add(tree); }

        public IList<SyntaxTree> GetChildren() { return children; }

        public bool IsLeaf() { return token != null; }

        public bool ContainType<T>() { return SyntaxTree.ContainType<T>(this); }

        public static bool ContainType<T>(SyntaxTree syntaxTree)
        {
            if (syntaxTree is T)
                return true;
            else
            {
                for (int i = 0; i < syntaxTree.children.Count; ++i)
                {
                    if (ContainType<T>(syntaxTree.children[i]))
                        return true;
                }
            }
            return false;
        }

        public abstract object Eval(Environment e);

        public abstract void Compile(List<byte> codeStream);
    }
}
