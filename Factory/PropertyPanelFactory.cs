using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using CrazyStorm.Core;

namespace CrazyStorm
{
    class PropertyPanelFactory
    {
        private PropertyPanelFactory()
        { }
        public static UserControl Create(Component component)
        {
            switch (component.GetType().Name)
            {
                case "Emitter":
                    return new EmitterPanel(component as Emitter);
                case "Laser":
                    return new LaserPanel(component as Laser);
                case "Mask":
                    return new MaskPanel(component as Mask);
                case "Rebound":
                    return new ReboundPanel(component as Rebound);
                case "Force":
                    return new ForcePanel(component as Force);
                default:
                    return null;
            }
        }
    }
}
