/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2015 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ComponentModel;

namespace CrazyStorm.Core
{
    public class FileResource : Resource
    {
        #region Private Members
        string absolutePath;
        #endregion

        #region Public Members
        public string AbsolutePath { get { return absolutePath; } }
        #endregion

        #region Constructor
        public FileResource(string label, string absolutePath)
            : base(label)
        {
            this.absolutePath = absolutePath;
        }
        #endregion

        #region Public Methods
        public override void CheckValid()
        {
            valid = System.IO.File.Exists(absolutePath);
        }
        #endregion
    }
}
