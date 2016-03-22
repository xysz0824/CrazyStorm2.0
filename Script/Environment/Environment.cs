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
    public class Environment
    {
        #region Private Members
        Dictionary<string, object> globals;
        Dictionary<string, object> locals;
        Dictionary<string, Struct> structs;
        Dictionary<string, Function> functions;
        #endregion

        #region Public Members
        public IDictionary<string, object> Globals { get { return globals; } }
        public IDictionary<string, object> Locals { get { return locals; } }
        public IDictionary<string, Struct> Structs { get { return structs; } }
        public IDictionary<string, Function> Functions { get { return functions; } }
        #endregion

        #region Constructor
        public Environment()
        {
            globals = new Dictionary<string, object>();
            locals = new Dictionary<string, object>();
            structs = new Dictionary<string, Struct>();
            functions = new Dictionary<string, Function>();
        }

        public Environment(Environment environment)
        {
            globals = new Dictionary<string, object>();
            locals = new Dictionary<string, object>();
            structs = new Dictionary<string, Struct>();
            functions = new Dictionary<string, Function>();
            if (environment != null)
            {
                foreach (var item in environment.globals)
                {
                    globals.Add(item.Key, item.Value);
                }
                foreach (var item in environment.locals)
                {
                    locals.Add(item.Key, item.Value);
                }
                foreach (var item in environment.structs)
                {
                    structs.Add(item.Key, item.Value);
                }
                foreach (var item in environment.functions)
                {
                    functions.Add(item.Key, item.Value);
                }
            }
        }
        #endregion

        #region Public Members
        public void PutGlobal(string name, object value) { globals[name] = value; }

        public object GetGlobal(string name) { return globals.ContainsKey(name) ? globals[name] : null; }

        public void RemoveGlobal(string name) { globals.Remove(name); }

        public void PutLocal(string name, object value) { locals[name] = value; }

        public object GetLocal(string name) { return locals.ContainsKey(name) ? locals[name] : null; }

        public void RemoveLocal(string name) { locals.Remove(name); }

        public void PutStruct(string name, Struct s) { structs[name] = s; }

        public Struct GetStructs(string name) { return structs.ContainsKey(name) ? structs[name] : null; }

        public void PutFunction(string name, Function function) { functions[name] = function; }

        public Function GetFunction(string name) { return functions.ContainsKey(name) ? functions[name] : null; }
        #endregion
    }
}
