using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CrazyStorm_Player.CrazyStorm
{
    interface IPlayData
    {
        void LoadPlayData(BinaryReader reader);
    }
}
