/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace CrazyStorm.Core
{
    public interface IPlayable
    {
        bool Update(float frameScale, float currentFrame);
        void Reset();
    }
}
