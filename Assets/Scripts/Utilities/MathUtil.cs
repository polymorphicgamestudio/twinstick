using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public static class MathUtil
{

    public static float2 ClampMagnitude(float2 data, float magnitude)
    {
        if ((data.x * data.x) + (data.y * data.y) > (magnitude * magnitude))
        {
            return math.normalize(data) * magnitude;
            

        }

        return data;

    }

    public static float SqrMagnitude(float2 data)
    {
        return ((data.x * data.x) + (data.y * data.y));
    }

    public static float Magnitude(float2 data)
    {
        float sqrMagnitude = SqrMagnitude(data);
        if (sqrMagnitude == 0)
            return 0;

        return math.sqrt(sqrMagnitude);

    }

    public static float RandomGaussian(float stdDev, float mean = 0)
    {
        //return 0;

        return mean + stdDev * 
            math.sqrt(-2.0f * math.log(UnityEngine.Random.value)) * math.sin(2.0f * Mathf.PI * UnityEngine.Random.value);
    }

    public static float RandomGaussianThreaded(float stdDev, float mean, Unity.Mathematics.Random rand)
    {
        return mean + stdDev *
    math.sqrt(-2.0f * math.log(rand.NextFloat(0, 1))) * math.sin(2.0f * Mathf.PI * rand.NextFloat(0, 1));

    }


}
