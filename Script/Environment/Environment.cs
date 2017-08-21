/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2017 
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
        IDictionary<string, object> properties;
        IDictionary<string, Function> functions;
        #endregion

        #region Public Members
        public IDictionary<string, float> Globals { get { return globals; } }
        public IDictionary<string, float> Locals { get { return locals; } }
        public IDictionary<string, object> Properties { get { return properties; } }
        #endregion

        #region Constructor
        public Environment()
        {
            globals = new Dictionary<string, float>();
            locals = new Dictionary<string, float>();
            properties = new Dictionary<string, object>();
            functions = new Dictionary<string, Function>();
            InitializeSystemFunctions();
        }

        public Environment(Environment environment)
        {
            globals = new Dictionary<string, float>();
            locals = new Dictionary<string, float>();
            properties = new Dictionary<string, object>();
            functions = new Dictionary<string, Function>();
            if (environment != null)
            {
                foreach (var item in environment.globals)
                    globals.Add(item.Key, item.Value);
                
                foreach (var item in environment.locals)
                    locals.Add(item.Key, item.Value);
                
                foreach (var item in environment.properties)
                    properties.Add(item.Key, item.Value);
                
                foreach (var item in environment.functions)
                    functions.Add(item.Key, item.Value);
            }
        }
        #endregion

        #region Private Methods
        void InitializeSystemFunctions()
        {
            PutFunction("dist", new Expression.Function(typeof(Core.Vector2), typeof(Core.Vector2)));
            PutFunction("angle", new Expression.Function(typeof(Core.Vector2), typeof(Core.Vector2)));
            PutFunction("rand", new Expression.Function(typeof(float), typeof(float)));
            PutFunction("randi", new Expression.Function(typeof(float), typeof(float)));
            PutFunction("sin", new Expression.Function(typeof(float)));
            PutFunction("cos", new Expression.Function(typeof(float)));
            PutFunction("tan", new Expression.Function(typeof(float)));
            PutFunction("pi", new Expression.Function());
            PutFunction("e", new Expression.Function());
            PutFunction("asin", new Expression.Function(typeof(float)));
            PutFunction("acos", new Expression.Function(typeof(float)));
            PutFunction("atan", new Expression.Function(typeof(float)));
            PutFunction("exp", new Expression.Function(typeof(float)));
            PutFunction("log", new Expression.Function(typeof(float), typeof(float)));
            PutFunction("pow", new Expression.Function(typeof(float), typeof(float)));
            PutFunction("sqrt", new Expression.Function(typeof(float)));
        }
        void PutFunction(string name, Function function)
        {
            functions[name] = function;
        }
        #endregion

        #region Public Methods
        public void PutGlobal(string name, float value) 
        {
            globals[name] = value; 
        }
        public float? GetGlobal(string name) 
        {
            if (globals.ContainsKey(name))
                return globals[name];
                
            return null;
        }
        public void RemoveGlobal(string name) 
        {
            globals.Remove(name); 
        }
        public void PutLocal(string name, float value) 
        {
            locals[name] = value;
        }
        public float? GetLocal(string name)
        {
            if (locals.ContainsKey(name))
                return locals[name];
                
            return null;
        }
        public void RemoveLocal(string name) 
        {
            locals.Remove(name);
        }
        public void PutProperty(string name, object value)
        {
            properties[name] = value;
            if (value is Core.Vector2)
            {
                var vector2 = (Core.Vector2)value;
                properties[name + ".x"] = vector2.x;
                properties[name + ".y"] = vector2.y;
            }
            else if (value is Core.RGB)
            {
                var rgb = (Core.RGB)value;
                properties[name + ".r"] = rgb.r;
                properties[name + ".g"] = rgb.g;
                properties[name + ".b"] = rgb.b;
            }
        }
        public object GetProperty(string name)
        {
            return properties.ContainsKey(name) ? properties[name] : null;
        }
        public void RemoveProperty(string name)
        {
            if (!properties.ContainsKey(name))
                return;

            if (properties[name] is Core.Vector2)
            {
                properties.Remove(name + ".x");
                properties.Remove(name + ".y");
            }
            else if (properties[name] is Core.RGB)
            {
                properties.Remove(name + ".r");
                properties.Remove(name + ".g");
                properties.Remove(name + ".b");
            }
            properties.Remove(name);
        }
        public Function GetFunction(string name)
        {
            return functions.ContainsKey(name) ? functions[name] : null;
        }
        #endregion
    }
}
