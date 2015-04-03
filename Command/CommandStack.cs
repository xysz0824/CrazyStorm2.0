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
    public class CommandStack
    {
        Stack<Command> doCommands;
        Stack<Command> undoCommands;
        public CommandStack()
        {
            doCommands = new Stack<Command>();
            undoCommands = new Stack<Command>();
        }
        public Command DoPeek()
        {
            return doCommands.Peek();
        }
        public void DoPush(Command command)
        {
            doCommands.Push(command);
        }
        public Command DoPop()
        {
            return doCommands.Pop();
        }
        public Command UndoPeek()
        {
            return undoCommands.Peek();
        }
        public void UndoPush(Command command)
        {
            undoCommands.Push(command);
        }
        public Command UndoPop()
        {
            return undoCommands.Pop();
        }
        public void Clear()
        {
            doCommands.Clear();
            undoCommands.Clear();
        }
    }
}
