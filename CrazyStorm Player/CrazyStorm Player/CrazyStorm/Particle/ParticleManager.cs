using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrazyStorm_Player.CrazyStorm
{
    class ParticleManager
    {
        public static ParticleQuadTree ParticleQuadTree { get; private set; }
        public static List<Particle> ParticlePool { get; private set; }
        public static int ParticleIndex { get; private set; }
        public static List<CurveParticle> CurveParticlePool { get; private set; }
        public static int CurveParticleIndex { get; private set; }
        public static void Initialize(int windowWidth, int windowHeight, int particleMaximum, int curveParticleMaximum)
        {
            ParticleQuadTree = new ParticleQuadTree(-windowWidth, windowWidth, -windowHeight, windowHeight);
            ParticlePool = new List<Particle>(particleMaximum);
            for (int i = 0; i < particleMaximum; ++i)
                ParticlePool.Add(new Particle());

            CurveParticlePool = new List<CurveParticle>(curveParticleMaximum);
            for (int i = 0; i < curveParticleMaximum; ++i)
                CurveParticlePool.Add(new CurveParticle());
        }
        public static ParticleBase GetParticle(ParticleBase template)
        {
            if (template is Particle)
            {
                ParticleIndex = ParticleIndex % ParticlePool.Count;
                ParticlePool[ParticleIndex].Copy(template);
                ParticleQuadTree.Insert(ParticlePool[ParticleIndex]);
                return ParticlePool[ParticleIndex++];
            }
            else
            {
                CurveParticleIndex = CurveParticleIndex % CurveParticlePool.Count;
                CurveParticlePool[CurveParticleIndex].Copy(template);
                ParticleQuadTree.Insert(CurveParticlePool[CurveParticleIndex]);
                return CurveParticlePool[CurveParticleIndex++];
            }
        }
        public static List<ParticleBase> SearchByRect(int left, int right, int top, int bottom)
        {
            return ParticleQuadTree.SearchByRect(left, right, top, bottom);
        }
        public static void Update()
        {
            for (int i = 0; i < ParticlePool.Count; ++i)
                ParticlePool[i].Update();

            for (int i = 0; i < CurveParticlePool.Count; ++i)
                CurveParticlePool[i].Update();
        }
    }
}
