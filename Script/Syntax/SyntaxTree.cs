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

        public abstract object Eval(Environment e);
    }
}
