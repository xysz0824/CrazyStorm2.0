/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2016
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace CrazyStorm.Core
{
    public class ParticleManager
    {
        public delegate void ParticleDrawHanlder(Particle particle);
        public static event ParticleDrawHanlder OnParticleDraw;
        public delegate void CurveParticleDrawHandler(CurveParticle curveParticle);
        public static event CurveParticleDrawHandler OnCurveParticleDraw;

        static ParticleQuadTree particleQuadTree;
        static int reserved;
        static List<Particle> particlePool;
        static int particleIndex;
        static List<CurveParticle> curveParticlePool;
        static int curveParticleIndex;
        public static void Initialize(int windowWidth, int windowHeight, int reservedDist,
            int particleMaximum, int curveParticleMaximum)
        {
            particleQuadTree = new ParticleQuadTree(-windowWidth, windowWidth, -windowHeight, windowHeight);
            reserved = reservedDist;
            particlePool = new List<Particle>(particleMaximum);
            for (int i = 0; i < particleMaximum; ++i)
                particlePool.Add(new Particle());

            curveParticlePool = new List<CurveParticle>(curveParticleMaximum);
            for (int i = 0; i < curveParticleMaximum; ++i)
                curveParticlePool.Add(new CurveParticle());
        }
        public static ParticleBase GetParticle(ParticleBase template)
        {
            if (template is Particle)
            {
                particleIndex = particleIndex % particlePool.Count;
                particlePool[particleIndex] = template.Copy() as Particle;
                particlePool[particleIndex].Alive = true;
                particleQuadTree.Insert(particlePool[particleIndex]);
                return particlePool[particleIndex++];
            }
            else
            {
                curveParticleIndex = curveParticleIndex % curveParticlePool.Count;
                curveParticlePool[curveParticleIndex] = template.Copy() as CurveParticle;
                curveParticlePool[curveParticleIndex].Alive = true;
                particleQuadTree.Insert(curveParticlePool[curveParticleIndex]);
                return curveParticlePool[curveParticleIndex++];
            }
        }
        public static void Insert(ParticleBase particleBase)
        {
            particleQuadTree.Insert(particleBase);
        }
        public static List<ParticleBase> SearchByRect(float left, float right, float top, float bottom)
        {
            return particleQuadTree.SearchByRect(left, right, top, bottom);
        }
        public static bool OutOfWindow(float x, float y)
        {
            return x < particleQuadTree.Left / 2 - reserved || x > particleQuadTree.Right / 2 + reserved ||
            y < particleQuadTree.Top / 2 - reserved || y > particleQuadTree.Bottom / 2 + reserved;
        }
        public static void Update()
        {
            for (int i = 0; i < particlePool.Count; ++i)
            {
                if (particlePool[i].Alive && !particleQuadTree.OutofRange(particlePool[i]))
                    particlePool[i].Update();
                else if (particlePool[i].Alive)
                    particlePool[i].Alive = false;
            }
            for (int i = 0; i < curveParticlePool.Count; ++i)
            {
                if (curveParticlePool[i].Alive && !particleQuadTree.OutofRange(curveParticlePool[i]))
                    curveParticlePool[i].Update();
                else if (curveParticlePool[i].Alive)
                    curveParticlePool[i].Alive = false;
            }
        }
        public static void Draw()
        {
            //TODO BlendType
            if (OnParticleDraw != null)
            {
                for (int i = 0; i < particlePool.Count; ++i)
                    if (particlePool[i].Alive)
                        OnParticleDraw(particlePool[i]);
            }
            if (OnCurveParticleDraw != null)
            {
                for (int i = 0; i < curveParticlePool.Count; ++i)
                    if (curveParticlePool[i].Alive)
                        OnCurveParticleDraw(curveParticlePool[i]);
            }
        }
    }
}
