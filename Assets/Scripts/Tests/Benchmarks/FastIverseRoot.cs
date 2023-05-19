using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;

public class FastIverseRoot : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        int count = 1000000;

        float inv = UnityEngine.Random.value * 100; 

        Profiler.BeginSample("SQRT");

        for (int i = 0; i < count; i++)
        {
            math.sqrt(inv);

        }

        Profiler.EndSample();

        float f = 0;
        Profiler.BeginSample("fast inverse");

        for (int i = 0; i < count; i++)
        {
            f =  inv / inv;
        }

        Profiler.EndSample();


    }
}
