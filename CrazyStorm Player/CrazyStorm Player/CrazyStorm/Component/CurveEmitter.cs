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
