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
    class Access : SyntaxTree
    {
        public Access(Token name, SyntaxTree obj)
        {
            Token = name;
            AddChild(obj);
        }

        public SyntaxTree GetObject() { return GetChildren()[0]; }

        public override object Test(Environment e)
        {
            var name = (string)Token.GetValue();
            var var = e.GetLocal(name);
            if (var == null)
                throw new ScriptException("Undefination error.");

            var s = e.GetStructs(var.GetType().ToString());
            if (s == null)
                throw new ScriptException("Undefination error.");

            var subEnvironment = new Environment();
            foreach (var item in s.GetFields())
                subEnvironment.PutLocal(item.Key, item.Value);

            return GetObject().Test(subEnvironment);
        }
    }
}
