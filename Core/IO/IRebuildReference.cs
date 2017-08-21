/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2017 
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace CrazyStorm.Core
{
    public interface IRebuildReference<T>
    {
        void RebuildReferenceFromCollection(IList<T> collection);
    }
}
