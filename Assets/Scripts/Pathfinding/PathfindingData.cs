using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace ShepProject
{

    [System.Serializable]
    public struct SquareNode
    {

        /*
         * only 2 states, walkable and unwalkable
         * 
         * 
         */

        public float2 position;
        public bool walkable;

    }


    [System.Serializable]
    public struct SquareGridSetupData
    {

        public bool fitPlayableArea;
        public int rows;
        public int columns;
        public float nodeLength;
        public float3 origin;
        public Color walkableColor;
        public Color unwalkableColor;

    }

    


}
