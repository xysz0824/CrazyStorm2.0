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
    class FirstAsTopComparer : IComparer<ParticleBase>
    {
        public static readonly FirstAsTopComparer Instance = new FirstAsTopComparer();
        public int Compare(ParticleBase a, ParticleBase b)
        {
            int c = b.Emitter.LayerID - a.Emitter.LayerID;
            if (c != 0) return c;
            c = (int)((9 - a.RenderOrder % 10) - (9 - b.RenderOrder % 10));
            if (c != 0) return c;
            return (int)(a.RenderOrder - b.RenderOrder);
        }
    }
    class LastAsTopComparer : IComparer<ParticleBase>
    {
        public static readonly LastAsTopComparer Instance = new LastAsTopComparer();
        public int Compare(ParticleBase a, ParticleBase b)
        {
            int c = b.Emitter.LayerID - a.Emitter.LayerID;
            if (c != 0) return c;
            c = (int)((9 - a.RenderOrder % 10) - (9 - b.RenderOrder % 10));
            if (c != 0) return c;
            BlendType blendType = (BlendType)(9 - a.RenderOrder % 10);
            if (blendType != BlendType.AlphaBlend) return (int)(a.RenderOrder - b.RenderOrder);
            return (int)(b.RenderOrder - a.RenderOrder);
        }
    }
    public static class ParticleManager
    {
        static void ReturnParticle(ParticleBase instance)
        {
            if (instance is Particle) ParticlePool.Return((instance as Particle).PoolObject);
            else if (instance is CurveParticle) CurveParticlePool.Return((instance as CurveParticle).PoolObject);
            instance.Emitter?.Particles.Remove(instance);
        }
        static void UpdateParticles(List<ParticleBase> particles, float frameScale)
        {
            for (int i = 0; i < particles.Count; ++i)
            {
                var instance = particles[i];
                if (instance.Alive) instance.Update(frameScale);
                else
                {
                    ReturnParticle(instance);
                    particles.RemoveAt(i);
                    i--;
                }
            }
        }
        public static readonly int MAX_MASK_COUNT = 8;

        public delegate void LayerDrawHandler(ParticleSystem system, Layer layer, BlendType blendType);
        public static event LayerDrawHandler OnLayerDraw;
        public delegate void ParticleDrawHandler(Particle particle);
        public static event ParticleDrawHandler OnParticleDraw;
        public delegate void CurveParticleDrawHandler(CurveParticle curveParticle);
        public static event CurveParticleDrawHandler OnCurveParticleDraw;

        //static ParticleQuadTree particleQuadTree;
        static int left, right, top, bottom;
        static int particlePreserved, curvePreserved;
        static long currentIndex;
        static Dictionary<ParticleSystem, List<ParticleBase>> activeParticles;
        static ParticleBase[] searchResult;
        static Vector2[] maskPositionArray = new Vector2[MAX_MASK_COUNT];
        static Vector2[] maskSizeArray = new Vector2[MAX_MASK_COUNT];
        static float[] maskShapeArray = new float[MAX_MASK_COUNT];
        static float[] maskLayerArray = new float[MAX_MASK_COUNT];
        static float[] maskRotateArray = new float[MAX_MASK_COUNT];
        static Vector2[] maskRotateTrigArray = new Vector2[MAX_MASK_COUNT];
        static Vector2[] maskEllipseInvSizeSqArray = new Vector2[MAX_MASK_COUNT];
        public static int MaximumParticleCount => searchResult.Length;
        public static int ActiveParticleCount
        {
            get
            {
                int count = 0;
                if (activeParticles != null)
                {
                    foreach (var particles in activeParticles)
                    {
                        count += particles.Value.Count;
                    }
                }
                return count;
            }
        }
        public static Vector2[] MaskPositionArray => maskPositionArray;
        public static Vector2[] MaskSizeArray => maskSizeArray;
        public static float[] MaskShapeArray => maskShapeArray;
        public static float[] MaskLayerArray => maskLayerArray;
        public static float[] MaskRotateArray => maskRotateArray;
        public static Vector2[] MaskRotateTrigArray => maskRotateTrigArray;
        public static Vector2[] MaskEllipseInvSizeSqArray => maskEllipseInvSizeSqArray;
        public static int MaskCount { get; set; }
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
            currentIndex = 0;
            ParticleSystem.Reset(100);
            Layer.Reset(500);
            MultiEmitterPool.Reset(5000);
            CurveEmitterPool.Reset(500);
            EventFieldPool.Reset(500);
            RebounderPool.Reset(500);
            ForceFieldPool.Reset(500);
            CenterPool.Reset(500);
            ParticlePool.Reset(particleMaximum);
            CurveParticlePool.Reset(curveParticleMaximum);
            Curve.Reset(curveParticleMaximum);
            activeParticles = new Dictionary<ParticleSystem, List<ParticleBase>>();
            searchResult = new ParticleBase[particleMaximum + curveParticleMaximum];
            EventExecutor.Reset(MaximumParticleCount * 10);
            OnParticleDraw = null;
            OnCurveParticleDraw = null;
        }
        public static ParticleBase GetParticle(ParticleSystem system, int layerID, ParticleBase template)
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
            particle.System = system;
            particle.RenderOrder = currentIndex * 10 + 9 - (int)template.BlendType;
            particle.ID = (currentIndex++) % MaximumParticleCount;
            particle.Reset();
            particle.Alive = true;
            if (!activeParticles.ContainsKey(particle.System)) activeParticles[particle.System] = new List<ParticleBase>();
            activeParticles[particle.System].Add(particle);
            //particleQuadTree.Insert(particle);
            return particle;
        }
        //public static void Insert(ParticleBase particleBase)
        //{
        //    particleQuadTree.Insert(particleBase);
        //}
        public static ParticleBase[] SearchByRect(ParticleSystem system, 
            Vector2 center, float halfW, float halfH, float rotation, out int count)
        {
            int index = 0;
            if (!activeParticles.ContainsKey(system))
            {
                count = 0;
                return searchResult;
            }
            var particles = activeParticles[system];
            for (int i = 0; i < particles.Count; ++i)
            {
                var instance = particles[i];
                if (!instance.Alive) continue;
                var v = MathHelper.Rotate(instance.PPosition - center, -rotation);
                if (v.x >= -halfW && v.x <= halfW && v.y >= -halfH && v.y <= halfH)
                {
                    searchResult[index++] = instance;
                }
            }
            count = index;
            return searchResult;
        }
        public static ParticleBase[] SearchByEllipse(ParticleSystem system, 
            Vector2 center, float halfW, float halfH, float rotation, out int count)
        {
            int index = 0;
            if (!activeParticles.ContainsKey(system))
            {
                count = 0;
                return searchResult;
            }
            var particles = activeParticles[system];
            for (int i = 0; i < particles.Count; ++i)
            {
                var instance = particles[i];
                if (!instance.Alive) continue;
                var v = MathHelper.Rotate(instance.PPosition - center, -rotation);
                if ((v.x * v.x) / (halfW * halfW) + (v.y * v.y) / (halfH * halfH) <= 1f)
                {
                    searchResult[index++] = instance;
                }
            }
            count = index;
            return searchResult;
        }
        public static ParticleBase[] CheckCollision(
            bool dead, Vector2 playerLast, Vector2 player, float r, out int count, out Vector2 newPos)
        {
            var index = 0;
            newPos = player;
            foreach (var kv in activeParticles)
            {
                var particles = kv.Value;
                for (int i = 0; i < particles.Count; ++i)
                {
                    var instance = particles[i];
                    if (!instance.Alive) continue;
                    if (Masked(instance.PPosition))
                    {
                        instance.PMasked = true;
                        continue;
                    }
                    var logicOffset = instance.System.LogicOffset;
                    if (instance.CheckCollision(playerLast - logicOffset, player - logicOffset, r)) searchResult[index++] = instance;
                    else
                    {
                        var judge = instance.CheckVolume(dead, playerLast - logicOffset, player - logicOffset, out newPos);
                        newPos += logicOffset;
                        if (judge) searchResult[index++] = instance;
                        player = newPos;
                    }
                }
            }
            count = index;
            return searchResult;
        }
        public static bool OutOfWindow(ParticleBase particle)
        {
            var outPoint = particle.GetOutPoint() + particle.System.LogicOffset;
            var reserved = particle is CurveParticle ? curvePreserved : particlePreserved;
            return outPoint.x < left / 2 - reserved || outPoint.x > right / 2 + reserved ||
                outPoint.y < top / 2 - reserved || outPoint.y > bottom / 2 + reserved;
        }
        private static bool Masked(Vector2 pos)
        {
            if (MaskCount == 0) return false;
            float result = MathHelper.Lerp(1, MathHelper.Lerp(0, 1, MaskLayerArray[0] - 1), MaskLayerArray[0]);
            for (int i = 0; i < MaskCount; ++i)
            {
                bool masked = false;
                var d = pos - MaskPositionArray[i];
                d = MathHelper.Rotate(d, -MaskRotateArray[i]);
                if (MaskShapeArray[i] == 0) //Rect
                {
                    masked = d.x >= MaskSizeArray[i].x || -MaskSizeArray[i].x >= d.x || d.y >= MaskSizeArray[i].y || -MaskSizeArray[i].y >= d.y;
                }
                else if (MaskShapeArray[i] == 1) //Ellipse
                {
                    d.x *= d.x;
                    d.y *= d.y;
                    masked = d.x / (MaskSizeArray[i].x * MaskSizeArray[i].x) + d.y / (MaskSizeArray[i].y * MaskSizeArray[i].y) >= 1;
                }
                result = MathHelper.Lerp(result, MathHelper.Lerp(result + (masked ? 0 : 1), result * (masked ? 1 : 0), MaskLayerArray[i] - 1), Math.Min(1, MaskLayerArray[i]));
            }
            return result == 0;
        }
        public static void ClearLayerMasks()
        {
            MaskCount = 0;
            Array.Clear(MaskPositionArray, 0, MAX_MASK_COUNT);
            Array.Clear(MaskSizeArray, 0, MAX_MASK_COUNT);
            Array.Clear(MaskShapeArray, 0, MAX_MASK_COUNT);
            Array.Clear(MaskLayerArray, 0, MAX_MASK_COUNT);
            Array.Clear(MaskRotateArray, 0, MAX_MASK_COUNT);
            Array.Clear(MaskRotateTrigArray, 0, MAX_MASK_COUNT);
            Array.Clear(MaskEllipseInvSizeSqArray, 0, MAX_MASK_COUNT);
        }
        public static void UpdateLayerMasks(GenericContainer<Layer> layers, Layer layer)
        {
            for (int j = 0; j < layers.Count; ++j)
            {
                if (layers[j] == layer) continue;
                for (int k = 0; k < layers[j].Components.Count; ++k)
                {
                    var eventField = layers[j].Components[k] as EventField;
                    if (eventField == null) continue;
                    if (eventField.BindingTarget == null)
                    {
                        eventField.UpdateMutexMask();
                    }
                    else
                    {
                        eventField.BindingUpdate(1, false, 0);
                    }
                }
            }
            for (int k = 0; k < layer.Components.Count; ++k)
            {
                var eventField = layer.Components[k] as EventField;
                if (eventField == null) continue;
                if (eventField.BindingTarget == null)
                {
                    eventField.UpdateLayerMask();
                }
                else
                {
                    eventField.BindingUpdate(2, false, 0);
                }
            }
        }
        public static void UpdateLayerMasks(GenericContainer<Layer> layers)
        {
            for (int i = 0; i < layers.Count; ++i)
            {
                UpdateLayerMasks(layers, layers[i]);
            }
        }
        public static void Update(float frameRate)
        {
            foreach (var kv in activeParticles)
            {
                var particles = kv.Value;
                var frameScale = kv.Key.FrameFactor * ParticleSystem.FRAME_RATE_BASE / frameRate;
                UpdateParticles(particles, frameScale);
            }
        }
        public static void SkipFrame(ParticleSystem system, bool replayFromStart, float frameRate)
        {
            if (!activeParticles.ContainsKey(system)) return;
            var particles = activeParticles[system];
            if (replayFromStart)
            {
                for (int i = 0; i < particles.Count; ++i)
                {
                    ReturnParticle(particles[i]);
                }
                particles.Clear();
                activeParticles.Remove(system);
                return;
            }
            var frameScale = system.FrameFactor * ParticleSystem.FRAME_RATE_BASE / frameRate;
            UpdateParticles(particles, frameScale);
        }
        public static void Draw(ParticleSystem system)
        {
            if (!activeParticles.ContainsKey(system)) return;
            var particles = activeParticles[system];
            if (system.OrderType == OrderType.FirstAsTop) particles.Sort(FirstAsTopComparer.Instance);
            else particles.Sort(LastAsTopComparer.Instance);
            int currentLayerID = -1;
            for (int i = 0; i < particles.Count; ++i)
            {
                var instance = particles[i];
                if (!instance.Alive) continue;
                var layerID = instance.Emitter.LayerID;
                if (currentLayerID != layerID)
                {
                    currentLayerID = layerID;
                    BlendType blendType = (BlendType)(9 - instance.RenderOrder % 10);
                    OnLayerDraw(system, system.Layers[layerID], blendType);
                }
                if (instance is Particle) OnParticleDraw(instance as Particle);
                if (instance is CurveParticle) OnCurveParticleDraw(instance as CurveParticle);
            }
        }
    }
}
