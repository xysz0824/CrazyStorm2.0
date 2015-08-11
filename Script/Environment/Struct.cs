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
    public class Struct
    {
        Dictionary<string, object> fields;
        public Struct()
        {
            fields = new Dictionary<string, object>();
        }

        public void PutField(string name, object value)
        {
            fields.Add(name, value);
        }

        public object GetField(string name) { return fields.ContainsKey(name) ? fields[name] : null; }

        public Dictionary<string, object> GetFields() { return fields; }
    }
}
