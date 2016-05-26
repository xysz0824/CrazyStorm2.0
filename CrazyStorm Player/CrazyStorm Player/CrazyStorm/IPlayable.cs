using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrazyStorm_Player.CrazyStorm
{
    interface IPlayable
    {
        bool Update(int currentFrame);
        void Reset();
    }
}
