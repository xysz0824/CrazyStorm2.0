using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX.Direct3D9;

namespace CrazyStorm_Player.CrazyStorm
{
    class ParticleManager
    {
        public delegate void ParticleDrawHanlder(Particle particle);
        public static event ParticleDrawHanlder OnParticleDraw;
        public delegate void CurveParticleDrawHandler(CurveParticle curveParticle);
        public static event CurveParticleDrawHandler OnCurveParticleDraw;

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
                ParticlePool[ParticleIndex].Alive = true;
                ParticlePool[ParticleIndex].Copy(template);
                ParticleQuadTree.Insert(ParticlePool[ParticleIndex]);
                return ParticlePool[ParticleIndex++];
            }
            else
            {
                CurveParticleIndex = CurveParticleIndex % CurveParticlePool.Count;
                CurveParticlePool[CurveParticleIndex].Alive = true;
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
                if (ParticlePool[i].Alive)
                    ParticlePool[i].Update();

            for (int i = 0; i < CurveParticlePool.Count; ++i)
                if (CurveParticlePool[i].Alive)
                    CurveParticlePool[i].Update();
        }
        public static void Draw()
        {
            if (OnParticleDraw != null)
            {
                for (int i = 0; i < ParticlePool.Count; ++i)
                    if (ParticlePool[i].Alive)
                        OnParticleDraw(ParticlePool[i]);
            }
            if (OnCurveParticleDraw != null)
            {
                for (int i = 0; i < CurveParticlePool.Count; ++i)
                    if (CurveParticlePool[i].Alive)
                        OnCurveParticleDraw(CurveParticlePool[i]);
            }
        }
    }
}
