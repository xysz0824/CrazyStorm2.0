/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2016
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrazyStorm_Player.CrazyStorm
{
    class CurveEmitter : Emitter
    {
        public CurveEmitter()
        {
            Template = new CurveParticle();
        }
    }
}
