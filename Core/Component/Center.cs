/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrazyStorm.Core
{
    public class CenterPool : PoolObject<CenterPool, NullData>
    {
        public Center Instance { get; private set; }
        public CenterPool()
        {
            Instance = new Center();
            Instance.PoolObject = this;
        }
    }
    public class Center : Component
    {
        public CenterPool PoolObject { get; set; }
        public override Component Instantiate()
        {
            return CenterPool.Rent(NullData.Empty).Instance;
        }
        public override void Destroy()
        {
            CenterPool.Return(PoolObject);
        }
    }
}
