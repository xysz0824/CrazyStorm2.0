using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrazyStorm.Core;

namespace CrazyStorm
{
    class ComponentFactory
    {
        private ComponentFactory()
        { }
        public static Component Create(string name)
        {
            switch (name)
            {
                case "Emitter":
                    return new Emitter();
                case "Laser":
                    return new Laser();
                case "Mask":
                    return new Mask();
                case "Rebound":
                    return new Rebound();
                case "Force":
                    return new Force();
                default:
                    return null;
            }
        }
    }
}
