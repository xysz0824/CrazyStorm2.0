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
        public static readonly int MAX_MASK_COUNT = 8;

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
        static Vector2[] maskPositionArray = new Vector2[MAX_MASK_COUNT];
        static Vector2[] maskSizeArray = new Vector2[MAX_MASK_COUNT];
        static float[] maskShapeArray = new float[MAX_MASK_COUNT];
        static float[] maskTypeArray = new float[MAX_MASK_COUNT];
        static float[] maskRotateArray = new float[MAX_MASK_COUNT];
        static int maskCount;
        public static int ActiveParticleCount => activeParticles != null ? activeParticles.Count : 0;
        public static Vector2[] MaskPositionArray => maskPositionArray;
        public static Vector2[] MaskSizeArray => maskSizeArray;
        public static float[] MaskShapeArray => maskShapeArray;
        public static float[] MaskTypeArray => maskTypeArray;
        public static float[] MaskRotateArray => maskRotateArray;
        public static int MaskCount => maskCount;
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
            int order = layerID * searchResult.Length * 10 + 9 - (int)template.BlendType;
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
        public static ParticleBase[] SearchByRect(Vector2 center, float halfW, float halfH, float rotation, out int count)
        {
            int index = 0;
            for (int i = 0; i < activeParticles.Count; ++i)
            {
                var instance = activeParticles[i];
                if (instance.Alive)
                {
                    var v = MathHelper.Rotate(instance.PPosition - center, -rotation);
                    if (v.x >= -halfW && v.x <= halfW && v.y >= -halfH && v.y <= halfH)
                    {
                        searchResult[index++] = instance;
                    }
                }
            }
            count = index;
            return searchResult;
        }
        public static ParticleBase[] SearchByEllipse(Vector2 center, float halfW, float halfH, float rotation, out int count)
        {
            int index = 0;
            for (int i = 0; i < activeParticles.Count; ++i)
            {
                var instance = activeParticles[i];
                if (instance.Alive)
                {
                    var v = MathHelper.Rotate(instance.PPosition - center, -rotation);
                    if ((v.x * v.x) / (halfW * halfW) + (v.y * v.y) / (halfH * halfH) <= 1f)
                    {
                        searchResult[index++] = instance;
                    }
                }
            }
            count = index;
            return searchResult;
        }
        public static ParticleBase[] CheckCollision(bool dead, Vector2 playerLast, Vector2 player, float r, out int count, out Vector2 newPos)
        {
            var index = 0;
            newPos = player;
            for (int i = 0; i < activeParticles.Count; ++i)
            {
                var instance = activeParticles[i];
                if (!instance.Alive) continue;
                if (Masked(instance.PPosition))
                {
                    instance.PMasked = true;
                    continue;
                }
                if (instance.CheckCollision(playerLast, player, r)) searchResult[index++] = instance;
                else
                {
                    var judge = instance.CheckVolume(dead, playerLast, player, out newPos);
                    if (judge)  searchResult[index++] = instance;
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
        private static bool Masked(Vector2 pos)
        {
            if (maskCount == 0) return false;
            float result = MathHelper.Lerp(1, MathHelper.Lerp(0, 1, MaskTypeArray[0] - 1), MaskTypeArray[0]);
            for (int i = 0; i < maskCount; ++i)
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
                result = MathHelper.Lerp(result, MathHelper.Lerp(result + (masked ? 0 : 1), result * (masked ? 1 : 0), MaskTypeArray[i] - 1), Math.Min(1, MaskTypeArray[i]));
            }
            return result == 0;
        }
        public static void UpdateLayerMasks(IList<Layer> layers)
        {
            maskCount = 0;
            Array.Clear(MaskPositionArray, 0, MAX_MASK_COUNT);
            Array.Clear(MaskSizeArray, 0, MAX_MASK_COUNT);
            Array.Clear(MaskShapeArray, 0, MAX_MASK_COUNT);
            Array.Clear(MaskTypeArray, 0, MAX_MASK_COUNT);
            Array.Clear(MaskRotateArray, 0, MAX_MASK_COUNT);
            for (int i = 0; i < layers.Count; ++i)
            {
                for (int j = 0; j < layers.Count; ++j)
                {
                    if (j == i) continue;
                    foreach (var component in layers[j].Components)
                    {
                        var eventField = component as EventField;
                        if (eventField == null) continue;
                        if (eventField.BindingTarget == null)
                        {
                            if (maskCount >= MAX_MASK_COUNT || !eventField.LayerMask || !eventField.Visibility || !eventField.LayerMaskMutex) continue;
                            MaskPositionArray[maskCount] = eventField.Position;
                            MaskSizeArray[maskCount] = new Vector2(eventField.HalfWidth, eventField.HalfHeight);
                            MaskShapeArray[maskCount] = eventField.FieldShape == FieldShape.Circle ? 1 : 0;
                            MaskTypeArray[maskCount] = eventField.LayerMaskType == LayerMaskType.OutsideMask ? 1 : 2;
                            MaskRotateArray[maskCount] = (float)MathHelper.DegToRad(eventField.Rotation);
                            maskCount++;
                        }
                        else
                        {
                            eventField.BindingUpdate((frameRate) =>
                            {
                                if (maskCount >= MAX_MASK_COUNT || !eventField.LayerMask || !eventField.Visibility || !eventField.LayerMaskMutex) return;
                                MaskPositionArray[maskCount] = eventField.Position;
                                MaskSizeArray[maskCount] = new Vector2(eventField.HalfWidth, eventField.HalfHeight);
                                MaskShapeArray[maskCount] = eventField.FieldShape == FieldShape.Circle ? 1 : 0;
                                MaskTypeArray[maskCount] = eventField.LayerMaskType == LayerMaskType.OutsideMask ? 1 : 2;
                                MaskRotateArray[maskCount] = (float)MathHelper.DegToRad(eventField.Rotation);
                                maskCount++;
                            }, false, 0);
                        }
                    }
                }
                foreach (var component in layers[i].Components)
                {
                    var eventField = component as EventField;
                    if (eventField == null) continue;
                    if (eventField.BindingTarget == null)
                    {
                        if (maskCount >= MAX_MASK_COUNT || !eventField.LayerMask || !eventField.Visibility) continue;
                        MaskPositionArray[maskCount] = eventField.Position;
                        MaskSizeArray[maskCount] = new Vector2(eventField.HalfWidth, eventField.HalfHeight);
                        MaskShapeArray[maskCount] = eventField.FieldShape == FieldShape.Circle ? 1 : 0;
                        MaskTypeArray[maskCount] = (int)eventField.LayerMaskType + 1;
                        MaskRotateArray[maskCount] = (float)MathHelper.DegToRad(eventField.Rotation);
                        maskCount++;
                    }
                    else
                    {
                        eventField.BindingUpdate((frameRate) =>
                        {
                            if (maskCount >= MAX_MASK_COUNT || !eventField.LayerMask || !eventField.Visibility) return;
                            MaskPositionArray[maskCount] = eventField.Position;
                            MaskSizeArray[maskCount] = new Vector2(eventField.HalfWidth, eventField.HalfHeight);
                            MaskShapeArray[maskCount] = eventField.FieldShape == FieldShape.Circle ? 1 : 0;
                            MaskTypeArray[maskCount] = (int)eventField.LayerMaskType + 1;
                            MaskRotateArray[maskCount] = (float)MathHelper.DegToRad(eventField.Rotation);
                            maskCount++;
                        }, false, 0);
                    }
                }
            }
        }
        public static void Update(float frameRate)
        {
            for (int i = 0; i < activeParticles.Count; ++i)
            {
                var instance = activeParticles[i];
                var frameScale = instance.System.FrameFactor * ParticleSystem.FRAME_RATE_BASE / frameRate;
                if (instance.Alive && !OutOfRange(instance)) instance.Update(frameScale);
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
            if (orderType == OrderType.FirstAsTop) activeParticles.Sort((a, b) => b.CompareTo(a));
            else activeParticles.Sort();
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
