using System;
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
    public struct PathNode : IComparable<PathNode>, IEquatable<PathNode>
    {
        public int parentIndex;
        public int index;

        public float gCost;
        public float hCost;
        public float FCost => gCost + hCost;

        public int CompareTo(PathNode other)
        {
            if (FCost < other.FCost)
            {
                return -1;
            }
            else if (FCost > other.FCost)
            {
                return 1;
            }

            return 0;

        }

        public bool Equals(PathNode other)
        {
            return index == other.index;
        }


        public override string ToString()
        {
            return "I: " + index + "G: " + gCost + " H: " + hCost + "F: " + FCost;
        }

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

	//will be used for queueing a job
	//then this info will be used inside the job, not the float3s
    public struct PathStartInfo
    {
        public int startNodeIndex;
        public int endNodeIndex;


    }


    public struct PathData
    {

        /*
         * a pointer to the array which stores the indices of which nodes are in the path
         *      - int 
         * length of the path
         *      - ushort
         * 
         * paths will need to have a max length to which allocates the node index arrays
         * 
         * 
         */


        public int pathStartIndex;
        public ushort pathLength;


    }



}
