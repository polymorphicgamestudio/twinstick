using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;


namespace ShepProject
{

    public struct CopyNativeArrayJob<T> : IJobParallelFor
        where T : struct
    {

        public NativeArray<T> copyFrom;
        public NativeArray<T> copyTo;

        public void Execute(int index)
        {
            copyTo[index] = copyFrom[index];
        }

    }

    public struct ResetNativeArrayJob<T> : IJobParallelFor
        where T : struct
    {
        //[NativeDisableContainerSafetyRestriction]
        public NativeArray<T> array;
        public void Execute(int index)
        {
            array[index] = default;
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