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
    public class CurveEmitter : Emitter
    {
        #region Private Members
        CurveParticle curveParticle;
        #endregion

        #region Public Members
        public CurveParticle CurveParticle { get { return curveParticle; } }
        #endregion

        #region Constructor
        public CurveEmitter()
            : base()
        {
            Name = "NewCurveEmitter";
            curveParticle = new CurveParticle();
        }
        #endregion
    }
}
