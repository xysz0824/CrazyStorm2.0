/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2015 
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CrazyStorm.Core
{
    public interface IPlayData
    {
        List<byte> GeneratePlayData();
    }
}
