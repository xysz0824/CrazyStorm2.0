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
    public class Environment
    {
        #region Private Members
        IDictionary<string, float> globals;
        IDictionary<string, float> locals;
        IDictionary<string, Function> functions;
        #endregion

        #region Public Members
        public IDictionary<string, float> Globals { get { return globals; } }
        public IDictionary<string, float> Locals { get { return locals; } }
        public IDictionary<string, Function> Functions { get { return functions; } }
        #endregion

        #region Constructor
        public Environment()
        {
            globals = new Dictionary<string, float>();
            locals = new Dictionary<string, float>();
            functions = new Dictionary<string, Function>();
        }

        public Environment(Environment environment)
        {
            globals = new Dictionary<string, float>();
            locals = new Dictionary<string, float>();
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
                foreach (var item in environment.functions)
                {
                    functions.Add(item.Key, item.Value);
                }
            }
        }
        #endregion

        #region Private Methods
        bool TryPutVariable(IDictionary<string, float> map, string name, object value)
        {
            if (value is int || value is float || value is bool || value is Enum)
            {
                map[name] = Convert.ToSingle(value);
                return true;
            }
            else if (value is Core.Vector2)
            {
                var vector2 = (Core.Vector2)value;
                map[name + ".x"] = vector2.x;
                map[name + ".y"] = vector2.y;
                return true;
            }
            else if (value is Core.RGB)
            {
                var rgb = (Core.RGB)value;
                map[name + ".r"] = rgb.r;
                map[name + ".g"] = rgb.g;
                map[name + ".b"] = rgb.b;
                return true;
            }
            else
                return false;
        }
        #endregion

        #region Public Methods
        public void PutGlobal(string name, float value) { globals[name] = value; }

        public float? GetGlobal(string name) 
        {
            if (globals.ContainsKey(name))
                return globals[name];
            else
                return null;
        }
        public void RemoveGlobal(string name) { globals.Remove(name); }

        public bool TryPutLocal(string name, object value) { return TryPutVariable(locals, name, value); }

        public float? GetLocal(string name)
        {
            if (locals.ContainsKey(name))
                return locals[name];
            else
                return null;
        }

        public void RemoveLocal(string name) 
        {
            if (!locals.Remove(name))
            {
                List<string> removeKeys = new List<string>();
                foreach (var pair in locals)
                {
                    if (pair.Key.StartsWith(name + "."))
                        removeKeys.Add(pair.Key);
                }
                for (int i = 0; i < removeKeys.Count; ++i)
                    locals.Remove(removeKeys[i]);
            }
        }

        public void PutFunction(string name, Function function) { functions[name] = function; }

        public Function GetFunction(string name) { return functions.ContainsKey(name) ? functions[name] : null; }
        #endregion
    }
}
