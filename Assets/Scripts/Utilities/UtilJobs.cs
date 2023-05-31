using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;


namespace ShepProject
{

    public struct ResetNativeArrayJob<T> : IJobParallelFor
        where T : struct
    {
        //[NativeDisableContainerSafetyRestriction]
        public NativeArray<T> array;
        public void Execute(int index)
        {
            array[index] = new T();
        }
    }

    public struct ResetNativeArrayWithValueJob<T> : IJobParallelFor
    where T : struct
    {

        public T value;

        //[NativeDisableContainerSafetyRestriction]
        public NativeArray<T> array;
        public void Execute(int index)
        {
            array[index] = value;
        }
    }





}