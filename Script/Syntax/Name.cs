using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrazyStorm.Script
{
    class Name : SyntaxTree
    {
        public Name(Token token)
            : base()
        {
            Token = token;
        }

        public override object Test(Environment e)
        {
            var name = (string)Token.GetValue();
            //Find this in local variable.
            var result = e.GetLocal(name);
            if (result == null)
            {
                //Find this in global variable.
                result = e.GetGlobal(name);
                if (result == null)
                    throw new ScriptException("Undefination error.");
            }
            return result;
        }
    }
}
