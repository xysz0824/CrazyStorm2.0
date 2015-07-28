using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrazyStorm.Script
{
    public class Environment
    {
        Dictionary<string, object> globals;
        Dictionary<string, object> locals;
        Dictionary<string, Struct> structs;
        Dictionary<string, Function> functions;

        public Environment()
        {
            globals = new Dictionary<string, object>();
            locals = new Dictionary<string, object>();
            structs = new Dictionary<string, Struct>();
            functions = new Dictionary<string, Function>();
        }

        public void PutGlobal(string name, object value) { globals[name] = value; }

        public object GetGlobal(string name) { return globals.ContainsKey(name) ? globals[name] : null; }

        public void PutLocal(string name, object value) { locals[name] = value; }

        public object GetLocal(string name) { return locals.ContainsKey(name) ? locals[name] : null; }

        public void PutStruct(string name, Struct s) { structs[name] = s; }

        public Struct GetStructs(string name) { return structs.ContainsKey(name) ? structs[name] : null; }

        public void PutFunction(string name, Function function) { functions[name] = function; }

        public Function GetFunction(string name) { return functions.ContainsKey(name) ? functions[name] : null; }
    }
}
