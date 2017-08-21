/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2017
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace CrazyStorm.Core
{
    public interface ILoadPlayData
    {
        void LoadPlayData(BinaryReader reader, float version);
    }
}
