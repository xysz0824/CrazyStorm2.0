/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2015 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrazyStorm.Core;

namespace CrazyStorm
{
    class ClipBoard
    {
        public event Action ContentChanged;

        List<Component> content;
        public ClipBoard()
        {
            content = new List<Component>();
        }
        public void Put(List<Component> content)
        {
            if (content != null)
            {
                foreach (var item in content)
                    this.content.Add(item);
                if (ContentChanged != null)
                    ContentChanged();
            }
        }
    }
}
