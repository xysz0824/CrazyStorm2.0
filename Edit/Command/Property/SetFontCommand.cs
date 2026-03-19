/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using CrazyStorm.Core;

namespace CrazyStorm
{
    class SetFontCommand : Command
    {
        public override void Redo(CommandStack stack)
        {
            base.Redo(stack);
            var text = Parameter[0] as Text;
            var newFontFamily = Parameter[1] as string;
            var newFontFace = Parameter[2] as string;
            var update = Parameter[3] as Action<Text, string,string>;
            History[0] = text.FontFamily;
            History[1] = text.FontFace;
            update(text, newFontFamily, newFontFace);
        }
        public override void Undo(CommandStack stack)
        {
            base.Undo(stack);
            var text = Parameter[0] as Text;
            var oldFontFamily = History[0] as string;
            var oldFontFace = History[1] as string;
            var update = Parameter[3] as Action<Text, string, string>;
            update(text, oldFontFamily, oldFontFace);
        }
    }
}
