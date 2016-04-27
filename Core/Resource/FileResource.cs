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
        string relativePath;
        #endregion

        #region Public Members
        public string AbsolutePath { get { return absolutePath; } }
        public string RelatviePath { get { return relativePath; } }
        #endregion

        #region Constructor
        public FileResource(string label, string absolutePath)
            : base(label)
        {
            this.absolutePath = absolutePath;
            relativePath = absolutePath.Replace(AppDomain.CurrentDomain.BaseDirectory, "");
        }
        #endregion

        #region Public Methods
        public override void CheckValid()
        {
            if (relativePath != absolutePath)
                absolutePath = AppDomain.CurrentDomain.BaseDirectory + relativePath;
            
            isValid = System.IO.File.Exists(absolutePath);
        }
        #endregion
    }
}
