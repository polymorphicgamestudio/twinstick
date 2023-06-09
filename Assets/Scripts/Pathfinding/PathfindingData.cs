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

        private bool4x2 obstructions;

        public void ResetObstructions()
        {

            obstructions = default;

        }

        public bool LeftObstructed
        {
            get => obstructions.c0.w;
            set => obstructions.c0.w = value;
        }

        public bool TopLeftObstructed
        {
            get => obstructions.c0.x;
            set => obstructions.c0.x = value;
        }

        public bool TopObstructed
        {
            get => obstructions.c0.y;
            set => obstructions.c0.y = value;
        }

        public bool TopRightObstructed
        {
            get => obstructions.c0.z;
            set => obstructions.c0.z = value;
        }

        public bool RightObstructed
        {
            get => obstructions.c1.w;
            set => obstructions.c1.w = value;
        }

        public bool BottomRightObstructed
        {
            get => obstructions.c1.x;
            set => obstructions.c1.x = value;
        }

        public bool BottomObstructed
        {
            get => obstructions.c1.y;
            set => obstructions.c1.y = value;
        }

        public bool BottomLeftObstructed
        {
            get => obstructions.c1.z;
            set => obstructions.c1.z = value;
        }


    }

    public enum NodeDirection
    {
        Left = 1, TopLeft, Top, TopRight, Right, BottomRight, Bottom, BottomLeft, NoMovement

    }



    [System.Serializable]
    public struct PathNode : IComparable<PathNode>, IEquatable<PathNode>
    {
        public int parentIndex;
        public int index;

        public float gCost;
        public float hCost;
        public float FCost => gCost + hCost;

        public byte direction;

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

}
