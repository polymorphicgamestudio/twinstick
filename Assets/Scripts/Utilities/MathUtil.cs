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
        return math.sqrt((data.x * data.x) + (data.y * data.y));

    }


}
