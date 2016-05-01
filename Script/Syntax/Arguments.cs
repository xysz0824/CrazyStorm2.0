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
    class Arguments : SyntaxTree
    {
        public Arguments(List<SyntaxTree> arguments)
            : base()
        {
            foreach (var argument in arguments)
                AddChild(argument);
        }

        public int Count { get { return GetChildren().Count; } }

        public IList<SyntaxTree> GetArguments() { return GetChildren(); }

        public override object Test(Environment e)
        {
            var list = GetArguments();
            var resultList = new List<object>();
            foreach (var item in list)
                resultList.Add(item.Test(e));

            return resultList;
        }
    }
}
