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
        const int MaxSize = 512;
        List<Command> redoCommands;
        List<Command> undoCommands;
        #endregion

        #region Constructor
        public CommandStack()
        {
            redoCommands = new List<Command>();
            undoCommands = new List<Command>();
        }
        #endregion

        #region Private Methods
        static void CheckLimit(List<Command> list)
        {
            if (list.Count > MaxSize)
                list.RemoveAt(0);
        }
        static void DoCompact(List<Command> list, Command command)
        {
            //As for move command, it's nessesary to merge to reduce occupation of command stack.
            if (list.Count > 0
                && list[list.Count - 1] is MoveComponentCommand 
                && command             is MoveComponentCommand)
            {
                var last   = list[list.Count - 1] as MoveComponentCommand;
                var now = command              as MoveComponentCommand;
                last.Move += now.Move;
            }
            else
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
        public void Clear()
        {
            redoCommands.Clear();
            undoCommands.Clear();
        }
        #endregion
    }
}
