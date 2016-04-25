/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2015 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace CrazyStorm.Core
{
    public class VariableResource : Resource, ICloneable
    {
        #region Private Members
        float value;
        #endregion

        #region Public Members
        public float Value 
        { 
            get { return value; }
            set { this.value = value; }
        }
        #endregion

        #region Constructor
        public VariableResource(string label)
            : base(label)
        {
        }
        #endregion

        #region Public Methods
        public override void CheckValid()
        {
            isValid = true;
        }
        public object Clone()
        {
            return MemberwiseClone();
        }
        #endregion
    }
}
