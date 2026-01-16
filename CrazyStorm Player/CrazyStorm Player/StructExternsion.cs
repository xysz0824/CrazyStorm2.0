using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrazyStorm_Player
{
    internal static class StructExternsion
    {
        public static CrazyStorm.Core.Vector2 ToCore(this Vector2 v)
        {
            return new CrazyStorm.Core.Vector2(v.X, v.Y);
        }
        public static Vector2 ToXna(this CrazyStorm.Core.Vector2 v)
        {
            return new Vector2(v.x, v.y);
        }
        private static Vector2[] vectors;
        public static void SetValue(this EffectParameter effectParameter, CrazyStorm.Core.Vector2[] value)
        {
            if (vectors == null || vectors.Length != value.Length) vectors = new Vector2[value.Length];
            for (int i = 0; i < value.Length; ++i) vectors[i] = new Vector2(value[i].x, value[i].y);
            effectParameter.SetValue(vectors);
        }
    }
}
