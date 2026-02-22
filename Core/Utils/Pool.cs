/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CrazyStorm.Core
{
    public struct NullData 
    {
        public static NullData Empty;
    }
    public class PoolObject<T, B> where T : PoolObject<T, B>, new() where B : struct
    {
        static T[] pool;
        static int instanceID = 0;
        public static int InstanceID => instanceID;
        public bool PoolState { get; private set; }

        public static void Reset(int capacity)
        {
            pool = new T[capacity];
            for (int i = 0; i < pool.Length; ++i)
            {
                pool[i] = new T();
            }
            instanceID = 0;
        }
        public static T Rent(B initData)
        {
            T instance = null;
            int searchCount = 0;
            do
            {
                instance = pool[instanceID];
                instanceID = (instanceID + 1) % pool.Length;
                searchCount++;
                if (searchCount >= pool.Length)
                {
                    throw new Exception($"{typeof(T).Name} is overflowing");
                }
            }
            while (instance.PoolState);
            instance.Initialize(initData);
            instance.PoolState = true;
            return instance;
        }
        public static void Return(T instance)
        {
            if (instance == null) return;
            instance.OnReturn();
            instance.PoolState = false;
        }
        public virtual void Initialize(B initData) { }
        public virtual void OnReturn() { }
    }
}
