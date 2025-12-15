/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace CrazyStorm.Core
{
    public class ParticleManager
    {
        public delegate void ParticleDrawHanlder(Particle particle);
        public static event ParticleDrawHanlder OnParticleDraw;
        public delegate void CurveParticleDrawHandler(CurveParticle curveParticle);
        public static event CurveParticleDrawHandler OnCurveParticleDraw;

        //static ParticleQuadTree particleQuadTree;
        static int left;
        static int right;
        static int top;
        static int bottom;
        static int particlePreserved;
        static List<Particle> particlePool;
        static int particleIndex;
        static int curvePreserved;
        static List<CurveParticle> curveParticlePool;
        static int curveParticleIndex;
        static ParticleBase[] searchResult;
        public static void Initialize(int windowWidth, int windowHeight, int particlePreservedDist, int curvePreservedDist,
            int particleMaximum, int curveParticleMaximum)
        {
            //particleQuadTree = new ParticleQuadTree(-windowWidth, windowWidth, -windowHeight, windowHeight);
            left = -windowWidth;
            right = windowWidth;
            top = -windowHeight;
            bottom = windowHeight;
            particlePreserved = particlePreservedDist;
            curvePreserved = curvePreservedDist;
            particlePool = new List<Particle>(particleMaximum);
            for (int i = 0; i < particleMaximum; ++i)
            {
                var particle = new Particle();
                particle.ID = i;
                particlePool.Add(particle);
            }

            curveParticlePool = new List<CurveParticle>(curveParticleMaximum);
            for (int i = 0; i < curveParticleMaximum; ++i)
            {
                var curveParticle = new CurveParticle();
                curveParticle.ID = particleMaximum + i;
                curveParticlePool.Add(curveParticle);
            }
            Curve.Reset(curveParticleMaximum);
            searchResult = new ParticleBase[particleMaximum];
            OnParticleDraw = null;
            OnCurveParticleDraw = null;
        }
        public static ParticleBase GetParticle(int layerID, ParticleBase template)
        {
            int order = layerID * 1000000000 + 9 - (int)template.BlendType;
            if (template is Particle)
            {
                do
                {
                    particleIndex = (particleIndex + 1) % particlePool.Count;
                }
                while (particlePool[particleIndex].Alive);
                particlePool[particleIndex] = template.Clone() as Particle;
                particlePool[particleIndex].Reset();
                particlePool[particleIndex].Alive = true;
                particlePool[particleIndex].RenderOrder = order + particleIndex * 10;
                //particleQuadTree.Insert(particlePool[particleIndex]);
                return particlePool[particleIndex];
            }
            else
            {
                do
                {
                    curveParticleIndex = (curveParticleIndex + 1) % curveParticlePool.Count;
                }
                while (curveParticlePool[curveParticleIndex].Alive);
                curveParticlePool[curveParticleIndex] = template.Clone() as CurveParticle;
                curveParticlePool[curveParticleIndex].Reset();
                curveParticlePool[curveParticleIndex].Alive = true;
                curveParticlePool[curveParticleIndex].RenderOrder = order + curveParticleIndex * 10;
                //particleQuadTree.Insert(curveParticlePool[curveParticleIndex]);
                return curveParticlePool[curveParticleIndex];
            }
        }
        //public static void Insert(ParticleBase particleBase)
        //{
        //    particleQuadTree.Insert(particleBase);
        //}
        public static ParticleBase[] SearchByRect(float left, float right, float top, float bottom, out int count)
        {
            int index = 0;
            for (int i = 0; i < particlePool.Count; ++i)
            {
                if (particlePool[i].Alive)
                {
                    float x = particlePool[i].PPosition.x;
                    float y = particlePool[i].PPosition.y;
                    if (x >= left && x <= right && y >= top && y <= bottom)
                    {
                        searchResult[index++] = particlePool[i];
                    }
                }
            }
            for (int i = 0; i < curveParticlePool.Count; ++i)
            {
                if (curveParticlePool[i].Alive)
                {
                    float x = curveParticlePool[i].PPosition.x;
                    float y = curveParticlePool[i].PPosition.y;
                    if (x >= left && x <= right && y >= top && y <= bottom)
                    {
                        searchResult[index++] = curveParticlePool[i];
                    }
                }
            }
            count = index;
            return searchResult;
        }
        public static ParticleBase[] CheckCollision(float bx, float by, float x, float y, float r, out int count)
        {
            var index = 0;
            for (int i = 0; i < particlePool.Count; ++i)
            {
                if (particlePool[i].Alive && particlePool[i].CheckCollision(bx, by, x, y, r))
                {
                    searchResult[index++] = particlePool[i];
                }
            }
            for (int i = 0; i < curveParticlePool.Count; ++i)
            {
                if (curveParticlePool[i].Alive && curveParticlePool[i].CheckCollision(bx, by, x, y, r))
                {
                    searchResult[index++] = curveParticlePool[i];
                }
            }
            count = index;
            return searchResult;
        }
        public static bool OutOfWindow(ParticleBase particle)
        {
            var outPoint = particle.GetOutPoint();
            var reserved = particle is CurveParticle ? curvePreserved : particlePreserved;
            return outPoint.x < left / 2 - reserved || outPoint.x > right / 2 + reserved ||
                outPoint.y < top / 2 - reserved || outPoint.y > bottom / 2 + reserved;
        }
        private static bool OutOfRange(ParticleBase particle)
        {
            var outPoint = particle.GetOutPoint();
            return outPoint.x < left || outPoint.x > right ||
                outPoint.y < top || outPoint.y > bottom;
        }
        public static void Update()
        {
            particlePool.Sort();
            curveParticlePool.Sort();
            for (int i = 0; i < particlePool.Count; ++i)
            {
                int order = particlePool[i].RenderOrder;
                if (particlePool[i].Alive && !OutOfRange(particlePool[i]))
                    particlePool[i].Update();
                else if (particlePool[i].Alive)
                    particlePool[i].Alive = false;
            }
            for (int i = 0; i < curveParticlePool.Count; ++i)
            {
                if (curveParticlePool[i].Alive && !OutOfRange(curveParticlePool[i]))
                    curveParticlePool[i].Update();
                else if (curveParticlePool[i].Alive)
                {
                    Curve.Return(curveParticlePool[i].Curve);
                    curveParticlePool[i].Alive = false;
                }
            }
        }
        public static void Draw()
        {
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
