using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrazyStorm_Player.CrazyStorm
{
    class ReferenceHelper
    {
        public static void RebuildObjectReference(CrazyStorm.File file)
        {
            foreach (var particleSystem in file.ParticleSystems)
            {
                //Rebuild all custom types
                foreach (var customType in particleSystem.CustomTypes)
                    customType.RebuildReferenceFromCollection(file.Images);
                //Collect all particle types
                var particleTypes = new List<ParticleType>();
                //TODO
                particleTypes.AddRange(particleSystem.CustomTypes);
                //Collect all components
                var components = new List<Component>();
                foreach (var layer in particleSystem.Layers)
                    components.AddRange(layer.Components);
                //Rebuild components reference
                foreach (var component in components)
                {
                    component.RebuildReferenceFromCollection(components);
                    //Rebuild particles reference
                    if (component is Emitter)
                        (component as Emitter).Template.RebuildReferenceFromCollection(particleTypes);
                }
            }
        }
    }
}
