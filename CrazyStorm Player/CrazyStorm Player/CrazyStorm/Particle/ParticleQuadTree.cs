/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2016
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrazyStorm_Player.CrazyStorm
{
    class ParticleQuadTree
    {
        const int MaxDepth = 4;
        public LinkedList<ParticleBase> Particles { get; private set; }
        public ParticleQuadTree[] children { get; private set; }
        public int Left { get; set; }
        public int Right { get; set; }
        public int Top { get; set; }
        public int Bottom { get; set; }
        public int OriginX { get; private set; }
        public int OriginY { get; private set; }
        public ParticleQuadTree(int left, int right, int top, int bottom)
        {
            children = new ParticleQuadTree[4];
            Left = left;
            Right = right;
            Top = top;
            Bottom = bottom;
            OriginX = (Left + Right) / 2;
            OriginY = (Top + Bottom) / 2;
        }
        public void Insert(ParticleBase particleBase, int depth = 0)
        {
            if (depth >= MaxDepth)
            {
                if (Particles == null)
                    Particles = new LinkedList<ParticleBase>();

                Particles.AddLast(particleBase);
                particleBase.QuadTree = this;
                return;
            }
            var x = particleBase.PPosition.x - OriginX;
            var y = particleBase.PPosition.y - OriginY;
            if (x > 0 && y > 0)
            {
                if (children[0] == null)
                    children[0] = new ParticleQuadTree(OriginX, Right, OriginY, Bottom);

                children[0].Insert(particleBase, depth + 1);
            }
            else if (x > 0 && y <= 0)
            {
                if (children[1] == null)
                    children[1] = new ParticleQuadTree(OriginX, Right, Top, OriginY);

                children[1].Insert(particleBase, depth + 1);
            }
            else if (x <= 0 && y <= 0)
            {
                if (children[2] == null)
                    children[2] = new ParticleQuadTree(Left, OriginX, OriginY, Bottom);

                children[2].Insert(particleBase, depth + 1);
            }
            else
            {
                if (children[3] == null)
                    children[3] = new ParticleQuadTree(Left, OriginX, Top, OriginY);

                children[3].Insert(particleBase, depth + 1);
            }
        }
        public List<ParticleBase> SearchByRect(float left, float right, float top, float bottom)
        {
            List<ParticleBase> results = new List<ParticleBase>();
            if (Particles != null)
            {
                results.AddRange(Particles);
                return results;
            }
            if (children[0] != null && right > OriginX && bottom > OriginY)
                results.AddRange(children[0].SearchByRect(left, right, top, bottom));
            
            if (children[1] != null && right > OriginX && top <= OriginY)
                results.AddRange(children[1].SearchByRect(left, right, top, bottom));
            
            if (children[2] != null && left <= OriginX && top <= OriginY)
                results.AddRange(children[2].SearchByRect(left, right, top, bottom));
            
            if (children[3] != null && left <= OriginX && bottom > OriginY)
                results.AddRange(children[3].SearchByRect(left, right, top, bottom));
            
            return results;
        }
        public bool OutofRange(ParticleBase particleBase)
        {
            return particleBase.PPosition.x < Left || particleBase.PPosition.x > Right ||
                particleBase.PPosition.y < Top || particleBase.PPosition.y > Bottom;
        }
        public void Update(ParticleBase particleBase)
        {
            if (OutofRange(particleBase))
            {
                Particles.Remove(particleBase);
                ParticleManager.Insert(particleBase);
            }
        }
    }
}
