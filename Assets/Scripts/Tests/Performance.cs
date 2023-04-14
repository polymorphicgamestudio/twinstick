using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

public class Performance : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        int count = 1000000;
        float value = 0;


        Profiler.BeginSample("UnityEngine.Random");
        for (int i = 0; i < count; i++) {

            value = Random.value;


		}
		Profiler.EndSample();


		Unity.Mathematics.Random r = Unity.Mathematics.Random.CreateFromIndex((uint)(Time.time * 100000 * Time.realtimeSinceStartup));
		Profiler.BeginSample("Unity.Mathematics.Random");
		for (int i = 0; i < count; i++) {

			value = r.NextFloat();


		}

        Profiler.EndSample();

		System.Random rand = new System.Random((int)(Time.time * 100000 * Time.realtimeSinceStartup));
        double val = 0;

		Profiler.BeginSample("System.Random");

		for (int i = 0; i < count; i++) {

			val = rand.NextDouble();


		}
		Profiler.EndSample();

	}
}
