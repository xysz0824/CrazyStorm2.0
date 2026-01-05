/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using System;
using System.Collections.Generic;
using System.Management.Instrumentation;
using System.Runtime.CompilerServices;
using System.Text;

namespace CrazyStorm.Core
{
    public class ParticlePool : PoolObject<ParticlePool, NullData>
    {
        public Particle Instance { get; private set; }
        public ParticlePool()
        {
            Instance = new Particle();
            Instance.PoolObject = this;
        }
    }
    public class CurveParticlePool : PoolObject<CurveParticlePool, NullData>
    {
        public CurveParticle Instance { get; private set; }
        public CurveParticlePool()
        {
            Instance = new CurveParticle();
            Instance.PoolObject = this;
        }
        public override void OnReturn()
        {
            if (Instance != null)
            {
                Curve.Return(Instance.Curve);
            }
        }
    }
    public static class ParticleManager
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
        static int curvePreserved;
        static int instanceID;
        static List<ParticleBase> activeParticles;
        static ParticleBase[] searchResult;

        public static int ActiveParticleCount => activeParticles != null ? activeParticles.Count : 0;
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
            instanceID = 0;
            ParticlePool.Reset(particleMaximum);
            CurveParticlePool.Reset(curveParticleMaximum);
            Curve.Reset(curveParticleMaximum);
            activeParticles = new List<ParticleBase>();
            searchResult = new ParticleBase[particleMaximum + curveParticleMaximum];
            OnParticleDraw = null;
            OnCurveParticleDraw = null;
        }
        public static ParticleBase GetParticle(int layerID, ParticleBase template)
        {
            ParticleBase particle = null;
            if (template is Particle)
            {
                var poolObject = ParticlePool.Rent(NullData.Empty);
                template.CopyTo(poolObject.Instance);
                particle = poolObject.Instance;
            }
            else if (template is CurveParticle)
            {
                var poolObject = CurveParticlePool.Rent(NullData.Empty);
                template.CopyTo(poolObject.Instance);
                particle = poolObject.Instance;
            }
            int order = layerID * 1000000000 + 9 - (int)template.BlendType;
            particle.RenderOrder = order + instanceID * 10;
            particle.ID = instanceID++;
            particle.Reset();
            particle.Alive = true;
            activeParticles.Add(particle);
            //particleQuadTree.Insert(particle);
            return particle;
        }
        //public static void Insert(ParticleBase particleBase)
        //{
        //    particleQuadTree.Insert(particleBase);
        //}
        public static ParticleBase[] SearchByRect(float left, float right, float top, float bottom, out int count)
        {
            int index = 0;
            for (int i = 0; i < activeParticles.Count; ++i)
            {
                var instance = activeParticles[i];
                if (instance.Alive)
                {
                    float x = instance.PPosition.x;
                    float y = instance.PPosition.y;
                    if (x >= left && x <= right && y >= top && y <= bottom)
                    {
                        searchResult[index++] = instance;
                    }
                }
            }
            count = index;
            return searchResult;
        }
        public static ParticleBase[] CheckCollision(float bx, float by, float x, float y, float r, out int count)
        {
            var index = 0;
            for (int i = 0; i < activeParticles.Count; ++i)
            {
                var instance = activeParticles[i];
                if (instance.Alive && instance.CheckCollision(bx, by, x, y, r))
                {
                    searchResult[index++] = instance;
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
            for (int i = 0; i < activeParticles.Count; ++i)
            {
                var instance = activeParticles[i];
                if (instance.Alive && !OutOfRange(instance)) instance.Update();
                else if (instance.Alive) instance.Alive = false;
                if (!instance.Alive)
                {
                    if (instance is Particle) ParticlePool.Return((instance as Particle).PoolObject);
                    else if (instance is CurveParticle) CurveParticlePool.Return((instance as CurveParticle).PoolObject);
                    activeParticles.RemoveAt(i);
                    i--;
                }
            }
        }
        public static void Draw(OrderType orderType)
        {
            if (orderType == OrderType.FirstAsTop) activeParticles.Sort();
            else activeParticles.Sort((a, b) => b.CompareTo(a));
            for (int i = 0; i < activeParticles.Count; ++i)
            {
                var instance = activeParticles[i];
                if (!instance.Alive) continue;
                if (instance is Particle) OnParticleDraw(instance as Particle);
                if (instance is CurveParticle) OnCurveParticleDraw(instance as CurveParticle);
            }
        }
    }
}
