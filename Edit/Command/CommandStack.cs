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
        public event Action StackChanged;

        #region Private Members
        const int MaxSize = 32;
        List<Command> redoCommands;
        List<Command> undoCommands;
        #endregion

        #region Constructor
        public CommandStack()
        {
            redoCommands = new List<Command>(MaxSize);
            undoCommands = new List<Command>(MaxSize);
        }
        #endregion

        #region Private Methods
        static void CheckLimit(List<Command> list)
        {
            if (list.Count >= MaxSize)
                list.RemoveAt(0);
        }
        static void DoCompact(List<Command> list, Command command)
        {
            //As for move command, it's better to merge them to reduce occupation of command stack.
            if (list.Count > 0
                && list[list.Count - 1]  is MoveComponentCommand 
                && command             is MoveComponentCommand)
            {
                var last   = list[list.Count - 1]   as MoveComponentCommand;
                var now = command              as MoveComponentCommand;
                if (last.IsSameTarget(now))
                {
                    last.Move += now.Move;
                    return;
                }
            }
            list.Add(command);
        }
        #endregion

        #region Public Methods
        public Command RedoPeek()
        {
            if (redoCommands.Count == 0)
                return null;
            return redoCommands[redoCommands.Count - 1];
        }
        public void RedoPush(Command command)
        {
            CheckLimit(redoCommands);
            DoCompact(redoCommands, command);
            if (StackChanged != null)
                StackChanged();
        }
        public Command RedoPop()
        {
            var pop = redoCommands[redoCommands.Count - 1];
            redoCommands.RemoveAt(redoCommands.Count - 1);
            if (StackChanged != null)
                StackChanged();
            return pop;
        }
        public Command UndoPeek()
        {
            if (undoCommands.Count == 0)
                return null;
            return undoCommands[undoCommands.Count - 1];
        }
        public void UndoPush(Command command)
        {
            CheckLimit(undoCommands);
            undoCommands.Add(command);
            if (StackChanged != null)
                StackChanged();
        }
        public Command UndoPop()
        {
            var pop = undoCommands[undoCommands.Count - 1];
            undoCommands.RemoveAt(undoCommands.Count - 1);
            if (StackChanged != null)
                StackChanged();
            return pop;
        }
        public void UndoClear()
        {
            undoCommands.Clear();
            if (StackChanged != null)
                StackChanged();
        }
        #endregion
    }
}
