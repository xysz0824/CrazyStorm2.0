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
    public class Arguments : SyntaxTree
    {
        public Arguments(List<SyntaxTree> arguments)
            : base()
        {
            foreach (var argument in arguments)
                AddChild(argument);
        }

        public int Count { get { return GetChildren().Count; } }

        public IList<SyntaxTree> GetArguments() { return GetChildren(); }

        public override object Eval(Environment e)
        {
            var list = GetArguments();
            var resultList = new List<object>();
            foreach (var item in list)
                resultList.Add(item.Eval(e));

            return resultList;
        }

        public override void Compile(List<byte> codeStream)
        {
            foreach (var item in GetArguments())
                item.Compile(codeStream);

            byte[] code = VM.CreateInstruction(VMCode.ARGUMENTS, Count);
            codeStream.AddRange(code);
        }
    }
}
