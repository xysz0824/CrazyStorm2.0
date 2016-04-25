/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2015 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrazyStorm.Core
{
    public class MultiEmitter : Emitter
    {
        #region Private Members
        Particle particle;
        #endregion

        #region Public Members
        public Particle Particle { get { return particle; } }
        #endregion

        #region Constructor
        public MultiEmitter()
            : base()
        {
            particle = new Particle();
        }
        #endregion

        #region Public Methods
        public override object Clone()
        {
            var clone = base.Clone() as MultiEmitter;
            clone.particle = particle.Clone() as Particle;
            return clone;
        }
        #endregion
    }
}
