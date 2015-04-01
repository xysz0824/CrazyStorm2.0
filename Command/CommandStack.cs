/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2015 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrazyStorm
{
    class CommandStack
    {
        static readonly Stack<Command> doCommands = new Stack<Command>();
        static readonly Stack<Command> undoCommands = new Stack<Command>();
        public static void DoPush(Command command)
        {
            doCommands.Push(command);
        }
        public static Command DoPop()
        {
            return doCommands.Pop();
        }
        public static void UndoPush(Command command)
        {
            undoCommands.Push(command);
        }
        public static Command UndoPop()
        {
            return undoCommands.Pop();
        }
        public static void Clear()
        {
            doCommands.Clear();
            undoCommands.Clear();
        }
    }
}
